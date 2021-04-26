//------------------------------------------------------------------------------
// BaseGlobalCoreMgr.cs
// Copyright 2018 2018/4/24 
// Created by CYM on 2018/4/24
// Owner: CYM
// 好汉游戏流程触发事件的全局管理器
//------------------------------------------------------------------------------

using UnityEngine;

namespace CYM
{
    public class BaseGFlowMgr : BaseMgr
    {
        #region prop
        protected IBaseBattleMgr BattleMgr => BaseGlobal.BattleMgr;
        protected IBaseSubBattleMgr SubBattleMgr => BaseGlobal.SubBattleMgr;
        protected BaseLuaMgr LuaMgr => BaseGlobal.LuaMgr;
        protected BaseLoaderMgr LoaderMgr => BaseGlobal.LoaderMgr;
        protected BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        protected BasePerformMgr PerfomMgr => BaseGlobal.PerformMgr;
        protected GameObject ResourceObj;
        protected GameObject SystemObj;
        #endregion

        #region life
        //适用于游戏启动后创建的对象(放在Resources目录下)
        protected virtual string ResourcePrefab => Const.STR_Inv;
        //适用于全部加载完成后创建的系统对象(放在Bundle/System目录下)
        protected virtual string SystemPrefab => Const.STR_Inv;
        public override MgrType MgrType => MgrType.Global;
        public override void OnCreate()
        {
            base.OnCreate();
            if (!ResourcePrefab.IsInv())
                ResourceObj = GRMgr.GetResources(ResourcePrefab, true);
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnBattleLoadStart += OnBattleLoadStart;
                BattleMgr.Callback_OnStartNewGame += OnStartNewGame;
                BattleMgr.Callback_OnBackToStart += OnBackToStart;
                BattleMgr.Callback_OnBattleLoad += OnBattleLoad;
                BattleMgr.Callback_OnBattleLoaded += OnBattleLoaded;
                BattleMgr.Callback_OnBattleLoadedScene += OnBattleLoadedScene;
                BattleMgr.Callback_OnBattleReadDataEnd += OnBattleReadDataEnd;
                BattleMgr.Callback_OnBattleUnLoad += OnBattleUnLoad;
                BattleMgr.Callback_OnBattleUnLoaded += OnBattleUnLoaded;
                BattleMgr.Callback_OnLoadingProgressChanged += OnLoadingProgressChanged;
                BattleMgr.Callback_OnStartCustomBattleCoroutine += OnStartCustomBattleCoroutine;
                BattleMgr.Callback_OnEndCustomBattleCoroutine += OnEndCustomBattleCoroutine;
            }

            if (SubBattleMgr != null)
            {
                SubBattleMgr.Callback_OnBattleLoad += OnSubBattleLoad;
                SubBattleMgr.Callback_OnBattleLoaded += OnSubBattleLoaded;
                SubBattleMgr.Callback_OnBattleUnLoad += OnSubBattleUnLoad;
                SubBattleMgr.Callback_OnBattleUnLoaded += OnSubBattleUnLoaded;
                SubBattleMgr.Callback_OnBattleLoadedScene += OnSubBattleLoadedScene;
            }

            if (LoaderMgr != null)
            {
                LoaderMgr.Callback_OnStartLoad += OnStartLoad;
                LoaderMgr.Callback_OnLoadEnd += OnLoadEnd;
                LoaderMgr.Callback_OnAllLoadEnd1 += OnAllLoadEnd1;
                LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            }

            if (LuaMgr != null)
            {
                LuaMgr.Callback_OnParseStart += OnLuaParseStart;
                LuaMgr.Callback_OnParseEnd += OnLuaParseEnd;
            }
        }

        public override void OnDisable()
        {
            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnBattleLoadStart -= OnBattleLoadStart;
                BattleMgr.Callback_OnStartNewGame -= OnStartNewGame;
                BattleMgr.Callback_OnBackToStart -= OnBackToStart;
                BattleMgr.Callback_OnBattleLoad -= OnBattleLoad;
                BattleMgr.Callback_OnBattleLoaded -= OnBattleLoaded;
                BattleMgr.Callback_OnBattleLoadedScene -= OnBattleLoadedScene;
                BattleMgr.Callback_OnBattleReadDataEnd -= OnBattleReadDataEnd;
                BattleMgr.Callback_OnBattleUnLoad -= OnBattleUnLoad;
                BattleMgr.Callback_OnBattleUnLoaded -= OnBattleUnLoaded;
                BattleMgr.Callback_OnLoadingProgressChanged -= OnLoadingProgressChanged;
                BattleMgr.Callback_OnStartCustomBattleCoroutine -= OnStartCustomBattleCoroutine;
                BattleMgr.Callback_OnEndCustomBattleCoroutine -= OnEndCustomBattleCoroutine;
            }

            if (SubBattleMgr != null)
            {
                SubBattleMgr.Callback_OnBattleLoad -= OnSubBattleLoad;
                SubBattleMgr.Callback_OnBattleLoaded -= OnSubBattleLoaded;
                SubBattleMgr.Callback_OnBattleUnLoad -= OnSubBattleUnLoad;
                SubBattleMgr.Callback_OnBattleUnLoaded -= OnSubBattleUnLoaded;
                SubBattleMgr.Callback_OnBattleLoadedScene -= OnSubBattleLoadedScene;
            }

            if (LoaderMgr != null)
            {
                LoaderMgr.Callback_OnStartLoad -= OnStartLoad;
                LoaderMgr.Callback_OnLoadEnd -= OnLoadEnd;
                LoaderMgr.Callback_OnAllLoadEnd1 -= OnAllLoadEnd1;
                LoaderMgr.Callback_OnAllLoadEnd2 -= OnAllLoadEnd2;
            }

            if (LuaMgr != null)
            {
                LuaMgr.Callback_OnParseStart -= OnLuaParseStart;
                LuaMgr.Callback_OnParseEnd -= OnLuaParseEnd;
            }
            base.OnDisable();
        }
        #endregion

        #region Sub Battle
        protected virtual void OnSubBattleUnLoad()
        {

        }

        protected virtual void OnSubBattleLoad()
        {

        }
        protected virtual void OnSubBattleUnLoaded()
        {

        }

        protected virtual void OnSubBattleLoaded()
        {

        }
        protected virtual void OnSubBattleLoadedScene()
        {
        }
        #endregion

        #region Callback
        protected virtual void OnBattleLoadStart()
        {
        }
        protected virtual void OnStartNewGame() { }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
        }
        protected virtual void OnBackToStart() { }
        protected virtual void OnBattleLoad() { }
        protected virtual void OnBattleLoaded() { }
        protected virtual void OnBattleLoadedScene() { }
        protected virtual void OnBattleReadDataEnd() { }
        protected virtual void OnBattleUnLoad() { }
        protected virtual void OnBattleUnLoaded() { }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
        }
        protected virtual void OnLoadingProgressChanged(string info, float val) { }
        protected virtual void OnStartLoad() { }
        protected virtual void OnLoadEnd(LoadEndType type, string info) { }
        protected virtual void OnAllLoadEnd1() { }
        protected virtual void OnAllLoadEnd2()
        {
            if (!SystemPrefab.IsInv())
                SystemObj = GRMgr.System.Get(SystemPrefab, true);
        }
        protected virtual void OnLuaParseStart() { }
        protected virtual void OnLuaParseEnd() { }
        protected virtual void OnStartCustomBattleCoroutine() { }
        protected virtual void OnEndCustomBattleCoroutine() { }
        #endregion
    }
}