//#define REAL_PAYMENT

using MysticManaCommon;
using MysticManaCommon.ContentBase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIFederation : UIWindow
{
    private const string PrefabPath = "Prefab/UI/Federation/UIFederation";
    public static UIFederation Create()
    {
        GameObject go = GameObject.Instantiate(MtAssetBundles.Load(PrefabPath)) as GameObject;
        UIFederation window = UIWindow.Create<UIFederation>(go, GetCanvasCenter(), true);

        return window;
    }

    private MtFederation fedInfo = null;

    [SerializeField] private Text windowTitleText = null;

    [SerializeField] private Transform buttonGrid = null;
    [SerializeField] private GameObject backButtonObj = null;
    [SerializeField] private GameObject masterUserPanel = null;
    [SerializeField] private GameObject mainPanel = null;
    [SerializeField] private GameObject areaPanel = null;
    [SerializeField] private GameObject federationTroopsPanel = null;
    [SerializeField] private GameObject federationStorePanel = null;
    [SerializeField] private GameObject federationStatusPanel = null;
    [SerializeField] private Sprite[] flagSprites = null;
    [SerializeField] private Sprite[] shapeSprites = null;

    [SerializeField] private Image flagImage = null;
    [SerializeField] private Image shapeImage = null;
    [SerializeField] private Text nameText = null;
    [SerializeField] private Text areaText = null;
    [SerializeField] private Text levelText = null;
    [SerializeField] private Text battlePowerText = null;
    [SerializeField] private Text memberCountText = null;
    [SerializeField] private Text masterNameText = null;
    [SerializeField] private Text introductionText = null;
    [SerializeField] private Text expText = null;
    [SerializeField] private Slider expSlider = null;
    [SerializeField] private GameObject closeNumButtons = null;

    [SerializeField] private Transform memberGrid = null;
    [SerializeField] private Transform r2ListTransform = null;
    [SerializeField] private Transform r3ListTransform = null;
    [SerializeField] private Transform r4ListTransform = null;
    [SerializeField] private RectTransform r2Arrow = null;
    [SerializeField] private RectTransform r3Arrow = null;
    [SerializeField] private RectTransform r4Arrow = null;
    [SerializeField] private Image masterAvatarImage = null;
    [SerializeField] private Text masterNameTopText = null;
    [SerializeField] private Text masterCombatPowerPointText = null;
    [SerializeField] private Text masterCulturePointText = null;
    [SerializeField] private Text masterCollectionPointText = null;
    [SerializeField] private Text r2MemberCountText = null;
    [SerializeField] private Text r3MemberCountText = null;
    [SerializeField] private Text r4MemberCountText = null;
    [SerializeField] private VerticalLayoutGroup layoutGroup = null;

    [SerializeField] private GameObject troopBadgePanel = null;
    [SerializeField] private Text troopBadgeText = null;

    [SerializeField] private GameObject giftPanel = null;
    [SerializeField] private Transform giftGrid = null;
    [SerializeField] private Text giftPanelSliverCoinText = null;

    [SerializeField] private GameObject warningNoTroops = null;
    [SerializeField] private GameObject warningNoGift = null;

    [SerializeField] private GameObject federationQuestPanel = null;
    [SerializeField] private Transform questElementGridTrs = null;
    [SerializeField] private GameObject emptyQuestText = null;
    [SerializeField] private Transform noticeButton = null;

    [SerializeField] private GameObject settingBadge = null;
    [SerializeField] private Text settingBadgeText = null;

    [SerializeField] private GameObject questBadge = null;
    [SerializeField] private Text questBadgeText = null;

    [SerializeField] private Button[] categoryButtons = null;

    public bool isProcessingFederationGift = false;
    public override void Initialize()
    {
        base.Initialize();

        RefreshQuestBadge();
        OnClickBackButton();
    }

    private void CloseAllPanel()
    {
        windowTitleText.text = MWText.instance.GetText(MWText.EText.FEDERATION);

        warehousePanel.SetActive(false);
        configPanel.SetActive(false);
        mainPanel.SetActive(false);
        areaPanel.SetActive(false);
        federationStorePanel.SetActive(false);
        supportPanel.SetActive(false);
        federationTroopsPanel.SetActive(false);
        giftPanel.SetActive(false);
        storeDetailPanel.SetActive(false);
        federationQuestPanel.SetActive(false);
        federationStatusPanel.SetActive(true);

        foreach (Button btn in categoryButtons)
        {
            btn.interactable = true;
        } 
    }

    public void RefreshFederationEXP()
    {
        long max_exp = MtStatic.GetFederationNextEXP(fedInfo.EXP, out fedInfo.Level);
        expText.text = string.Format("{0}/{1}", fedInfo.EXP, max_exp);
        expSlider.maxValue = max_exp;
        expSlider.value = fedInfo.EXP;

        levelText.text = UIUtility.GetLevelString(fedInfo.Level);
        giftPanelFedrationLevelText.text = levelText.text;

        giftPanelFedrationEXPText.text = expText.text;
        giftPanelFedrationGiftEXPText.text = string.Format("{0}/{1}", fedInfo.GiftEXP, MtStatic.FederationMaxGiftEXP);;
        giftPanelFedrationEXPSlider.maxValue = expSlider.maxValue;
        giftPanelFedrationEXPSlider.value = expSlider.value;
        giftPanelGiftEXPSlider.maxValue = MtStatic.FederationMaxGiftEXP;
        giftPanelGiftEXPSlider.value = fedInfo.GiftEXP;
    }

    private void OnResponseUserFederationInfo(MtPacket_UserFederationInfo_Response typed_pk)
    {
        CloseAllPanel();
        categoryButtons[0].interactable = false;

        mainPanel.SetActive(true);
        RefreshPanelIcons();

        fedInfo = typed_pk.FederationInfo;

        nameText.text = "[" + fedInfo.Initial + "] " + fedInfo.Name;
        masterNameText.text = fedInfo.MasterName;
        battlePowerText.text = UIUtility.ToCommaNumber(fedInfo.TotalBattlePower);
        memberCountText.text = fedInfo.MemberCount + "/" + fedInfo.MaxMemberCount;
        introductionText.text = fedInfo.IntroductionText;

        giftPanelFedrationNameText.text = nameText.text;

        // 연맹 경험치
        RefreshFederationEXP();

        if (fedInfo.PositionX == 0 && fedInfo.PositionY == 0)
        {
            areaText.text = MWText.instance.GetText(MWText.EText.NO_AREA);
        }
        else
        {
            areaText.text = "X" + fedInfo.PositionX + ", Y" + fedInfo.PositionY;
        }

        RefreshFlagImage();
        RefreshCityInformation();
        this.members = typed_pk.Members;

        RefreshFederationMembers(typed_pk.Members);

        settingBadge.SetActive(typed_pk.ApplicantCount > 0);
        settingBadgeText.text = typed_pk.ApplicantCount.ToString();
    }

    private void RefreshFlagImage()
    {
        fedInfo.FlagType = fedInfo.FlagType == MtFederationFlagTypes.None ? MtFederationFlagTypes.One : fedInfo.FlagType;
        fedInfo.ShapeType = fedInfo.ShapeType == MtFederationShapeTypes.None ? MtFederationShapeTypes.One : fedInfo.ShapeType;

        flagImage.sprite = flagSprites[(int)fedInfo.FlagType - 1];
        shapeImage.sprite = shapeSprites[(int)fedInfo.ShapeType - 1];
    }

    public void ReciveFederationGift(long giftItemIDX, Action<MtPacket_RecieveFederationGift_Response> responseAction)
    {
        if (!isProcessingFederationGift)
        {
            Debug.Log("RequestFedartionGift");
            isProcessingFederationGift = true;
            LobbyScene.Get().RequestRecieveFederationGift(giftItemIDX, responseAction);
        }
    }

    public void OnClickBackButton()
    {
        backButtonObj.SetActive(false);
        CloseAllPanel();
        categoryButtons[0].interactable = false;
        mainPanel.SetActive(true);
        federationTroopsPanel.SetActive(false);
        federationStorePanel.SetActive(false);

        RefreshPanelIcons();

        int federationTroopCount = AccountInfo.instance.federationTroopCount;
        troopBadgePanel.SetActive(federationTroopCount > 0);
        troopBadgeText.text = federationTroopCount.ToString();

        LobbyScene.Get().RequestFederationInfo(OnResponseUserFederationInfo);
    }

    public MtFederation GetFederationInfo()
    {
        return fedInfo;
    }

    #region 집결 패널
    [Header("집결")]
    [SerializeField] private Transform troopsParent = null;

    public void OnClickFederationTroops()
    {
        DestroyChildren(troopsParent);

        CloseAllPanel();
        categoryButtons[1].interactable = false;
        backButtonObj.SetActive(true);
        masterUserPanel.SetActive(false);
        federationTroopsPanel.SetActive(true);
        RefreshPanelIcons();

        LobbyScene.Get().RequestListOfFederationTroops(OnResponseFederationTroops, MtTroopOpenTypes.OpenForFederation);
    }

    private void OnResponseFederationTroops(MtPacket_ListOfFederationTroops_Response typed_pk)
    {
        DestroyChildren(troopsParent);

        foreach (MtFederationTroop troop in typed_pk.FederationTroops)
        {
            FederationTroopElement.Create(this, troopsParent, troop);
        }

        warningNoTroops.SetActive(typed_pk.FederationTroops.Count == 0);
    }
    #endregion

    #region 영토 패널
    [Header("영토")]
    [SerializeField] private GameObject buildButton = null;
    [SerializeField] private GameObject moveKingdomButton = null;
    [SerializeField] private Button moveToCenter1Button = null;
    [SerializeField] private Slider sherinStandingSlider = null;
    [SerializeField] private Text sherinStandingText = null;
    [SerializeField] private Button sherinButton = null;

    [SerializeField] private Slider oldCastleStandingSlider = null;
    [SerializeField] private Text oldCastleStandingText = null;
    [SerializeField] private Button oldCastleButton = null;

    [SerializeField] private Slider marenStandingSlider = null;
    [SerializeField] private Text marenStandingText = null;
    [SerializeField] private Button marenButton = null;

    [SerializeField] private Slider lahindelStandingSlider = null;
    [SerializeField] private Text lahindelStandingText = null;
    [SerializeField] private Button lahindelButton = null;

    [SerializeField] private Slider arontraStandingSlider = null;
    [SerializeField] private Text arontraStandingText = null;
    [SerializeField] private Button arontraButton = null;

    [SerializeField] private Slider wooboldStandingSlider = null;
    [SerializeField] private Text wooboldStandingText = null;
    [SerializeField] private Button wooboldButton = null;

    [SerializeField] private Slider titanStandingSlider = null;
    [SerializeField] private Text titanStandingText = null;
    [SerializeField] private Button titanButton = null;

    [SerializeField] private Slider lastStageStandingSlider = null;
    [SerializeField] private Text lastStageStandingText = null;
    [SerializeField] private Button lastStageButton = null;

    [SerializeField] private Text[] solidWoodTexts = null;
    [SerializeField] private Text[] burntWoodTexts = null;

    [SerializeField] private Text sherinBuffText = null;
    [SerializeField] private Text oldCastleBuffText = null;
    [SerializeField] private Text marenBuffText = null;
    [SerializeField] private Text lahindelBuffText = null;
    [SerializeField] private Text arontraBuffText = null;
    [SerializeField] private Text wooboldBuffText = null;
    [SerializeField] private Text titanBuffText = null;
    [SerializeField] private Text lastStageBuffText = null;

    [SerializeField] private Transform sherinBuildingTr = null;
    [SerializeField] private Transform oldCastleBuildingTr = null;
    [SerializeField] private Transform marenBuildingTr = null;
    [SerializeField] private Transform lahindelBuildingTr = null;
    [SerializeField] private Transform arontraBuildingTr = null;
    [SerializeField] private Transform wooboldBuildingTr = null;
    [SerializeField] private Transform titanBuildingTr = null;
    [SerializeField] private Transform lastStageBuildingTr = null;

    [SerializeField] private GameObject requiement3Lv = null;
    [SerializeField] private GameObject requiement5Lv = null;
    [SerializeField] private GameObject requiement7Lv = null;
    [SerializeField] private GameObject requiement9Lv = null;
    [SerializeField] private GameObject requiement12Lv = null;
    [SerializeField] private GameObject requiement15Lv = null;
    [SerializeField] private GameObject requiement17Lv = null;

    private void RefreshCityInformation()
    {
        int level;
        long minStanding;
        sherinStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.SherinQuestStation], out level, out minStanding) - minStanding;
        sherinBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.SHERIN_BUFF), level);
        sherinStandingSlider.value = fedInfo.StandingList[MtTileTypes.SherinQuestStation] - minStanding;
        sherinStandingText.text = UIUtility.GetLevelString(level);

        oldCastleStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.OldCastleQuestStation], out level, out minStanding) - minStanding;
        oldCastleBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.OLDCASTLE_BUFF), level);
        oldCastleStandingSlider.value = fedInfo.StandingList[MtTileTypes.OldCastleQuestStation] - minStanding;
        oldCastleStandingText.text = UIUtility.GetLevelString(level);

        marenStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.MarenQuestStation], out level, out minStanding) - minStanding;
        marenBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.MAREN_BUFF), level);
        marenStandingSlider.value = fedInfo.StandingList[MtTileTypes.MarenQuestStation] - minStanding;
        marenStandingText.text = UIUtility.GetLevelString(level);

        lahindelStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.LahindelQuestStation], out level, out minStanding) - minStanding;
        lahindelBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.LAHINDEL_BUFF), level);
        lahindelStandingSlider.value = fedInfo.StandingList[MtTileTypes.LahindelQuestStation] - minStanding;
        lahindelStandingText.text = UIUtility.GetLevelString(level);

        arontraStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.CastleOfDotakQuestStation], out level, out minStanding) - minStanding;
        arontraBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.ARONTRA_BUFF), level);
        arontraStandingSlider.value = fedInfo.StandingList[MtTileTypes.CastleOfDotakQuestStation] - minStanding;
        arontraStandingText.text = UIUtility.GetLevelString(level);

        wooboldStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.WooboldVillageQuestStation], out level, out minStanding) - minStanding;
        wooboldBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.WOOBOLD_BUFF), level);
        wooboldStandingSlider.value = fedInfo.StandingList[MtTileTypes.WooboldVillageQuestStation] - minStanding;
        wooboldStandingText.text = UIUtility.GetLevelString(level);

        titanStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.TitansGardenQuestStation], out level, out minStanding) - minStanding;
        titanBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.TITAN_BUFF), level);
        titanStandingSlider.value = fedInfo.StandingList[MtTileTypes.TitansGardenQuestStation] - minStanding;
        titanStandingText.text = UIUtility.GetLevelString(level);

        lastStageStandingSlider.maxValue = MtStatic.GetMaxStanding(fedInfo.StandingList[MtTileTypes.LastStageQuestStation], out level, out minStanding) - minStanding;
        lastStageBuffText.text = string.Format(MWText.instance.GetText(MWText.EText.LASTSTAGE_BUFF), level);
        lastStageStandingSlider.value = fedInfo.StandingList[MtTileTypes.LastStageQuestStation] - minStanding;
        lastStageStandingText.text = UIUtility.GetLevelString(level);

        foreach (Text txt in solidWoodTexts)
        {
            txt.text = AccountInfo.instance.solidWood.ToString();
        }

        foreach (Text txt in burntWoodTexts)
        {
            txt.text = AccountInfo.instance.burntWood.ToString();
        }

        sherinButton.interactable = AccountInfo.instance.solidWood > 0;
        oldCastleButton.interactable = AccountInfo.instance.solidWood > 0;
        marenButton.interactable = AccountInfo.instance.solidWood > 0;
        lahindelButton.interactable = AccountInfo.instance.solidWood > 0;

        arontraButton.interactable = AccountInfo.instance.burntWood > 0;
        wooboldButton.interactable = AccountInfo.instance.burntWood > 0;
        titanButton.interactable = AccountInfo.instance.burntWood > 0;
        lastStageButton.interactable = AccountInfo.instance.burntWood > 0;

        requiement3Lv.SetActive(fedInfo.Level < 3);
        requiement5Lv.SetActive(fedInfo.Level < 5);
        requiement7Lv.SetActive(fedInfo.Level < 7);
        requiement9Lv.SetActive(fedInfo.Level < 9);
        requiement12Lv.SetActive(fedInfo.Level < 12);
        requiement15Lv.SetActive(fedInfo.Level < 15);
        requiement17Lv.SetActive(fedInfo.Level < 17);
    }

    public void OnClickFederationArea()
    {
        backButtonObj.SetActive(true);
        masterUserPanel.SetActive(false);
        CloseAllPanel();
        categoryButtons[2].interactable = false;
        areaPanel.SetActive(true);
        RefreshPanelIcons();

        if (fedInfo.PositionX != 0 || fedInfo.PositionY != 0)
        {
            buildButton.SetActive(false);
            moveKingdomButton.SetActive(true);
            moveToCenter1Button.enabled = true;
        }
        else
        {
            buildButton.SetActive(true);
            moveKingdomButton.SetActive(false);
            moveToCenter1Button.enabled = false;
        }
    }

    public void OnClickBuildFederationCenter()
    {
        UIFederationCenterFinder.Create();
        Close();
    }

    public void OnClickMoveToFederationCenter()
    {
        ZoneTerrain.Get().GotoPosition(fedInfo.PositionX, fedInfo.PositionY);
        Close();
    }

    public void OnClickMoveKingdom()
    {
        //if (!MtStatic.IsQuestStepConditionOk(AccountInfo.instance.questStep, MtStatic.FirstQuestIDX_MoveKingdom))
        //{
        //    string questName = MtGameDB.GetLocalizedString(MtLocalizationTags.Mt_QuestNames, MtStatic.FirstQuestIDX_MoveKingdom);
        //    UIToastMessage.Create(string.Format(MWText.instance.GetText(MWText.EText.ALERT_CANNOT_DO_NOW), questName));

        //    return;
        //}

        foreach (MtTileInfo ti in ZoneTilesSet.StaticMyTileInfos.Values)
        {
            if (ti.RemainBuildSeconds > 0)
            {
                UIToastMessage.Create(MWText.EText.WANING_CANT_MOVE_INBUILD);
                return;
            }
        }

        foreach (MovingHeroElement mhe in ZoneTerrain.Get().uiHome.GetMovingHeroList())
        {
            if (mhe.GetAvatarState() != MtAvatarStates.WaitInCastle)
            {
                UIToastMessage.Create(MWText.EText.WANING_CANT_MOVE_INMOVING);
                return;
            }
        }

        MtTileInfo castle_ti = ZoneTilesSet.GetMyTileInfo(MtTileTypes.PlayerCastleTile);
        if (castle_ti != null && castle_ti.EnvironmentType == MtEnvironmentTypes.GrassField)
        {
            UICommonConfirm.Create(MWText.instance.GetText(MWText.EText.ALERT), MWText.instance.GetText(MWText.EText.E_1859), OnConfirmMoveKingdom);
        }
        else
        {
            UICommonConfirm.Create(MWText.instance.GetText(MWText.EText.ALERT), MWText.instance.GetText(MWText.EText.E_2035), OnConfirmMoveKingdom);
        }
    }

    private void OnConfirmMoveKingdom(string title, string[] parameters)
    {
        foreach (MtTileInfo ti in ZoneTilesSet.StaticMyTileInfos.Values)
        {
            if (ti.RemainBuildSeconds > 0)
            {
                UIToastMessage.Create(MWText.EText.WANING_CANT_MOVE_INBUILD);
                return;
            }
        }

        foreach (MovingHeroElement mhe in ZoneTerrain.Get().uiHome.GetMovingHeroList())
        {
            if (mhe.GetAvatarState() != MtAvatarStates.WaitInCastle)
            {
                UIToastMessage.Create(MWText.EText.WANING_CANT_MOVE_INMOVING);
                return;
            }
        }

        MtTileInfo castle_ti = ZoneTilesSet.GetMyTileInfo(MtTileTypes.PlayerCastleTile);
        LobbyScene.Get().RequestMoveKingdomToFederation(castle_ti != null && castle_ti.EnvironmentType == MtEnvironmentTypes.GrassField, OnResponseMoveKingdomToFederation);
    }

    private void OnResponseMoveKingdomToFederation(MtPacket_MoveKingdomToFederation_Response typed_pk)
    {
        AccountInfo.instance.castlePositionX = typed_pk.NewCastlePositionX;
        AccountInfo.instance.castlePositionY = typed_pk.NewCastlePositionY;
        ZoneTerrain.Get().zoneTiles.ClearTileInfos();

        ZoneTerrain.Get().BackToMyCastle();

        Close();
    }

    private int currentAmount = 1;
    public void OnClickSherinAmount()
    {
        OnClickSherin(currentAmount);
    }

    public void OnClickOldCastleAmount()
    {
        OnClickOldCastle(currentAmount);
    }

    public void OnClickMarenAmount()
    {
        OnClickMaren(currentAmount);
    }

    public void OnClickLahindelAmount()
    {
        OnClickLahindel(currentAmount);
    }

    public void OnClickArontraAmount()
    {
        OnClickArontra(currentAmount);
    }

    public void OnClickWooboldAmount()
    {
        OnClickWoobold(currentAmount);
    }

    public void OnClickTitanAmount()
    {
        OnClickTitan(currentAmount);
    }

    public void OnClickLastStageAmount()
    {
        OnClickLastStage(currentAmount);
    }

    private void CheckSolidWood(Button stationButton)
    {
        bool flag = false;
        if (AccountInfo.instance.solidWood - 1 > 98)
        {
            currentAmount = 99;
            flag = true;
        }
        else if (AccountInfo.instance.solidWood - 1 > 9)
        {
            currentAmount = 10;
            flag = true;
        }
        else
        {
            currentAmount = 1;
        }

        Transform tr = stationButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(flag);
            tr.GetComponentInChildren<Text>().text = "×" + currentAmount.ToString();

            closeNumButtons.SetActive(flag);
        }
    }

    private void CheckBurntWood(Button stationButton)
    {
        bool flag = false;
        if (AccountInfo.instance.burntWood - 1 > 98)
        {
            currentAmount = 99;
            flag = true;
        }
        else if (AccountInfo.instance.burntWood - 1 > 9)
        {
            currentAmount = 10;
            flag = true;
        }
        else
        {
            currentAmount = 1;
        }

        Transform tr = stationButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(flag);
            tr.GetComponentInChildren<Text>().text = "×" + currentAmount.ToString();

            closeNumButtons.SetActive(flag);
        }
    }

    public void OnClickCloseNumButtons()
    {
        closeNumButtons.SetActive(false);

        Transform tr = sherinButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }

        tr = oldCastleButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }

        tr = marenButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }

        tr = lahindelButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }

        tr = arontraButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }

        tr = wooboldButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }

        tr = titanButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }

        tr = lastStageButton.transform.Find("NumButton");
        if (tr != null)
        {
            tr.gameObject.SetActive(false);
        }
    }

    public void OnClickSherin(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.SherinQuestStation, amount, OnResponseGiveWoodToCity);
        CheckSolidWood(sherinButton);
    }

    public void OnClickOldCastle(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.OldCastleQuestStation, amount, OnResponseGiveWoodToCity);
        CheckSolidWood(oldCastleButton);
    }

    public void OnClickMaren(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.MarenQuestStation, amount, OnResponseGiveWoodToCity);
        CheckSolidWood(marenButton);
    }

    public void OnClickLahindel(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.LahindelQuestStation, amount, OnResponseGiveWoodToCity);
        CheckSolidWood(lahindelButton);
    }

    public void OnClickArontra(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.CastleOfDotakQuestStation, amount, OnResponseGiveWoodToCity);
        CheckBurntWood(arontraButton);
    }

    public void OnClickWoobold(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.WooboldVillageQuestStation, amount, OnResponseGiveWoodToCity);
        CheckBurntWood(wooboldButton);
    }

    public void OnClickTitan(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.TitansGardenQuestStation, amount, OnResponseGiveWoodToCity);
        CheckBurntWood(titanButton);
    }

    public void OnClickLastStage(int amount)
    {
        OnClickCloseNumButtons();

        LobbyScene.Get().RequestGiveWoodToCity(MtTileTypes.LastStageQuestStation, amount, OnResponseGiveWoodToCity);
        CheckBurntWood(lastStageButton);
    }

    private const string WoodEffectPath = "Prefab/UIEffect/FX_Get_Wood_1";
    private const string ExpEffectPath = "Prefab/UIEffect/FX_Get_Wood_1_bar";

    private void OnResponseGiveWoodToCity(MtPacket_GiveWoodToCity_Response typed_pk)
    {
        if (fedInfo.StandingList.ContainsKey(typed_pk.QuestStationType))
        {
            if (MtTileInfo.IsGreenQuestStation(typed_pk.QuestStationType))
            {
                AccountInfo.instance.solidWood += typed_pk.woodBalance;
            }
            else
            {
                AccountInfo.instance.burntWood += typed_pk.woodBalance;
            }

            fedInfo.StandingList[typed_pk.QuestStationType] = typed_pk.NewStadingPoint;

            UnityEngine.Object woodEffectObj = MtAssetBundles.Load(WoodEffectPath);
            GameObject woodEffect = Instantiate(woodEffectObj as GameObject);

            UnityEngine.Object expEffectObj = MtAssetBundles.Load(ExpEffectPath);
            GameObject expEffect = Instantiate(expEffectObj as GameObject);

            if (typed_pk.QuestStationType == MtTileTypes.SherinQuestStation)
            {
                woodEffect.transform.SetParent(sherinBuildingTr);
                expEffect.transform.SetParent(sherinStandingSlider.transform);
            }
            else if (typed_pk.QuestStationType == MtTileTypes.OldCastleQuestStation)
            {
                woodEffect.transform.SetParent(oldCastleBuildingTr);
                expEffect.transform.SetParent(oldCastleStandingSlider.transform);
            }
            else if (typed_pk.QuestStationType == MtTileTypes.MarenQuestStation)
            {
                woodEffect.transform.SetParent(marenBuildingTr);
                expEffect.transform.SetParent(marenStandingSlider.transform);
            }
            else if (typed_pk.QuestStationType == MtTileTypes.LahindelQuestStation)
            {
                woodEffect.transform.SetParent(lahindelBuildingTr);
                expEffect.transform.SetParent(lahindelStandingSlider.transform);
            }
            else if (typed_pk.QuestStationType == MtTileTypes.CastleOfDotakQuestStation)
            {
                woodEffect.transform.SetParent(arontraBuildingTr);
                expEffect.transform.SetParent(arontraStandingSlider.transform);
            }
            else if (typed_pk.QuestStationType == MtTileTypes.WooboldVillageQuestStation)
            {
                woodEffect.transform.SetParent(wooboldBuildingTr);
                expEffect.transform.SetParent(wooboldStandingSlider.transform);
            }
            else if (typed_pk.QuestStationType == MtTileTypes.TitansGardenQuestStation)
            {
                woodEffect.transform.SetParent(titanBuildingTr);
                expEffect.transform.SetParent(titanStandingSlider.transform);
            }
            else if (typed_pk.QuestStationType == MtTileTypes.LastStageQuestStation)
            {
                woodEffect.transform.SetParent(lastStageBuildingTr);
                expEffect.transform.SetParent(lastStageStandingSlider.transform);
            }

            expEffect.transform.localPosition = Vector3.zero;
            Destroy(expEffect, 5);
            woodEffect.transform.localPosition = Vector3.zero;
            Destroy(woodEffect, 5);

            RefreshCityInformation();
        }
    }

    private void OnYesGiveWoodToCity(string title, string[] parameters)
    {
        MtTileTypes stationType = (MtTileTypes)int.Parse(parameters[0]);

        LobbyScene.Get().RequestGiveWoodToCity(stationType, 1, OnResponseGiveWoodToCity);
    }
    #endregion

    #region 연맹원 패널
    private List<MtFederationMember> members = null;
    [SerializeField] private Text combatPointHeaderText = null;
    [SerializeField] private Text culturePointHeaderText = null;

    class MemberCPComparer : IComparer, IComparer<MtFederationMember>
    {
        public int Compare(MtFederationMember x, MtFederationMember y)
        {
            return y.CombatPowerPoint.CompareTo(x.CombatPowerPoint);
        }

        public int Compare(object x, object y)
        {
            return Compare((MtFederationMember)x, (MtFederationMember)y);
        }
    }

    class MemberCultureComparer : IComparer, IComparer<MtFederationMember>
    {
        public int Compare(MtFederationMember x, MtFederationMember y)
        {
            return y.CulturePoint.CompareTo(x.CulturePoint);
        }

        public int Compare(object x, object y)
        {
            return Compare((MtFederationMember)x, (MtFederationMember)y);
        }
    }

    private void RefreshFederationMembers(List<MtFederationMember> members)
    {
        DestroyChildren(memberGrid);

        #region 수직 스크롤형
        foreach (MtFederationMember member in members)
        {
            FederationMemberElement.Create(memberGrid, member);
        } 
        #endregion

    }

    private bool combatPointSorted = false;
    public void OnClickCombatPointSort()
    {
        if (!combatPointSorted && !culturePointSorted)
        {
            List<MtFederationMember> sortedMembers = new List<MtFederationMember>(members);
            sortedMembers.Sort(new MemberCPComparer());
            combatPointSorted = true;
            culturePointSorted = false;

            combatPointHeaderText.text = MWText.instance.GetText(MWText.EText.COMBAT_POWER_TEXT) + " <color=#fede87>↓</color>";
            culturePointHeaderText.text = MWText.instance.GetText(MWText.EText.CULTUREPOINT);

            RefreshFederationMembers(sortedMembers);
        }
        else
        {
            combatPointHeaderText.text = MWText.instance.GetText(MWText.EText.COMBAT_POWER_TEXT);
            culturePointHeaderText.text = MWText.instance.GetText(MWText.EText.CULTUREPOINT);
            combatPointSorted = false;
            culturePointSorted = false;

            RefreshFederationMembers(members);
        }
    }

    private bool culturePointSorted = false;
    public void OnClickCulturePointSort()
    {
        if (!culturePointSorted && !combatPointSorted)
        {
            List<MtFederationMember> sortedMembers = new List<MtFederationMember>(members);
            sortedMembers.Sort(new MemberCultureComparer());
            culturePointSorted = true;
            combatPointSorted = false;

            combatPointHeaderText.text = MWText.instance.GetText(MWText.EText.COMBAT_POWER_TEXT);
            culturePointHeaderText.text = MWText.instance.GetText(MWText.EText.CULTUREPOINT) + " <color=#fede87>↓</color>";

            RefreshFederationMembers(sortedMembers);
        }
        else
        {
            combatPointHeaderText.text = MWText.instance.GetText(MWText.EText.COMBAT_POWER_TEXT);
            culturePointHeaderText.text = MWText.instance.GetText(MWText.EText.CULTUREPOINT);
            combatPointSorted = false;
            culturePointSorted = false;

            RefreshFederationMembers(members);
        }
    }

    //private bool r2Expanded = true;
    //public void OnClickR2Bar()
    //{
    //    r2Expanded = !r2Expanded;
    //    r2Arrow.localRotation = Quaternion.Euler(0, 0, r2Expanded ? 0 : 180);

    //    SetChildrenActive(r2ListTransform, r2Expanded);
    //    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);
    //}

    //private bool r3Expanded = true;
    //public void OnClickR3Bar()
    //{
    //    r3Expanded = !r3Expanded;
    //    r3Arrow.localRotation = Quaternion.Euler(0, 0, r3Expanded ? 0 : 180);

    //    SetChildrenActive(r3ListTransform, r3Expanded);
    //    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);
    //}

    //private bool r4Expanded = true;
    //public void OnClickR4Bar()
    //{
    //    r4Expanded = !r4Expanded;
    //    r4Arrow.localRotation = Quaternion.Euler(0, 0, r4Expanded ? 0 : 180);

    //    SetChildrenActive(r4ListTransform, r4Expanded);
    //    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);
    //}
    public void OnClickFederationQuestButton()
    {
        backButtonObj.SetActive(true);
        CloseAllPanel();
        categoryButtons[5].interactable = false;
        federationQuestPanel.gameObject.SetActive(true);
        windowTitleText.text = MWText.instance.GetText(MWText.EText.E_2608);


        emptyQuestText.SetActive(false);

        while(questElementGridTrs.childCount != 0)
        {
            DestroyImmediate(questElementGridTrs.GetChild(0).gameObject);
        }
        
        foreach (MtQuest entry in AccountInfo.instance.questList)
        {
            if(entry.QuestType == MtQuestTypes.UnlimitedGuildQuest)
            {
                AcceptedQuestElement.CreateFederationElement(questElementGridTrs, this, entry);
            }
        }       

        if(questElementGridTrs.childCount == 0)
        {
            emptyQuestText.SetActive(true);
        }
    }

    public void ReleaseAllSubQuestTraceCheckMarkImg()
    {
        for (int i = 0; i < questElementGridTrs.childCount; i++)
        {
            questElementGridTrs.GetChild(i).GetComponent<AcceptedQuestElement>().DeactiveCheckMarkImg();
        }
    }
    public void OnClickLeaveFederation()
    {
        UICommonConfirm.Create(MWText.instance.GetText(MWText.EText.LEAVE_FEDERATION),
            string.Format(MWText.instance.GetText(MWText.EText.E_1886), fedInfo.Name), LeaveTheFederation);
    }

    public void LeaveTheFederation(string title, string[] parameters)
    {
        LobbyScene.Get().RequestLeaveFederation(OnResponseLeaveFederation);
    }

    public void OnResponseLeaveFederation(MtPacket_LeaveFederation_Response typed_pk)
    {
        if(typed_pk.UserIDX > 0)
        {
            UIToastMessage.Create(string.Format(MWText.instance.GetText(MWText.EText.E_1887), fedInfo.Name));
            AccountInfo.instance.federationIdx = 0;
            AccountInfo.instance.federationName = "";

            ZoneTerrain.Get().LeaveFederation(typed_pk.UserIDX);

            MtGameDB.ResetFederationChatLog();

            int visibleQuestIDX = PlayerPrefs.GetInt("TraceSubQuestIDX", 0);
            if (visibleQuestIDX > 0)
            {
                UIQuestBox.Get().RemoveVisibleQuest(visibleQuestIDX);
            }

            Close();
        }
        else
        {
            UIToastMessage.Create(MWText.instance.GetText(MWText.EText.ERROR_FATAL_VALID)); // "오류");
        }
    }

    #endregion

    #region 선물 패널
    [Header("선물")]
    [SerializeField] private Text giftPanelFedrationNameText = null;
    [SerializeField] private Text giftPanelFedrationLevelText = null;
    [SerializeField] private Text giftPanelFedrationEXPText = null;
    [SerializeField] private Text giftPanelFedrationGiftEXPText = null;
    [SerializeField] private Slider giftPanelFedrationEXPSlider = null;
    [SerializeField] private Slider giftPanelGiftEXPSlider = null;
    [SerializeField] private GameObject bigGiftBadge = null;
    [SerializeField] private Button bigGiftButton = null;
    [SerializeField] private Sprite[] giftSprites = null;
    [SerializeField] private Image bigGiftImage = null;

    private MtFederationGift bigGiftItem = null;

    public void OnClickFederationGift()
    {
        backButtonObj.SetActive(true);
        CloseAllPanel();
        categoryButtons[3].interactable = false;
        giftPanel.SetActive(true);
        federationStatusPanel.SetActive(false);
        warningNoGift.SetActive(false);
        RefreshPanelIcons();

        windowTitleText.text = MWText.instance.GetText(MWText.EText.E_2353); // + " " + MWText.instance.GetText( // "연맹 선물";

        bigGiftBadge.SetActive(false);
        bigGiftButton.interactable = false;
        bigGiftItem = null;

        if(fedInfo.Level >= 1 && fedInfo.Level <= 4)
        {
            bigGiftImage.sprite = giftSprites[0];
        }
        else if(fedInfo.Level >= 5 && fedInfo.Level <= 9)
        {
            bigGiftImage.sprite = giftSprites[1];
        }
        else if(fedInfo.Level >= 10 && fedInfo.Level <= 19)
        {
            bigGiftImage.sprite = giftSprites[2];
        }
        else if(fedInfo.Level >= 20)
        {
            bigGiftImage.sprite = giftSprites[3];
        }


        LobbyScene.Get().RequestFederationGiftList(OnResponseGiftItemList);
    }

    public void OnResponseGiftItemList(MtPacket_FederationGiftList_Response typed_pk)
    {
        for (int i = 0; i < giftGrid.childCount; i++)
        {
            Destroy(giftGrid.GetChild(i).gameObject);
        }

        foreach (var item in typed_pk.FederationGiftList)
        {
            if (item.GiftType == MtFederationGiftType.BigGift)
            {
                if (!item.IsRecived)
                {
                    bigGiftBadge.SetActive(true);
                    bigGiftButton.interactable = true;
                    bigGiftItem = item;
                }
            }
            else
            {
                FederationGiftElement.Create(this, giftGrid, item, RecountSliverCoin);
            }
        }

        warningNoGift.SetActive(typed_pk.FederationGiftList.Count == 0);
        giftPanelSliverCoinText.text = string.Format("{0:n0}", AccountInfo.instance.federationSilver); 
    }

    public void RecountSliverCoin(long amount)
    {
        giftPanelSliverCoinText.text = string.Format("{0:n0}", amount);

    }

    public void OnClickBigGift()
    {
        if (bigGiftItem != null)
        {
            LobbyScene.Get().RequestRecieveFederationGift(bigGiftItem.IDX, OnRecieveFederationGift);
        }
    }

    public void OnRecieveFederationGift(MtPacket_RecieveFederationGift_Response type_pk)
    {
        if (1 == type_pk.Result)
        {
            UIRewardPanel rp = UIRewardPanel.Create();
            rp.AddReward(bigGiftItem.ItemType, 0, bigGiftItem.ItemAmount);

            if (bigGiftItem.FederationSilverAmount > 0)
            {
                rp.AddReward(MtItemTypes.FederationSilver, 0, bigGiftItem.FederationSilverAmount);
                RecountSliverCoin(AccountInfo.instance.federationSilver + bigGiftItem.FederationSilverAmount);
            }

            UIKingdomInfo.Get().ResetAccountData();

            fedInfo.EXP = type_pk.FederationEXP;
            fedInfo.Level = type_pk.FederationLevel;
            fedInfo.GiftEXP = type_pk.FederationGiftEXP;

            RefreshFederationEXP();

            bigGiftBadge.SetActive(false);
            bigGiftButton.interactable = false;
            bigGiftItem = null;
        }
    }

    #endregion

    #region 창고 패널
    [SerializeField] private GameObject warehousePanel = null;

    public void OnClickWarehouse()
    {
#if UNITY_EDITOR
        CloseAllPanel();
        categoryButtons[4].interactable = false;
        backButtonObj.SetActive(true);

        warehousePanel.SetActive(true);
#else
        UIToastMessage.Create(MWText.EText.UNDER_CONSTRUCTION);
#endif
    }
    #endregion

    #region 연구 패널
    public void OnClickResearch()
    {
        UIToastMessage.Create(MWText.EText.UNDER_CONSTRUCTION);
    }
    #endregion

    #region 연맹 상점
    [Header("연맹 상점")]
    [SerializeField] private Transform productParent = null;
    private List<MtProduct> productList = new List<MtProduct>();
    private List<ProductElement> elements = new List<ProductElement>();
    public MtProduct lastClickedProduct = null;

    [SerializeField] private Text federationSilverAmountText = null;
    [SerializeField] private GameObject storeDetailPanel = null;

    public void OnClickFederationStore()
    {
        windowTitleText.text = MWText.instance.GetText(MWText.EText.E_2354);

        backButtonObj.SetActive(true);
        masterUserPanel.SetActive(false);
        CloseAllPanel();
        categoryButtons[6].interactable = false;
        federationStorePanel.SetActive(true);
        RefreshPanelIcons();

        federationStatusPanel.SetActive(true);
        storeDetailPanel.SetActive(false);

        federationSilverAmountText.text = UIUtility.ToCommaNumber(AccountInfo.instance.federationSilver);

        LobbyScene.Get().RequestProductList(OnResponsePruductList);
    }



    private void OnResponsePruductList(MtPacket_ProductList_Response typed_pk)
    {
        productList.Clear();
        DestroyChildren(productParent);

        ScrollRect rect = null;

        foreach (MtProduct product in typed_pk.ProductList)
        {
            MtDataManager.FillProductData(product);
            productList.Add(product);
        }

        foreach (MtProduct product in productList)
        {
            if (product.Catetory == MtProductCategories.FederationProduct)
            {
                elements.Add(ProductElement.Create(this, productParent, product));
            }
        }
    }
    public void OnClickProduct(MtProduct productInfo, Sprite productImage)
    {
        lastClickedProduct = productInfo;

        if (productInfo.InAppID.Equals(""))
        {
            LobbyScene.Get().RequestBuyProduct(productInfo.IDX, OnResponseBuyProduct);
        }
        else
        {
#if REAL_PAYMENT
            storeController.InitiatePurchase(productInfo.InAppID);
#endif
        }
    }

    private void OnYesToBuy(string text, string[] parameters)
    {
        int idx = (int.Parse(parameters[0]));
        LobbyScene.Get().RequestBuyProduct(idx, OnResponseBuyProduct);
    }


    private void OnResponseBuyProduct(MtPacket_BuyProduct_Response typed_pk)
    {
        if (typed_pk.Result)
        {

            if (typed_pk.ProductType == MtProductTypes.LightElement)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_DIAMOND)), false);

                pan.AddReward(MtItemTypes.LightElement, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.ProductType == MtProductTypes.DarkElement)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_OBSIDIAN)), false);

                pan.AddReward(MtItemTypes.DarkElement, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.ProductType == MtProductTypes.FireElement)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_RUBY)), false);

                pan.AddReward(MtItemTypes.FireElement, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.ProductType == MtProductTypes.WaterElement)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_SAPPHIRE)), false);

                pan.AddReward(MtItemTypes.WaterElement, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.ProductType == MtProductTypes.Wood)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_WOOD)), false);

                pan.AddReward(MtItemTypes.Wood, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.ProductType == MtProductTypes.Stone)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_STONE)), false);

                pan.AddReward(MtItemTypes.Stone, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.ProductType == MtProductTypes.Manastone)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_MANASTONE)), false);

                pan.AddReward(MtItemTypes.Manastone, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.ProductType == MtProductTypes.Gold)
            {
                UIRewardPanel pan = UIRewardPanel.Create(string.Format(MWText.instance.GetText(MWText.EText.SHOP_BUY_SUCCEED), MWText.instance.GetText(MWText.EText.RESOURCE_GOLD)), false);

                pan.AddReward(MtItemTypes.Gold, 0, typed_pk.ContentValue);
            }
            else if (typed_pk.RetainItemType != MtItemTypes.None)
            {
                UIRewardPanel pan = UIRewardPanel.Create("", true);

                pan.AddReward(typed_pk.RetainItemType, 0, typed_pk.ContentValue);
            }

            foreach (ProductElement element in elements)
            {
                element.RefreshLimitationState();
            }

            AccountInfo.instance.federationSilver += typed_pk.FederationSilverBalance;
            federationSilverAmountText.text = AccountInfo.instance.federationSilver.ToString();

            UIKingdomInfo.Get().ResetAccountData();
        }
        else
        {
            UIToastMessage.Create(MWText.instance.GetText(MWText.EText.SHOP_BUY_FAIL));
        }
    }
    #endregion

    #region 지원 패널
    [Header("지원")]
    [SerializeField] private GameObject supportPanel = null;
    [SerializeField] private Transform supportElementParentGridTrs = null;
    [SerializeField] private GameObject notEnoughListMessage = null;
    [SerializeField] private Button supportButton = null;
    [SerializeField] private Text supportButtonText = null;
    private List<int> federationMemberSupportIDXs = new List<int>();
    public void OnClickSupport()
    {
        if(supportElementParentGridTrs.childCount != 0)
        {
            while(supportElementParentGridTrs.childCount > 0)
            {
                DestroyImmediate(supportElementParentGridTrs.GetChild(0).gameObject);
            }
        }


        backButtonObj.SetActive(true);
        CloseAllPanel();
        categoryButtons[7].interactable = false;
        warningNoGift.SetActive(false);
        supportPanel.gameObject.SetActive(true);
        RefreshPanelIcons();

        windowTitleText.text = MWText.instance.GetText(MWText.EText.E_1949); // 연맹지원

        LobbyScene.Get().RequestFederationSupportList(OnResponseFederationSupportList);
    }

    public void OnResponseFederationSupportList(MtPacket_FederationSupportList_Response typed_pk)
    {
        federationMemberSupportIDXs.Clear();
        AccountInfo.instance.FederationMemberSupportList.Clear();

        int possibleSupportCount = 0;

        if (typed_pk.SupportList.Count > 0)
        {
            supportButton.interactable = true;
            notEnoughListMessage.gameObject.SetActive(false);
            supportButtonText.color = new Color(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f);

            foreach (MtFederationSupport element in typed_pk.SupportList)
            {
                if (element.UserName == AccountInfo.instance.name)
                {
                    SupportElement.Create(supportElementParentGridTrs, element);
                    //federationMemberSupportIDXs.Add(element.IDX);
                }


            }
            foreach (MtFederationSupport element in typed_pk.SupportList)
            {
                if (element.UserName != AccountInfo.instance.name)
                {
                    SupportElement.Create(supportElementParentGridTrs, element);
                    federationMemberSupportIDXs.Add(element.IDX);
                    AccountInfo.instance.FederationMemberSupportList.Add(element.IDX);
                    possibleSupportCount++;
                }
            }

            foreach (int item in AccountInfo.instance.FederationMemberSupportList)
            {
                Debug.Log("SupportListElement IDX : " + item.ToString());
            }
        }
        else
        {
            notEnoughListMessage.gameObject.SetActive(true);
        }
        
        if(possibleSupportCount == 0)
        {
            supportButton.interactable = false;            
            supportButtonText.color = new Color(125.0f / 255.0f, 125.0f / 255.0f, 125.0f / 255.0f);
        }
    }


    public void OnClickAllSupportButton()
    {
        LobbyScene.Get().RequestAcceptFederationSupport(federationMemberSupportIDXs);
        AccountInfo.instance.FederationMemberSupportList.Clear();
        //LobbyScene.Get().uiLobbyScene.SetActiveFederationSupportButton(false);

        if (supportElementParentGridTrs.childCount != 0)
        {
            while (supportElementParentGridTrs.childCount > 0)
            {
                DestroyImmediate(supportElementParentGridTrs.GetChild(0).gameObject);
            }
        }

        supportButton.interactable = false;
        notEnoughListMessage.gameObject.SetActive(true);
        supportButtonText.color = new Color(125.0f / 255.0f, 125.0f / 255.0f, 125.0f / 255.0f);
    }
    #endregion

    # region 설정패널
    [Header("설정")]
    [SerializeField] private Button configTabButton = null;
    [SerializeField] private GameObject configPanel = null;
    [SerializeField] public Transform applicantsGridTrs = null;
    [SerializeField] private GameObject notEnoughtApplicationText = null;
    [SerializeField] private GameObject configButtonPanel = null;
    [SerializeField] private GameObject applyType1 = null;
    [SerializeField] private GameObject applyType2 = null;
    [SerializeField] private InputField fedIntroInput = null;

    public void OnClickShowPendingJoinUserList()
    {
        configButtonPanel.SetActive(false);
        //backButtonObj.GetComponent<Button>().onClick.RemoveAllListeners();
        //backButtonObj.GetComponent<Button>().onClick.AddListener(CloseApplicantsPanel);
        //backButtonObj.SetActive(true);
        //LobbyScene.Get().RequestPendingJoinUserList(OnResponsePendingFederationJoinUserList);
    }

    public void OnResponsePendingFederationJoinUserList(MtPacket_PendingFederationJoinUserList_Response typed_pk)
    {
        notEnoughtApplicationText.SetActive(false);
        while (applicantsGridTrs.childCount > 0)
        {
            DestroyImmediate(applicantsGridTrs.GetChild(0).gameObject);
        }

        if (typed_pk.PendingJoinUserList.Count > 0)
        {
            foreach (MtJoinFederationPendingUser entry in typed_pk.PendingJoinUserList)
            {
                FederationApplicantElement.Create(this, entry);
            }
        }
        else
        {
            notEnoughtApplicationText.SetActive(true);
        }
    }

    public void OnClickCloseApplicantsPanelButton()
    {
        configButtonPanel.SetActive(true);
    }


    public void OnClickConfigTabButtotn()
    {
        if (configPanel.activeSelf) return;
        if (fedInfo.MasterUserIDX != AccountInfo.instance.idx)
        {
            UIToastMessage.Create(MWText.EText.E_2561);
            return;
        }

        masterUserPanel.SetActive(false);
        CloseAllPanel();

        backButtonObj.SetActive(true);
        configPanel.SetActive(true);
        configButtonPanel.SetActive(true);
        RefreshPanelIcons();

        applyType1.SetActive(fedInfo.ApplyType == MtFederationApplyTypes.JoinImmediately);
        applyType2.SetActive(fedInfo.ApplyType != MtFederationApplyTypes.JoinImmediately);

        fedIntroInput.text = fedInfo.IntroductionText;

        LobbyScene.Get().RequestPendingJoinUserList(OnResponsePendingFederationJoinUserList);
    }

    public void OnClickFederationTabButton()
    {
        OnClickBackButton();
    }

    public void SetActiveNotEnoughApplicationText(bool isActive, int applicantCount)
    {
        notEnoughtApplicationText.SetActive(isActive);
        settingBadge.SetActive(applicantCount > 0);
        settingBadgeText.text = applicantCount.ToString();
    }

    public void OnClickNotice()
    {
        UICommonToolTip.Create(noticeButton, fedInfo.IntroductionText);
    }

    public void OnClickChangeApplyType()
    {
        applyType1.SetActive(!applyType1.activeSelf);
        applyType2.SetActive(!applyType1.activeSelf);
    }

    public void OnClickConfirmSaveSetting()
    {
        fedInfo.ApplyType = applyType1.activeSelf ? MtFederationApplyTypes.JoinImmediately : MtFederationApplyTypes.JoinAfterConfirm;
        fedInfo.IntroductionText = fedIntroInput.text;

        LobbyScene.Get().RequestChangeFederationSetting(fedInfo.ApplyType, fedInfo.IntroductionText);
        UIToastMessage.Create(MWText.EText.E_2562);
        OnClickBackButton();
    }

    #endregion


    [SerializeField] private Transform helpIcon = null;

    public void RefreshPanelIcons()
    {
        UIButton btn = helpIcon.GetComponent<UIButton>();
        btn.onClick.RemoveAllListeners();

        UnityAction action = null;

        string buttonName = "";

        if(mainPanel.activeSelf)
        {
            buttonName = "Member";
        }
        else if(federationTroopsPanel.activeSelf)
        {
            action = (delegate { UICommonToolTip.Create(helpIcon, MWText.instance.GetText(MWText.EText.HELP_TROOP), UICommonToolTip.TooltipDirection.Up); });
            
            buttonName = "Troop";
        }
        else if(areaPanel.activeSelf)
        {
            string text = MWText.instance.GetText(MWText.EText.HELP_AREA1) + "\n\n" + MWText.instance.GetText(MWText.EText.HELP_AREA2);

            action = (delegate { UICommonToolTip.Create(helpIcon, text, UICommonToolTip.TooltipDirection.Up); });

            buttonName = "Area";
        }
        else if(federationStorePanel.activeSelf)
        {
            action = (delegate { UICommonToolTip.Create(helpIcon, MWText.instance.GetText(MWText.EText.HELP_SHOP), UICommonToolTip.TooltipDirection.Up); });

            buttonName = "Shop";
        }
        else if(giftPanel.activeSelf)
        {
            action = (delegate { UICommonToolTip.Create(helpIcon, MWText.instance.GetText(MWText.EText.HELP_GIFT), UICommonToolTip.TooltipDirection.Up); });

            buttonName = "Gift";
        }
        else if (supportPanel.activeSelf)
        {
            action = (delegate { UICommonToolTip.Create(helpIcon, MWText.instance.GetText(MWText.EText.HELP_SUPPORT), UICommonToolTip.TooltipDirection.Up); });

            buttonName = "Support";
        }

        if (action != null)
            btn.onClick.AddListener(action);
        
        btn.gameObject.SetActive(action != null);
    }

    public void RefreshQuestBadge()
    {
        int questBadgeCount = 0;

        foreach (MtQuest quest in AccountInfo.instance.questList)
        {
            if (quest.IsAccomplished() && quest.QuestType == MtQuestTypes.UnlimitedGuildQuest)
            {
                questBadgeCount++;
            }
        }

        questBadge.SetActive(questBadgeCount > 0);
        questBadgeText.text = questBadgeCount.ToString();
    }

    public void RetainExperience(int amount)
    {
        fedInfo.EXP += amount;
        RefreshFederationEXP();
    }
}