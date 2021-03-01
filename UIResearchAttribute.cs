using MysticManaCommon.ContentBase;
using MysticManaCommon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AttributeSlotInfo
{
    public MtNationAttribute attributeInfo = null;
    public short learningLevel = 0;
    public bool isLearning = false;
    public DateTime learningEndTime;
    public DateTime learningStartTime;

    public AttributeSlotInfo() {}
    public AttributeSlotInfo(MtNationAttribute attribute, short learningLevel, bool isLearning, DateTime learningEndTime , DateTime learningStartTime)
    {
        this.attributeInfo = attribute;
        this.learningLevel = learningLevel;
        this.isLearning = isLearning;
        this.learningEndTime = learningEndTime;
        this.learningStartTime = learningStartTime;
    }
}

public class UIResearchAttribute : UIWindow {
  
    private const string prefabPath = "Prefab/UI/Attribute/UIResearchAttribute";
    private static new UIResearchAttribute singleton = null;

    public static UIResearchAttribute Create(bool bAdvisorDoIt = false)
    {
        GameObject go = Instantiate(MtAssetBundles.Load(prefabPath) as GameObject);
        UIResearchAttribute window = UIWindow.Create<UIResearchAttribute>(go, GetCanvasCenter(), true);
        window.bAdvisorDoIt = bAdvisorDoIt;

        if (bAdvisorDoIt)
        {
            window.transform.localPosition = UIUtility.UnMeaningPosition;
        }

        singleton = window;

        return window;
    }

    public static UIResearchAttribute Get()
    {
        return singleton;
    }

    public Dictionary<MtNationAttributeTypes, AttributeSlot> attributeSlots = new Dictionary<MtNationAttributeTypes, AttributeSlot>();

    private MtMyNationAttribute researchInProgressAttribute = null;

    private MtNationAttribute curSelectedAttributeInfo = null;

    [SerializeField] private GameObject internalAffairsGridGO = null;
    [SerializeField] private GameObject combatGridGO = null;

    [SerializeField] private Button internalAffairsTabButton = null;
    [SerializeField] private Button combatTabButton = null;
    [SerializeField] private ScrollRect scrollRect = null;
    [SerializeField] private GameObject attributeInfoWindow = null;


    //텍스트
    [SerializeField] private Text curSelectedAttributeNameText = null;
    [SerializeField] private Text curSelectedAttributeDescriptionText = null;
    [SerializeField] private Text category1Text = null;
    [SerializeField] private Text category2Text = null;


    [SerializeField] GameObject maxLevelLabel = null;
    [SerializeField] GameObject levelInfoPanel = null;
    [SerializeField] private Text curSelectedAttributeCurLevelText = null;
    [SerializeField] private Text curSelectedAttributeNextLevelText = null;

    [SerializeField] private GameObject addValueTextPanel = null;
    [SerializeField] private Text curSelectedAttributeCurValueText = null;
    [SerializeField] private Text curSelectedAttributeNextValueText = null;
    [SerializeField] private Text maxValueLabel = null;

    [SerializeField] private GameObject Gold = null;
    [SerializeField] private Text GoldAmount = null;
    [SerializeField] private GameObject ManaStone = null;
    [SerializeField] private Text ManaStoneAmount = null;
    [SerializeField] private GameObject Stone = null;
    [SerializeField] private Text StoneAmount = null;
    [SerializeField] private GameObject Wood = null;
    [SerializeField] private Text WoodAmount = null;
    [SerializeField] private Text learnDurationTimeText = null;

    [SerializeField] private GameObject requredMageTowerLevel = null;
    [SerializeField] private Text requredMageTowerLevelText = null;


    [SerializeField] private Button detailViewButton = null;

    [SerializeField] private Button researchButton = null;
    [SerializeField] private Button justNowButton = null;
    [SerializeField] private Button accelationButton = null;
    [SerializeField] private Text progressBarTimeText = null;

    [SerializeField] private GameObject researchCostPanel = null;
    [SerializeField] private GameObject researchProgressPanel = null;
    [SerializeField] private Slider researchProgressSlider = null;


    //디테일패널 변수
    [SerializeField] private GameObject detailPanel = null;
    [SerializeField] private Transform detailElementGridTrs = null;
    [SerializeField] private GameObject detailElementPrefab = null;
    
    [SerializeField] private Text detailAttributeNameText = null;
    [SerializeField] private Text affectValueContentText = null;

    [SerializeField] private Transform goldCostIcon = null;
    [SerializeField] private Transform manastoneCostIcon = null;
    [SerializeField] private Transform stoneCostIcon = null;
    [SerializeField] private Transform woodCostIcon = null;

    [SerializeField] private GameObject selectSlotMark = null;
    [SerializeField] private Image attributeImage = null;
    [SerializeField] private Slider levelSlider = null;
    [SerializeField] private Text levelSliderText = null;

    [SerializeField] private GameObject currentResearchPanel = null;
    [SerializeField] private Image currentAttributeImage = null;
    [SerializeField] private Slider currentTimeSlider = null;
    [SerializeField] private Text currentTimeText = null;

    [SerializeField] private Text costGemText = null;

    private bool bAdvisorDoIt = false;
    private List<MtItemTypes> shortageTypes = new List<MtItemTypes>();
    private List<long> shortageValue = new List<long>();
    private MtNationAttributeTypes lastClickType = MtNationAttributeTypes.None;

    private int goldCost, stoneCost, woodCost;
    
