<?php

include getenv('LIBDIR') . "/config.inc";

// Redis 연결 설정
initializeConnections();

// Redis Clients
$mainClient     = $GLOBALS["MGClient"];
$scoreClient    = $GLOBALS["WRClient"];
$accountClient  = $GLOBALS["UClient"];

// Model Import
use sample\model\userProfile;
use sample\model\socialManager;
use sample\model\userCore;
use sample\model\inventory;
use common\UserAccount;

$modelGuild     = new \sample\model\guildManager();
$modelProfile   = new \sample\model\userProfile();
$modelSocial    = new \sample\model\socialManager();
$modelUser      = new \sample\model\userCore();
$modelInventory = new \sample\model\inventory();
$modelFoodie    = new \sample\model\foodieManager();
$modelAccount   = new \common\UserAccount($GLOBALS["UClient"]);

// 길드 ID 범위 설정
$guildStartId   = DEFAULT_GUILD_ID;
$guildEndId     = $accountClient->get("GUILD_SEQUENCE");

// 시즌 업데이트
$currentSeason  = $mainClient->get('GUILD_SEASON_SEQ');
$newSeason      = $mainClient->incrby('GUILD_SEASON_SEQ', 1);

// 보상 기준 목록
$rewardCategories = array(
    REWARD_COIN  => TYPE_COIN,
    REWARD_RUBY  => TYPE_RUBY,
    REWARD_FOOD  => TYPE_FOOD,
);

// 개인 랭킹 보상 처리
for ($guildId = $guildStartId; $guildId <= $guildEndId; $guildId++) {
    foreach ($rewardCategories as $rewardKey => $categoryType) {
        $rankingList = $modelGuild->getTopContributors($categoryType, $guildId, 0, 0, $currentSeason, true);
        
        if ($rankingList[0][COUNT] > 0) {
            $rewardInfo = $modelGuild->fetchRewardInfo(REWARD_MODE_INDIVIDUAL, $rewardKey);
            
            $playerId       = $rankingList[0][USER_NO];
            $itemType       = $rewardInfo[REWARD_TYPE];
            $itemId         = $rewardInfo[REWARD_ID];
            $itemQty        = $rewardInfo[REWARD_VALUE];
            $rewardMessage  = $rewardInfo[MSG];
            
            dispatchRewardMail($playerId, FEED_TYPE_GUILD_LOCAL, $itemType, $itemId, $itemQty, $rewardMessage);

            $guildMember = $modelGuild->getMemberDetails($guildId, $playerId);
            $rankingCount = 0;

            switch ($categoryType) {
                case TYPE_COIN:
                    $rankingCount = $guildMember[RANK_COUNT_COIN] + 1;
                    break;
                case TYPE_RUBY:
                    $rankingCount = $guildMember[RANK_COUNT_RUBY] + 1;
                    break;
                case TYPE_FOOD:
                    $rankingCount = $guildMember[RANK_COUNT_FOOD] + 1;
                    break;
            }
            $modelGuild->updateContributionRanking($guildId, $playerId, $categoryType, $rankingCount);

            $guildInfo   = $modelGuild->getGuildDetails($guildId);            
            $accData     = $modelAccount->getAccountInfo($playerId);
            $userData    = $modelUser->getUserDetails($playerId);
            $itemDetails = $modelInventory->getItem($itemId);
            $itemDesc    = $itemDetails[NAME] . ":" . $itemDetails[SUB_NAME];

            \sample\logger\GameLog::logGuildReward(\sample\logger\GameLog::TYPE_REWARD_LOCAL, $accData[ACCOUNT_GUID], "", "", "", 
                $userData[USERNAME], $userData[LEVEL], $userData[CHARM_TOTAL], $userData[FAME_TOTAL], 
                $guildInfo[GUILD_NAME], $guildInfo[GUILD_ID], $guildInfo[GUILD_LEVEL], $currentSeason, 
                $rewardKey, 0, 0, 0, 0, 0, 0, $itemDesc, "", "");
        }
    }
}

