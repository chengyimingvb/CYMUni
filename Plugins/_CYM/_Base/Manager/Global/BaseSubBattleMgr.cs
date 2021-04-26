//------------------------------------------------------------------------------
// BaseSubBattleMgr.cs
// Copyright 2019 2019/12/10 
// Created by CYM on 2019/12/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.DLC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYM
{
    public class BaseSubBattleMgr<TData> : BaseMgr, IBaseSubBattleMgr
        where TData : TDBaseBattleData, new()
    {
        #region state
        public bool IsInBattle => CurData != null;

        public string BattleID { get; private set; } = "";
        public string SceneName { get; private set; } = "";

        public bool IsLoadingBattle { get; private set; } = false;

        public bool IsGameStart { get; private set; } = false;

        public bool IsLoadBattleEnd { get; private set; } = false;
        #endregion

        #region prop
        // 场景的Bundle资源
        protected Asset SceneAsset;
        protected Coroutineter SubBattleCoroutine => BaseGlobal.SubBattleCoroutineter;
        protected Coroutineter BattleCoroutine => BaseGlobal.BattleCoroutineter;
        protected BaseGRMgr GRMgr => BaseGlobal.GRMgr;

        public Scene Scene_Start => SceneManager.GetSceneByName(Const.SCE_Start);
        public Scene Scene_Self => SceneManager.GetSceneByName(SceneName);
        #endregion

        #region Callback Val
        public event Callback Callback_OnBattleLoad;
        public event Callback Callback_OnBattleLoaded;
        public event Callback Callback_OnBattleLoadedScene;
        public event Callback Callback_OnBattleUnLoad;
        public event Callback Callback_OnBattleUnLoaded;
        public event Callback Callback_OnBattleLoadStart;
        public event Callback Callback_OnGameStart;
        public event Callback<string, float> Callback_OnLoadingProgressChanged;
        public event Callback Callback_OnRandTip;
        #endregion

        #region Table
        ITDLuaMgr TDLuaMgr;
        public TData CurData { get; private set; }
        public float DelayLoadSceneTime { get; private set; } = 0.1f;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            Callback_OnBattleLoad += OnSubBattleLoad;
            Callback_OnBattleLoaded += OnSubBattleLoaded;
            Callback_OnBattleLoadedScene += OnSubBattleLoadedScene;
            Callback_OnBattleUnLoad += OnSubBattleUnLoad;
            Callback_OnBattleUnLoaded += OnSubBattleUnLoaded;
            Callback_OnBattleLoadStart += OnSubBattleLoadStart;
            Callback_OnGameStart += OnSubBattleGameStart;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(typeof(TData));
        }
        #endregion

        #region set
        public virtual void Load(string battleID = "")
        {
            if (IsInBattle)
            {
                CLog.Error("正处于SubBattle中");
                return;
            }
            TDBaseBattleData tempData = TDLuaMgr.Get<TData>(battleID);
            if (tempData == null)
            {
                CLog.Error("没有这个战场:{0}", battleID);
                return;
            }
            SubBattleCoroutine.Kill();
            CurData = tempData.Copy<TData>();
            if (CurData != null)
            {
                BattleID = tempData.TDID;
                CurData.OnBeAdded(SelfBaseGlobal);
                BattleCoroutine.Run(_LoadBattle());
            }
            else
            {
                CLog.Error("Battle not found ！error id=" + battleID);
            }
        }
        public virtual void UnLoad()
        {
            if (!IsInBattle)
            {
                CLog.Error("没有加载SubBattle");
                return;
            }
            SubBattleCoroutine.Kill();
            BattleCoroutine.Run(_UnLoadBattle(() =>
            {
                SceneManager.SetActiveScene(BaseGlobal.BattleMgr.SceneSelf);
                BattleCoroutine.Run(BackToBattle());
            }));
        }
        #endregion

        #region enumator
        IEnumerator<float> _RandTip()
        {
            while (IsLoadingBattle)
            {
                Callback_OnRandTip?.Invoke();
                yield return Timing.WaitForSeconds(10.0f);
            }
            yield break;
        }
        // 加载战场
        // readData:是否读取数据
        IEnumerator<float> _LoadBattle()
        {
            yield return Timing.WaitForOneFrame;
            Callback_OnBattleLoadStart?.Invoke();
            float startTime = Time.realtimeSinceStartup;
            IsLoadingBattle = true;
            IsLoadBattleEnd = false;
            IsGameStart = false;
            //开始Tip
            SubBattleCoroutine.Run(_RandTip());
            //开始加载
            Callback_OnBattleLoad?.Invoke();
            yield return Timing.WaitForOneFrame;
            OnLoadSceneStart();
            Callback_OnLoadingProgressChanged?.Invoke("Start to load", 0.0f);
            //演示几秒,给UI渐变的时间
            yield return Timing.WaitForSeconds(DelayLoadSceneTime);
            //加载场景
            SceneName = CurData.GetRawSceneName();
            SceneAsset = GRMgr.LoadScene(SceneName);
            while (!SceneAsset.IsDone)
            {
                yield return Timing.WaitForOneFrame;
                Callback_OnLoadingProgressChanged?.Invoke("LoadingScene", SceneAsset.Progress * 0.8f);
            }
            //延时一帧
            yield return Timing.WaitForOneFrame;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneName));
            Callback_OnBattleLoadedScene?.Invoke();
            //这里必须延迟一帧,等待UI创建,注册事件
            yield return Timing.WaitForOneFrame;
            GC.Collect();
            Callback_OnLoadingProgressChanged?.Invoke("GC", 1.0f);
            IsLoadingBattle = false;
            //场景加载结束
            Callback_OnBattleLoaded?.Invoke();
            IsLoadBattleEnd = true;
            Callback_OnGameStart?.Invoke();
            IsGameStart = true;
        }

        // 卸载战场
        IEnumerator<float> _UnLoadBattle(Callback onDone)
        {
            if (CurData == null) yield break;
            IsLoadingBattle = false;
            IsLoadBattleEnd = false;
            IsGameStart = false;
            CurData.OnBeRemoved();
            CurData = null;
            yield return Timing.WaitForOneFrame;
            Callback_OnBattleUnLoad?.Invoke();
            yield return Timing.WaitForOneFrame;
            Callback_OnLoadingProgressChanged?.Invoke("Start to load", 0.01f);
            //延时一秒.防止UI卡住
            yield return Timing.WaitForSeconds(0.3f);
            var wait = SceneManager.UnloadSceneAsync(SceneName);
            while (!wait.isDone)
            {
                yield return Timing.WaitForOneFrame;
                Callback_OnLoadingProgressChanged?.Invoke("UnloadScene", wait.progress);
            }

            //卸载未使用的资源
            GRMgr.UnloadScene(SceneAsset);

            Callback_OnLoadingProgressChanged?.Invoke("GC", 1.0f);
            BaseGlobal.ResumeGame();
            yield return Timing.WaitForOneFrame;
            Callback_OnBattleUnLoaded?.Invoke();
            onDone?.Invoke();
        }
        protected virtual IEnumerator<float> BackToBattle()
        {
            yield return Timing.WaitForOneFrame;
        }
        #endregion

        #region Callback
        protected virtual void OnLoadSceneStart()
        {
        }
        protected virtual void OnSubBattleLoad()
        {
        }
        protected virtual void OnSubBattleLoaded()
        {
        }
        protected virtual void OnSubBattleLoadedScene()
        {
        }
        protected virtual void OnSubBattleUnLoad()
        {
        }
        protected virtual void OnSubBattleUnLoaded()
        {
        }

        public virtual void OnSubBattleGameStart()
        {

        }

        protected virtual void OnSubBattleLoadStart()
        {

        }
        #endregion
    }
}