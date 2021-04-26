using DG.Tweening;
using Invoke;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UUIView : UView
    {
        #region Callback Value
        public Callback<bool> Callback_OnShowComplete { get; set; }
        #endregion

        #region Inspector
        [FoldoutGroup("ViewGroup"), SerializeField,Tooltip("是否将界面置为到最前方")] 
        public bool IsFocus = true;

        [FoldoutGroup("Animator")]
        public List<UIPosAnimator> PosAnimator = new List<UIPosAnimator>();
        [FoldoutGroup("Animator")]
        public List<UIScaleAnimator> ScaleAnimator = new List<UIScaleAnimator>();

        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly, SceneObjectsOnly]
        public UButton BntClose;
        [FoldoutGroup("Inspector"), SerializeField, ChildGameObjectsOnly, SceneObjectsOnly]
        public UText Title;
        #endregion

        #region 内部
        protected List<UIAnimator> EffectShows = new List<UIAnimator>();
        protected HashList<RectTransform> LayoutsDirty { get; private set; } = new HashList<RectTransform>();
        protected IBaseScreenMgr BaseScreenMgr => BaseGlobal.ScreenMgr;
        protected Graphic[] graphics { get; private set; }
        protected CanvasGroup canvasGroup { get; private set; }
        protected Vector3 sourceAnchoredPosition3D;
        protected Vector3 sourceLocalScale;
        protected Vector2 sourceAnchorMax;
        protected Vector2 sourceAnchorMin;
        protected Vector2 sourceAnchoredPosition;
        protected Vector2 sourceSizeData;
        protected Vector3 sourcePivot;
        //界面顶层自动排版
        protected LayoutGroup LayoutGroup { get; private set; }
        //用于子界面Panel互斥
        protected UMutexer PanelMutexer { get; private set; } = new UMutexer();
        IJob openDelayJob;
        #endregion

        #region life
        protected virtual string TitleKey => Const.STR_Inv;
        protected virtual string CloseKey => "Close";
        protected virtual string GetTitle()
        {
            if (TitleKey.IsInv()) return "None";
            return GetStr(TitleKey);
        }
        public override void Awake()
        {
            base.Awake();
            graphics = GetComponentsInChildren<Graphic>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && RectTrans != null)
                canvasGroup = GO.AddComponent<CanvasGroup>();
            LayoutGroup = GetComponent<LayoutGroup>();

            //初始化变化组件
            EffectShows.AddRange(PosAnimator);
            EffectShows.AddRange(ScaleAnimator);
            foreach (var item in EffectShows)
                item.Init(this);

            sourceAnchorMax = RectTrans.anchorMax;
            sourceAnchorMin = RectTrans.anchorMin;
            sourceLocalScale = RectTrans.localScale;
            sourceSizeData = RectTrans.sizeDelta;
            sourceAnchoredPosition = RectTrans.anchoredPosition;
            sourceAnchoredPosition3D = RectTrans.anchoredPosition3D;
            sourcePivot = RectTrans.pivot;
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            if (BntClose != null) BntClose.Init(new UButtonData() { NameKey = CloseKey, OnClick = OnClickClose });
            if (Title != null) Title.Init(new UTextData() { Name = GetTitle, IsTrans = false });
            BaseScreenMgr.Callback_OnSetPlayerBase += OnSetPlayerBase;
        }
        // 将界面挂到其他界面下
        public sealed override void Attach(ViewLevel viewLevel, UView beAttchedView)
        {
            base.Attach(viewLevel, beAttchedView);
            ResetSourcePosData();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            foreach (var item in FixedUpdateControls)
            {
                if (item.IsDirtyShow) item.RefreshShow();
                if (item.IsDirtyData) item.RefreshData();
                if (item.IsDirtyRefresh) item.Refresh();
                if (item.IsDirtyCell) item.RefreshCell();
                item.OnFixedUpdate();
            }
            foreach (var item in Mutexers)
                item.OnFixedUpdate();
            PanelMutexer.OnFixedUpdate();
            PanelMutexer.Current?.OnFixedUpdate();

            if (LayoutsDirty.Count > 0)
            {
                foreach (var item in LayoutsDirty)
                    item.RebuildLayout();
                LayoutsDirty.Clear();
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in UpdateControls)
                item.OnUpdate();
            foreach (var item in Mutexers)
                item.OnUpdate();
            PanelMutexer.OnUpdate();
            PanelMutexer.Current?.OnUpdate();
        }
        // 销毁UI
        public override void DoDestroy()
        {
            base.DoDestroy();
            BaseScreenMgr.Callback_OnSetPlayerBase -= OnSetPlayerBase;
            Panels.Clear();
            Mutexers.Clear();
            UpdateControls.Clear();
            FixedUpdateControls.Clear();
        }
        // 获取控件
        protected sealed override void FetchSubjects()
        {
            if (GO == null) return;
            if (IsRootView) return;

            //获得所有的控件
            var tempControls = GO.GetComponentsInChildren<UControl>(true);
            if (tempControls != null)
            {
                foreach (var item in tempControls)
                {
                    if (item is UPanel panel)
                    {
                        AddPanel(panel);
                        item.OnBeFetched();
                    }
                    else if (item.IsCanBeViewFetch)
                    {
                        AddControl(item);
                        item.OnBeFetched();
                    }                    
                }
            }
        }
        #endregion

        #region set
        public void ResetSourcePosData()
        {
            RectTrans.localScale = sourceLocalScale;
            RectTrans.anchorMax = sourceAnchorMax;
            RectTrans.anchorMin = sourceAnchorMin;
            RectTrans.sizeDelta = sourceSizeData;
            Trans.localPosition = sourceLocalPos;
            RectTrans.anchoredPosition = sourceAnchoredPosition;
            RectTrans.anchoredPosition3D = sourceAnchoredPosition3D;
            RectTrans.pivot = sourcePivot;
        }
        // 显示或者关闭界面
        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            if (IsShow == b && !force) return;
            base.Show(b, useGroup, force);
            IsShow = b;

            //执行界面的显示逻辑
            if (IsShow)
            {
                //刷新数据和组件
                SetDirtyAll();
                //设置焦点
                if (IsFocus) SetFocus();
                OnOpen(this, useGroup);
            }
            else
            {
                OnClose(useGroup);
            }
            OnShow();

            //设置时间
            float mainShowTime = 0;
            if (ShowTime >= 0) mainShowTime = ShowTime;
            else mainShowTime = IsSameTime ? Duration : (b ? InTime : OutTime);

            //停止之前的tween
            if (alphaTween != null) alphaTween.Kill();
            if (scaleTween != null) scaleTween.Kill();
            if (moveTween != null) moveTween.Kill();

            //Alpha效果
            if (IsReset) canvasGroup.alpha = IsShow ? 0.0f : 1.0f;
            alphaTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, b ? 1.0f : 0.0f, mainShowTime);
            alphaTween.SetDelay(Delay);
            if (IsShow) alphaTween.OnComplete(OnFadeIn);
            else alphaTween.OnComplete(OnFadeOut);

            //缩放效果
            if (IsScale) OnShowScaleEffect(mainShowTime);
            //位移效果
            if (IsMove) OnShowMoveEffect(mainShowTime);
            //屏蔽/取消屏蔽 UI点击
            if (canvasGroup != null) canvasGroup.blocksRaycasts = IsShow;
            //触发控件的ViewShow事件
            foreach (var item in Controls) item.OnViewShow(b);
            //触发Panel的ViewShow事件
            foreach (var item in Panels) item.Value.OnViewShow(b);
            //触发动画特效
            foreach (var item in EffectShows) item.OnShow(b);
        }
        public override void Enable(bool b)
        {
            IsEnable = b;
            if (Canvas != null) Canvas.enabled = IsEnable;
            if (GraphicRaycaster != null) GraphicRaycaster.enabled = IsEnable;
            if (CanvasScaler != null) CanvasScaler.enabled = IsEnable;
        }
        public override void Interactable(bool b)
        {
            if (canvasGroup != null)
                canvasGroup.interactable = b;
        }
        public void SetDirtyLayout(LayoutGroup layout)
        {
            RectTransform rectTrans = layout.transform as RectTransform;
            SetDirtyLayout(rectTrans);
        }
        public void SetDirtyLayout(RectTransform rectTrans)
        {
            if (LayoutsDirty.Contains(rectTrans)) return;
            LayoutsDirty.Add(rectTrans);
        }
        #endregion

        #region Refresh
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh())
                    item.Refresh();
            }
        }
        public sealed override void RefreshCell()
        {
            base.RefreshCell();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh())
                    item.RefreshCell();
            }
        }
        public sealed override void RefreshData()
        {
            base.RefreshData();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh())
                    item.RefreshData();
            }
        }
        public sealed override void RefreshShow()
        {
            base.RefreshShow();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh(false))
                    item.RefreshShow();
            }
        }
        public sealed override void RefreshAll()
        {
            base.RefreshAll();
            foreach (var item in Controls)
            {
                if (item.IsCanAutoRefresh(false))
                    item.RefreshAll();
            }
        }
        #endregion

        #region dirty
        public sealed override void SetDirtyShow()
        {
            base.SetDirtyShow();
            PanelMutexer.Current?.SetDirtyShow();
            foreach (var item in Mutexers)
                item.SetDirtyShow();
        }
        public sealed override void SetDirtyData()
        {
            base.SetDirtyData();
            PanelMutexer.Current?.SetDirtyData();
            foreach (var item in Mutexers)
                item.SetDirtyData();
        }
        public sealed override void SetDirtyRefresh()
        {
            base.SetDirtyRefresh();
            PanelMutexer.Current?.SetDirtyRefresh();
            foreach (var item in Mutexers)
                item.SetDirtyRefresh();
        }
        public sealed override void SetDirtyAll()
        {
            base.SetDirtyAll();
            PanelMutexer.Current?.SetDirtyAll();
            foreach (var item in Mutexers)
                item.SetDirtyAll();
        }
        #endregion

        #region control
        protected HashList<UControl> FixedUpdateControls { get; private set; } = new HashList<UControl>();
        protected HashList<UControl> UpdateControls { get; private set; } = new HashList<UControl>();
        protected HashList<UControl> Controls { get; private set; } = new HashList<UControl>();
        public void ActiveControlFixedUpdate(UControl control)
        {
            if (!control.IsCanBeViewFetch)
            {
                CLog.Error("错误:{0},无法作为View的控件", control.GOName);
                return;
            }
            FixedUpdateControls.Add(control);
        }
        public void ActiveControlUpdate(UControl control)
        {
            if (!control.IsCanBeViewFetch)
            {
                CLog.Error("错误:{0},无法作为View的控件", control.GOName);
                return;
            }
            UpdateControls.Add(control);
        }
        protected void AddControl(UControl item)
        {
            item.PUIView = this;
            Controls.Add(item);
            if (item.NeedFixedUpdate)
                ActiveControlFixedUpdate(item);
            if (item.NeedUpdate)
                ActiveControlUpdate(item);
        }
        #endregion

        #region Panel
        protected Dictionary<string, UPanel> Panels { get; private set; } = new Dictionary<string, UPanel>();
        //默认的MainPanel,用户需要将其命名为Main,会自动赋值给这个变量
        protected UPanel MainPanel { get; private set; }
        // 创建Panel
        protected T CreatePanel<T>(string path) where T : UPanel
        {
            T panel = UIMgr.CreateUIGO<T>(path);
            if (panel == null) return null;
            panel.Trans.SetParent(Trans);
            panel.ResetSourcePosData();
            panel.Trans.SetAsLastSibling();
            panel.GO.name = path;
            panel.PUIView = this;
            AddPanel(panel);
            return panel;
        }
        public void AddPanel(UPanel panel)
        {
            panel.PUIView = this;
            if (MainPanel == null)
                MainPanel = panel;
            Panels.Add(panel.GOName, panel);
            PanelMutexer.Add(panel);
        }
        public void RemovePanel(UPanel panel)
        {
            if (MainPanel == panel)
                MainPanel = null;
            Panels.Remove(panel.GOName);
            PanelMutexer.Remove(panel);
        }
        // 获得Panel
        protected T GetPanel<T>(string name) where T : UPanel
        {
            if (Panels.ContainsKey(name))
                return Panels[name] as T;
            return null;
        }
        #endregion

        #region Mutexer 组件互斥组,一次只能显示一个组件
        //默认用户自定义添加的第一个Mutex对象
        protected UMutexer MainMutexer { get; private set; } = null;
        protected List<UMutexer> Mutexers { get; private set; } = new List<UMutexer>();
        /// <summary>
        /// 添加互斥
        /// </summary>
        /// <param name="isNeedReset">是否重置</param>
        /// <param name="isShowOne">是否至少选择一个</param>
        /// <param name="controls"></param>
        /// <returns></returns>
        protected UMutexer AddMutexerMain(bool isNeedReset, bool isShowOne, params UControl[] controls)
        {
            if (controls == null) return null;
            //如果有默认Panel的话,添加到默认Panel
            if (MainPanel != null)
            {
                foreach (var item in controls)
                {
                    if (item.IsCanBeViewFetch)
                    {
                        CLog.Error("错误:{0}MainPanel的控件", item.GOName);
                        return null;
                    }
                }
                var ret = MainPanel.AddMutexer(isNeedReset, isShowOne, controls);
                if (MainMutexer == null)
                    MainMutexer = ret;
                return ret;
            }
            //没有默认Panel的话,添加到主界面
            else
            {
                var ret = AddMutexer(isNeedReset, isShowOne, controls);
                if (MainMutexer == null)
                    MainMutexer = ret;
                return ret;
            }
        }
        protected UMutexer AddMutexer(bool isNeedReset, bool isShowOne, params UControl[] controls)
        {
            if (controls == null) return null;
            foreach (var item in controls)
            {
                if (!item.IsCanBeViewFetch)
                {
                    CLog.Error("错误:{0}不能作为View的控件", item.GOName);
                    return null;
                }
            }
            var temp = new UMutexer(controls, isNeedReset, isShowOne);
            Mutexers.Add(temp);
            foreach (var item in controls)
                Controls.Remove(item);
            return temp;
        }
        #endregion

        #region Callback
        protected virtual void OnSetPlayerBase(BaseUnit arg1, BaseUnit arg2)
        {
            SetDirtyAll();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            SetDirtyAll();
        }
        protected virtual void OnClickClose(UControl control, PointerEventData data) => Show(false);
        protected void OnShowScaleEffect(float mainShowTime)
        {
            if (scaleTween != null) scaleTween.Kill();
            Vector3 minScale = sourceLocalScale * 0.001f;
            if (IsShow && TweenMove.IsReset) Trans.localScale = minScale;
            if (mainShowTime >= 0)
            {
                scaleTween = Trans.DOScale(
                IsShow ? sourceLocalScale : minScale,
                mainShowTime)
                .SetEase(IsShow ? TweenScale.InEase : TweenScale.OutEase)
                .SetDelay(Delay);
            }
        }
        protected void OnShowMoveEffect(float mainShowTime)
        {
            if (IsDragged) return;
            if (moveTween != null) moveTween.Kill();
            if (IsShow && TweenMove.IsReset) RectTrans.anchoredPosition = TweenMove.StartPos;
            if (mainShowTime >= 0)
            {
                moveTween = DOTween.To(
                    () => RectTrans.anchoredPosition,
                    (x) => RectTrans.anchoredPosition = x,
                    IsShow ? sourceAnchoredPosition : TweenMove.StartPos,
                    mainShowTime)
                    .SetEase(IsShow ? TweenMove.InEase : TweenMove.OutEase)
                    .SetDelay(Delay);
            }
        }
        #endregion

        #region open & close
        protected override void OnOpenDelay(UView baseView, bool useGroup)
        {
            base.OnOpenDelay(baseView, useGroup);
            LayoutGroup?.RebuildLayout();
        }
        protected override void OnOpen(UView baseView, bool useGroup)
        {
            base.OnOpen(baseView, useGroup);
            openDelayJob?.Kill();
            openDelayJob = Util.Invoke(() => OnOpenDelay(baseView,useGroup), 0.01f);
            //if (LayoutGroup != null)
            //    Util.Invoke(() => LayoutGroup.RebuildLayout(), 0.01f);
        }
        protected override void OnClose(bool useGroup)
        {
            PanelMutexer.ShowDefault();
            foreach (var item in Mutexers)
                item.TestReset();
            base.OnClose(useGroup);
        }
        protected override void OnFadeIn()
        {
            base.OnFadeIn();
            Callback_OnShowComplete?.Invoke(true);
        }
        protected override void OnFadeOut()
        {
            base.OnFadeOut();
            if (IsScale) Trans.localScale = Vector3.one * 0.001f;
            if (IsMove) RectTrans.anchoredPosition = TweenMove.StartPos;
            Callback_OnShowComplete?.Invoke(false);
        }
        #endregion

        #region 工具函数包装
        protected static BaseModalBoxView BaseModalBoxView => BaseModalBoxView.Default;
        protected static BaseTooltipView BaseTooltipView => BaseTooltipView.Default;
        public static void ShowTip(string key, params string[] ps) => BaseTooltipView.Show(key, ps);
        public static void ShowTipStr(string str) => BaseTooltipView.ShowStr(str);
        protected void ShowOKCancleDlg(string key, string descKey, Callback BntOK, Callback BntCancle, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOKCancle(key, descKey, BntOK, BntCancle, paras);
        }
        protected void ShowOKCancleDlg(string descKey, Callback BntOK, Callback BntCancle, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOKCancle(descKey, BntOK, BntCancle, paras);
        }
        protected void ShowOKDlg(string key, string descKey, Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOK(key, descKey, BntOK, paras);
        }
        protected void ShowOKDlg(string descKey, Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.SetNeedDirtyView(this);
            BaseModalBoxView.ShowOK(descKey, BntOK, paras);
        }
        #endregion
    }
}
