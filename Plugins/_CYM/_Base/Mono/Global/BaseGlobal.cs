//**********************************************
// Class Name	: CYMBase
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.UI;
using DG.Tweening;
using CYM.CharacterController;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using RapidGUI;
using Invoke;
//using SickDev.DevConsole;

namespace CYM
{
    [HideMonoScript]
    public class BaseGlobal : BaseCoreMono
    {
        #region Global
        public static GameObject TempGO { get; private set; }
        public static Transform TempTrans => TempGO.transform;
        public static BaseGlobal Ins { get; protected set; }
        public static BuildConfig BuildConfig => BuildConfig.Ins;
        public static Dictionary<Type, IUnitSpawnMgr> UnitSpawnMgrs { get; private set; } = new Dictionary<Type, IUnitSpawnMgr>();
        public static List<BaseUIMgr> UIMgrs { get; private set; } = new List<BaseUIMgr>();
        public static HashSet<string> CommandLineArgs { get; private set; } = new HashSet<string>();
        private static BoolState BoolPause { get; set; } = new BoolState();
        public static Camera MainCamera { get; private set; }
        #endregion

        #region need New :这里的对象必须在基类里面手动赋值
        public static IBaseSettingsMgr SettingsMgr { get; protected set; }
        public static IBaseDBMgr DBMgr { get; protected set; }
        public static IBaseDifficultMgr DiffMgr { get; protected set; }
        public static IBaseScreenMgr ScreenMgr { get; protected set; }
        public static BaseLoaderMgr LoaderMgr { get; protected set; }
        public static BaseGRMgr GRMgr { get; protected set; }
        public static BaseLuaMgr LuaMgr { get; protected set; }
        public static BaseTextAssetsMgr TextAssetsMgr { get; protected set; }
        public static BaseLogoMgr LogoMgr { get; protected set; }
        public static BaseExcelMgr ExcelMgr { get; protected set; }
        public static BaseLanguageMgr LangMgr { get; protected set; }
        public static BaseAudioMgr AudioMgr { get; protected set; }
        public static BaseConditionMgr ACM { get; protected set; }
        public static BaseInputMgr InputMgr { get; protected set; }
        public static BasePoolMgr PoolMgr { get; protected set; }
        #endregion

        #region 非必要组件
        public static IBaseLogicTurnMgr LogicTurnMgr { get; protected set; }
        public static IBasePlotMgr PlotMgr { get; protected set; }
        public static IBaseTalkMgr TalkMgr { get; protected set; }
        public static IBaseNarrationMgr NarrationMgr { get; protected set; }
        public static IBaseBattleMgr BattleMgr { get; protected set; }
        public static IBaseSubBattleMgr SubBattleMgr { get; protected set; }
        public static IBaseAttrMgr AttrMgr { get; protected set; }
        public static IBaseBuffMgr BuffMgr { get; protected set; }
        public static IBaseArticleMgr ArticleMgr { get; protected set; }
        public static IBaseRelationMgr RelationMgr { get; protected set; }
        public static BaseBGMMgr BGMMgr { get; protected set; }
        public static BaseLoginMgr LoginMgr { get; protected set; }
        public static BaseUnlockMgr UnlockMgr { get; protected set; }
        public static BaseCameraMgr CameraMgr { get; protected set; }
        public static BaseCamera2DMgr Camera2DMgr { get; protected set; }
        public static BaseNetMgr NetMgr { get; protected set; }
        public static BaseDateTimeMgr DateTimeMgr { get; protected set; }
        public static BaseConsoleMgr DevConsoleMgr { get; protected set; }
        public static BasePlatSDKMgr PlatSDKMgr { get; protected set; }
        public static BaseCursorMgr CursorMgr { get; protected set; }
        public static BaseFOWMgr FOWMgr { get; protected set; }
        public static BaseLoggerMgr LoggerMgr { get; protected set; }
        public static BaseAStarMgr AStarMgr { get; protected set; }
        public static BaseAStar2DMgr AStar2DMgr { get; protected set; }
        public static BasePerformMgr PerformMgr { get; protected set; }
        public static BaseRefMgr RefMgr { get; protected set; }
        #endregion

