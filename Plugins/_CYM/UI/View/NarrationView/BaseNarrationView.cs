//------------------------------------------------------------------------------
// BaseNarrationView.cs
// Copyright 2018 2018/3/26 
// Created by CYM on 2018/3/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Pool;
using CYM.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class BaseNarrationView : UUIView
    {
        #region inspector
        [SerializeField]
        UText Desc;
        [SerializeField]
        CanvasGroup CanvasGroup;
        [SerializeField]
        UImage Bg;
        [SerializeField]
        UImage Image;
        [SerializeField]
        UText KeyTip;
        #endregion

        #region private
        Tween TweenText;
        Tween TweenAlpha;
        UText CurDesc;
        GOPool DescPool;
        IBaseNarrationMgr NarrationMgr => BaseGlobal.NarrationMgr;
        BaseBGMMgr BGMMgr => BaseGlobal.BGMMgr;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Title.CancleInit();
            DescPool = new GOPool(Desc.gameObject, CanvasGroup.transform);
            Bg.Init(new UImageData { OnClick = OnClickBg });
            KeyTip.Init(new UTextData { Name = GetKeyTip, IsTrans = false });
            Desc.Show(false);

            //event
            NarrationMgr.Callback_OnStartNarration += OnStartNarration;
            NarrationMgr.Callback_OnNextNarration += OnNextNarration;
            NarrationMgr.Callback_OnEndNarration += OnEndNarration;
        }
        public override void OnDestroy()
        {
            //event
            NarrationMgr.Callback_OnStartNarration -= OnStartNarration;
            NarrationMgr.Callback_OnNextNarration -= OnNextNarration;
            NarrationMgr.Callback_OnEndNarration -= OnEndNarration;

            DescPool.Destroy();
            base.OnDestroy();
        }
        #endregion

        #region set
        public virtual void Show(NarrationFragment fragment)
        {
            Show(true);
            if (TweenText != null)
                TweenText.Complete();
            if (TweenAlpha != null)
                TweenAlpha.Kill();

            if (fragment.IsNewPage)
            {
                DescPool.DespawnAll();
            }

            GameObject tempGO = DescPool.Spawn();
            CurDesc = tempGO.GetComponent<UText>();
            CurDesc.RichName.text = "";
            CurDesc.IsAnimation = false;
            CurDesc.Show(true);
            CurDesc.transform.SetAsLastSibling();
            CurDesc.Name.CrossFadeAlpha(0, 0f, true);
            Title.NameText = fragment.GetName();
            if (fragment.IsNewPage)
            {
                CurDesc.RichName.Content = "";
                CanvasGroup.alpha = 0.0f;
                TweenAlpha = DOTween.To(() => CanvasGroup.alpha, (x) => CanvasGroup.alpha = x, 1.0f, 0.3f);
                CurDesc.Name.CrossFadeAlpha(1,0.5f,true);
            }
            else
            {
                string temp = fragment.GetDesc();
                CurDesc.Name.CrossFadeAlpha(1, 0.5f, true);
            }
            CurDesc.RichName.Content = fragment.GetDesc();
            CurDesc.RichName.RefreshRichText();
            Sprite preSprite = Image.Icon.sprite;
            Sprite newSprite = fragment.GetIcon();
            if (newSprite!=null)
                Image.Icon.sprite = newSprite;
            Title.Show(fragment.CurPage == 0);
            Image.Show(true);
        }
        #endregion

        #region Callback
        protected virtual void OnClickBg(UControl presenter, PointerEventData arg3)
        {
            NarrationMgr.Next();
        }
        protected virtual string GetKeyTip()
        {
            return GetStr("Text_单击继续");
        }

        void OnStartNarration(TDBaseNarrationData narration, NarrationFragment fragment)
        {
            Show(fragment);
            BGMMgr.StartBGM(narration.Music);
        }
        void OnNextNarration(TDBaseNarrationData narration, NarrationFragment fragment, int index)
        {
            Show(fragment);
        }
        void OnEndNarration(TDBaseNarrationData narration, NarrationFragment fragment)
        {
            Show(false);
            BGMMgr.Revert();
        }
        #endregion
    }
}