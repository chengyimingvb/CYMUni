//------------------------------------------------------------------------------
// BaseMainCS.cs
// Copyright 2019 2019/4/9 
// Created by CYM on 2019/4/9
// Owner: CYM
// 填写类的描述...
// 外部CSharp执行脚本的基类
//------------------------------------------------------------------------------

namespace CYM
{
    public class BaseMainCS
    {
        #region prop
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        protected IBaseBattleMgr BattleMgr => BaseGlobal.BattleMgr;
        protected BaseLuaMgr LuaMgr => BaseGlobal.LuaMgr;
        protected BaseLoaderMgr LoaderMgr => BaseGlobal.LoaderMgr;
        protected BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        #endregion

        #region life   
        public BaseMainCS()
        {
            BattleMgr.Callback_OnStartNewGame += OnStartNewGame;
            BattleMgr.Callback_OnGameStartOver += OnGameStartOver;
            BattleMgr.Callback_OnBackToStart += OnBackToStart;
            BattleMgr.Callback_OnBattleLoad += OnBattleLoad;
            BattleMgr.Callback_OnBattleLoaded += OnBattleLoaded;
            BattleMgr.Callback_OnBattleLoadedScene += OnBattleLoadedScene;
            BattleMgr.Callback_OnBattleUnLoad += OnBattleUnLoad;
            BattleMgr.Callback_OnBattleUnLoaded += OnBattleUnLoaded;
            BattleMgr.Callback_OnGameStart += OnGameStart;
            BattleMgr.Callback_OnGameStarted += OnGameStarted;
            BattleMgr.Callback_OnLoadingProgressChanged += OnLoadingProgressChanged;
            BattleMgr.Callback_OnStartCustomBattleCoroutine += OnStartCustomBattleCoroutine;
            BattleMgr.Callback_OnEndCustomBattleCoroutine += OnEndCustomBattleCoroutine;

            LoaderMgr.Callback_OnLoadEnd += OnLoadEnd;
            LoaderMgr.Callback_OnAllLoadEnd1 += OnAllLoadEnd;
            LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;

        }
        //主函数,脚本被加载的时候执行
        protected virtual void Main() { }
        #endregion

        #region Callback
        protected virtual void OnStartNewGame() { }
        protected virtual void OnGameStartOver() { }
        protected virtual void OnBackToStart() { }
        protected virtual void OnBattleLoad() { }
        protected virtual void OnBattleLoaded() { }
        protected virtual void OnBattleLoadedScene() { }
        protected virtual void OnBattleUnLoad() { }
        protected virtual void OnBattleUnLoaded() { }
        protected virtual void OnGameStart() { }
        protected virtual void OnGameStarted() { }
        protected virtual void OnLoadingProgressChanged(string info, float val) { }
        protected virtual void OnLoadEnd(LoadEndType type, string info) { }
        protected virtual void OnAllLoadEnd() { }
        protected virtual void OnAllLoadEnd2() { }
        protected virtual void OnStartCustomBattleCoroutine() { }
        protected virtual void OnEndCustomBattleCoroutine() { }
        #endregion

    }
}