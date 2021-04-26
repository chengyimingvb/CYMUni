//------------------------------------------------------------------------------
// BaseLogoMgr.cs
// Copyright 2018 2018/11/12 
// Created by CYM on 2018/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM
{
    public class BaseLogoMgr : BaseGFlowMgr, ILoader
    {
        Tweener tweener;
        UIConfig LogoConfig => UIConfig.Ins;
        List<LogoData> Logos => UIConfig.Ins.Logos;
        Image LogoImage => BaseLogoPlayer.Logo;
        UVideo LogoVideo => BaseLogoPlayer?.Video;
        public bool IsShowedLogo { get; private set; } = false;

        LogoPlayer BaseLogoPlayer;
        CanvasGroup CanvasGroup;

        #region life
        protected override string ResourcePrefab => "BaseLogoPlayer";
        protected override void OnStartLoad()
        {
            base.OnStartLoad();
            BaseLogoPlayer = ResourceObj.GetComponent<LogoPlayer>();
            CanvasGroup = BaseLogoPlayer.GetComponent<CanvasGroup>();
        }
        protected override void OnAllLoadEnd1()
        {
            base.OnAllLoadEnd1();
            DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, 0.0f, 1.0f).OnComplete(OnTweenEnd);
        }

        private void OnTweenEnd()
        {
            GameObject.Destroy(BaseLogoPlayer.gameObject);
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (LogoVideo != null && LogoVideo.IsPlaying())
            {
                if (Input.anyKeyDown)
                {
                    LogoVideo.Stop();
                }
            }
        }
        #endregion

        #region loader
        public string GetLoadInfo()
        {
            return "Show Logo";
        }

        public IEnumerator Load()
        {
            bool IsNoLogo = Logos == null || Logos.Count == 0;
            if (LogoConfig.IsEditorMode() && !IsNoLogo)
            {
                while (BaseLogoPlayer == null) yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(0.1f);
                for (int i = 0; i < Logos.Count; ++i)
                {
                    if (tweener != null) tweener.Kill();
                    if (Logos[i].IsImage())
                    {
                        LogoImage.color = new Color(1, 1, 1, 0);
                        tweener = DOTween.ToAlpha(() => LogoImage.color, x => LogoImage.color = x, 1.0f, Logos[i].InTime);
                        LogoImage.sprite = Logos[i].Logo;
                        LogoImage.SetNativeSize();
                        yield return new WaitForSeconds(Logos[i].WaitTime);
                        if (tweener != null) tweener.Kill();
                        tweener = DOTween.ToAlpha(() => LogoImage.color, x => LogoImage.color = x, 0.0f, Logos[i].OutTime);
                    }
                    else if (Logos[i].IsVideo())
                    {
                        LogoVideo.Play(Logos[i].Video);
                        while (LogoVideo.IsPreparing)
                            yield return new WaitForEndOfFrame();
                        LogoVideo.Show();
                        while (LogoVideo.IsPlaying())
                            yield return new WaitForEndOfFrame();
                        if (!IsNextVideo(i))
                            LogoVideo.Close();
                    }

                    if (i < Logos.Count - 1)
                        yield return new WaitForSeconds(Logos[i].OutTime);
                }
                yield return new WaitForSeconds(0.5f);
            }
            IsShowedLogo = true;

            bool IsNextVideo(int i)
            {
                var index = i + 1;
                if (index < Logos.Count - 1)
                {
                    return Logos[index].IsVideo();
                }
                return false;
            }
        }
        #endregion
    }
}