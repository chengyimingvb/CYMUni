//**********************************************
// Class Name	: CYMBase
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CYM.Excel;
using CYM.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

namespace CYM
{
    #region battle
    public interface IBaseBattleMgr
    {
        Scene SceneStart { get; }
        Scene SceneSelf { get; }

        void StartNewGame(string battleId = "");
        void ContinueGame();
        void LoadGame(string dbKey);
        void GoToStart();
        void LoadBattle(string tdid);
        void LockBattleStartFlow(bool b);
        void UnPauseLoadingView();

        bool IsInPauseLoadingView { get; }
        bool IsInBattle { get; }

        string SceneName { get; }
        string BattleID { get; }
        bool IsLoadingBattle { get; }
        bool IsGameStartOver { get; }
        bool IsLoadBattleEnd { get; }
        bool IsLockBattleStartFlow { get; }
        bool IsLoadedScene { get; }

        #region Callback
        event Callback Callback_OnStartNewGame;
        event Callback Callback_OnCloseLoadingView;
        event Callback Callback_OnGameStartOver;
        event Callback Callback_OnBackToStart;
        event Callback Callback_OnBattleLoad;
        event Callback Callback_OnBattleLoaded;
        event Callback Callback_OnBattleLoadedScene;
        event Callback Callback_OnBattleReadDataEnd;
        event Callback Callback_OnBattleUnLoad;
        event Callback Callback_OnBattleUnLoaded;
        event Callback Callback_OnGameStart;
        event Callback Callback_OnGameStarted;
        event Callback Callback_OnBattleLoadStart;
        event Callback<string, float> Callback_OnLoadingProgressChanged;
        event Callback Callback_OnStartCustomBattleCoroutine;
        event Callback Callback_OnEndCustomBattleCoroutine;
        event Callback Callback_OnRandTip;
        event Callback Callback_OnInPauseLoadingView;
        #endregion
    }
    public interface IBaseSubBattleMgr
    {
        void Load(string battleID = "");
        void UnLoad();

        bool IsInBattle { get; }
        string SceneName { get; }
        string BattleID { get; }
        bool IsLoadingBattle { get; }
        bool IsGameStart { get; }
        bool IsLoadBattleEnd { get; }

        #region Callback
        event Callback Callback_OnBattleLoad;
        event Callback Callback_OnBattleLoaded;
        event Callback Callback_OnBattleLoadedScene;
        event Callback Callback_OnBattleUnLoad;
        event Callback Callback_OnBattleUnLoaded;
        event Callback Callback_OnBattleLoadStart;
        event Callback Callback_OnGameStart;
        event Callback<string, float> Callback_OnLoadingProgressChanged;
        event Callback Callback_OnRandTip;
        #endregion
    }
    public interface IBattleClear
    {
        void Clear();
    }
    #endregion

    #region UI
    public interface IUIDirty
    {
        bool IsDirtyShow { get; }
        bool IsDirtyData { get; }
        bool IsDirtyCell { get; }
        bool IsDirtyRefresh { get; }

        void SetDirtyAll(float delay);
        void SetDirtyAll();
        void SetDirtyShow();
        void SetDirtyData();
        void SetDirtyRefresh();
        void SetDirtyCell();

        void RefreshAll();
        void RefreshShow();
        void RefreshCell();
        void RefreshData();
        void Refresh();

        void OnFixedUpdate();
    }
    public interface IBaseCheckBoxContainer
    {
        bool GetIsToggleGroup();
        bool GetIsAutoSelect();
        void SelectItem(UControl arg1);
    }
    #endregion

    #region other
    public interface IOnAnimTrigger
    {
        void OnAnimTrigger(int param);
    }
    public interface IPrespawner
    {
        List<string> GetPrespawnPerforms();
    }
    public interface IBaseAlertMgr
    {
        void Remove(long id);
    }
    #endregion

