//------------------------------------------------------------------------------
// BasePHIItem.cs
// Copyright 2019 2019/5/16 
// Created by CYM on 2019/5/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BasePHIItemData : UPresenterData
    {
        public Func<TDBasePersonData> Person = null;
    }

    public class BasePHIItem : UPresenter<BasePHIItemData>
    {
        #region inspector
        [FoldoutGroup("Point")]
        public Transform NoRopeHelmetPoint;

        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Bare;
        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Eye;
        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Nose;
        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Hair;
        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Mouse;
        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Brow;
        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Beard;
        [FoldoutGroup("Face")]
        public UnityEngine.UI.Image Decorate;

        [FoldoutGroup("Other")]
        public bool IsShowAgeStr = false;//是否显示年龄字符
        [FoldoutGroup("Other")]
        public UnityEngine.UI.Image BG;
        [FoldoutGroup("Other")]
        public UnityEngine.UI.Image Helmet;
        [FoldoutGroup("Other")]
        public UnityEngine.UI.Image Body;
        [FoldoutGroup("Other")]
        public UnityEngine.UI.Image Frame;
        [FoldoutGroup("Other")]
        public UnityEngine.UI.Image Star;

        [FoldoutGroup("Text")]
        public UnityEngine.UI.Text Name;
        [FoldoutGroup("Text")]
        public UnityEngine.UI.Text Age;
        #endregion

        #region prop
        TDBasePersonData LastPerson;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            LastPerson = Data.Person?.Invoke();
            if (LastPerson != null)
            {
                if (Name) Name.text = LastPerson.GetName();
                if (Age) Age.text = LastPerson.GetAgeStr(IsShowAgeStr);

                if (Bare) Bare.overrideSprite = LastPerson.GetPSprite(PHIPart.PBare);
                if (Eye) Eye.overrideSprite = LastPerson.GetPSprite(PHIPart.PEye);
                if (Nose) Nose.overrideSprite = LastPerson.GetPSprite(PHIPart.PNose);
                if (Hair) Hair.overrideSprite = LastPerson.GetPSprite(PHIPart.PHair);
                if (Mouse) Mouse.overrideSprite = LastPerson.GetPSprite(PHIPart.PMouse);
                if (Brow) Brow.overrideSprite = LastPerson.GetPSprite(PHIPart.PBrow);

                if (BG) BG.overrideSprite = LastPerson.GetPSprite(PHIPart.PBG);
                if (Body) Body.overrideSprite = LastPerson.GetPSprite(PHIPart.PBody);
                if (Frame) Frame.overrideSprite = LastPerson.GetPSprite(PHIPart.PFrame);

                if (Beard)
                {
                    var sprite = LastPerson.GetPSprite(PHIPart.PBeard);
                    Beard.gameObject.SetActive(sprite != null);
                    Beard.overrideSprite = sprite;
                }
                if (Decorate)
                {
                    var sprite = LastPerson.GetPSprite(PHIPart.PDecorate);
                    Decorate.gameObject.SetActive(sprite != null);
                    Decorate.overrideSprite = sprite;
                }
                if (Helmet)
                {
                    var sprite = LastPerson.GetPSprite(PHIPart.PHelmet);
                    Helmet.gameObject.SetActive(sprite != null);
                    Helmet.overrideSprite = sprite;

                    //(Rope)3 and (Normal)5
                    if (LastPerson.IsRopeHelmet()) Helmet.transform.SetParent(Body.transform);
                    else Helmet.transform.SetParent(NoRopeHelmetPoint);
                }

                if (Star)
                {
                    Star.overrideSprite = LastPerson.GetStarIcon();
                }
            }
        }
        #endregion

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
        }
    }
}