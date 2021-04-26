using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UCheckBoxData : UButtonData
    {
        //自动关联_N和_On图标
        public bool IsAutoIcon = false;
        // 连接的Presenter
        public UControl LinkControl = null;
        // 连接的View
        public UUIView LinkView = null;
        public Func<bool> IsOn = null;
        public Callback<bool> OnValueChanged = null;

        public Func<Sprite> ActiveBg = null;
        public string ActiveBgStr = Const.STR_Inv;

        public Func<Sprite> ActiveIcon = null;
        public string ActiveIconStr = Const.STR_Inv;

        public Color? ActiveTextCol = null;
        public Color? ActiveIconCol = null;
        public Color? ActiveBgCol = null;

        #region get
        public Sprite GetActiveIcon()
        {
            if (ActiveIcon != null)
            {
                return ActiveIcon.Invoke();
            }
            if (!ActiveIconStr.IsInv())
                return BaseGRMgr.Icon.Get(ActiveIconStr);
            return null;
        }
        public Sprite GetActiveBg()
        {
            if (ActiveBg != null)
            {
                return ActiveBg.Invoke();
            }
            if (!ActiveBgStr.IsInv())
                return BaseGRMgr.Icon.Get(ActiveIconStr);
            return null;
        }
        #endregion
    }
    [AddComponentMenu("UI/Control/UCheckBox")]
    [HideMonoScript]
    public class UCheckBox : UPresenter<UCheckBoxData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public Text Text;
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly]
        public GameObject ActiveObj;

        [FoldoutGroup("Inspector BG"), SerializeField, SceneObjectsOnly]
        public Image Bg;
        [FoldoutGroup("Inspector BG"), SerializeField, SceneObjectsOnly]
        public Image ActiveBg;

        [FoldoutGroup("Inspector Icon"), SerializeField, SceneObjectsOnly]
        public Image Icon;
        [FoldoutGroup("Inspector Icon"), SerializeField, SceneObjectsOnly]
        public Image ActiveIcon;
        [FoldoutGroup("Inspector Icon"), SerializeField, SceneObjectsOnly]
        public Image OverlayIcon;
        #endregion

        #region data
        [SerializeField, FoldoutGroup("Data"), Tooltip("此值为True的时候,当IsOn==true的时候关闭ActiveIcon,反之亦然")]
        bool Inverse = false;
        [SerializeField, FoldoutGroup("Data"), Tooltip("此值为True的时候,切换Bg/Icon和ActiveBg/ActiveIcon")]
        bool IsSwitch = true;
        #endregion

        #region prop
        public bool IsOn { get; protected set; } = false;
        public bool IsToggleGroup
        {
            get
            {
                bool isPToggleGroup = false;
                if (PDupplicate != null)
                    isPToggleGroup = PDupplicate.GetIsToggleGroup();
                if (PScroll != null)
                    isPToggleGroup = PScroll.GetIsToggleGroup();
                if (Data != null && Data.IsOn != null)
                    isPToggleGroup = false;
                return isPToggleGroup;
            }
        }
        public bool IsHaveLink => Data.LinkControl != null || Data.LinkView != null;
        protected Sprite SourceSprite { get; private set; }
        protected Color SourceTextCol { get; private set; }
        protected Color SourceIconCol { get; private set; }
        protected Color SourceBgCol { get; private set; }
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
        }
        public override void OnBeFetched()
        {
            base.OnBeFetched();
            if (Icon != null)
            {
                SourceSprite = Icon.sprite;
                SourceIconCol = Icon.color;
            }
            if (Bg != null)
            {
                SourceBgCol = Bg.color;
            }
            if (Text != null)
            {
                SourceTextCol = Text.color;
            }
        }
        public override void Init(UCheckBoxData data)
        {
            base.Init(data);
            //自动获得激活图片,后缀有_N自动替换成_On 否则自动增加_On
            if (data.IsAutoIcon)
            {
                string sourceIcon = data.IconStr;
                if (!data.IconStr.EndsWith(Const.Suffix_Checkbox_N))
                {
                    data.IconStr = sourceIcon + Const.Suffix_Checkbox_N;
                }
                if (!data.ActiveIconStr.EndsWith(Const.Suffix_Checkbox_On))
                {
                    data.ActiveIconStr = sourceIcon + Const.Suffix_Checkbox_On;
                }
            }

            //link
            if (data.LinkView != null)
            {
                data.LinkView.Callback_OnShow += OnLinkShow;
            }
            if (data.LinkControl != null)
            {
                data.LinkControl.Callback_OnShow += OnLinkShow;
                if (data.LinkControl is UPanel panel)
                {
                    panel?.DettachFromPanelList();
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Data != null)
            {
                if (Data.LinkView != null) Data.LinkView.Callback_OnShow -= OnLinkShow;
                if (Data.LinkControl != null) Data.LinkControl.Callback_OnShow -= OnLinkShow;
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            if (!IsToggleGroup)
            {
                RefreshState();
            }
            else
            {
                RefreshStateBySelect();
            }
            RefreshEffect();
            RefreshActiveEffect();
            RefreshLink();
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            //如果不是ToggleGroup 则执行以下操作
            if (CheckCanClick() &&
                IsInteractable &&
                !IsToggleGroup)
            {
                RefreshStateByInput(!IsOn);
                RefreshActiveEffect();
                RefreshLink();
            }
            base.OnPointerClick(eventData);
        }
        #endregion

        #region set
        public void SetCheck(bool isOn)
        {
            RefreshStateByInput(isOn);
            RefreshActiveEffect();
            RefreshLink();
        }
        #endregion

        #region Callback
        private void OnLinkShow(bool arg1)
        {
            RefreshStateByLink();
            RefreshActiveEffect();
        }
        #endregion

        #region utile
        void RefreshState()
        {
            if (Data.IsOn != null) IsOn = Data.IsOn.Invoke();
            else if (Data.LinkView != null) IsOn = Data.LinkView.IsShow;
            else if (Data.LinkControl != null) IsOn = Data.LinkControl.IsShow;
            Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshStateByLink()
        {
            if (Data?.LinkView != null) IsOn = Data.LinkView.IsShow;
            else if (Data?.LinkControl != null) IsOn = Data.LinkControl.IsShow;
            Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshStateByInput(bool inputState)
        {
            IsOn = inputState;
            Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshLink()
        {
            if (Data?.LinkControl != null) Data?.LinkControl.ShowDirect(IsOn,true);
            else if (Data?.LinkView != null) Data?.LinkView.ShowDirect(IsOn,true);
        }
        void RefreshEffect()
        {
            if (Data != null)
            {
                //刷新内容
                if (Text != null)
                {
                    Text.text = Data.GetName();
                }
                if (Icon != null)
                {
                    var temp = Data.GetIcon();
                    if (temp != null) Icon.overrideSprite = temp;
                }
                if (ActiveIcon != null)
                {
                    var temp = Data.GetActiveIcon();
                    if (temp != null) ActiveIcon.overrideSprite = temp;
                }
                if (Bg != null)
                {
                    var temp = Data.GetBg();
                    if (temp) Bg.overrideSprite = temp;
                }
                if (ActiveBg != null)
                {
                    var temp = Data.GetActiveBg();
                    if (temp) ActiveBg.overrideSprite = temp;
                }
            }
        }
        void RefreshActiveEffect()
        {            
            //设置激活游戏对象
            if (ActiveObj != null)
            {
                if (IsOn) ActiveObj.SetActive(Inverse ? false : true);
                else ActiveObj.SetActive(Inverse ? true : false);
            }
            //叠加图标 overlay
            if (OverlayIcon != null)
            {
                if (IsOn) OverlayIcon.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, 0.1f, true);
                else OverlayIcon.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, 0.1f, true);
            }
            //切换图标
            if (IsSwitch)
            {
                //Icon
                if (Icon != null && ActiveIcon != null)
                {
                    if (IsOn) Icon.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, 0.1f, true);
                    else Icon.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, 0.1f, true);

                    if (IsOn) ActiveIcon.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, 0.1f, true);
                    else ActiveIcon.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, 0.1f, true);
                }
                //Bg
                if (Bg != null && ActiveBg != null)
                {
                    if (IsOn) Bg.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, 0.1f, true);
                    else Bg.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, 0.1f, true);

                    if (IsOn) ActiveBg.CrossFadeAlpha(Inverse ? 0.0f : 1.0f, 0.1f, true);
                    else ActiveBg.CrossFadeAlpha(Inverse ? 1.0f : 0.0f, 0.1f, true);
                }
            }
            //颜色变化
            if (Data != null)
            {
                //文字颜色变化
                if (Text != null && Data.ActiveTextCol != null)
                {
                    if (IsOn) Text.color = Inverse? SourceTextCol : Data.ActiveTextCol.Value;
                    else Text.color = Inverse? Data.ActiveTextCol.Value : SourceTextCol;
                }
                //Icon颜色变化
                if (Icon != null && Data.ActiveIconCol != null)
                {
                    if (IsOn) Icon.color = Inverse? SourceIconCol : Data.ActiveIconCol.Value;
                    else Icon.color = Inverse? Data.ActiveIconCol.Value: SourceIconCol;
                }
                //Bg颜色变化
                if (Bg != null && Data.ActiveBgCol != null)
                {
                    if (IsOn) Bg.color = Inverse? SourceBgCol: Data.ActiveBgCol.Value;
                    else Bg.color = Inverse ? Data.ActiveBgCol.Value : SourceBgCol;
                }
            }
        }
        void RefreshStateBySelect()
        {
            if (PDupplicate != null)
                RefreshStateBySelect(PDupplicate.CurSelectIndex);
            else if (PScroll != null)
                RefreshStateBySelect(PScroll.CurSelectIndex);
            else
            {
                throw new Exception("没有BaseDupplicate 或者 BaseScroll," + GOName);
            }

            void RefreshStateBySelect(int index)
            {
                IsOn = index == Index;
                Data?.OnValueChanged?.Invoke(IsOn);
            }
        }
        public void RefreshStateAndActiveEffectBySelect()
        {
            RefreshStateBySelect();
            RefreshActiveEffect();
        }
        public void ForceCloseLink()
        {
            if (Data?.LinkControl != null) 
                Data?.LinkControl.ShowDirect(false, true);
            else if (Data?.LinkView != null) 
                Data?.LinkView.ShowDirect(false, true);
        }
        #endregion

        #region wrap
        public string NameText
        {
            get { return Text.text; }
            set { Text.text = value; }
        }
        public Sprite BgSprite
        {
            get { return Bg.sprite; }
            set { Bg.sprite = value; }
        }
        public Sprite ActiveBgSprite
        {
            get { return ActiveBg.sprite; }
            set { ActiveBg.sprite = value; }
        }
        public Sprite IconSprite
        {
            get { return Icon.sprite; }
            set { Icon.sprite = value; }
        }
        public Sprite ActiveIconSprite
        {
            get { return ActiveIcon.sprite; }
            set { ActiveIcon.sprite = value; }
        }
        public Sprite OverlayIconSprite
        {
            get { return OverlayIcon.sprite; }
            set { OverlayIcon.sprite = value; }
        }
        #endregion
    }
}