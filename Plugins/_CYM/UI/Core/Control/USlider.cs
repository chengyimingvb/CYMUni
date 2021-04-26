using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class USliderData : UButtonData
    {
        public Callback<float> OnValueChanged;
        public Func<float, string> ValueText = (x) => { return UIUtil.Per(x); };
        public Func<float> Value = () => 0;
        public float MaxVal = 1.0f;
        public float MinVal = 0.0f;
        public string WVCClip = "UI_WVSlider";
    }
    [AddComponentMenu("UI/Control/USlider")]
    [HideMonoScript]
    public class USlider : UPresenter<USliderData>
    {
        #region presenter
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        RectTransform Handle;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        RectTransform Fill;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        Text Name;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        Text Value;
        #endregion

        #region prop
        Slider Com;
        bool isInRefresh = false;
        RectTransform[] childRectTrans;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
            childRectTrans = GetComponentsInChildren<RectTransform>();
            Com = GO.SafeAddComponet<Slider>();
            Com.handleRect = Handle;
            Com.fillRect = Fill;
        }
        public override void Init(USliderData data)
        {
            base.Init(data);
            if (Com != null)
            {
                isInRefresh = true;
                Com.onValueChanged.AddListener(this.OnValueChanged);
                Com.maxValue = data.MaxVal;
                Com.minValue = data.MinVal;
                Com.value = data.Value.Invoke();
                isInRefresh = false;
            }
        }
        public override void Cleanup()
        {
            if (Com != null)
                Com.onValueChanged.RemoveAllListeners();
            base.Cleanup();
        }
        public override void Refresh()
        {
            isInRefresh = true;
            base.Refresh();
            if (Com != null)
            {
                Com.maxValue = Data.MaxVal;
                Com.minValue = Data.MinVal;
                Com.value = Data.Value.Invoke();
            }
            if (Name != null)
                Name.text = Data.GetName();
            RefreshValueChange();
            isInRefresh = false;
        }
        #endregion

        #region set
        void RefreshValueChange()
        {
            if (Value != null)
            {
                Value.text = Data.ValueText.Invoke(Com.value);
            }
        }
        #endregion

        #region callback
        void OnValueChanged(float value)
        {
            Data.OnValueChanged?.Invoke(value);
            if (isInRefresh)
                return;
            RefreshValueChange();
            if (Com.wholeNumbers)
                PlayClip(Data.WVCClip);
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            foreach (var item in childRectTrans)
            {
                if (item == Com.handleRect)
                    base.OnPointerClick(eventData);
            }
        }
        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            Com.interactable = b;
        }
        #endregion
    }

}