    #region DB
    public interface IDBListConverMgr<T> where T : DBBase
    {
        void LoadDBData(List<T> data);
        List<T> GetDBData();
    }
    public interface IDBConverMgr<T> where T : DBBase
    {
        void LoadDBData(T data);
        T GetDBData();
    }
    public interface IDBMono
    {
        // 游戏加载读取数据
        void OnRead1<TDBData>(TDBData data) where TDBData : DBBaseGame, new();
        void OnRead2<TDBData>(TDBData data) where TDBData : DBBaseGame, new();
        void OnRead3<TDBData>(TDBData data) where TDBData : DBBaseGame, new();
        // 读取数据结束
        void OnReadEnd<TDBData>(TDBData data) where TDBData : DBBaseGame, new();
        // 存储数据
        void OnWrite<TDBData>(TDBData data) where TDBData : DBBaseGame, new();
    }
    #endregion

    public interface IMono
    {
        void OnEnable();
        void OnSetNeedFlag();
        void Awake();
        void OnAffterAwake();
        void Start();
        void OnAffterStart();
        void OnUpdate();
        void OnFixedUpdate();
        void OnDisable();
        void OnDestroy();
        T AddComponent<T>() where T : BaseMgr, new();
        void RemoveComponent(BaseMgr component);
    }
    public interface IUnit
    {
        bool IsInited { get; }
        // 角色第一次创建，逻辑初始化的时候
        void OnInit();
        // 角色复活后触发
        void OnReBirth();
        // 角色第一次创建或者复活都会触发
        void OnBirth();
        // 角色第一次创建或者复活都会触发
        void OnBirth2();
        // 角色第一次创建或者复活都会触发
        void OnBirth3();
        // 角色假死亡
        void OnDeath();
        // 角色真的死亡
        void OnRealDeath();
        // 帧回合
        void OnGameFrameTurn(int gameFramesPerSecond);
        void OnGameStart1();
        void OnGameStart2();
        // 游戏开始后触发
        void OnGameStarted1();
        void OnGameStarted2();
        void OnGameStartOver();

        // 逻辑回合
        void OnGameLogicTurn();
        // 手动更新
        void ManualUpdate();
    }
    public interface ITDLuaMgr
    {
        Type DataType { get; }
        void OnLuaParseStart();
        void OnLuaParseEnd();
        void OnExcelParseStart();
        void OnExcelParseEnd();
        void OnAllLoadEnd1();
        void OnAllLoadEnd2();
        Dictionary<string, TDBaseData> BaseDatas { get; }
        T Get<T>(string key) where T : TDBaseData;
        IList GetRawGroup(string group);
        bool Contains(string key);
        List<object> ObjValues { get; }
        List<string> Keys { get; }
        TableMapper TableMapper { get;}
        void AddAlterRangeFromObj(IEnumerable<object> data);
    }

    public interface IUnitSpawnMgr
    {
        Type UnitType { get; }
        BaseUnit GetBaseUnit(long rtid);
        BaseUnit GetBaseUnit(string tdid);
        void Despawn(BaseUnit data,float delay=0);
        bool IsHave(BaseUnit unit);
    }
    public interface ITDSpawnMgr
    {
        Type UnitType { get; }
        TDBaseData GetBaseData(long rtid);
        TDBaseData GetBaseData(string tdid);
        void Despawn(TDBaseData data, float delay = 0);
        bool IsHave(TDBaseData unit);
    }
    public interface ISpawnMgr<T> where T : class, IBase
    {
        T Gold { get; }
        IDDicList<T> Data { get; }
        event Callback<T> Callback_OnAdd;
        event Callback<T> Callback_OnSpawnGold;
        event Callback<T> Callback_OnSpawn;
        event Callback<T> Callback_OnDespawn;
        event Callback<T> Callback_OnDataChanged;
        Callback<T> Callback_OnUnitMgrAdded { get; set; }
        Callback<T> Callback_OnUnitMgrRemoved { get; set; }
        Callback<T> Callback_OnUnitMgrOccupied { get; set; }
        void Clear();
        void OnSpawned(string tdid, long rtid, T unit);
        void SpawnAdd(T unit, string tdid, long? rtid = null, int? team = null);
        T SpawnNew(string tdid, Vector3? spwanPoint = null, Quaternion? quaternion = null, int? team = null, long? rtid = null, string prefab = null);
        T Spawn(string id, Vector3? spwanPoint = null, Quaternion? quaternion = null, int? team = null, long? rtid = null, string prefab = null);
        void Despawn(T data,float delay=0);
        bool IsGlobal { get; }