        #region prop
        public static Coroutineter CommonCoroutineter { get; protected set; }
        public static Coroutineter MainUICoroutineter { get; protected set; }
        public static Coroutineter BattleCoroutineter { get; protected set; }
        public static Coroutineter SubBattleCoroutineter { get; protected set; }
        #endregion

        #region life
        public override LayerData LayerData => Const.Layer_System;
        public override MonoType MonoType => MonoType.Global;
        protected override void OnAttachComponet() { }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        void OnInitStart()
        {

        }
        public override void Awake()
        {
            if (Ins == null) Ins = this;
            //创建临时对象
            TempGO = new GameObject("TempGO");
            TempGO.hideFlags = HideFlags.HideInHierarchy;
            //使应用程序无法关闭
            Application.wantsToQuit += OnWantsToQuit;
            WinUtil.DisableSysMenuButton();
            //创建必要的文件目录
            FileUtil.EnsureDirectory(Const.Path_Dev);
            FileUtil.EnsureDirectory(Const.Path_Screenshot);
            FileUtil.EnsureDirectory(Const.Path_LocalDB);
            FileUtil.EnsureDirectory(Const.Path_CloudDB);
            //添加必要的组件
            SetupComponent<Videoer>();
            SetupComponent<Prefers>();
            SetupComponent<Feedback>();
            SetupComponent<FPSCounter>();
            SetupComponent<GlobalMonoMgr>();
            SetupComponent<GlobalUITextMgr>();
            SetupComponent<IMUIErrorCatcher>();
            SetupComponent<IMUIWaterMarker>();
            SetupComponent<IMUIOptions>();
            //初始化LuaReader
            LuaReader.Init(BuildConfig.NameSpace);
            DOTween.Init();
            DOTween.instance.transform.SetParent(Trans);
            Timing.Instance.transform.SetParent(Trans);
            QueueHub.Instance.transform.SetParent(Trans);
            Delay.Ins.transform.SetParent(Trans);
            RapidGUIBehaviour.Instance.transform.SetParent(Trans);
            //创建所有DataParse
            OnProcessAssembly();
            base.Awake();
            //添加SDK组件
            OnAddPlatformSDKComponet();
            //读取命令行参数
            OnProcessCMDArgs();
            DontDestroyOnLoad(this);
            //携程
            CommonCoroutineter = new Coroutineter("Common");
            MainUICoroutineter = new Coroutineter("MainUI");
            BattleCoroutineter = new Coroutineter("Battle");
            SubBattleCoroutineter = new Coroutineter("SubBattle");
            Pos = Const.VEC_GlobalPos;
            //CALLBACK
            LoaderMgr.Callback_OnAllLoadEnd1 += OnAllLoadEnd1;
            LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            LuaMgr.Callback_OnParseStart += OnLuaParseStart;
            LuaMgr.Callback_OnParseEnd += OnLuaParseEnd;
            //Test
            OnTest();
        }
        public override void Start()
        {
            base.Start();
        }
        void OnTest()
        {
            Delay.Run(1,()=>CLog.Info("Delay 测试成功"));
            SuperInvoke.Run(() => CLog.Info("SuperInvoke 测试成功"),2) ;
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedGUI = true;
            NeedUpdate = true;
            NeedLateUpdate = true;
            NeedGameLogicTurn = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            MainCamera = Camera.main;
        }
        public override void OnDestroy()
        {
            //CALLBACK
            LoaderMgr.Callback_OnAllLoadEnd1 -= OnAllLoadEnd1;
            LoaderMgr.Callback_OnAllLoadEnd2 -= OnAllLoadEnd2;
            LuaMgr.Callback_OnParseStart -= OnLuaParseStart;
            LuaMgr.Callback_OnParseEnd -= OnLuaParseEnd;
            Application.wantsToQuit -= OnWantsToQuit;
            UIMgrs.Clear();
            UnitSpawnMgrs.Clear();
            base.OnDestroy();
        }
        // 添加平台SDK组建
        protected void OnAddPlatformSDKComponet()
        {
            var type = BuildConfig.Ins.Distribution;
            if (type == Distribution.Steam) PlatSDKMgr = AddSteamSDKMgr();
            else if (type == Distribution.Rail) PlatSDKMgr = AddRailSDKMgr();
            else if (type == Distribution.Turbo) PlatSDKMgr = AddTurboSDKMgr();
            else if (type == Distribution.Trial) PlatSDKMgr = AddTrialSDKMgr();
            else if (type == Distribution.Gaopp) PlatSDKMgr = AddGaoppSDKMgr();
            else if (type == Distribution.Usual) PlatSDKMgr = AddUsualSDKMgr();
            else CLog.Error("未知SDK:" + type.ToString());
        }
        public override void OnDisable()
        {
            GC.Collect();
            base.OnDisable();
        }
        public void OnApplicationQuit() { }
        private void OnProcessAssembly()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
            foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (item.IsClass)
                {
                    if (item.IsSubclassOf(typeof(IMUIBase)))
                    {
                        if (item == typeof(IMUIOptions)) continue;
                        if (!item.IsAbstract && !item.IsGenericType)
                        {
                            GO.AddComponent(item);
                        }
                    }
                }
            }
#endif
        }
        private void OnProcessCMDArgs()
        {
#if UNITY_EDITOR
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            foreach (var item in commandLineArgs)
            {
                CommandLineArgs.Add(item);
                if (item == "-GM")
                {
                    DiffMgr.SetGMMod(true);
                }
                CLog.Info("CMDline arg:"+item);
            }
#endif
        }
        #endregion

        #region Add Platform SDK
        protected virtual BasePlatSDKMgr AddSteamSDKMgr() => AddComponent<BaseSteamSDKMgr>();
        protected virtual BasePlatSDKMgr AddRailSDKMgr() => AddComponent<BaseRailSDKMgr>();
        protected virtual BasePlatSDKMgr AddTurboSDKMgr() => AddComponent<BaseTurboSDKMgr>();
        protected virtual BasePlatSDKMgr AddTrialSDKMgr() => AddComponent<BaseTrialSDKMgr>();
        protected virtual BasePlatSDKMgr AddGaoppSDKMgr() => AddComponent<BaseGaoppSDKMgr>();
        protected virtual BasePlatSDKMgr AddUsualSDKMgr() => AddComponent<BaseUsualSDKMgr>();
        #endregion

        #region Callback
        protected bool OnWantsToQuit() => false;
        protected virtual void OnAllLoadEnd1() { }
        protected virtual void OnAllLoadEnd2() { }
        protected virtual void OnLuaParseStart() { }
        protected virtual void OnLuaParseEnd() { }
        #endregion

        #region pause
        // 停止游戏
        public static void PauseGame(bool b)
        {
            BoolPause.Push(b);
            doPauseGame();
        }
        public static void ResumeGame()
        {
            BoolPause.Reset();
            doPauseGame();
        }
        private static void doPauseGame()
        {
            if (BoolPause.IsIn())
            {
                GlobalMonoMgr.SetPauseType(MonoType.Unit);
                BattleCoroutineter.Pause();
                SubBattleCoroutineter.Pause();
                KinematicCharacterSystem.AutoSimulation = false;
            }
            else
            {
                GlobalMonoMgr.SetPauseType(MonoType.None);
                BattleCoroutineter.Resume();
                SubBattleCoroutineter.Pause();
                KinematicCharacterSystem.AutoSimulation = true;
            }
        }
        #endregion

        #region set
        public static void Quit()
        {
            if (!Application.isEditor)
            {
                GRMgr.UnLoadLoadedAssetBundle();
                Process.GetCurrentProcess().Kill();
                Application.Quit();
                Environment.Exit(0);
            }
        }
        public override T AddComponent<T>()
        {
            var ret = base.AddComponent<T>();
            //自动赋值
            if (ret is IUnitSpawnMgr spawner)
            {
                UnitSpawnMgrs.Add(spawner.UnitType, spawner);
            }
            else if (ret is BaseUIMgr uiMgr)
            {
                UIMgrs.Add(uiMgr);
            }
            //自动赋值的基类组件
            else if (ret is IBaseSettingsMgr && SettingsMgr == null) SettingsMgr = ret as IBaseSettingsMgr;

            else if (ret is IBaseDBMgr && DBMgr == null) DBMgr = ret as IBaseDBMgr;

            else if (ret is IBaseDifficultMgr && DiffMgr == null) DiffMgr = ret as IBaseDifficultMgr;

            else if (ret is IBaseScreenMgr && ScreenMgr == null) ScreenMgr = ret as IBaseScreenMgr;

            else if (ret is BaseLoaderMgr && LoaderMgr == null) LoaderMgr = ret as BaseLoaderMgr;

            else if (ret is BaseGRMgr && GRMgr == null) GRMgr = ret as BaseGRMgr;

            else if (ret is BaseLogoMgr && LogoMgr == null) LogoMgr = ret as BaseLogoMgr;

            else if (ret is BaseExcelMgr && ExcelMgr == null) ExcelMgr = ret as BaseExcelMgr;

            else if (ret is BaseLuaMgr && LuaMgr == null) LuaMgr = ret as BaseLuaMgr;

            else if (ret is BaseTextAssetsMgr && TextAssetsMgr == null) TextAssetsMgr = ret as BaseTextAssetsMgr;

            else if (ret is BaseLanguageMgr && LangMgr == null) LangMgr = ret as BaseLanguageMgr;

            else if (ret is BaseConditionMgr && ACM == null) ACM = ret as BaseConditionMgr;

            else if (ret is BaseInputMgr && InputMgr == null) InputMgr = ret as BaseInputMgr;

            else if (ret is BasePoolMgr && PoolMgr == null) PoolMgr = ret as BasePoolMgr;

            else if (ret is BaseAudioMgr && AudioMgr == null) AudioMgr = ret as BaseAudioMgr;

            //非必要组件
            else if (ret is IBaseAttrMgr && AttrMgr == null) AttrMgr = ret as IBaseAttrMgr;
            else if (ret is IBaseBuffMgr && BuffMgr == null) BuffMgr = ret as IBaseBuffMgr;
            else if (ret is IBaseArticleMgr && ArticleMgr == null) ArticleMgr = ret as IBaseArticleMgr;
            else if (ret is IBasePlotMgr && PlotMgr == null) PlotMgr = ret as IBasePlotMgr;
            else if (ret is IBaseTalkMgr && TalkMgr == null) TalkMgr = ret as IBaseTalkMgr;
            else if (ret is IBaseNarrationMgr && NarrationMgr == null) NarrationMgr = ret as IBaseNarrationMgr;
            else if (ret is IBaseRelationMgr && RelationMgr == null) RelationMgr = ret as IBaseRelationMgr;
            else if (ret is IBaseLogicTurnMgr && LogicTurnMgr == null) LogicTurnMgr = ret as IBaseLogicTurnMgr;
            else if (ret is IBaseBattleMgr && BattleMgr == null) BattleMgr = ret as IBaseBattleMgr;
            else if (ret is IBaseSubBattleMgr && SubBattleMgr == null) SubBattleMgr = ret as IBaseSubBattleMgr;
            else if (ret is BaseUnlockMgr && UnlockMgr == null) UnlockMgr = ret as BaseUnlockMgr;
            else if (ret is BaseNetMgr && NetMgr == null) NetMgr = ret as BaseNetMgr;
            else if (ret is BaseFOWMgr && FOWMgr == null) FOWMgr = ret as BaseFOWMgr;
            else if (ret is BaseLoggerMgr && LoggerMgr == null) LoggerMgr = ret as BaseLoggerMgr;
            else if (ret is BaseAStarMgr && AStarMgr == null) AStarMgr = ret as BaseAStarMgr;
            else if (ret is BaseAStar2DMgr && AStar2DMgr == null) AStar2DMgr = ret as BaseAStar2DMgr;
            else if (ret is BasePerformMgr && PerformMgr == null) PerformMgr = ret as BasePerformMgr;
            else if (ret is BaseRefMgr && RefMgr == null) RefMgr = ret as BaseRefMgr;
            else if (ret is BaseBGMMgr && BGMMgr == null) BGMMgr = ret as BaseBGMMgr;
            else if (ret is BaseConsoleMgr && DevConsoleMgr == null) DevConsoleMgr = ret as BaseConsoleMgr;
            else if (ret is BasePlatSDKMgr && PlatSDKMgr == null) PlatSDKMgr = ret as BasePlatSDKMgr;
            else if (ret is BaseCursorMgr && CursorMgr == null) CursorMgr = ret as BaseCursorMgr;
            else if (ret is BaseDateTimeMgr && DateTimeMgr == null) DateTimeMgr = ret as BaseDateTimeMgr;
            else if (ret is BaseCameraMgr && CameraMgr == null) CameraMgr = ret as BaseCameraMgr;
            else if (ret is BaseCamera2DMgr && Camera2DMgr == null) Camera2DMgr = ret as BaseCamera2DMgr;

            return ret;
        }
        public static List<IBattleClear> ClearWhenUnload { get; private set; } = new List<IBattleClear>();
        public static void AddToClearWhenUnload(IBattleClear clear)
        {
            ClearWhenUnload.Add(clear);
        }
        #endregion

        #region get
        public static Transform GetTransform(Vector3 pos)
        {
            TempTrans.position = pos;
            return TempTrans;
        }
        public static IUnitSpawnMgr GetSpawnMgr(Type unitType)
        {
            if (UnitSpawnMgrs.ContainsKey(unitType))
            {
                return UnitSpawnMgrs[unitType];
            }
            return null;
        }
        public static TUnit GetUnit<TUnit>(long id, bool isLogError = true) where TUnit : BaseUnit
        {
            if (id.IsInv()) return null;
            var ret = GetUnit(id, typeof(TUnit)) as TUnit;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏实体!!!,{0}", id);
            }
            return ret;
        }
        public static TUnit GetUnit<TUnit>(string id, bool isLogError = true) where TUnit : BaseUnit
        {
            if (id.IsInv()) return null;
            var ret = GetUnit(id, typeof(TUnit)) as TUnit;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏实体!!!,{0}", id);
            }
            return ret;
        }
        public static BaseUnit GetUnit(long id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in UnitSpawnMgrs)
                {
                    var temp = item.Value.GetBaseUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (UnitSpawnMgrs.ContainsKey(unitType))
                {
                    return UnitSpawnMgrs[unitType].GetBaseUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得Entity,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static BaseUnit GetUnit(string id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in UnitSpawnMgrs)
                {
                    var temp = item.Value.GetBaseUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (UnitSpawnMgrs.ContainsKey(unitType))
                {
                    return UnitSpawnMgrs[unitType].GetBaseUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得Entity,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static HashList<BaseUnit> GetUnit(List<long> ids)
        {
            HashList<BaseUnit> data = new HashList<BaseUnit>();
            foreach (var item in ids)
            {
                var entity = GetUnit(item);
                if (entity == null) continue;
                data.Add(entity);
            }
            return data;
        }
        public static List<long> GetUnitIDs(HashList<BaseUnit> entity)
        {
            List<long> ids = new List<long>();
            foreach (var item in entity)
            {
                if (item.IsInv()) continue;
                ids.Add(item.ID);
            }
            return ids;
        }
        #endregion

        #region is
        // 是否暂停游戏
        public static bool IsPause => BoolPause.IsIn();
        // 是否处于读取数据阶段
        public static bool IsUnReadData { get; set; } = true;
        public static bool IsHaveCommandLineArg(string arg)=> CommandLineArgs.Contains(arg);
        #endregion
    }
}
