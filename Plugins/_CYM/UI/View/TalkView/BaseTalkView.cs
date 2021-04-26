﻿//**********************************************
// Class Name	: BaseTooltipView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class BaseTalkView : UUIView
    {
        #region inspector
        public BaseTalkItem LeftItem;
        public BaseTalkItem RightItem;
        public BaseTalkItem MidItem;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            var itemData = new BaseTalkItemData
            {
                Bg = new UImageData { OnClick = OnTalkItemClick },
                KeyTip = new UTextData { Name = GetKeyTip, IsTrans = false },
                SelectTip = new UTextData { Name = GetSelectTip, IsTrans = false },
                Option = new UButtonData { OnClick = OnTalkOptionClick },
                CurSelectOptionIndex = CurSelectOptionIndex,
            };
            LeftItem.Init(itemData);
            RightItem.Init(itemData);
            MidItem.Init(itemData);
        }
        #endregion

        #region set
        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            base.Show(b, useGroup, force);
            if (!b)
            {
                ShowOff();
            }
        }

        protected void ShowOff()
        {
            LeftItem.Show(false);
            RightItem.Show(false);
            MidItem.Show(false);
        }
        protected void Show(TDBaseTalkData talkData, TDBaseTalkFragment fragment)
        {
            ShowOff();
            if (fragment.Type == TalkType.Left)
            {
                LeftItem.Trans.SetAsLastSibling();
                LeftItem.Show(talkData, fragment);
            }
            else if (fragment.Type == TalkType.Right)
            {
                RightItem.Trans.SetAsLastSibling();
                RightItem.Show(talkData, fragment);
            }
            else if (fragment.Type == TalkType.Mid)
            {
                MidItem.Trans.SetAsLastSibling();
                MidItem.Show(talkData, fragment);
            }
        }

        #endregion

        #region is
        public bool IsTypeEnd()
        {
            return LeftItem.IsTypeEnd && RightItem.IsTypeEnd && MidItem.IsTypeEnd;
        }
        #endregion

        #region callback
        protected virtual void OnTalkItemClick(UControl control, PointerEventData p)
        {

        }
        protected virtual void OnTalkOptionClick(UControl control, PointerEventData p)
        {

        }
        protected virtual string GetKeyTip()
        {
            return "Enter";
        }
        protected virtual string GetSelectTip()
        {
            return "Select";
        }
        protected virtual int CurSelectOptionIndex()
        {
            throw new System.NotImplementedException("此函数必须实现");
        }
        #endregion

    }

}