        T GetUnit(long rtid);
        T GetUnit(string tdid);

        void OnDataChanged(T data);
    }
    public interface IBasePlotMgr
    {
        #region set
        int PushIndex(int? index=null);
        void Start(string id);
        void RunTemp(IEnumerator<float> enumerator, string flag = null);
        void RunMain();
        void Stop();
        void EnableAI(bool b);
        void SetPlotPause(bool b, int type = 0);
        #endregion

        #region get
        CoroutineHandle CustomStartBattleCoroutine();
        #endregion

        #region is
        bool IsInPlotPause();
        bool IsInPlot();
        bool IsInPlot(params string[] tdid);
        bool IsEnableAI { get;}
        int CurPlotIndex { get; }
        #endregion

        #region ghost
        void AddToGhostSelUnits(params BaseUnit[] unit);
        void AddToGhostMoveUnits(params BaseUnit[] unit);
        void RemoveFromGhostMoveUnits(params BaseUnit[] unit);
        void AddToGhostAIUnits(params BaseUnit[] unit);
        void RemoveFromGhostSelUnits(params BaseUnit[] unit);
        void RemoveFromGhostAIUnits(params BaseUnit[] unit);
        void AddToGhostAnimUnits(params BaseUnit[] unit);
        void RemoveFromGhostAnimUnits(params BaseUnit[] unit);
        bool IsGhostSel(BaseUnit unit);
        bool IsGhostMove(BaseUnit unit);
        bool IsGhostAI(BaseUnit unit);
        bool IsGhostAnim(BaseUnit unit);
        #endregion
    }
    public interface IBaseDBMgr
    {
        DBBaseGame CurBaseGameData { get; }
        DBBaseGame StartNewGame();
        DBBaseGame LoadGame(string ID);
        DBBaseGame SaveCurGameAs(string ID, bool isSnapshot = false, bool isAsyn = true,bool isDirtyList=true, bool isHide = false, bool forceLocalArchive = false);
        DBBaseGame AutoSave(bool isSnapshot = false);
        DBBaseGame SaveTemp(bool useSnapshot = false);
        DBBaseGame Snapshot(bool isSnapshot = true);
        void DeleteArchives(string ID);
        void UseRemoteArchives(bool isUse);
        void ReadGameDBData();
        void WriteGameDBData();

        #region get
        IBaseArchiveMgr GetBaseAchieveMgr(bool isLocal = true);
        string GetDefaultSaveName();
        string GetTempSavePath();
        #endregion

        #region is
        // 是否存在当前的存档
        bool IsHaveSameArchives(string ID);
        // 是否有游戏数据
        bool IsHaveGameData();
        // 是否可以使用云存档
        bool IsCanUseRemoteArchives();
        // 是否为本地存档
        bool IsCurArchivesLocal();
        // 是否可以继续游戏
        bool IsCanContinueGame();
        #endregion
    }
    public interface IBaseArchiveFile
    {
        string Name { get; }
        DateTime SaveTime { get; }
        bool IsBroken { get; }
        ArchiveHeader Header { get; }
        DateTime FileTime { get; }
        // 未损坏且版本为最新
        // 则认为可以读取
        bool IsLoadble { get; }
        // 存档版本是否兼容
        bool IsCompatible { get; }
        TimeSpan PlayTime { get; }
        DBBaseGame BaseGameDatas { get; }
    }
    public interface IBaseArchiveMgr
    {
        List<IBaseArchiveFile> GetAllBaseArchives(bool isRefresh = false);
        void SetArchiveListDirty();
    }
    public interface IBaseSettingsMgr
    {
        DBBaseSettings GetBaseSettings();
        void Save();
    }
    public interface IBaseDifficultMgr
    {
        #region set
        void SetDiffType(GameDiffType type);
        void SetGMMod(bool b);
        void SetAnalytics(bool b);
        void SetHavePlot(bool b);
        #endregion

