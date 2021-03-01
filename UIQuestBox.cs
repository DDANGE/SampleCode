using MysticManaCommon;
using MysticManaCommon.ContentBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UIQuestList;

public class UIQuestBox : UIControl
{
    public const string PrefabPath = "UIQuestBox";
    public static UIQuestBox Create(int top = 320)
    {
        if (singleton == null)
        {
            Transform parent = FindBottomParent();
            GameObject go = Instantiate(MtAssetBundles.Load("prefab/ui/quest/" + PrefabPath)) as GameObject;

            singleton = Create<UIQuestBox>(go, parent);
            singleton.InitializeOnce();
        }

        singleton.transform.SetParent(UIKingdomInfo.Get().uiQuestBoxParentTrs);

        RectTransform rect = singleton.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;//new Vector2(10, top);
        singleton.transform.SetAsFirstSibling();

        //singleton.transform.SetParent(FindHeaderParent());

        //singleton.questList = AccountInfo.instance.questList;

        return singleton;
    }

    class QuestIDXComparer : IComparer, IComparer<MtQuest>
    {
        public int Compare(MtQuest x, MtQuest y)
        {
            return x.IDX.CompareTo(y.IDX);
        }

        public int Compare(object x, object y)
        {
            return Compare((MtQuest)x, (MtQuest)y);
        }
    }

    private static UIQuestBox singleton;

    //PlayerPref에 저장해둘 리스트
    private List<int> visibleQuestList = null;
    //AccountInfo에서 받아온 보유중인 퀘스트리스트
    private Dictionary<int, MtQuest> myQuestDic = new Dictionary<int, MtQuest>();
    //필드정보창에 보여줄 퀘스트 리스트
    private Dictionary<int, MtQuest> visibleQuestDic = new Dictionary<int, MtQuest>();

    // 손가락으로 가르킴이 예약된 퀘스트 IDX
    private List<int> pointingReserved = new List<int>();

    public Transform gridParent = null;

    [SerializeField] private Image backPanelImg = null;
    [SerializeField] private Animator questBoxAnimator = null;

    [SerializeField] private GameObject zoomButton = null;
    [SerializeField] private Transform uiArrowCollidie = null;

    private int traceSubQuestIDX = -1;

    public static UIQuestBox Get()
    {
        return singleton;
    }

    private void InitializeOnce()
    {
        ExtractVisibleQuestFromAcceptedQuests();
        Refresh();

#if MM_FHD
        gameObject.SetActive(false);
#endif
    }

    public void DestroyPointingFinger()
    {
    }

