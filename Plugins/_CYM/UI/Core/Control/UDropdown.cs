using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UDropdownData : UButtonData
    {
        public Type Enum=null;
        public Func<string[]> Opts;
        public Callback<int> OnValueChanged;
        public Func<int> Value;
    }
    [AddComponentMenu("UI/Control/UDropdown")]
    [HideMonoScript]
    public class UDropdown : UPresenter<UDropdownData>
    {
        #region presenter
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        Text Name;
        [FoldoutGroup("Inspector"), Required,ChildGameObjectsOnly, SerializeField, SceneObjectsOnly]
        RectTransform Template;
        [FoldoutGroup("Inspector"), Required, ChildGameObjectsOnly, SerializeField, SceneObjectsOnly]
        Text CaptionText;
        [FoldoutGroup("Inspector"), Required, ChildGameObjectsOnly, SerializeField, SceneObjectsOnly]
        Text ItemText;
        #endregion

        #region prop
        string[] opts;
        Dropdown Com;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
            Com = GO.SafeAddComponet<Dropdown>();
            Template.gameObject.SetActive(false);
            Com.template = Template;
            Com.captionText = CaptionText;
            Com.alphaFadeSpeed = 0.15f;
            Com.itemText = ItemText;
        }
        public override void Init(UDropdownData data)
        {
            base.Init(data);
            if (Com != null)
            {
                Com.onValueChanged.AddListener(this.OnValueChanged);
            }
            if (Data.Enum != null)
            {
                opts = Data.Enum.GetEnumNames();
                for(int i=0;i<opts.Length;++i)
                {
                    opts[i] = (Data.Enum.Name + "." + opts[i]).GetName();
                }
            }
        }
        public override void Cleanup()
        {
            if (Com != null)
            {
                Com.ClearOptions();
                Com.onValueChanged.RemoveAllListeners();
            }
            base.Cleanup();
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Com != null)
            {                
                List<Dropdown.OptionData> listOp = new List<Dropdown.OptionData>();
                if (opts != null)
                {
                    foreach (var item in opts)
                        listOp.Add(new Dropdown.OptionData(item));
                }
                else if (Data.Opts != null)
                {
                    foreach (var item in Data.Opts.Invoke())
                        listOp.Add(new Dropdown.OptionData(item));
                }
                Com.ClearOptions();
                Com.AddOptions(listOp);

                if (Data.IsInteractable != null)
                    Com.interactable = Data.IsInteractable.Invoke(Index);
                if (Data.Value != null)
                    Com.SetValueWithoutNotify(Data.Value.Invoke());
            }
            if (Name != null)
                Name.text = Data.GetName();
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }
        void OnValueChanged(int index)
        {
            Data.OnValueChanged?.Invoke(index);
            PlayClickAudio();
        }
        #endregion
    }

}