        #region get
        GameDiffType GetDiffType();
        DBBaseGameDiff GetBaseSettings();
        #endregion

        #region is
        bool IsAnalytics();
        bool IsGMMode();
        bool IsSettedGMMod();
        bool IsHavePlot();
        #endregion
    }
    public interface IBaseScreenMgr
    {
        BaseUnit TempPlayer { get; }
        BaseUnit BaseLocalPlayer { get; }
        BaseUnit BasePrePlayer { get; }
        string SelectedChara { get; }
        void SelectChara(string tdid);
        BaseUnit GetUnit(string id);
        event Callback<BaseUnit, BaseUnit> Callback_OnSetPlayerBase;
    }
    public interface IBaseTalkMgr
    {
        #region set
        TDBaseTalkFragment StartOption(string id);
        TDBaseTalkFragment Start(string id, int index = 0);
        TDBaseTalkFragment Next();
        void ClickOption(int index);
        void ClickTalk();
        string SelectOption(int index);
        void SelectPreOption();
        void SelectNextOption();
        void Stop();
        bool IsHave();
        bool IsInOption();
        bool IsLockNextTalk { get; }
        #endregion

        #region get
        TDBaseTalkFragment CurTalkFragment();
        #endregion
    }
    public interface IBaseNarrationMgr
    {
        #region Callback
        Callback<TDBaseNarrationData, NarrationFragment> Callback_OnStartNarration { get; set; }
        Callback<TDBaseNarrationData, NarrationFragment, int> Callback_OnNextNarration { get; set; }
        Callback<TDBaseNarrationData, NarrationFragment> Callback_OnEndNarration { get; set; }
        #endregion

        #region set
        NarrationFragment Start(string id);
        NarrationFragment Next();
        void Stop();
        bool IsHave();
        #endregion

        #region get
        NarrationFragment CurNarrationFragment();
        #endregion
    }
    public interface IBaseLogicTurnMgr
    {
        bool IsInGlobalMoveState { get; }
        bool IsInPlayerTurn { get; }
        BoolState IsLockTurnState { get; }
        bool IsTurnCount(int count);
    }
    public interface IBaseSenseMgr
    {
        float Radius { get; }
        string SenseName { get; }
        void DoTestEnter(Collider col);
        void DoTestExit(Collider col);
        void DoCollect();
    }
    public interface IBaseWarfareData
    {
        long ID { get; set; }
        string TDID { get; set; }
        int WarDay { get; set; }
        float AttackersWarPoint { get; set; }
        float DefensersWarPoint { get; set; }
        void AddToConvence(BaseUnit chief, BaseUnit unit);
    }
    public interface IUnitMgr
    {
        Type UnitType { get; }

        #region set
        BaseUnit Add(string tdid);
        BaseUnit Add(int rtid);
        BaseUnit SpawnNew(string id, Vector3 pos, Quaternion? quaternion = null);
        void Despawn(BaseUnit legion);
        void Occupied(BaseUnit unit);
        Vector3 CalcAveragePos();
        void SortByScore();
        #endregion

        #region is
        bool IsHave();
        bool IsHave(BaseUnit unit);
        bool IsHave(string tdid);
        #endregion
    }
    public interface IBaseAttrMgr
    {
        void DoCost<TCostType>(List<Cost<TCostType>> datas, bool isReverse = false) where TCostType : Enum;
    }
    public interface IBaseBuffMgr
    {
        void Add(List<string> buffName);
        void Remove(List<string> buffName);