    private IEnumerator CheckRoutin(MtQuest quest, QuestProgressText questProgressText)
    {
        while (UIQuestConversation.IsLoaded() || UIPointingFinger.IsLoaded() || UIWindow.IsLoaded() || Prologue.IsLoaded() || UIBuildResult.IsLoaded() || (!questBoxAnimator.GetCurrentAnimatorStateInfo(0).IsName("Show") && !questBoxAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE")))
        {
            yield return new WaitForSeconds(0.3f);
        }
        
        //if(quest.IsAccomplished())
            questProgressText.StartPointingOnMission();

        pointingReserved.Remove(quest.IDX);

        yield return null;
    }

    public void Refresh()
    {
        if (UIWindow.IsLoaded())
        {
            UIWindow.AddCloseAction(Refresh);
            return;
        }

        if (LobbyScene.Get().GetBattleStage().IsLoaded())
        {
            LobbyScene.Get().GetBattleStage().AddUnloadAction(Refresh);
            return;
        }

        if (Prologue.IsLoaded())
        {
            UIWindow.AddCloseAction(Refresh);
        }

        DestroyChildren(gridParent);
        RefreshVisibleQuest();

        List<MtQuest> questList = new List<MtQuest>(visibleQuestDic.Values);
        questList.Sort(new QuestIDXComparer());

        bool hasMainQuest = false;

        List<QuestProgressText> progressTextList = new List<QuestProgressText>();

        if (questList.Count > 0)
        {
            foreach (MtQuest quest in questList)
            {
                QuestProgressText questProgressText = QuestProgressText.Create(gridParent, this, quest);

                progressTextList.Add(questProgressText);

                if (pointingReserved.Contains(quest.IDX) && !UITutorial.IsLoaded() && questProgressText.gameObject.activeSelf)
                {
                    if (UIQuestConversation.IsLoaded() || UIPointingFinger.IsLoaded() || UIWindow.IsLoaded() || Prologue.IsLoaded() || UIBuildResult.IsLoaded())
                    {
                        StartCoroutine(CheckRoutin(quest, questProgressText));
                    }
                    else
                    {
                        questProgressText.StartPointingOnMission();
                        pointingReserved.Remove(quest.IDX);
                    }
                }
                else
                {
                    pointingReserved.Remove(quest.IDX);
                }

                foreach (MtQuestMission mission in quest.Missions)
                {
                    if (!mission.IsAccomplished())
                    {
                        MissionProgressText.Create(questProgressText.transform, quest, mission);
                        break;
                    }
                }

                if (quest.QuestType == MtQuestTypes.MainStory)
                {
                    hasMainQuest = true;
                }
            }
        }

        if (!hasMainQuest && !UIQuestConversation.IsLoaded())
        {
            MtQuest quest = MtDataManager.GetQuestDataFromPreRequirementIDX(AccountInfo.instance.questStep);

            if (quest != null)
            {
                QuestProgressText questProgressText = QuestProgressText.Create(gridParent, this, quest, true);
                questProgressText.transform.SetAsFirstSibling();

                questList.Add(quest);
            }
        }

        zoomButton.SetActive(questList.Count > 0);

        if (questList.Count == 0)
        {
            ZoneTerrain.Get().zoneTiles.RefreshQuestMarks();
        }
        else
        {
            foreach (MtQuest quest in questList)
            {
                foreach (MtQuestMission missionInfo in quest.Missions)
                {
                    int x = 0, y = 0;

                    if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromCastle)
                    {
                        x = missionInfo.TargetIndex1 + AccountInfo.instance.castlePositionX;
                        y = missionInfo.TargetIndex2 + AccountInfo.instance.castlePositionY;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromSherinStation)
                    {
                        x = MtStatic.SherinStationX;
                        y = MtStatic.SherinStationY;

                        MtTileInfo.GetPositionOnMyField(MtStatic.SherinStationX, MtStatic.SherinStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromWooboldVillageStation)
                    {
                        x = MtStatic.WooboldVillageStationX;
                        y = MtStatic.WooboldVillageStationY;

                        MtTileInfo.GetPositionOnLavaField(MtStatic.WooboldVillageStationX, MtStatic.WooboldVillageStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromOldCastleStation)
                    {
                        x = MtStatic.OldCastleStationX;
                        y = MtStatic.OldCastleStationY;

                        MtTileInfo.GetPositionOnMyField(MtStatic.OldCastleStationX, MtStatic.OldCastleStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromLahindelStation)
                    {
                        x = MtStatic.LahindelStationX;
                        y = MtStatic.LahindelStationY;

                        MtTileInfo.GetPositionOnMyField(MtStatic.LahindelStationX, MtStatic.LahindelStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromMarenStation)
                    {
                        x = MtStatic.MarenStationX;
                        y = MtStatic.MarenStationY;

                        MtTileInfo.GetPositionOnMyField(MtStatic.MarenStationX, MtStatic.MarenStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromDotakCastle)
                    {
                        x = MtStatic.DotakCastleStationX;
                        y = MtStatic.DotakCastleStationY;

                        MtTileInfo.GetPositionOnLavaField(MtStatic.DotakCastleStationX, MtStatic.DotakCastleStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromTitansGardenStation)
                    {
                        x = MtStatic.TitansGardenStationX;
                        y = MtStatic.TitansGardenStationY;

                        MtTileInfo.GetPositionOnLavaField(MtStatic.TitansGardenStationX, MtStatic.TitansGardenStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }
                    else if (missionInfo.ActionType == MtMissionActionTypes.GotoLocationFromLastStageStation)
                    {
                        x = MtStatic.LastStageStationX;
                        y = MtStatic.LastStageStationY;

                        MtTileInfo.GetPositionOnLavaField(MtStatic.LastStageStationX, MtStatic.LastStageStationY, out x, out y, AccountInfo.instance.castlePositionX, AccountInfo.instance.castlePositionY);

                        x += missionInfo.TargetIndex1;
                        y += missionInfo.TargetIndex2;
                    }

                    if (x != 0 || y != 0)
                    {
                        ZoneTile tile = ZoneTerrain.Get().zoneTiles.GetTileByXY(x, y);
                        if (tile != null)
                        {
                            tile.Refresh3D();
                        }
                    }
                }
            }
        }

        if (questList.Count == 0)
            backPanelImg.enabled = false;
        else
            backPanelImg.enabled = true;

        RefreshHeight();
    }

    public void RefreshHeight()
    {
        if (LobbyScene.Get().GetBattleStage().IsLoaded()) return;
        if (UIWindow.IsLoaded()) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent.GetComponent<RectTransform>());

        float totalHeight = 55; //Rect Top + Bottom size
        QuestProgressText[] qpts = gridParent.GetComponentsInChildren<QuestProgressText>();
        foreach (QuestProgressText qpt in qpts)
        {
            MissionProgressText mpt = qpt.GetComponentInChildren<MissionProgressText>();

            if (mpt != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(mpt.GetComponent<RectTransform>());
            }

            RectTransform rt = qpt.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            totalHeight += rt.rect.height + 2; // + cell spacing
        }

        const float originHeightSize = 400.0f;

        //최초 사이즈보다 크면 늘린다
        if (totalHeight > originHeightSize) totalHeight = originHeightSize;

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, totalHeight);

        uiArrowCollidie.localScale = new Vector3(rectTransform.sizeDelta.x + 100f, rectTransform.sizeDelta.y, 1);
        uiArrowCollidie.localPosition = new Vector3(uiArrowCollidie.localScale.x / 2f, -(uiArrowCollidie.localScale.y / 2f), 0);
    }       

    public void AddVisibleQuest(MtQuest quest)
    { 
        if (!visibleQuestDic.ContainsKey(quest.IDX))
        {
            visibleQuestDic.Add(quest.IDX, quest);
        }
    }

    public void AddVisibleQuest(MtQuest quest, bool isPointing = false)
    {
        if (!visibleQuestDic.ContainsKey(quest.IDX))
        {
            visibleQuestDic.Add(quest.IDX, quest);

            if (isPointing)
            {
                pointingReserved.Add(quest.IDX);
            }
        }

        Refresh();
    }

    public void AddVisibleFederationQuest(MtQuest quest, bool isPointing = false)
    {      
        List<int> visibleFederationQuestIDX = new List<int>();

        foreach (MtQuest entry in visibleQuestDic.Values)
        {
            if(entry.QuestType == MtQuestTypes.UnlimitedGuildQuest)
            {
                visibleFederationQuestIDX.Add(entry.IDX);
            }
        }

        foreach (int entry in visibleFederationQuestIDX)
        {
            RemoveVisibleQuest(entry);
        }

        visibleQuestDic.Add(quest.IDX, quest);
        traceSubQuestIDX = quest.IDX;
        PlayerPrefs.SetInt("TraceSubQuestIDX", quest.IDX);

        if (isPointing)
        {
            pointingReserved.Add(quest.IDX);
        }
        Refresh();
    }

    public void AddSubQuest(MtQuest quest)
    {
        if (!visibleQuestDic.ContainsKey(quest.IDX))
        {
            visibleQuestDic.Add(quest.IDX, quest);
        }
    }

    public void AddVisibleSubQuest(MtQuest quest, bool isPointing = false)
    {
        //현재추적중이던 서브퀘스트를 지운다
        if (traceSubQuestIDX >= 0 && visibleQuestDic.ContainsKey(traceSubQuestIDX))
        {
            visibleQuestDic.Remove(traceSubQuestIDX);
            traceSubQuestIDX = -1;
        }

        visibleQuestDic.Add(quest.IDX, quest);
        traceSubQuestIDX = quest.IDX;
        PlayerPrefs.SetInt("TraceSubQuestIDX", quest.IDX);

        if (isPointing)
        {
            pointingReserved.Add(quest.IDX);
        }
        Refresh();
    }

    public bool IsExistInVisibleList(int questIDX)
    {
        return visibleQuestDic.ContainsKey(questIDX);
    }

    public void StartPointing()
    {
        bool showing = questBoxAnimator.GetCurrentAnimatorStateInfo(0).IsName("Show") || questBoxAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE");

        if (!showing) return;

        AddPointingReserved(AccountInfo.instance.GetCurrentMainQuestIDX());
    }

    public void EndPointing()
    {
        QuestProgressText[] qpts = gridParent.GetComponentsInChildren<QuestProgressText>();
        foreach (QuestProgressText qpt in qpts)
        {
            qpt.EndPointing();
        }
    }

    public void RemoveVisibleQuest(MtQuest quest)
    {
        if (visibleQuestDic.ContainsKey(quest.IDX))
        {
            visibleQuestDic.Remove(quest.IDX);           
        }

        Refresh();
    }
   
    public void RemoveVisibleQuest(int questIDX)
    {
        if (visibleQuestDic.ContainsKey(questIDX))
        {
            if(visibleQuestDic[questIDX].QuestType != MtQuestTypes.MainStory)
            {
                traceSubQuestIDX = -1;
                PlayerPrefs.DeleteKey("TraceSubQuestIDX");
            }
            visibleQuestDic.Remove(questIDX);
        }

        Refresh();
    }

    public void ExtractVisibleQuestFromAcceptedQuests()
    {
        bool bAddFederation = false;                

        foreach (MtQuest entry in AccountInfo.instance.questList)
        {
            if (entry.QuestType == MtQuestTypes.MainStory && !visibleQuestDic.ContainsKey(entry.IDX))
            {
                visibleQuestDic.Add(entry.IDX, entry);
                pointingReserved.Add(entry.IDX);
            }
        }


        int traceUnlimitedGuildQuestQuestIDX = 0;

        if (AccountInfo.instance.federationIdx > 0)
        {
            traceUnlimitedGuildQuestQuestIDX = PlayerPrefs.GetInt("TraceSubQuestIDX", 0);
        }

        foreach (MtQuest entry in AccountInfo.instance.questList)
        {
            if (entry.QuestType == MtQuestTypes.Battle && PlayerPrefs.GetInt("TraceBattleQuest", 1) == 1)
            {                
                visibleQuestDic.Add(entry.IDX, entry);
            }
            else if (entry.QuestType == MtQuestTypes.Finance && PlayerPrefs.GetInt("TraceFinanceQuest", 1) == 1)
            {
                visibleQuestDic.Add(entry.IDX, entry);
            }
            else if (entry.QuestType == MtQuestTypes.UnlimitedGuildQuest && !bAddFederation)
            {
                if (entry.IDX == traceUnlimitedGuildQuestQuestIDX)
                {
                    visibleQuestDic.Add(entry.IDX, entry);
                    bAddFederation = true;
                }
            }
            else if(entry.QuestType == MtQuestTypes.Event)
            {

            }
        }
    }

    public void AddPointingReserved(int questIDX)
    {
        pointingReserved.Add(questIDX);
        Refresh();
    }

    public int GetVisibleQuestAmount()
    {
        return visibleQuestDic.Count;
    }

    public void RefreshMissionProgress(MtQuestMission missionInfo)
    {
        List<MtQuest> questList = new List<MtQuest>(visibleQuestDic.Values);

        foreach (MtQuest entry in questList)
        {
            for (int i = 0; i < entry.Missions.Count; i++)
            {
                if (entry.Missions[i].IDX == missionInfo.IDX)
                {
                    entry.Missions[i] = missionInfo;

                }
            }
        }

        RefreshVisibleQuest();
        Refresh();
    }

    public void RefreshVisibleQuest()
    {
        List<MtQuest> questList = new List<MtQuest>(visibleQuestDic.Values);

        visibleQuestDic.Clear();

        int difficulty = 9999;
        MtQuest easiestSubQuest = null;

        for (int i = 0; i < questList.Count; i++)
        {
            visibleQuestDic.Add(questList[i].IDX, questList[i]);

            if (questList[i].Difficulty < difficulty)
            {
                easiestSubQuest = questList[i];
                difficulty = questList[i].Difficulty;
            }
        }

        if (easiestSubQuest != null
            && MtStatic.IsQuestStepConditionOk(AccountInfo.instance.questStep, MtStatic.FirstQuestIDX_Hunt)
            && !visibleQuestDic.ContainsKey(easiestSubQuest.IDX))
        {
            visibleQuestDic.Add(easiestSubQuest.IDX, easiestSubQuest);
        }
    }

    public void OnClick()
    {
        if (!BattleScene.instance.IsLoaded())
        {
            LobbyScene.Get().uiLobbyScene.ShowQuestList();
        }
    }

    private bool maximized = true;
    public void OnClickButton()
    {
        string trigger = maximized ? "Hide" : "Show";

        if (!maximized)
        {
            if (gridParent.childCount > 0)
            {
                gridParent.GetChild(0).GetComponent<QuestProgressText>().StartPointingOnMission();
            }
        }
        else
        {
            EndPointing();
        }

        maximized = !maximized;
        questBoxAnimator.speed = 1;
        questBoxAnimator.ResetTrigger("Hide");
        questBoxAnimator.ResetTrigger("Show");
        questBoxAnimator.SetTrigger(trigger);

        if (maximized)
        {
            rectTransform.anchoredPosition = Vector3.zero;
        }
    }

    public bool IsMaximized()
    {
        return maximized;
    }

    public new void Hide()
    {
        EndPointing();
        rectTransform.anchoredPosition = UIUtility.UnMeaningPosition;
    }

    public new void Show()
    {
        if (UISelectHeroPanel.IsLoaded() || UIQuestConversation.IsLoaded()) return;

        RefreshHeight();

        rectTransform.anchoredPosition = Vector3.zero;
    }
}
