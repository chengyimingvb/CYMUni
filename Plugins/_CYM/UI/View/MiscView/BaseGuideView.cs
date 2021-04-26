//------------------------------------------------------------------------------
// GuideView.cs
// Created by CYM on 2021/3/22
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM.UI
{
    public class BaseGuideView :BaseStaticUIView<BaseGuideView>
    {
        #region inspector
        [SerializeField]
        UICircleFocusing UICircle;
        [SerializeField]
        UIRectFocusing UIRect;
        [SerializeField]
        UPointer UIPointer;
        #endregion

        #region prop
        public bool IsMaskOnce { get; private set; }
        public UControl MaskedControl { get; private set; }
        public UControl PointedControl { get; private set; }
        #endregion

        #region life
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (IsShow)
            {
                if (UIPointer != null)
                {
                    if (PointedControl != null)
                    {
                        var view = PointedControl.PUIView;
                        var panel = PointedControl.PPanel;
                        var container = PointedControl.PContainer;
                        if (container != null)
                        {
                            UIPointer.ShowDirect(container.IsShow);
                        }
                        else if (panel != null)
                        {
                            UIPointer.ShowDirect(panel.IsShow);
                        }
                        else if (view != null)
                        {
                            UIPointer.ShowDirect(view.IsShow);
                        }
                    }
                }
            }
        }
        #endregion

        #region is       
        public bool IsInMask
        {
            get {
                if (!IsShow)
                    return false;
                if (UICircle && UICircle.gameObject.activeSelf)
                    return true;
                if (UIRect && UIRect.gameObject.activeSelf)
                    return true;
                return false;
            }
        }
        #endregion

        #region set
        public void CloseMask()
        {
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(false);
            UIPointer?.Close();
            MaskedControl = null;
            PointedControl = null;
        }
        public void CloseWhenMaskOnce()
        {
            if (IsMaskOnce)
                CloseMask();
        }
        #endregion

        #region pointer
        public void ShowPointer(UControl control,float rot=90, bool maskOnce = true)
        {
            if (control == null)
                return;
            if (control.RectTrans == null)
                return;
            Show(true);
            UIPointer?.Show(true);
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(false);
            if (UIPointer)
            {
                UIPointer.SetPosAndRot(RectTrans.InverseTransformPoint(control.RectTrans.position),new Vector3(0,0, rot));
                IsMaskOnce = maskOnce;
                PointedControl = control;
                if (maskOnce)
                    control.UIMgr.AddIgnoreBlockClickOnce(control);
            }
        }
        #endregion

        #region circle
        public void ShowCircle(UControl control, bool ignoreOnce = true)
        {
            ShowCircle(control, 1, ignoreOnce);
        }
        public void ShowCircle(UControl control, float scale = 1, bool ignoreOnce = true)
        {
            if (control == null)
                return;
            RectTransform rectTrans = control.RectTrans;
            float maxVal = Mathf.Max(rectTrans.sizeDelta.x, rectTrans.sizeDelta.y);
            ShowRawCircle(control, maxVal / 2 * scale, ignoreOnce);
        }
        public void ShowRawCircle(UControl control, float radius, bool ignoreOnce = true)
        {
            if (control == null)
                return;
            if (control.RectTrans == null)
                return;
            if (!control.PUIView.IsShow)
            {
                CLog.Error("错误！，控件的父节点UIView没有显示");
                return;
            }
            Show(true);
            UIPointer?.Show(false);
            UIRect?.gameObject.SetActive(false);
            UICircle?.gameObject.SetActive(true);
            UICircle?.SetCenter(RectTrans.InverseTransformPoint(control.RectTrans.position));
            UICircle?.SetRadius(radius);
            IsMaskOnce = ignoreOnce;
            MaskedControl = control;
            if (ignoreOnce)
                control.UIMgr.AddIgnoreBlockClickOnce(control);
        }
        #endregion

        #region rect
        public void ShowRect(UControl control, bool ignoreOnce = true)
        {
            ShowRect(control, 1, ignoreOnce);
        }
        public void ShowRect(UControl control, float scale = 1, bool ignoreOnce = true)
        {
            if (control == null)
                return;
            RectTransform rectTrans = control.RectTrans;
            ShowRawRect(control, rectTrans.sizeDelta.x / 2 * scale, rectTrans.sizeDelta.y / 2 * scale, ignoreOnce);
        }
        public void ShowRawRect(UControl control, float x, float y, bool ignoreOnce = true)
        {
            if (control == null)
                return;
            if (control.RectTrans == null)
                return;
            if (!control.IsActiveSelf)
                return;
            if (!control.PUIView.IsShow)
            {
                CLog.Error("错误！，控件的父节点UIView没有显示");
                return;
            }
            Show(true);
            UIPointer?.Show(false);
            UICircle?.gameObject.SetActive(false);
            UIRect?.gameObject.SetActive(true);
            UIRect?.SetCenter(RectTrans.InverseTransformPoint(control.RectTrans.position));
            UIRect?.SetRect(x, y);
            IsMaskOnce = ignoreOnce;
            MaskedControl = control;
            if (ignoreOnce)
                control.UIMgr.AddIgnoreBlockClickOnce(control);
        }
        #endregion

    }
}