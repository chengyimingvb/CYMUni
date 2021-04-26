using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UDupplicateData:UPresenterData
    {
        /// <summary>
        /// obj1=presenter
        /// obj2=用户自定义数据
        /// </summary>
        public new Callback<object, object> OnRefresh = null;
        /// <summary>
        /// obj1=presenter
        /// obj2=用户自定义数据
        /// </summary>
        public Callback<object, object> OnFixedRefresh = null;
        /// <summary>
        /// 获取用户自定义数据 方法
        /// </summary>
        public Func<IList> GetCustomDatas = null;
        /// <summary>
        /// int1 =当前的index
        /// int2 =上次的index
        /// </summary>
        public Callback<int, int> OnSelectChange = null;
        /// <summary>
        /// 当前选得index
        /// </summary>
        public Callback<int> OnClickSelected = null;
        /// <summary>
        /// 固定数量
        /// </summary>
        public int FixedCount = 0;

        public bool IsAutoClose => FixedCount == 0;
        public int GetInitCount()
        {
            if (FixedCount >= 0)
                return FixedCount;
            if (GetCustomDatas != null)
                return GetCustomDatas.Invoke().Count;
            return 0;
        }
    }
    [AddComponentMenu("UI/Control/UDupplicate")]
    [HideMonoScript]
    public partial class UDupplicate : UPresenter<UDupplicateData>, IBaseCheckBoxContainer
    {
        #region Inspector
        [FoldoutGroup("CheckBox"), SerializeField, Tooltip("是否会自动选择一个,自动Toggle")]
        bool IsToggleGroup = false;
        [FoldoutGroup("CheckBox"), SerializeField, Tooltip("是否默认选择一个"), HideIf("Inspector_HideAutoSelect")]
        bool IsAutoSelect = true;
        [FoldoutGroup("CheckBox"), SerializeField, Tooltip("是否刷新自生界面")]
        bool IsLinkSelfView = false;
        [FoldoutGroup("CheckBox"), SceneObjectsOnly, SerializeField, Tooltip("链接的基础组件,选择变化的时候会自动刷新")]
        UControl LinkControl;

        [FoldoutGroup("Data"), SerializeField]
        bool IsInitOnStart = false;
        [FoldoutGroup("Data"), ShowIf("Inspector_IsInitCount"), SerializeField]
        int Count = 0;
        [FoldoutGroup("Data"), ShowIf("Inspector_IsPrefab"), SerializeField]
        GameObject Prefab;
        #endregion

        #region public
        public List<GameObject> GOs { get; private set; } = new List<GameObject>();
        public List<UControl> Controls { get; private set; } = new List<UControl>();
        //子对象数量
        public int GOCount => GOs.Count;
        //当前选择的index
        public int CurSelectIndex { get; protected set; } = Const.INT_Inv;
        //上一次的选择
        public int PreSelectIndex { get; protected set; } = Const.INT_Inv;
        #endregion

        #region prop
        // 用户自定义数据缓存
        IList CustomDatas = new List<object>();
        // 刷新Layout
        Timer RefreshLayoutTimer = new Timer(0.02f);
        List<UCheckBox> ToggleGroupCheckBoxs { get; set; } = new List<UCheckBox>();
        bool IsInitedCount = false;
        #endregion

        #region life
        public override bool NeedFixedUpdate => true;
        public override bool IsAutoInit => true;
        public override bool IsCollection => true;
        public override bool IsAutoFetchSubControls => false;
        protected override void Awake()
        {
            base.Awake();
            if (IsInitOnStart) InitCount(Count);
            if (GetIsAutoSelect()) CurSelectIndex = 0;
        }
        protected override void Start()
        {
            base.Start();
            if (IsInitOnStart) InitData();
        }
        public override void OnBeFetched()
        {
            base.OnBeFetched();
        }
        public override void OnViewShow(bool b)
        {
            base.OnViewShow(b);
            CurSelectIndex = 0;
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            CurSelectIndex = 0;
        }
        // 刷新
        public override void Refresh()
        {
            if (Data.OnRefresh != null)
            {
                IsDirtyRefresh = false;
                CustomDatas = Data.GetCustomDatas?.Invoke();
                //自动选择
                if (GetIsAutoSelect() &&
                    CustomDatas != null)
                {
                    if (CurSelectIndex > CustomDatas.Count)
                        CurSelectIndex = 0;
                    else if (CurSelectIndex < 0 && CustomDatas.Count > 0)
                        CurSelectIndex = 0;
                }

                foreach (var item in Controls)
                {
                    if (item == null)
                        continue;
                    //如果CustomData数量少于presenter数量,则关闭Slot
                    if (
                        Data.IsAutoClose &&
                        CustomDatas != null &&
                        item.Index >= CustomDatas.Count
                        )
                    {
                        item.Close();
                        continue;
                    }
                    item.Show();
                    Data.OnRefresh.Invoke(item, GetCustomData(CustomDatas, item.Index));
                    if (GetIsToggleGroup() && item is UCheckBox checkBox && checkBox.IsToggleGroup)
                        checkBox.RefreshStateAndActiveEffectBySelect();
                }
                RefreshLayoutTimer.Restart();
            }
            else
            {
                base.Refresh();
            }
            Data.OnSelectChange?.Invoke(CurSelectIndex, PreSelectIndex);
        }
        public override void OnFixedUpdate()
        {
            if (!IsShow)
                return;
            if (Data.OnFixedRefresh != null)
            {
                foreach (var item in Controls)
                {
                    if (item == null)
                        continue;
                    //如果CustomData数量少于presenter数量,则关闭Slot
                    if (Data.IsAutoClose &&
                        CustomDatas != null &&
                        item.Index >= CustomDatas.Count)
                    {
                        item.Close();
                        continue;
                    }
                    item.Show();
                    Data.OnFixedRefresh.Invoke(item, GetCustomData(CustomDatas, item.Index));
                }
            }
            else
            {
                base.OnFixedUpdate();
            }

            if (RefreshLayoutTimer.CheckOverOnce())
            {
                //刷新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(RectTrans);
            }
        }
        #endregion

        #region init
        public TP[] Init<TP, TD>(Func<IList> getCustomDatas, Callback<object, object> onRefresh, Callback<int> onClickSelected = null)
            where TP : UPresenter<TD>, new()
            where TD : UPresenterData, new()
        {
            Data.GetCustomDatas = getCustomDatas;
            Data.OnRefresh = onRefresh;
            Data.OnFixedRefresh = null;
            Data.OnClickSelected = onClickSelected;
            return InitCountAndData<TP, TD>(Data.GetInitCount());
        }
        public TP[] Init<TP, TD>(TD data, Func<IList> getCustomDatas, Callback<object, object> onRefresh, Callback<object, object> onFixedRefresh, Callback<int> onClickSelected)
            where TP : UPresenter<TD>, new()
            where TD : UPresenterData, new()
        {
            Data.GetCustomDatas = getCustomDatas;
            Data.OnRefresh = onRefresh;
            Data.OnFixedRefresh = onFixedRefresh;
            Data.OnClickSelected = onClickSelected;
            return InitCountAndData<TP, TD>(Data.GetInitCount(), data);
        }
        public TP[] Init<TP, TD>(int fixedCount, TD pData = null, Callback<object, object> onRefresh = null, Callback<object, object> onFixedRefresh = null)
            where TP : UPresenter<TD>, new()
            where TD : UPresenterData, new()
        {
            Data.FixedCount = fixedCount;
            Data.OnRefresh = onRefresh;
            Data.OnFixedRefresh = onFixedRefresh;

            return InitCountAndData<TP, TD>(Data.GetInitCount(), pData); ;
        }
        public TP[] Init<TP, TD>(params TD[] data)
            where TP : UPresenter<TD>, new()
            where TD : UPresenterData, new()
        {
            if (data == null)
            {
                Show(false);
                return null;
            }
            Data.OnRefresh = null;
            Data.OnFixedRefresh = null;
            return InitCountAndData<TP, TD>(data.Length, data);
        }
        public TP[] Init<TP, TD>(Callback<object, object> onRefresh /*obj1=presenter obj2=用户自定义数据*/, Callback<object, object> onFixedRefresh, params TD[] data)
            where TP : UPresenter<TD>, new()
            where TD : UPresenterData, new()
        {
            if (data == null) return null;
            Data.OnRefresh = onRefresh;
            Data.OnFixedRefresh = onFixedRefresh;
            return InitCountAndData<TP, TD>(data.Length, data);
        }
        public TP[] Init<TP, TD>(UDupplicateData pdata,TD data)
            where TP : UPresenter<TD>, new()
            where TD : UPresenterData, new()
        {
            ReInit(pdata);
            return InitCountAndData<TP, TD>(Data.GetInitCount(), data);
        }
        #endregion

        #region get custom
        public T GetCustomData<T>(UControl item) where T : class
        {
            if (CustomDatas == null) return null;
            if (item.Index >= CustomDatas.Count) return null;
            if (item.Index < 0) return null;
            if (item.Index >= CustomDatas.Count) return null;
            return CustomDatas[item.Index] as T;
        }
        public T GetCustomData<T>(int curSelect) where T : class
        {
            if (Controls.Count <= curSelect) return null;
            var item = Controls[curSelect];
            if (item.Index < 0) return null;
            if (item.Index >= CustomDatas.Count) return null;
            return CustomDatas[item.Index] as T;
        }
        public T GetCurCustomData<T>() where T : class
        {
            if (Controls.Count <= CurSelectIndex) return null;
            var item = Controls[CurSelectIndex];
            if (item.Index < 0) return null;
            if (item.Index >= CustomDatas.Count) return null;
            return CustomDatas[item.Index] as T;
        }
        object GetCustomData(IList customData, int index)
        {
            if (customData == null) return null;
            if (index >= customData.Count) return null;
            if (index < 0) return null;
            return customData[index];
        }
        #endregion

        #region get control
        public UControl GetControl(int index)
        {
            if (Controls.Count <= index) return null;
            var item = Controls[index];
            if (item.Index < 0) return null;
            return item;
        }
        public TP GetControl<TP>(int index) where TP:UControl
        {
            return GetControl(index) as TP;
        }
        #endregion

        #region get

        public bool GetIsToggleGroup()
        {
            return IsToggleGroup;
        }

        public bool GetIsAutoSelect()
        {
            return IsAutoSelect;
        }
        #endregion

        #region set
        public void ForControls(Callback<UControl> callback)
        {
            foreach (var item in Controls)
            {
                if (item.IsShow)
                {
                    callback(item);
                }
            }
        }
        #endregion

        #region select
        public void SelectItem(UControl arg1)
        {
            if (arg1 == null)
                return;
            PreSelectIndex = CurSelectIndex;
            CurSelectIndex = arg1.Index;
            Data.OnClickSelected?.Invoke(CurSelectIndex);

            //刷新关联界面
            if (LinkControl != null)
            {
                if (LinkControl is UScroll)
                    LinkControl.SetDirtyData();
                else
                    LinkControl.SetDirtyRefresh();
            }
            //刷星自生界面
            if (IsLinkSelfView)
            {
                PUIView?.SetDirtyAll();
            }

            //刷新自生
            if (arg1 is UCheckBox)
            {
                Refresh();
            }
        }
        public void SelectItem(int index)
        {
            var data = GetControl(index);
            SelectItem(data);
        }
        #endregion

        #region Inspector
        bool Inspector_HideAutoSelect()
        {
            return !IsToggleGroup;
        }
        #endregion
    }

}