        string GetTableDesc(List<string> ids, bool newLine = false, string split = Const.STR_Indent, float? anticipationFaction = null, bool appendHeadInfo = false);
        // 拼接所有传入的buff addtion 的字符窜
        string GetTableDesc(string id, bool newLine = false, string split = Const.STR_Indent, float? inputVal = null, bool appendHeadInfo = false);
    }
    public interface IBaseHUDMgr 
    {
        THUD SpawnDurableHUD<THUD>(string prefabName, BaseUnit target = null) where THUD : UHUDBar;
        UHUDText JumpChatBubbleStr(string str);
        UHUDText JumpChatBubble(string key);
    }
    public interface IBaseAttrAdditon
    {
        AttrOpType AddType { get; set; }
        AttrFactionType FactionType { get; set; }
        float Val { get; set; }
        float Faction { get; set; }
        float Step { get; set; }
        float InputValStart { get; set; }
        float RealVal { get; }
        float InputVal { get; }
        float Min { get; }
        float Max { get; }
    }
    public interface IBaseUpFactionData
    {
        UpFactionType FactionType { get; set; }
        float AnticipationVal(float? inputVal);
        float Val { get; }
        float RealVal { get; }
        float InputValStart { get; set; }
        float InputVal { get; }
        float Faction { get; set; }
        float Add { get; set; }
        float Percent { get; set; }
        string GetName();
        Sprite GetIcon();
        string GetDesc(bool isHaveSign = false, bool isHaveColor = false, bool isHeveAttrName = true);
    }
    public interface IBaseDipMgr
    {
        event Callback<BaseUnit> Callback_OnChangeRelation;
        event Callback<BaseUnit, int> Callback_OnChangeRelationShip;
        event Callback<BaseUnit, bool> Callback_OnChangeAlliance;
        event Callback<BaseUnit, bool> Callback_OnChangeMarriage;
        event Callback<BaseUnit, bool> Callback_OnChangeSubsidiary;
        event Callback<BaseUnit, int> Callback_OnChangeArmistice;
        event Callback<object> Callback_OnAddToAttacker;
        event Callback<object> Callback_OnAddToDefensers;
        event Callback<object> Callback_OnRemoveFromWar;
        event Callback<object, BaseUnit> Callback_OnDecalarWar;
        event Callback<object, BaseUnit> Callback_OnBeDecalarWar;
        event Callback<TDBaseAlertData, bool> Callback_OnBeAccept;
        event Callback<TDBaseAlertData, bool> Callback_OnAccept;

        #region Callback
        void OnChangeRelation(BaseUnit other);
        void OnChangeRelationShip(BaseUnit other, int relShip);
        void OnChangeAlliance(BaseUnit other, bool b);
        void OnChangeMarriage(BaseUnit other, bool b);
        void OnChangeSubsidiary(BaseUnit other, bool b);
        void OnChangeArmistice(BaseUnit other, int count);
        void OnAddToAttacker(object warData);
        void OnAddToDefensers(object warData);
        void OnRemoveFromWar(object warData);
        void OnDecalarWar(object warData, BaseUnit other);
        void OnBeDecalarWar(object warData, BaseUnit caster);
        void OnDeclarePeace(object warData, BaseUnit other);
        void OnBeAccept(TDBaseAlertData data, bool b);
        void OnAccept(TDBaseAlertData data, bool b);
        #endregion
    }
    public interface IBaseAStarMoveMgr
    {
        #region is
        bool IsCanTraversal(GraphNode node);
        #endregion

        #region AStar
        BaseTraversal Traversal { get; }
        void SetToNode(GraphNode node);
        GraphNode PreNode { get; }
        GraphNode CurNode { get; }
        #endregion

