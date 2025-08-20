<?php
class WebhookLibrary extends Bootstrap
{
    private $model = null;
    private $sendboxLib = null;

    public function init()
    {
        $this->model = $this->loadModel('Webhook');
        $this->sendboxLib = $this->loadLibrary('SendBox');
    }

    // 사용자 유효성 검사
    public function user_validation($jsonbody, $reqSignature)
    {
        $dataStd = json_decode($jsonbody);
        $user = $dataStd->user;
        $id = (int)$user->id;
        
        $this->check_signature($jsonbody, $reqSignature);
        
        // uservalidation check 
        Bootstrap::$acountDb = MysqliLib::getAccountInst(0, 0);
        $ret = $this->model->user_validation($id);

        if($ret['CNT'] > 0)
        {
            return WebhookStatusCode::SUCCESS_204;
        }
        else
        {
            $this->throwError(WebhookReturnMsg::INVALID_USER, 'Invalid user', WebhookStatusCode::REQUEST_INFO_ERROR_400);
        }

    }

    // 게임유저가 결제 프로세스를 완료할 때 마다 트랜잭션 세부 사항 전송
    public function payment($jsonbody, $reqSignature)
    {
        $dataStd = json_decode($jsonbody);
        $key = $dataStd->transaction->id;

        Bootstrap::$uno = (int)$dataStd->user->id;
        Bootstrap::$userDb = MysqliLib::getUserInst(1, 0, Bootstrap::$uno);
        
        $this->check_signature($jsonbody, $reqSignature, $key);

        $uptCnt = $this->model->insertStatus($key, WebhookStatus::INIT, $jsonbody);
        
        if($uptCnt==0) 
            $this->throwError(WebhookReturnMsg::ALREADY_PROGRESS, 'Transactions already in progress', WebhookStatusCode::REQUEST_INFO_ERROR_400);
        
            Bootstrap::$userDb->commit();
        return WebhookStatusCode::SUCCESS_200;
    }


    // 결제 완료 (지급)
    public function order_paid($jsonbody, $reqSignature)
    {
        $dataStd = json_decode($jsonbody);
        $key = $dataStd->order->invoice_id;
        $updParams = [];

        Bootstrap::$uno = (int)$dataStd->user->external_id;
        Bootstrap::$userDb = MysqliLib::getUserInst(1, 0, Bootstrap::$uno);

        $this->check_signature($jsonbody, $reqSignature, $key);

        // Status => processing 으로 변경
        $this->updateStatus($key, WebhookStatus::PROCESSING, $jsonbody);

        
        // Reward Data
        foreach ($dataStd->items as $value) {
            $quantity = $value->quantity;
            $itemInfo = explode("_", $value->sku);

            // 구매 개수만큼 추가
            for ($i=1; $i <= $quantity; $i++) { 
                $updParams[] = [ 'typ' => $itemInfo[0], 'idx' => $itemInfo[1], 'cnt' => $itemInfo[2]];
            }
        }

        $this->sendboxLib->insertSendBox(Bootstrap::$uno, Mail_type::webshop, $updParams, 60*24*365, 'Webshop');
        
        // Status => success 으로 변경
        $this->updateStatus($key, WebhookStatus::SUCCESS, $jsonbody);

        return WebhookStatusCode::SUCCESS_204;
    }

    // 결제 취소
    public function order_canceled($jsonbody, $reqSignature)
    {
        $dataStd = json_decode($jsonbody);

        Bootstrap::$uno = (int)$dataStd->user->external_id;
        Bootstrap::$userDb = MysqliLib::getUserInst(1, 0, Bootstrap::$uno);
        
        $this->check_signature($jsonbody, $reqSignature);

        $this->updateStatus($dataStd->order->invoice_id, WebhookStatus::CANCEL, $jsonbody);
        return WebhookStatusCode::SUCCESS_200;
    }
    

    // 진행중인 거래의 중복, 에러 등 status를 DB에 적재
    public function updateStatus($key, $status, $jsonbody)
    {
        $key = (int)$key;
        $cnt = $this->model->updateStatus($key, $status, $jsonbody);

        if($cnt > 0) 
        {
            Bootstrap::$userDb->commit();
        }
        else 
        {
            $this->throwError(WebhookReturnMsg::NO_TRX_PROGRESS, 'No transaction in progress', WebhookStatusCode::REQUEST_INFO_ERROR_400);
        }
    }

    // Header의 Signature가 Json Body + SecretKey의 Sha1과 동일한지 검증
    public function check_signature($jsonbody, $reqSignature, $key=0)
    {
        $sha = sha1($jsonbody.XSOLLA_WEBHOOK_SECRET_KEY, false);
        $servSignature = 'Signature '.$sha;

        if($servSignature != $reqSignature)
        {
            $this->throwError(WebhookReturnMsg::INVALID_SIGNATURE, 'INVALID SIGNATURE', WebhookStatusCode::REQUEST_INFO_ERROR_400, $key, __FUNCTION__);
        }
    }

    
    public function throwError($err, $msg, $code, $key=null, $func=null)
    {
        // DB Rollback
        if(!empty(Bootstrap::$acountDb))
            Bootstrap::$acountDb->rollback();
        if(!empty(Bootstrap::$userDb))
            Bootstrap::$userDb->rollback();

        switch ($func) {
            case 'check_signature':
                if($key != 0) $this->model->updateErrorCode($key, $code, $err);
                Bootstrap::$userDb->commit();
                break;
            
            default:
                
                break;
        }

        throw new WebhookException($err.','.$msg, $code);
    }
}