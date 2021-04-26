//------------------------------------------------------------------------------
// PresenterColorTransition.cs
// Copyright 2019 2019/4/7 
// Created by CYM on 2019/4/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    [System.Serializable]
    public class UIColorTransition : UITransition
    {
        #region Inspector
        [ValueDropdown("Inspector_StatePresets")]
        public string StateColorPreset = Const.STR_Custom;
        [HideIf("Inspector_IsHideStateColor")]
        public PresenterStateColor StateColor = new PresenterStateColor();
        #endregion

        #region prop
        private TweenerCore<Color, Color, ColorOptions> colorTween;
        #endregion

        #region LIFE
        public override void Init(UControl self)
        {
            base.Init(self);
            if (!StateColorPreset.IsInv() &&
                 StateColorPreset != Const.STR_Custom)
                StateColor = UIConfig.Ins.GetStateColor(StateColorPreset);
            if (Text != null)
            {
                Text.color = StateColor.Normal;
            }
            else if (Image != null)
            {
                Image.color = StateColor.Normal;
            }
            else if (Graphic != null)
                Graphic.color = Color.white;
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Enter, Duration).SetDelay(Delay);
            else if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Enter, Duration).SetDelay(Delay);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Press, Duration).SetDelay(Delay);
            if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Press, Duration).SetDelay(Delay);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            if (Image != null)
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {
                if (b)
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Disable, Duration).SetDelay(Delay);
            }
            else if (Image != null)
            {
                if (b)
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Disable, Duration).SetDelay(Delay);
            }
            else
            {
                if (b)
                    Graphic.CrossFadeColor(StateColor.Normal, Duration, true, true);
                else
                    Graphic.CrossFadeColor(StateColor.Disable, Duration, true, true);
            }
        }
        public override void OnSelected(bool b)
        {
            if (!IsInteractable) return;
            base.OnSelected(b);
            if (Graphic == null) return;
            if (colorTween != null) colorTween.Kill();
            if (Text != null)
            {

                if (b)
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Selected, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            else if (Image != null)
            {

                if (b)
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Selected, Duration).SetDelay(Delay);
                else
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            else
            {
                if (b)
                    Graphic.CrossFadeColor(StateColor.Selected, Duration, true, true);
                else
                    Graphic.CrossFadeColor(StateColor.Normal, Duration, true, true);
            }
        }
        #endregion

        #region inspector editor
        protected override bool Inspector_IsHideStateColor()
        {
            return !StateColorPreset.IsInv() && StateColorPreset != Const.STR_Custom;
        }
        protected string[] Inspector_StatePresets()
        {
            return new List<string>(UIConfig.Ins.PresenterStateColors.Keys).ToArray();
        }
        #endregion
    }
}