//------------------------------------------------------------------------------
// BaseHUDBar.cs
// Copyright 2019 2019/2/8 
// Created by CYM on 2019/2/8
// Owner: CYM
// HUDBar用于血条之类的,实时跟随在物体上的UI
//------------------------------------------------------------------------------

using CYM.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM
{
    public class UHUDBar : UHUDItem
    {
        #region prop
        protected BaseCameraMgr BaseCameraMgr => BaseGlobal.CameraMgr;
        protected BaseFOWMgr BaseFOWMgr => BaseGlobal.FOWMgr;
        #endregion

        #region life
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateAnchoredPosition();
        }
        /// <summary>
        /// 根据单位世界坐标的位置实时更新当前血条的映射点
        /// </summary>
        protected override void UpdateAnchoredPosition()
        {
            if (BaseGlobal.MainCamera == null) return;
            if (IsDestroy) return;
            if (RectTrans == null) return;
            if (CanvasScaler == null) return;
            if (SelfBaseGlobal == null) return;
            if (GetFollowObj() != null)
            {
                if (HideCondition())
                {
                    Show(false);
                }
                else
                {
                    Show(true);
                }

                if (GroupAlpha > 0.0f || IsShow)
                {
                    float offect = (Screen.width / CanvasScaler.referenceResolution.x) * (1 - CanvasScaler.matchWidthOrHeight)
                        + (Screen.height / CanvasScaler.referenceResolution.y) * CanvasScaler.matchWidthOrHeight;
                    Vector2 a = RectTransformUtility.WorldToScreenPoint(BaseGlobal.MainCamera, GetFollowObj().position + GetOffset());
                    Vector2 relationPos = new Vector2(a.x / offect, a.y / offect);
                    Vector3 anchorPos = relationPos;
                    if (RectTrans.localPosition != anchorPos)
                        RectTrans.localPosition = anchorPos;
                }
            }
            else
            {
                Show(false);
            }
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (BaseInputMgr.HoverHUDBar == this && !isShow)
            {
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                OnPointerExit(eventDataCurrentPosition);
            }
        }
        protected virtual bool HideCondition()
        {
            if (SelfUnit == null)
                return true;
            if (!SelfUnit.IsLive)
                return true;
            if (BaseFOWMgr != null)
            {
                if (BaseFOWMgr.IsInFog(SelfUnit.Pos))
                    return true;
            }
            if (!SelfUnit.IsRendered)
                return true;
            if (BaseCameraMgr.IsMoreTopHight)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            BaseInputMgr.SetHoverHUDBar(this);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            BaseInputMgr.SetHoverHUDBar(null);
        }
        #endregion

        #region inspector
        protected override bool Inspector_HideLifeTime()
        {
            return true;
        }
        #endregion
    }
}