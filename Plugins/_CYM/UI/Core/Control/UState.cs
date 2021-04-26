//------------------------------------------------------------------------------
// BaseState.cs
// Copyright 2019 2019/7/25 
// Created by CYM on 2019/7/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM
{
    /// <summary>
    /// None=-1
    /// Yes=0
    /// No=1
    /// </summary>
    public enum StateType:int
    { 
        None=-1,
        Yes=0,
        No=1,
    }
    public class UStateData : UPresenterData
    {
        public List<string> States = new List<string>();
        public Func<int> GetIndex = () => -1;
    }

    //Index:-1为空，0同意，1拒绝，2未知
    [AddComponentMenu("UI/Control/UState")]
    [HideMonoScript]
    public class UState : UPresenter<UStateData>
    {
        #region Inspector
        [SerializeField, Required, SceneObjectsOnly]
        UnityEngine.UI.Image ActiveIcon;
        [SerializeField, SceneObjectsOnly]
        UnityEngine.UI.Image HoverIcon;
        [SerializeField, AssetsOnly]
        List<Sprite> List;
        #endregion

        #region prop
        public int CurIndex { get; private set; } = -1;
        public bool IsChecked => CurIndex == 0;
        #endregion

        #region refresh
        protected override void Start()
        {
            base.Start();
            SetState(CurIndex);
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Data.GetIndex != null)
                SetState(Data.GetIndex());
        }
        #endregion

        #region set
        public void SetState(int index)
        {
            HoverIcon?.CrossFadeAlpha(0.0f, 0.05f, true);
            CurIndex = index;
            if (CurIndex == -1)
            {
                ActiveIcon.CrossFadeAlpha(0.0f, 0.1f, true);
                return;
            }
            else
            {
                if (Data.States == null || Data.States.Count <= CurIndex)
                {
                    if (List == null || CurIndex >= List.Count) return;
                    ActiveIcon.sprite = List[CurIndex];
                }
                else
                {
                    ActiveIcon.sprite = Data.States[CurIndex].GetIcon();
                }
                ActiveIcon.CrossFadeAlpha(1.0f, 0.1f, true);
            }
        }
        #endregion

        #region Callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (HoverIcon != null && CurIndex == -1)
                HoverIcon.CrossFadeAlpha(0.5f, 0.1f, true);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (HoverIcon != null)
                HoverIcon.CrossFadeAlpha(0.0f, 0.1f, true);
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            //如果不是ToggleGroup 则执行以下操作
            if (CheckCanClick() &&
                IsInteractable)
            {
                SetState(IsChecked ? -1 : 0);
            }
            HoverIcon?.CrossFadeAlpha(0.0f, 0.1f, true);
            base.OnPointerClick(eventData);
        }
        #endregion
    }
}