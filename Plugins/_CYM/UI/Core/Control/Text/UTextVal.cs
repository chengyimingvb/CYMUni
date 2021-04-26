//------------------------------------------------------------------------------
// TextValItem.cs
// Copyright 2019 2019/6/14 
// Created by CYM on 2019/6/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UTextValData : UTextData
    {
        public Func<string> Value;
        public Func<string> Adt;
    }
    [AddComponentMenu("UI/Control/UTextVal")]
    [HideMonoScript]
    public class UTextVal : UPresenter<UTextValData>
    {
        #region Inspector
        [FoldoutGroup("Inspector"), SerializeField, Required, SceneObjectsOnly]
        public Text Title;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Text Adt;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Text Value;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Image Icon;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Image Bg;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Title != null)
            {
                if (Data.Name != null)
                    Title.text = Data.Name.Invoke();
                else if (!Data.NameKey.IsInv())
                {
                    if (Data.IsTrans)
                        Title.text = Data.NameKey.GetName();
                    else
                        Title.text = Data.NameKey;
                }
            }

            if (Value != null)
            {
                if (Data.Value != null) 
                    Value.text = Data.Value.Invoke();
            }

            if (Adt != null)
            {
                if (Data.Adt != null) 
                    Adt.text = Data.Adt.Invoke();
            }

            if (Icon != null)
            {
                if (Data.Icon != null) 
                    Icon.overrideSprite = Data.Icon.Invoke();
                else if (!Data.IconStr.IsInv()) 
                    Icon.overrideSprite = Data.IconStr.GetIcon();
            }

            if (Bg != null)
            {
                Bg.overrideSprite = Data.GetIcon();
            }
        }
        #endregion

        #region wrap
        public string TitleText
        {
            get { return Title.text; }
            set { Title.text = value; }
        }
        public string AdtText
        {
            get { return Adt.text; }
            set { Adt.text = value; }
        }
        public string ValueText
        {
            get { return Value.text; }
            set { Value.text = value; }
        }
        public Sprite IconSprite
        {
            get { return Icon.sprite; }
            set { Icon.sprite = value; }
        }
        public Sprite BgSprite
        {
            get { return Bg.sprite; }
            set { Bg.sprite = value; }
        }
        public Color IconColor
        {
            get { return Icon.color; }
            set { Icon.color = value; }
        }
        public Color BgColor
        {
            get { return Bg.color; }
            set { Bg.color = value; }
        }
        #endregion
    }
}