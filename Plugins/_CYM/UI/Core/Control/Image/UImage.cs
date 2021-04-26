using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public class UImageData : UPresenterData
    {
        public Func<Color> Color;
        public Func<Sprite> Icon;
        public string IconStr;

        public Func<Sprite> Frame;
        public string FrameStr;
    }
    [AddComponentMenu("UI/Control/UImage")]
    [HideMonoScript]

    [RequireComponent(typeof(Image))]
    public class UImage : UPresenter<UImageData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), Required, SceneObjectsOnly]
        public Image Icon;
        [FoldoutGroup("Inspector"), SceneObjectsOnly]
        public Image Frame;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Data != null)
            {
                if (Icon)
                {
                    if (Data.Color != null)
                    {
                        Icon.color = Data.Color.Invoke();
                    }

                    Sprite sprite = null;
                    if (Data.Icon != null) sprite = Data.Icon.Invoke();
                    else if (!Data.IconStr.IsInv()) sprite = Data.IconStr.GetIcon();
                    Icon.overrideSprite = sprite;
                }
                if (Frame)
                {
                    Sprite sprite = null;
                    if (Data.Frame != null) sprite = Data.Frame.Invoke();
                    else if (!Data.FrameStr.IsInv()) sprite = Data.FrameStr.GetIcon();
                    Frame.overrideSprite = sprite;
                }
            }
        }
        public void Refresh(string icon)
        {
            if (!icon.IsInv() && Icon!=null)
                Icon.overrideSprite = icon.GetIcon();
        }
        public void Refresh(Sprite icon)
        {
            if(Icon) Icon.overrideSprite = icon;
        }
        public void Refresh(string icon,string frame)
        {
            if (Icon != null)
            {
                Icon.overrideSprite = icon.GetIcon();
            }
            if (Frame != null)
            {
                Frame.overrideSprite = frame.GetIcon();
            }
        }
        #endregion

        #region wrap
        public bool IsGrey
        {
            set
            {
                if (value)
                    Icon.material = BaseGlobal.GRMgr.ImageGrey;
                else
                    Icon.material = null;
            }
        }
        public Sprite IconSprite
        {
            get { return Icon.sprite; }
            set { Icon.sprite = value; }
        }
        public Sprite FrameSprite
        {
            get { return Frame.sprite; }
            set { Frame.sprite = value; }
        }
        public Sprite IconOverSprite
        {
            get { return Icon.overrideSprite; }
            set { Icon.overrideSprite = value; }
        }
        public Sprite FrameOverSprite
        {
            get { return Frame.overrideSprite; }
            set { Frame.overrideSprite = value; }
        }
        #endregion

        #region inspector
        public override void AutoSetup()
        {
            base.AutoSetup();
            Icon = GetComponent<Image>();
        }
        #endregion
    }
}
