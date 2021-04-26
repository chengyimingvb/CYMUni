using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UInputFieldData : UPresenterData
    {
        public Callback<string> OnValueChange;
        public Callback<string> OnEndEdit;
    }
    [AddComponentMenu("UI/Control/UInputField")]
    [HideMonoScript]
    public class UInputField : UPresenter<UInputFieldData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        InputField Com;
        #endregion

        #region life
        public override void Init(UInputFieldData data)
        {
            base.Init(data);
            Com.onValueChanged.AddListener(this.OnValueChanged);
            Com.onEndEdit.AddListener(this.OnEndEdit);
        }
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void OnDestroy()
        {
            Com.onValueChanged.RemoveAllListeners();
            base.OnDestroy();
        }
        #endregion

        #region get
        /// <summary>
        /// 输入的字符窜
        /// </summary>
        public string InputText
        {
            get
            {
                return Com.text;
            }
            set
            {
                Com.text = value;
            }
        }
        #endregion

        #region set
        public void EnableInput(bool b)
        {
            Com.readOnly = !b;
        }
        #endregion

        #region is
        public bool IsHaveText()
        {
            return !InputText.IsInv();
        }
        #endregion

        #region Callback
        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            Com.readOnly = !b;
            Com.interactable = b;
        }
        void OnValueChanged(string text)
        {
            Data?.OnValueChange?.Invoke(text);
        }
        void OnEndEdit(string text)
        {
            Data?.OnEndEdit?.Invoke(text);
        }
        #endregion
    }

}