    public override void Initialize()
    {
        base.Initialize();
     
        ////서버로 부터 특성리스트를 요청한다 Response델리게이트 전달       
        LobbyScene.Get().RequestNationAttributeList(OnResponseNationAttributeList);

        if (!bAdvisorDoIt)
        {
            ZoneTerrain.Get().uiHome.Hide();
        }
    }
    protected override void Update()
    {
        researchInProgressAttribute = AccountInfo.instance.GetResearchInProgressAttribute();
        if (researchInProgressAttribute != null)
        {
            TimeSpan ts = researchInProgressAttribute.LearningEndTime - researchInProgressAttribute.LearningStartTime;
            double learnTotalSecond = ts.TotalSeconds;

            TimeSpan remainTs = researchInProgressAttribute.LearningEndTime - DateTime.Now;
            double remainSecond = learnTotalSecond - remainTs.TotalSeconds;
            
            currentTimeSlider.value = (float)remainSecond / (float)learnTotalSecond;
            currentTimeText.text = UIUtility.GetProgressTimeText(remainTs);

            if (currentTimeSlider.value >= 1)
            {
                Close();
            }

            if (curSelectedAttributeInfo != null && curSelectedAttributeInfo.AttributeType == researchInProgressAttribute.AttributeType)
            {
                researchProgressSlider.value = currentTimeSlider.value;
                progressBarTimeText.text = currentTimeText.text;
            }

            
        }

        base.Update();
    }
    private void OnResponseNationAttributeList(MtPacket_NationAttributes_Response typed_pk)
    {
        //패킷 특성리스트 작성
        AccountInfo.instance.RefreshAttributeList(typed_pk.NationAttributes);

        MtMyNationAttribute researchInProgressAttribute = AccountInfo.instance.GetResearchInProgressAttribute();

        if(researchInProgressAttribute != null)
        {
            
            researchButton.interactable = false;

            this.researchInProgressAttribute = researchInProgressAttribute;

            currentResearchPanel.SetActive(true);
            currentAttributeImage.sprite = UIUtility.LoadResearchAttributeImage(researchInProgressAttribute.AttributeType);
        }

        if (bAdvisorDoIt)
        {
            ZoneTile tile = ZoneTerrain.Get().zoneTiles.GetMyTile(MtTileTypes.BuildingMageTower);

            int duration = 999999999;
            for (MtNationAttributeTypes attType = MtNationAttributeTypes.Architecture; attType <= MtNationAttributeTypes.Diplomacy; ++attType)
            {
                int level = AccountInfo.instance.GetMyNationAttributeLevel(attType);
                MtNationAttribute minAtt = MtDataManager.GetAttributeData(attType, 1);

                if (minAtt.MaxLevel > level)
                {
                    MtNationAttribute att = MtDataManager.GetAttributeData(attType, level + 1);

                    if (att != null && att.LearningDuration < duration && tile.tileInfo.TileLevel >= att.RequiredMageTowerLevel)
                    {
                        duration = att.LearningDuration;
                        curSelectedAttributeInfo = att;
                    }
                }
            }

            for (MtNationAttributeTypes attType = MtNationAttributeTypes.StrengtheningResearch; attType <= MtNationAttributeTypes.Machlessness; ++attType)
            {
                int level = AccountInfo.instance.GetMyNationAttributeLevel(attType);
                MtNationAttribute minAtt = MtDataManager.GetAttributeData(attType, 1);

                if (minAtt.MaxLevel > level)
                {
                    MtNationAttribute att = MtDataManager.GetAttributeData(attType, level + 1);

                    if (att != null && att.LearningDuration < duration && tile.tileInfo.TileLevel >= att.RequiredMageTowerLevel)
                    {
                        duration = att.LearningDuration;
                        curSelectedAttributeInfo = att;
                    }
                }
            }

            if (curSelectedAttributeInfo == null)
            {
                UIAdvisorTalkPop.Create(tile.transform, MWText.instance.GetText(MWText.EText.E_2398), true);
            }
            else
            {
                OnClickResearchButton();
            }
        }

        if (PlayerPrefs.HasKey("LastResearchTab"))
        {
            if ((MtNationAttributeUseTypes)PlayerPrefs.GetInt("LastResearchTab", 0) == MtNationAttributeUseTypes.InternalAffairs)
            {
                OnClickInternalAffairsTab();
            }
            else
            {
                OnClickCombatTab();
            }
        }
        else
        {
            OnClickInternalAffairsTab();
        }

        int tutorialStep = PlayerPrefs.GetInt("TutorialStep26", 0);

        if (tutorialStep == 0 && !bAdvisorDoIt)
        {
            UITutorial.Create(26);
        }
    }

