using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UTextData : UPresenterData
    {
        public bool IsTrans = true;

        #region 函数
        public Func<Sprite> Icon = null;
        public Func<Sprite> Bg = null;
        public Func<string> Name = null;
        #endregion

        #region 简化设置
        public string DefaultStr = Const.STR_Inv;
        public string BgStr = Const.STR_Inv;
        public string NameKey = Const.STR_Inv;
        public string IconStr = Const.STR_Inv; //如果没有设置,默认会使用NameKey
        #endregion

        #region get
        public string GetName()
        {
            string dynStr = Const.STR_Inv;
            string staStr = NameKey;
            if (Name != null)
            {
                dynStr = Name.Invoke();
            }
            if (staStr.IsInv() && dynStr.IsInv())
            {
                return DefaultStr;
            }
            if (!dynStr.IsInv())
            {
                return dynStr;
            }
            else return GetTransStr(staStr);
        }
        public Sprite GetIcon()
        {
            if (Icon != null)
            {
                return Icon.Invoke();
            }
            if (!IconStr.IsInv())
                return BaseGRMgr.Icon.Get(IconStr);
            else if (!NameKey.IsInv() && BaseGRMgr.Icon.IsHave(NameKey))
                return BaseGRMgr.Icon.Get(NameKey);
            return null;
        }
        public Sprite GetBg()
        {
            if (Bg != null)
            {
                return Bg.Invoke();
            }
            if (!BgStr.IsInv())
                return BaseGRMgr.Icon.Get(BgStr);
            return null;
        }
        public string GetTransStr(string str)
        {
            if (IsTrans)
                return BaseLanguageMgr.Get(str);
            return str;
        }
        #endregion
    }
    [AddComponentMenu("UI/Control/UText")]
    [HideMonoScript]
    public class UText : UPresenter<UTextData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Text Name;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Image Icon;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Image Bg;
        [FoldoutGroup("Data"),SerializeField]
        bool IsAnim = false;
        #endregion

        #region prop
        Tween Tween;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
            if(Name is RichText)
                RichName = (RichText)Name;
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Name != null)
            {
                NameText = Data.GetName();
            }
            if (Icon != null)
            {
                Icon.sprite = Data.GetIcon();
            }
            if (Bg != null)
            {
                Bg.sprite = Data.GetIcon();
            }
        }
        public void Refresh(string key, params object[] objs)
        {
            if (Name) Name.text = BaseLanguageMgr.Get(key, objs);
        }
        public void Refresh(string desc)
        {
            if (Name) Name.text = desc;
        }
        #endregion

        #region wrap
        public string NameText
        {
            get { return Name.text; }
            set
            {
                if (IsAnim)
                {
                    if (Tween != null)
                        Tween.Kill();
                    Tween = DOTween.To(() => Name.text, (x) => Name.text = x, value, 0.3f);
                }
                else
                {
                    Name.text = value;
                }
            }
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
            get { if (Icon == null) return Color.white; return Icon.color; }
            set { if (Icon == null) return; Icon.color = value; }
        }
        public Color BgColor
        {
            get { if (Bg == null) return Color.white; return Bg.color; }
            set { if (Bg == null) return; Bg.color = value; }
        }
        public bool IsAnimation
        {
            get { return IsAnim; }
            set { IsAnim = value; }
        }
        public RichText RichName
        {
            get; private set;
        }
        #endregion

        #region inspector
        public override void AutoSetup()
        {
            base.AutoSetup();
            if(Name==null)
                Name = GetComponent<Text>();
            if (Icon == null)
                Icon = GetComponentInChildren<Image>();
        }
        #endregion
    }
}