// 월드 랭킹 보상 처리
$worldRankKeys = array(
    RANK_TYPE_RUBY => GLOBAL_RANK_RUBY,
    RANK_TYPE_GOLD => GLOBAL_RANK_GOLD
);

$requiredPoint = $modelGuild->getStarPointThreshold(3)[NEED_POINT];

foreach ($worldRankKeys as $rankType => $redisKeyPrefix) {
    $rankKey = "{" . "$redisKeyPrefix}:$currentSeason";
    
    //최고 스코어 확인
    $topEntry = $scoreClient->zrevrange($rankKey, 0, 0, "withscores")[0][1];
    //참여이력이 전혀 없을때.. 
    if (empty($topEntry)) {
        continue; 
    }

    $topScore = $topEntry[0][1];
    //같은 스코어 길드리스트 작성
    $topGuilds = $rankingClient->zrevrangebyscore($rankKey, $topScore, $topScore, "withscores");

    //동점 스코어 셔플 1위길드 랜덤 선별
    if (count($topGuilds) > 1) {
        srand(intval($currentSeason));
        shuffle($topGuilds);
    }

    foreach ($topGuilds as $guildRank => $guildEntry) {
        $guildId = $guildEntry[0];
        $guildPoints = $modelGuild->getStarPoints($guildId);
        
        //길드가 등급이 지급기준등급 이하면 스코어가 높아도 지급하지 않는다.
        if ($guildPoints < $requiredPoint) 
            continue;

        $topReward = $modelGuild->fetchGuildRankReward($rankType);
        $guildMembers = $modelGuild->getAllGuildMembers($guildId);
        
        foreach ($guildMembers as $memberId) {
            dispatchRewardMail($memberId, FEED_TYPE_GUILD_WORLD, $topReward[REWARD_TYPE], $topReward[REWARD_ID], $topReward[REWARD_VALUE], $topReward[MSG]);
        }

        $guildDetails = $modelGuild->getGuildDetails($guildId);
        $itemMeta = $modelInventory->getItem($topReward[REWARD_ID]);
        $itemDesc = $itemMeta[NAME] . ":" . $itemMeta[SUB_NAME];
        $logType = ($rankType == RANK_TYPE_RUBY) ? 2 : 1;

        \sample\logger\GameLog::logGuildReward(\sample\logger\GameLog::TYPE_REWARD_WORLD, "TopGlobalGuild", "", "", "", "", "", "", "",
            $guildDetails[GUILD_NAME], $guildDetails[GUILD_ID], $guildDetails[GUILD_LEVEL], $currentSeason, 
            $logType, 0, 0, 0, 0, 0, 0, $itemDesc, "", "");
        
        // 한 개 길드만 처리 후 종료
        break; 
    }
}

// 시즌 데이터 초기화
for ($guildId = $guildStartId; $guildId <= $guildEndId; $guildId++) {
    $modelGuild->clearContributionRanking($guildId);
    $modelGuild->resetSeasonData($guildId);
}

// 보상 우편 전송 함수
function dispatchRewardMail($userId, $feedCategory, $itemType, $itemId, $quantity, $message)
{
    global $modelProfile, $modelSocial;

    $profile = $modelProfile->getUserProfile($userId);
    $sender = array(NAME => ADMIN_FEED_NAME, THUMBNAIL => ADMIN_FEED_THUMB);

    $feedContent = createFeedMessage(true, NOW, $sender[NAME], $feedCategory, $message, $itemType, $itemId, $quantity, $sender[THUMBNAIL]);
    $modelSocial->addFeedToUser($userId, $feedContent);

    $flags = $profile[FLAG];
    $updatedFlags = bitSetup($flags, USER_FLAG_FEED_NOTICE);
    $modelProfile->updateUserFlags($updatedFlags, $userId);
}
