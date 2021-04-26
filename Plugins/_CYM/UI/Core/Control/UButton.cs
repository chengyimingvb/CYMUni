using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UButtonData : UTextData
    {
        public Func<object, object> OnSorter = (x) => { return x; };
    }
    [AddComponentMenu("UI/Control/UButton")]
    [HideMonoScript]
    public class UButton : UPresenter<UButtonData>
    {
        #region 组建
        [FoldoutGroup("Inspector"),SerializeField, SceneObjectsOnly]
        public Text Text;
        [FoldoutGroup("Inspector"),SerializeField, Tooltip("可以位空"), SceneObjectsOnly]
        public Image Icon;
        [FoldoutGroup("Inspector"),SerializeField, Tooltip("可以位空"), SceneObjectsOnly]
        public Image Bg;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Data != null)
            {
                if (Text != null)
                {
                    var temp = Data.GetName();
                    if (!temp.IsInv()) Text.text = temp;
                }
                if (Icon != null)
                {
                    var temp = Data.GetIcon();
                    if (temp) Icon.overrideSprite = temp;
                }
                if (Bg != null)
                {
                    var temp = Data.GetBg();
                    if (temp) Bg.overrideSprite = temp;
                }
            }
        }
        public void Refresh(string icon)
        {
            if(Icon!=null)
                Icon.overrideSprite = icon.GetIcon();
        }
        #endregion

        #region wrap
        public string NameText
        {
            get { return Text.text; }
            set { Text.text = value; }
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
        #endregion

        #region editor
        public override void AutoSetup()
        {
            base.AutoSetup();
            if (Text == null)
                Text = GetComponentInChildren<Text>();
            if (Icon == null)
                Icon = GetComponentInChildren<Image>();
        }

        public static implicit operator Callback<object, object>(UButton v)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
