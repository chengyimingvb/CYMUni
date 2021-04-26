//------------------------------------------------------------------------------
// BaseLoadingView.cs
// Copyright 2020 2020/1/16 
// Created by CYM on 2020/1/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class BaseLoadingView : UUIView
    {
        #region presenter
        [SerializeField]
        List<Sprite> LoadingSprities;
        [SerializeField]
        public URawImage RawBG;
        [SerializeField]
        public UImage BG;
        [SerializeField]
        public UProgress Loading;
        [SerializeField]
        public UText LoadEndTip;
        [SerializeField]
        public UText Tip;
        [SerializeField]
        public UText EA_Desc;
        [SerializeField]
        public UImage Logo;
        #endregion

        #region mgr
        protected IBaseBattleMgr BattleMgr => BaseGlobal.BattleMgr;
        protected IBaseSubBattleMgr SubBattleMgr => BaseGlobal.SubBattleMgr;
        #endregion

        #region prop
        UITweenColor UITweenColor;
        BaseInputMgr InputMgr => BaseGlobal.InputMgr;
        #endregion

        #region life
        protected Sprite GetLogo() => null;
        protected override void OnCreatedView()
        {
            base.OnCreatedView();

            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnLoadingProgressChanged += OnLoadingProgressChanged;
                BattleMgr.Callback_OnRandTip += OnRandTip;
                BattleMgr.Callback_OnBattleUnLoad += OnBattleUnLoad;
                BattleMgr.Callback_OnBattleUnLoaded += OnBattleUnLoaded;
                BattleMgr.Callback_OnBattleLoadStart += OnLoadStart;
                BattleMgr.Callback_OnInPauseLoadingView += OnInPauseLoadingView;
            }

            if (SubBattleMgr != null)
            {
                SubBattleMgr.Callback_OnLoadingProgressChanged += OnLoadingProgressChanged;
                SubBattleMgr.Callback_OnRandTip += OnRandTip;
                SubBattleMgr.Callback_OnBattleUnLoad += OnBattleUnLoad;
                SubBattleMgr.Callback_OnBattleUnLoaded += OnBattleUnLoaded;
                SubBattleMgr.Callback_OnGameStart += OnSubBattleGameStart;
                SubBattleMgr.Callback_OnBattleLoadStart += OnLoadStart;
            }

            InputMgr.Callback_OnAnyKeyDown += OnAnyKeyDown;
            BG?.Init(new UImageData { OnClick = OnClickBG });
            if(LoadEndTip!=null)
                UITweenColor = LoadEndTip.GetComponent<UITweenColor>();

        }

        public override void DoDestroy()
        {
            if (BattleMgr != null)
            {
                BattleMgr.Callback_OnLoadingProgressChanged -= OnLoadingProgressChanged;
                BattleMgr.Callback_OnRandTip -= OnRandTip;
                BattleMgr.Callback_OnBattleUnLoad -= OnBattleUnLoad;
                BattleMgr.Callback_OnBattleUnLoaded -= OnBattleUnLoaded;
                BattleMgr.Callback_OnBattleLoadStart -= OnLoadStart;
                BattleMgr.Callback_OnInPauseLoadingView -= OnInPauseLoadingView;
            }

            if (SubBattleMgr != null)
            {
                SubBattleMgr.Callback_OnLoadingProgressChanged -= OnLoadingProgressChanged;
                SubBattleMgr.Callback_OnRandTip -= OnRandTip;
                SubBattleMgr.Callback_OnBattleUnLoad -= OnBattleUnLoad;
                SubBattleMgr.Callback_OnBattleUnLoaded -= OnBattleUnLoaded;
                SubBattleMgr.Callback_OnGameStart -= OnSubBattleGameStart;
                SubBattleMgr.Callback_OnBattleLoadStart -= OnLoadStart;
            }

            InputMgr.Callback_OnAnyKeyDown -= OnAnyKeyDown;
            base.DoDestroy();
        }

        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            base.Show(b, useGroup, force);
            if (b)
            {
                LoadEndTip?.Show(false);
                if (Loading)
                {
                    Loading.Show(true);
                    Loading.Value.supportRichText = true;
                    Loading.Refresh(0.0f, Util.GetStr("StartToLoad"));
                }
                if (Logo)
                {
                    Logo.Refresh(GetLogo());
                }
                OnRandTip();
                OnRandBG();
            }
        }
        #endregion

        #region Callback
        void OnLoadingProgressChanged(string str, float val)
        {
            Loading?.Refresh(val, string.Format("{0} {1}", str, UIUtil.PerC(val)));
        }
        private void OnRandTip()
        {
            Tip?.Refresh("", BaseLanguageMgr.RandLoadTip());
        }
        private void OnRandBG()
        {
            if (LoadingSprities != null && LoadingSprities.Count > 0)
                RawBG.Refresh(LoadingSprities.Rand());
        }
        private void OnInPauseLoadingView()
        {
            Loading?.Show(false);
            LoadEndTip?.Show(true);
            LoadEndTip?.Refresh(GetStr("Text_游戏加载完成"));
            UITweenColor?.DoTween();
        }
        private void OnSubBattleGameStart()
        {
            Show(false);
        }
        public override void OnCloseLoadingView()
        {
            base.OnCloseLoadingView();
            Show(false);
        }
        private void OnClickBG(UControl arg1, PointerEventData arg2)
        {
            ManualCloseLopTip();
        }
        private void OnAnyKeyDown()
        {
            ManualCloseLopTip();
        }
        void ManualCloseLopTip()
        {
            if (!BattleMgr.IsInBattle) return;
            if (!BattleMgr.IsLoadBattleEnd) return;
            if (!BattleMgr.IsInPauseLoadingView) return;
            BattleMgr.UnPauseLoadingView();
            Show(false);
        }
        #endregion

        #region Battle load
        private void OnLoadStart()
        {
            Show(true);
        }
        void OnBattleUnLoad()
        {
            Show(true);
        }
        void OnBattleUnLoaded()
        {
            Show(false);
        }
        #endregion
    }
}