    public void OnResponseLearnNationAttribute(MtPacket_LearnNationAttribute_Response typed_pk)
    {
        //클라이언트에서 조건 확인후 Request하기때문에 (-1, 0)값은 오면 안됨.
        if (typed_pk.Result == -1) //다른 연구 진행중
        {
            return;
        }
        else if (typed_pk.Result == -2)
        {
            return; //선행특성조건 불만족
        }
        else if (typed_pk.Result == -3)
        {
            return; //Max레벨
        }
        else if (typed_pk.Result == 0)//자원부족
        {
            return;
        }
        else
        {
            researchButton.GetComponentInChildren<Text>().text = MWText.instance.GetText(MWText.EText.E_948);
            researchButton.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);
            researchButton.interactable = false;

            //시작성공했으면 AccountInfo 값변경
            AccountInfo.instance.RefreshAttribute(typed_pk.MyNationAttribute);
            researchInProgressAttribute = AccountInfo.instance.GetResearchInProgressAttribute();

            if (researchInProgressAttribute != null)
            {
                if (curSelectedAttributeInfo.AttributeType == researchInProgressAttribute.AttributeType)
                {
                    TimeSpan ts = researchInProgressAttribute.LearningEndTime - DateTime.Now;
                    progressBarTimeText.text = ConverTimeToString((int)ts.TotalSeconds);
                }
            }

            AccountInfo.instance.gold -= typed_pk.CostGold;
            AccountInfo.instance.stone -= typed_pk.CostStone;
            AccountInfo.instance.wood -= typed_pk.CostWood;
            AccountInfo.instance.gem -= typed_pk.CostGem;

            UIKingdomInfo.Get().ResetAccountData();

            if(researchInProgressAttribute != null)
            {
                OnClickSlot(researchInProgressAttribute.AttributeType);
            }

            PlayerPrefs.SetInt("LastResearchTab", (int)curSelectedAttributeInfo.AttributeUseType);

            if (bAdvisorDoIt)
            {
                ZoneTile tile = ZoneTerrain.Get().zoneTiles.GetMyTile(MtTileTypes.BuildingMageTower);

                if (tile != null)
                {
                    string msg = string.Format(MWText.instance.GetText(MWText.EText.E_2174), curSelectedAttributeInfo.AttributeName + " " + UIUtility.GetLevelString(curSelectedAttributeInfo.CurLevel));

                    UIAdvisorTalkPop.Create(tile.transform, msg);
                }
            }

            AudioPlayer.Get().PlaySituationSound(SituationSoundType.ResearchAttribute);

            Close();
        }
    }

    public void OnClickInternalAffairsTab()
    {
        internalAffairsGridGO.gameObject.SetActive(true);
        combatGridGO.gameObject.SetActive(false);

        researchButton.gameObject.SetActive(false);
        accelationButton.gameObject.SetActive(false);

        SetButtonLockRecursively(internalAffairsTabButton.gameObject);
        SetReleaseLockButtonRecursively(combatTabButton.gameObject);

        RectTransform rectTrs = internalAffairsGridGO.GetComponent<RectTransform>();
        scrollRect.content = rectTrs;
        rectTrs.anchoredPosition = Vector2.zero;

        ClearSelectedAttributeInfo();
    }

    public void OnClickCombatTab()
    {
        combatGridGO.gameObject.SetActive(true);
        internalAffairsGridGO.gameObject.SetActive(false);

        researchButton.gameObject.SetActive(false);
        accelationButton.gameObject.SetActive(false);

        category1Text.color = new Color(85f / 255f, 52f / 255f, 18f / 255f, 128f / 255f);
        category2Text.color = new Color(255f / 255f, 246f / 255f, 200f / 255f);

        SetButtonLockRecursively(combatTabButton.gameObject);
        SetReleaseLockButtonRecursively(internalAffairsTabButton.gameObject);

        RectTransform rectTrs = combatGridGO.GetComponent<RectTransform>();
        scrollRect.content = rectTrs;
        rectTrs.anchoredPosition = Vector2.zero;

        ClearSelectedAttributeInfo();
    }

    [SerializeField] private Sprite backgroundImg1 = null;
    [SerializeField] private Sprite backgroundImg2 = null;
    public void OnClickDetailViewButton()
    {
        if (curSelectedAttributeInfo == null)
            return;

        detailAttributeNameText.text = curSelectedAttributeInfo.AttributeName;
        affectValueContentText.text = GetSimpleAttributeDescriptionText(curSelectedAttributeInfo.AttributeType);

        int level = AccountInfo.instance.GetMyNationAttributeLevel(curSelectedAttributeInfo.AttributeType);

        for (int i = 0; i < curSelectedAttributeInfo.MaxLevel; i++)
        {
            MtNationAttribute attribute = MtDataManager.GetAttributeData(curSelectedAttributeInfo.AttributeType, i + 1);
            GameObject go = Instantiate(detailElementPrefab);
            go.transform.SetParent(detailElementGridTrs);
            Text thisText1 = go.transform.GetChild(0).GetComponent<Text>();
            Text thisText2 = go.transform.GetChild(1).GetComponent<Text>();
            Text thisText3 = go.transform.GetChild(2).GetComponent<Text>();

            if (level == i + 1)
            {
                thisText1.color = new Color(254f / 255f, 222f / 255f, 135f / 255f);
                thisText2.color = new Color(254f / 255f, 222f / 255f, 135f / 255f);
                thisText3.color = new Color(254f / 255f, 222f / 255f, 135f / 255f);
                go.GetComponent<Image>().sprite = backgroundImg1;
            }
            else if (detailElementGridTrs.childCount % 2 != 0)
                go.GetComponent<Image>().sprite = backgroundImg2;

            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            thisText1.text = attribute.CurLevel.ToString();

            //기능추가인 특성은 설명으로 affectValue설정
            if (curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.BlackHand || curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.AssembleAttack)
                thisText2.text = curSelectedAttributeInfo.AttributeDescription;
            else
                thisText2.text = attribute.AffectValue.ToString();

            //AffectValue 표기설정
            if (curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.EndlessConquer || curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.EndlessMarch || curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.RemnantOfPower)
            {
                //초
                thisText2.text = go.transform.GetChild(1).GetComponent<Text>().text + MWText.instance.GetText(MWText.EText.E_1800);
            }
            else if (curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.AssembleCommand)
            {
                //명
                thisText2.text = go.transform.GetChild(1).GetComponent<Text>().text + MWText.instance.GetText(MWText.EText.E_1799);
            }
            else if (curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.BreakAtLimit || curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.BlackHand || curSelectedAttributeInfo.AttributeType == MtNationAttributeTypes.AssembleAttack)
            {
                //무표기
            }
            else
            {
                //%
                thisText2.text = go.transform.GetChild(1).GetComponent<Text>().text + "%";
            }
        }

        detailPanel.gameObject.SetActive(true);
    }

    public void OnClickCloseDetailPanelButton()
    {
        while(detailElementGridTrs.childCount > 0)
        {
            DestroyImmediate(detailElementGridTrs.GetChild(0).gameObject);
        }

        detailPanel.gameObject.SetActive(false);
    }
   
    public void OnClickResearchButton()
    {
        if (bAdvisorDoIt && shortageTypes.Count > 0)
        {
            ZoneTile tile = ZoneTerrain.Get().zoneTiles.GetMyTile(MtTileTypes.BuildingMageTower);

            if (tile != null)
            {
                UIAdvisorTalkPop.Create(tile.transform, MWText.instance.GetText(MWText.EText.E_2175), true);

                int tutorialStep = PlayerPrefs.GetInt("TutorialStep32", 0);

                if (tutorialStep == 0 && !UITutorial.IsLoaded())
                {
                    UITutorial.Create(32);
                }
            }

            Close();
            return;
        }

        if (shortageTypes.Count > 0)
        {
            UIShortageResources.Create(shortageTypes.ToArray(), shortageValue.ToArray(), delegate { RefreshCost(goldCost, stoneCost, woodCost); }, false);
        }
        else
        { 
            LobbyScene.Get().RequestLearnNationAttribute(curSelectedAttributeInfo.AttributeType, false , OnResponseLearnNationAttribute);
        }
    }

    public void OnClickJustNowButton()
    {
        if (shortageTypes.Count > 0)
        {
            UIShortageResources.Create(shortageTypes.ToArray(), shortageValue.ToArray(), delegate { RefreshCost(goldCost, stoneCost, woodCost); }, false);
        }
        else if(AccountInfo.instance.gem < MtStatic.GetJustNowCompleteGemCost(curSelectedAttributeInfo.LearningDuration))
        {
            UIToastMessage.Create(MWText.instance.GetText(MWText.EText.E_2281));
        }
        else
        {
            if(AccountInfo.instance.gold >= goldCost && AccountInfo.instance.stone >= stoneCost && AccountInfo.instance.wood >= woodCost)
             LobbyScene.Get().RequestLearnNationAttribute(curSelectedAttributeInfo.AttributeType, true, OnResponseLearnNationAttribute);
            else
                UIToastMessage.Create(MWText.instance.GetText(MWText.EText.E_2175));
        }
    }
   
    public void OnClickSlot(MtNationAttributeTypes type)
    {
        MtNationAttribute attributeInfo = null;
        MtNationAttribute nextAttributeInfo = null;

        int nextLevelCostGold = 0;
        int nextLevelCostStone = 0;
        int nextLevelCostWood = 0;
        int nextLevelLearningDurationTime = 0;

        int requiredMageTowerLevel = 0;

        bool researchAvailableFlag = true;

        //습득한 특성이면
        if (AccountInfo.instance.HasAttribute(type))
        {
            //0레벨 예외처리
            int curLevel = AccountInfo.instance.GetMyNationAttributeLevel(type);

            attributeInfo = MtDataManager.GetAttributeData(type, AccountInfo.instance.GetMyNationAttributeLevel(type) == 0 ? 1 : AccountInfo.instance.GetMyNationAttributeLevel(type));

            //MaxLevel이 아닐시
            if (attributeInfo.CurLevel < attributeInfo.MaxLevel)
            {
                nextAttributeInfo = MtDataManager.GetAttributeData(type, curLevel + 1);

                levelInfoPanel.gameObject.SetActive(true);
                maxLevelLabel.gameObject.SetActive(false);
                curSelectedAttributeCurLevelText.text = curLevel.ToString();
                curSelectedAttributeNextLevelText.text = nextAttributeInfo.CurLevel.ToString();


                addValueTextPanel.gameObject.SetActive(true);
                maxValueLabel.gameObject.SetActive(false);
                curSelectedAttributeCurValueText.text = (curLevel == 0 ? 0 : attributeInfo.AffectValue).ToString() + GetSignFromAttributeType(attributeInfo.AttributeType);
                curSelectedAttributeCurValueText.GetComponent<RectTransform>().sizeDelta = new Vector2(curSelectedAttributeCurValueText.preferredWidth, curSelectedAttributeCurValueText.preferredHeight);
                curSelectedAttributeNextValueText.text = nextAttributeInfo.AffectValue.ToString() + GetSignFromAttributeType(attributeInfo.AttributeType);
                curSelectedAttributeNextValueText.GetComponent<RectTransform>().sizeDelta = new Vector2(curSelectedAttributeNextValueText.preferredWidth, curSelectedAttributeNextLevelText.preferredHeight);

                nextLevelCostGold = nextAttributeInfo.Cost_Gold;
                nextLevelCostStone = nextAttributeInfo.Cost_Stone;
                nextLevelCostWood = nextAttributeInfo.Cost_Wood;
                nextLevelLearningDurationTime = nextAttributeInfo.LearningDuration;


                requiredMageTowerLevel = nextAttributeInfo.RequiredMageTowerLevel;                
            }
            else
            {
                levelInfoPanel.gameObject.SetActive(false);
                maxLevelLabel.gameObject.SetActive(true);

                addValueTextPanel.gameObject.SetActive(false);
                maxValueLabel.gameObject.SetActive(true);

                string str = attributeInfo.AffectValue.ToString() + GetSignFromAttributeType(attributeInfo.AttributeType);
                maxValueLabel.text = string.Format("(<color=#57FF4F> +{0}</color> )", str);


                researchAvailableFlag = false;
            }

            curSelectedAttributeNameText.text = attributeInfo.AttributeName;
            curSelectedAttributeDescriptionText.text = attributeInfo.AttributeDescription;
        }
        //습득하지 않은경우
        else
        {
            attributeInfo = MtDataManager.GetAttributeData(type, 1);

            levelInfoPanel.gameObject.SetActive(true);
            maxLevelLabel.gameObject.SetActive(false);

            curSelectedAttributeCurLevelText.text = "0"; 
            curSelectedAttributeNextLevelText.text = attributeInfo.CurLevel.ToString();

            maxValueLabel.gameObject.SetActive(true);
            addValueTextPanel.gameObject.SetActive(false);
            string str = attributeInfo.AffectValue.ToString() + GetSignFromAttributeType(attributeInfo.AttributeType);
            maxValueLabel.text = string.Format("(<color=#57FF4F> +{0}</color> )", str);

            curSelectedAttributeNameText.text = attributeInfo.AttributeName;
            curSelectedAttributeDescriptionText.text = attributeInfo.AttributeDescription;

            nextLevelCostGold = attributeInfo.Cost_Gold;
            nextLevelCostStone = attributeInfo.Cost_Stone;
            nextLevelCostWood = attributeInfo.Cost_Wood;
            nextLevelLearningDurationTime = attributeInfo.LearningDuration;

            requiredMageTowerLevel = attributeInfo.RequiredMageTowerLevel;
        }

        curSelectedAttributeInfo = attributeInfo;
        detailViewButton.interactable = true;

        MtTileInfo mageTowerTileInfo = ZoneTilesSet.GetMyTileInfo(MtTileTypes.BuildingMageTower);
        if (mageTowerTileInfo != null)
        {
            nextLevelLearningDurationTime -= (int)Math.Floor((float)nextLevelLearningDurationTime * (MtStatic.GetResearchAttributeSpeedBonus(mageTowerTileInfo.TileLevel) / 100.0f));
        }

        float att_value = AccountInfo.instance.GetNationAttributeValue(MtNationAttributeTypes.Magic);
        if (att_value > 0)
        {
            nextLevelLearningDurationTime -= (int)Math.Floor((float)nextLevelLearningDurationTime * (att_value / 100.0f));
        }

        att_value = AccountInfo.instance.GetNationAttributeValue(MtNationAttributeTypes.Enchanting);
        if (att_value > 0)
        {
            nextLevelLearningDurationTime -= (int)Math.Floor((float)nextLevelLearningDurationTime * (att_value / 100.0f));
        }

        MtWorldAffect aff = AccountInfo.instance.GetAffect(MtWorldAffectTypes.UnitResearchUp);
        if (aff != null)
        {
            nextLevelLearningDurationTime -= (int)Math.Floor((float)nextLevelLearningDurationTime * ((float)aff.AffectValue / 100.0f));
        }

        //소요시간 텍스트
        learnDurationTimeText.text = ConverTimeToString(nextLevelLearningDurationTime);
        costGemText.text = string.Format("{0:n0}" , MtStatic.GetJustNowCompleteGemCost(nextLevelLearningDurationTime));

        goldCost = nextLevelCostGold;
        stoneCost = nextLevelCostStone;
        woodCost = nextLevelCostWood;

        #region 자원조건확인
        //자원충족여부 
        RefreshCost(nextLevelCostGold,nextLevelCostStone,nextLevelCostWood);
        #endregion

        #region 마법사의탑 조건확인
        //마법사의탑 레벨확인
        if (requiredMageTowerLevel > 0)
            requredMageTowerLevel.SetActive(true);
        else
            requredMageTowerLevel.SetActive(false);

        MtBuilding data = MtDataManager.GetBuildingData(MtTileTypes.BuildingMageTower , 1);

        requredMageTowerLevelText.text = string.Format(data.BuildingName + " Lv{0}", requiredMageTowerLevel.ToString());

        if (requiredMageTowerLevel > mageTowerTileInfo.TileLevel)
        {
            requredMageTowerLevelText.color = new Color(230.0f / 255.0f, 69.0f / 255.0f, 69.0f / 255.0f);
            researchAvailableFlag = false;
        }
        else
            requredMageTowerLevelText.color = new Color(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f);
        #endregion

        #region 선행스킬 조건확인

        int[] prevAttributeIDXs = { attributeInfo.PrevAttributeIDX1, attributeInfo.PrevAttributeIDX2, attributeInfo.PrevAttributeIDX3, attributeInfo.PrevAttributeIDX4 };

        foreach (int idx in prevAttributeIDXs)
        {
            if (idx == 0)
                continue;

            MtNationAttribute attribute = MtDataManager.GetAttributeData(idx);

            if (AccountInfo.instance.HasAttribute(attribute.AttributeType))
            {
                if (attribute.CurLevel > AccountInfo.instance.GetMyNationAttributeLevel(attribute.AttributeType))
                    researchAvailableFlag = false;
            }
            else
            {
                researchAvailableFlag = false;
            }
        }

        #endregion

        if (researchInProgressAttribute != null)
        {
            researchAvailableFlag = false;
            if (researchInProgressAttribute.AttributeType == curSelectedAttributeInfo.AttributeType)
            {
                researchButton.GetComponentInChildren<Text>().text = MWText.instance.GetText(MWText.EText.E_948);                
                researchProgressPanel.gameObject.SetActive(true);
                researchCostPanel.gameObject.SetActive(false);
                researchButton.gameObject.SetActive(false);
                justNowButton.gameObject.SetActive(false);
                accelationButton.gameObject.SetActive(true);
            }
            else
            {
                researchProgressPanel.gameObject.SetActive(false);
                researchCostPanel.gameObject.SetActive(true);
                researchButton.gameObject.SetActive(true);
                justNowButton.gameObject.SetActive(true);
                accelationButton.gameObject.SetActive(false);
            }
        }
        else
        {
            researchProgressPanel.gameObject.SetActive(false);
            researchCostPanel.gameObject.SetActive(true);
            researchButton.gameObject.SetActive(true);
            justNowButton.gameObject.SetActive(true);
            accelationButton.gameObject.SetActive(false);
        }

        if (researchAvailableFlag) 
        { 
            researchButton.interactable = true;
            researchButton.GetComponent<Image>().color = new Color(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f);
            learnDurationTimeText.color = new Color(234f / 255.0f, 234f / 255.0f, 234f / 255.0f);

            justNowButton.interactable = true;
            justNowButton.GetComponent<Image>().color = new Color(1f, 1f, 1f);
            costGemText.color = new Color(234f / 255.0f, 234f / 255.0f, 234f / 255.0f);
        }
        else
        {
            researchButton.interactable = false;
            researchButton.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);
            learnDurationTimeText.color = new Color(234f / 255.0f, 234f / 255.0f, 234f / 255.0f, 0.5f);

            justNowButton.interactable = false;
            justNowButton.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);
            costGemText.color = new Color(234f / 255.0f, 234f / 255.0f, 234f / 255.0f, 0.5f);
        }

        attributeInfoWindow.SetActive(true);
        currentResearchPanel.SetActive(false);
        levelSlider.maxValue = attributeInfo.MaxLevel;

        levelSlider.value = AccountInfo.instance.GetMyNationAttributeLevel(attributeInfo.AttributeType);
        levelSliderText.text = string.Format("{0}/{1}", (int)levelSlider.value, attributeInfo.MaxLevel);
        attributeImage.sprite = UIUtility.LoadResearchAttributeImage(attributeInfo.AttributeType);
    }

    public void RefreshCost(int nextLevelCostGold, int nextLevelCostStone , int nextLevelCostWood)
    {
        shortageTypes.Clear();
        shortageValue.Clear();

        #region 자원조건확인
        //자원충족여부 
        if (nextLevelCostGold > 0)
        {
            Gold.gameObject.SetActive(true);
            GoldAmount.text = UIUtility.ToCommaNumber(nextLevelCostGold);

            if (AccountInfo.instance.gold < nextLevelCostGold)
            {
                //researchAvailableFlag = false;
                GoldAmount.color = new Color(230.0f / 255.0f, 69.0f / 255.0f, 69.0f / 255.0f);

                shortageTypes.Add(MtItemTypes.Gold);
                shortageValue.Add(nextLevelCostGold);
            }
            else
            {
                GoldAmount.color = Color.white;
            }
        }
        else
        {
            Gold.gameObject.SetActive(false);
            GoldAmount.color = Color.white;

        }

        if (nextLevelCostStone > 0)
        {
            Stone.gameObject.SetActive(true);
            StoneAmount.text = UIUtility.ToCommaNumber(nextLevelCostStone);

            if (AccountInfo.instance.stone < nextLevelCostStone)
            {
                //researchAvailableFlag = false;
                StoneAmount.color = new Color(230.0f / 255.0f, 69.0f / 255.0f, 69.0f / 255.0f);

                shortageTypes.Add(MtItemTypes.Stone);
                shortageValue.Add(nextLevelCostStone);
            }
            else
            {
                StoneAmount.color = Color.white;
            }
        }
        else
        {
            Stone.gameObject.SetActive(false);
            StoneAmount.color = Color.white;

        }

        if (nextLevelCostWood > 0)
        {
            Wood.gameObject.SetActive(true);
            WoodAmount.text = UIUtility.ToCommaNumber(nextLevelCostWood);

            if (AccountInfo.instance.wood < nextLevelCostWood)
            {
                //researchAvailableFlag = false;
                WoodAmount.color = new Color(230.0f / 255.0f, 69.0f / 255.0f, 69.0f / 255.0f);

                shortageTypes.Add(MtItemTypes.Wood);
                shortageValue.Add(nextLevelCostWood);
            }
            else
            {
                WoodAmount.color = Color.white;
            }
        }
        else
        {
            Wood.gameObject.SetActive(false);
            WoodAmount.color = Color.white;
        }
        #endregion
    }

    private void ClearSelectedAttributeInfo()
    {
        curSelectedAttributeNameText.text = "";
        curSelectedAttributeDescriptionText.text = "";

        maxLevelLabel.gameObject.SetActive(false);
        levelInfoPanel.gameObject.SetActive(false);

        maxValueLabel.gameObject.SetActive(false);
        addValueTextPanel.gameObject.SetActive(false);

        detailViewButton.interactable = false;

        researchProgressPanel.SetActive(false);
        researchCostPanel.SetActive(false);

        ManaStone.SetActive(false);
        Stone.SetActive(false);
        Wood.SetActive(false);
        requredMageTowerLevel.SetActive(false);

        selectSlotMark.gameObject.SetActive(false);
    }

    private string GetSimpleAttributeDescriptionText(MtNationAttributeTypes type)
    {
        //연구속도
        if (type == MtNationAttributeTypes.Architecture || type == MtNationAttributeTypes.Magic|| type == MtNationAttributeTypes.Enchanting || type == MtNationAttributeTypes.StrengtheningResearch)
        {
            return MWText.instance.GetText(MWText.EText.RESEARCH_SPEED);
        }
        //획득량
        else if(type == MtNationAttributeTypes.HeroTraining || type == MtNationAttributeTypes.SuspiciousPower)
        {
            return MWText.instance.GetText(MWText.EText.AMOUNT_ACQUIRED);
        }
        //효과량
        else if (type == MtNationAttributeTypes.PowerTurret)
        {
            return MWText.instance.GetText(MWText.EText.EFFECTIVE_AMOUNT);
        }
        //충전속도
        else if (type == MtNationAttributeTypes.FullMana)
        {
            return MWText.instance.GetText(MWText.EText.CHARGING_SPEED);
        }
        //훈련속도
        else if (type == MtNationAttributeTypes.SoldierControl)
        {
            return MWText.instance.GetText(MWText.EText.TRAINING_SPEED);
        }
        //치료속도
        else if (type == MtNationAttributeTypes.FirstAid)
        {
            return MWText.instance.GetText(MWText.EText.RECOVERY_SPEED);
        }
        //최대전투력
        else if (type == MtNationAttributeTypes.BreakAtLimit)
        {
            return MWText.instance.GetText(MWText.EText.MAX_ATTACK_POWER);
        }
        //보상&획득확률
        else if (type == MtNationAttributeTypes.SkilledCombat)
        {
            return MWText.instance.GetText(MWText.EText.E_1785);
        }
        //버프효과
        else if (type == MtNationAttributeTypes.PowerOfBalance || type == MtNationAttributeTypes.PowerOfConqueror || type == MtNationAttributeTypes.PowerOfStorm || type == MtNationAttributeTypes.UnknownPower)
        {
            return MWText.instance.GetText(MWText.EText.BUFF_EFFECT);
        }
        //감소량
        else if (type == MtNationAttributeTypes.Miracle || type == MtNationAttributeTypes.Diplomacy)
        {
            return MWText.instance.GetText(MWText.EText.REDUCTION_AMOUNT);
        }
        //연맹원 수
        else if (type == MtNationAttributeTypes.AssembleCommand)
        {
            return MWText.instance.GetText(MWText.EText.FEDERATION_MEMBER_AMOUNT);
        }
        //기능추가
        else if (type == MtNationAttributeTypes.BlackHand || type == MtNationAttributeTypes.AssembleAttack)
        {
            return MWText.instance.GetText(MWText.EText.ADD_FUNTION);
        }
        //건설속도
        else if (type == MtNationAttributeTypes.Architecture)
        {
            return MWText.instance.GetText(MWText.EText.BUILD_SPEED);
        }
        //채집량
        else if (type == MtNationAttributeTypes.Axe || type == MtNationAttributeTypes.Pick)
        {
            return MWText.instance.GetText(MWText.EText.COLLECTION_AMOUNT);
        }
        //채집속도
        else if (type == MtNationAttributeTypes.Felling || type == MtNationAttributeTypes.SkilledPick || type == MtNationAttributeTypes.GoldDetector 
            || type == MtNationAttributeTypes.SteelPickaxe|| type == MtNationAttributeTypes.LargeSaw|| type == MtNationAttributeTypes.Mining)
        {
            return MWText.instance.GetText(MWText.EText.COLLECTION_SPEED);
        }
        //적재속도
        else if (type == MtNationAttributeTypes.GoldMine|| type == MtNationAttributeTypes.SteelPickaxe|| type == MtNationAttributeTypes.Sawmil)
        {
            return MWText.instance.GetText(MWText.EText.LOADING_SPEED);
        }
        //생산속도
        else if (type == MtNationAttributeTypes.ManastoneMine || type == MtNationAttributeTypes.Museum || type == MtNationAttributeTypes.Prosperity)
        {
            return MWText.instance.GetText(MWText.EText.PRODUCTION_SPEED);
        }
        //부대적재량
        else if (type == MtNationAttributeTypes.Carriage)
        {
            return MWText.instance.GetText(MWText.EText.ARMY_LOAD_AMOUNT);
        }
        //보관량
        else if (type == MtNationAttributeTypes.WarehouseExpansion || type == MtNationAttributeTypes.Quarry)
        {
            return MWText.instance.GetText(MWText.EText.STORAGE_AMOUNT);
        }
        //채집량&적재속도
        else if (type == MtNationAttributeTypes.BumperYear)
        {
            return MWText.instance.GetText(MWText.EText.E_1797);
        }
        //이동속도
        else if (type == MtNationAttributeTypes.RapidRetreat|| type == MtNationAttributeTypes.ToughMarch)
        {
            return MWText.instance.GetText(MWText.EText.HERO_SPEED_RATIO);
        }
        //공격력
        else if (type == MtNationAttributeTypes.SwordOfLight || type == MtNationAttributeTypes.SwordOfDark|| type == MtNationAttributeTypes.SwordOfFire || type == MtNationAttributeTypes.Wachtower)
        {
            return MWText.instance.GetText(MWText.EText.E_1439);
        }
        //방어력
        else if (type == MtNationAttributeTypes.ArmorOfLight|| type == MtNationAttributeTypes.ArmorOfDark|| type == MtNationAttributeTypes.ArmorOfFire|| type == MtNationAttributeTypes.IronGate)
        {
            return MWText.instance.GetText(MWText.EText.E_1065);
        }
        //능력치
        else if (type == MtNationAttributeTypes.PowerOfCommander|| type == MtNationAttributeTypes.PowerOfHero || type == MtNationAttributeTypes.PowerOfPlunderer
           ||type == MtNationAttributeTypes.StrengthenEquipment || type == MtNationAttributeTypes.AbilityDevelopment|| type == MtNationAttributeTypes.PlacementForward
           || type == MtNationAttributeTypes.PlacementBackward|| type == MtNationAttributeTypes.Leadership|| type == MtNationAttributeTypes.ExcellentAssist
           || type == MtNationAttributeTypes.Authoritative || type == MtNationAttributeTypes.Overpower || type == MtNationAttributeTypes.Machlessness
           ||type == MtNationAttributeTypes.GlasslandConqueror)
        {
            return MWText.instance.GetText(MWText.EText.E_1064);
        }
        //지속시간
        else if (type == MtNationAttributeTypes.ExtensionOfBalance|| type == MtNationAttributeTypes.EndlessConquer|| type == MtNationAttributeTypes.EndlessMarch
            || type == MtNationAttributeTypes.RemnantOfPower)
        {
            return MWText.instance.GetText(MWText.EText.DURARION_TIME);
        }
        //생명력
        else if (type == MtNationAttributeTypes.HelmetOfLight|| type == MtNationAttributeTypes.HelmetOfDark|| type == MtNationAttributeTypes.HelmetOfFire)
        {
            return MWText.instance.GetText(MWText.EText.ETC_LIFE);
        }
        //획득확률
        else if (type == MtNationAttributeTypes.SharpEyes)
        {
            return MWText.instance.GetText(MWText.EText.ACHEIVE_PERCENT);            
        }
        else
        {
            Debug.Log(type.ToString() + "    don't have enumText");
            return "";
        }
    }

    public string ConverTimeToString(int seconds)
    {
        if (seconds == 0)
            return "00 : 00 : 00";

        string[] result = { "", "", "", "" };
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
        TimeSpan ts = new TimeSpan(0, 0, 0, seconds);
        if(ts.Days > 0)
            result[0] += ts.Days.ToString() + "D";

        if (ts.Hours > 0)
            result[1] += ts.Hours.ToString();
        else if (ts.TotalHours > 0)
            result[1] += "00";

        if (ts.Minutes > 0)
            result[2] += ts.Minutes.ToString();
        else if (ts.TotalMinutes > 0)
            result[2] += "00";

        if (ts.Seconds > 0)
            result[3] += ts.Seconds.ToString();
        else if (ts.TotalSeconds > 0)
            result[3] += "00";

        for (int i = 1; i < result.Length; i++)
        {
            if (result[i].Length <= 1)
            {
                result[i] = "0" + result[i];
            }
        }

        return result[0] + " " + result[1] + " : " + result[2] + " : " + result[3];
    }

    public void OnClickGold()
    {
        UIHowtoRetainBaloon.Create(MtItemTypes.Gold);
    }

    public void OnClickManastone()
    {
        UIHowtoRetainBaloon.Create(MtItemTypes.Manastone);
    }

    public void OnClickStone()
    {
        UIHowtoRetainBaloon.Create(MtItemTypes.Stone);
    }

    public void OnClickWood()
    {
        UIHowtoRetainBaloon.Create(MtItemTypes.Wood);
    }

    public void OnClickUseItem()
    {
        UIUseItem.Create_Research(UIUseItem.SpeedUpTypes.Research, curSelectedAttributeInfo.AttributeType);
    }
    private void SetButtonLockRecursively(GameObject obj)
    {
        float constColor = 255.0f;

        Button btn = obj.GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;

        Image img = obj.GetComponent<Image>();
        if (img != null)
            img.color = new Color(constColor / 255f, constColor / 255f, constColor / 255f);

        Text text = obj.GetComponent<Text>();
        if (text != null)
            text.color = new Color(255f / 255f, 246f / 255f, 200f / 255f);

        var outlines = obj.GetComponentsInChildren<Outline>();

        if (outlines != null)
        {
            foreach (var outline in outlines)
            {
                outline.effectColor = new Color(85f / 255f, 52f / 255f, 18f / 255f, 128f / 255f);
            }
        }


        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetButtonLockRecursively(obj.transform.GetChild(i).gameObject);
        }
    }
    private void SetReleaseLockButtonRecursively(GameObject obj)
    {
        float constColor = 140.0f;

        Button btn = obj.GetComponent<Button>();
        if (btn != null)
            btn.interactable = true;

        Image img = obj.GetComponent<Image>();
        if (img != null)
            img.color = new Color(constColor / 255f, constColor / 255f, constColor / 255f);

        Text text = obj.GetComponent<Text>();
        if (text != null)
            text.color = new Color(132f / 255f, 126f / 255f, 118f / 255f, 128f / 255f);

        var outlines = obj.GetComponentsInChildren<Outline>();

        if (outlines != null)
        {
            foreach (var outline in outlines)
            {
                outline.effectColor = new Color(0f, 0f, 0f, 128f / 255f);
            }
        }

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetReleaseLockButtonRecursively(obj.transform.GetChild(i).gameObject);
        }
    }

    private string GetSignFromAttributeType(MtNationAttributeTypes type)
    {
        if (type == MtNationAttributeTypes.EndlessConquer || type == MtNationAttributeTypes.EndlessMarch || type == MtNationAttributeTypes.RemnantOfPower)
        {
            //초
            return MWText.instance.GetText(MWText.EText.E_1800);
        }
        else if (type == MtNationAttributeTypes.AssembleCommand)
        {
            //명
            return MWText.instance.GetText(MWText.EText.E_1799);
        }
        else if (type == MtNationAttributeTypes.BreakAtLimit || type == MtNationAttributeTypes.BlackHand || type == MtNationAttributeTypes.AssembleAttack)
        {
            return "";
        }
        else
        {
            //%
            return "%";
        }
    }

    public void OnClickCloseAttributeInfo()
    {
        currentResearchPanel.SetActive(researchInProgressAttribute != null);
        attributeInfoWindow.SetActive(false);
    }

    public void OnClickSpeedUp()
    {
        if (researchInProgressAttribute != null)
        {
            UIUseItem.Create_Research(UIUseItem.SpeedUpTypes.Research, researchInProgressAttribute.AttributeType, false);
            //Close();
        }
    }

    public override void Close()
    {
        base.Close();

        if (!bAdvisorDoIt)
        {
            ZoneTerrain.Get().uiHome.Show(false);
        }
    }

    private void OnDestroy()
    {
        singleton = null;
    }
}