        #region get
        List<Vector3> GetDetailPathPoints(ABPath path = null);
        #endregion
    }
    public interface IBaseMoveMgr
    {
        #region Callback val
        Callback Callback_OnMoveStart { get; set; }
        Callback Callback_OnMovingAlone { get; set; }
        Callback Callback_OnMoveEnd { get; set; }
        Callback Callback_OnFirstMovingAlone { get; set; }
        #endregion

        #region prop
        float SearchedSpeed { get; }
        Vector3 SearchedPos { get; }
        Vector3 Destination { get; }
        BaseUnit MoveTarget_Unit { get; }
        #endregion

        #region val
        float BaseMoveSpeed { get; }
        #endregion

        #region is
        bool IsPositionChange { get; }
        bool IsRotationChange { get; }
        bool IsMoving { get; }
        bool IsCanMove { get; }
        bool IsHaveMoveTarget();
        //是否可以自动执行MoveTarget操作
        bool IsCanAutoExcuteMoveTarget();
        bool IsInDestination();
        bool IsNoInDestination();
        bool IsInPos(Vector3 targetPos,float k=0);
        #endregion

        #region set
        bool RePath();
        bool StartPath(Vector3 pos, float speed);
        void StopPath();
        void PreviewPath(Vector3 pos);
        bool ExcuteMoveTarget(bool isManual);
        #endregion

        #region rotate
        void Look(BaseUnit unit);
        void SetRotationY(float rot);
        void GrabNewQuateration(Quaternion? qua = null);
        void RandRotationY();
        #endregion
    }
    public interface IBaseArticleMgr
    {
        //我方的协议
        List<TDBaseArticleData> GetTempBaseSelfArticlies();
        //对方的协议
        List<TDBaseArticleData> GetTempBaseTargetArticlies();
        int CalcArticleScore();
        void RemoveArticle(long id);
        void RemoveArticle(TDBaseArticleData data);
        void PushNagotiationToAlert(TDBaseAlertData data);
        bool IsStarNegotiation { get; }
        T Get<T>(long id) where T : TDBaseArticleData;
    }
    public interface IBaseRelationMgr
    {
        event Callback Callback_OnBaseChangeRelation;
        T GetWarfareData<T>(long id) where T : class, IBaseWarfareData;
        int GetRelationShip(BaseUnit first, BaseUnit second);

        bool IsInWarfare(BaseUnit first);
        bool IsFriend(BaseUnit first, BaseUnit second);

        bool IsHaveAlliance(BaseUnit unit);
        bool IsHaveMarriage(BaseUnit unit);
        bool IsHaveVassal(BaseUnit unit);
        bool IsHaveSuzerain(BaseUnit unit);
    }
    public interface IBaseLegionStationedMgr
    {
        void LeaveBlocker(BaseUnit unit);
        void LeaveDefend(bool isForce = false);
        void Defend(BaseUnit unit, bool isCustomMoveLegion = true);
        void LeaveSiege(bool isForce = false);
        void Siege(BaseUnit unit);

        bool IsInDefend();
        bool IsInSiege();
    }
    public interface IBaseCastleStationedMgr
    {
        void MoveoutLegion();
        bool IsHaveDefender();
        bool IsInSiege();

        void OnBeDefend(BaseUnit unit);
        void OnUnBeDefend(BaseUnit unit);
        void OnBeSiege(BaseUnit unit);
        void OnUnSiege(BaseUnit unit);
    }
    public interface IBaseCameraMgr
    {
        event Callback<Camera> Callback_OnFetchCamera;
        float ScrollVal { get; }
        float GetCustomScrollVal(float maxVal);
        Camera MainCamera { get; }
        Transform MainCameraTrans { get; }
        void FetchCamera();
        void Enable(bool b);
        T GetPostSetting<T>() where T : PostProcessEffectSettings;
    }
    public interface IBaseMecAnimMgr
    {
        void ChangeState(int state, int index = 0);
    }
    public interface IBundleCacher
    {
        bool IsHave(string name);
        void RemoveNull();
    }
}
