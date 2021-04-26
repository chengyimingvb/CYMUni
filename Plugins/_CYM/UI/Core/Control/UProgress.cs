using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public enum ProgressType
    {
        Horizontal,
        Vertical,
    }

    public class UProgressData : UTextData
    {
        public Func<float> Value = () => { return 0.0f; };
        public Func<float, string> ValueText = (x) => { return UIUtil.TwoD(x); };
        public bool IsTween = false;
        public float TweenDuration = 1.0f;
    }
    [AddComponentMenu("UI/Control/UProgress")]
    [HideMonoScript]
    public class UProgress : UPresenter<UProgressData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Text Title;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Text Value;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Image Fill;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public Image Icon;
        #endregion

        #region Data
        [FoldoutGroup("Data"), SerializeField]
        public bool IsFill = true;
        [FoldoutGroup("Data"), SerializeField, ShowIf("Inspector_ShowIsProgressType")]
        public ProgressType ProgressType = ProgressType.Horizontal;
        #endregion

        #region prop
        Tweener tweener;
        float Amount = 1.0f;
        Vector2 SourceForceGroundSizeData = new Vector2();
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
            if (Fill != null)
                SourceForceGroundSizeData = Fill.rectTransform.sizeDelta;
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Data != null)
            {
                float perVal = Data.Value.Invoke();
                SetFillAmount(perVal, Data.IsTween,Data.TweenDuration);
                if (Value != null)
                    Value.text = Data.ValueText.Invoke(FillAmount);
                if (Icon != null)
                    Icon.sprite = Data.GetIcon();
                if (Title != null)
                    Title.text = Data.GetName();
            }
        }
        public virtual void Refresh(float val, string text)
        {
            SetFillAmount(val, false,0);
            if (Value != null)
                Value.text = text;
        }
        public virtual void Refresh(float val)
        {
            SetFillAmount(val, false, 0);
        }
        #endregion

        #region wrap
        public float FillAmount
        {
            get
            {
                if (Fill == null)
                    return 0;
                if (IsFill)
                    return Fill.fillAmount;
                else
                    return Amount;
            }
            set
            {
                if (Fill == null) return;
                Amount = value;
                if (IsFill)
                {
                    Fill.fillAmount = value;
                }
                else
                {
                    if (SourceForceGroundSizeData == Vector2.zero) return;
                    if (ProgressType == ProgressType.Horizontal)
                        Fill.rectTransform.sizeDelta = new Vector2(SourceForceGroundSizeData.x * Amount, SourceForceGroundSizeData.y);
                    else
                        Fill.rectTransform.sizeDelta = new Vector2(SourceForceGroundSizeData.x, SourceForceGroundSizeData.y * Amount);
                }
            }
        }
        public string TitleText
        {
            get { return Title.text; }
            set { Title.text = value; }
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
        #endregion

        #region set
        public void SetFillAmount(float percent, bool isTween = false,float duration=0.1f)
        {
            if (Fill != null)
            {
                if (isTween)
                {
                    if (tweener != null) tweener.Kill();
                    tweener = DOTween.To(() => FillAmount, x => FillAmount = x, percent, duration);
                }
                else
                {
                    FillAmount = percent;
                }
            }
        }
        public void TweenFillAmount(float start,float end, float duration = 0.1f)
        {
            if (Fill != null)
            {
                FillAmount = start;
                SetFillAmount(end,true,duration);
            }
        }
        #endregion

        #region inspector
        bool Inspector_ShowIsProgressType()
        {
            return !IsFill;
        }
        public override void AutoSetup()
        {
            base.AutoSetup();
            if (Fill == null)
                Fill = GetComponentInChildren<Image>();
            if (Value == null)
                Value = GetComponentInChildren<Text>();
        }
        #endregion
    }

}