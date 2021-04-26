using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 无限滚动控件,只支持竖向和横向
/// </summary>
namespace CYM.UI
{
    public class UScrollData : UPresenterData
    {
        public Func<IList> GetCustomDatas;
        /// <summary>
        /// object1=presenter
        /// object2=custom data
        /// </summary>
        public new Callback<object, object> OnRefresh;

        public List<Func<object, object>> Sorter = new List<Func<object, object>>();

        public Callback<int> OnSelectedChange;
        public Callback<int> OnSelectItem;

        public Func<string> EmptyDesc;
        public string EmptyDescKey = null;
    }
    [AddComponentMenu("UI/Control/UScroll")]
    [HideMonoScript]
    public partial class UScroll : UPresenter<UScrollData>, IBaseCheckBoxContainer, IDragHandler,IBeginDragHandler,IEndDragHandler
    {
        #region inspector
        [FoldoutGroup("Inspector"),ShowIf("Inspector_IsPrefab"), SerializeField]
        public UControl Prefab;
        [FoldoutGroup("Inspector"), SceneObjectsOnly, SerializeField, ChildGameObjectsOnly]
        public Scrollbar Scrollbar;
        [FoldoutGroup("Inspector"), SceneObjectsOnly, SerializeField, ChildGameObjectsOnly]
        public Text EmptyDesc;
        [FoldoutGroup("Inspector"), SceneObjectsOnly, SerializeField, ChildGameObjectsOnly, Required]
        public RectTransform Content;
        #endregion

        #region checkBox
        [FoldoutGroup("CheckBox"), SerializeField]
        public bool IsToggleGroup = false;
        [FoldoutGroup("CheckBox"), SerializeField,HideIf("Inspector_HideAutoSelect")]
        public bool IsAutoSelect = true;
        #endregion

        #region data
        [FoldoutGroup("Data"), SerializeField]
        bool IsFixedScrollBar = true;
        [FoldoutGroup("Data"), SerializeField, Tooltip("强制扩展,给按钮流出空间")]
        bool IsForceExpend = false;
        [FoldoutGroup("Data"), SerializeField]
        TextAnchor Anchor = TextAnchor.UpperLeft;
        [FoldoutGroup("Data"), SerializeField]
        ScrollDirectionEnum scrollDirection;
        [FoldoutGroup("Data"), SerializeField]
        float spacing;
        [FoldoutGroup("Data"), SerializeField]
        RectOffset padding;
        [FoldoutGroup("Data"), SerializeField]
        bool loop;
        [FoldoutGroup("Data"), SerializeField]
        ScrollbarVisibilityEnum scrollbarVisibility = ScrollbarVisibilityEnum.Always;
        [FoldoutGroup("Data"), SerializeField]
        ScrollRect.MovementType MovementType = ScrollRect.MovementType.Elastic;
        #endregion

        #region snap
        [FoldoutGroup("Snap"), SerializeField]
        bool snapping;
        [FoldoutGroup("Snap"), SerializeField]
        float snapVelocityThreshold;
        [FoldoutGroup("Snap"), SerializeField]
        float snapWatchOffset;
        [FoldoutGroup("Snap"), SerializeField]
        float snapJumpToOffset;
        [FoldoutGroup("Snap"), SerializeField]
        float snapCellCenterOffset;
        [FoldoutGroup("Snap"), SerializeField]
        bool snapUseCellSpacing;
        [FoldoutGroup("Snap"), SerializeField]
        TweenType snapTweenType;
        [FoldoutGroup("Snap"), SerializeField]
        float snapTweenTime;
        #endregion

        #region prop val
        public event Callback Callback_OnRelaodedData;
        public IList CacheCustomData { get; private set; } = new List<object>();
        float BasePrefabSize = 100;
        float ExpendSize = 1f;
        int NumOfGroupCell = 1;
        RectTransform PrefabRect;
        bool SortReversedList = false;
        int SortBy = -1;
        float ScrollBarSize = 0.0f;
        // 当前选择的index
        public int CurSelectIndex { get; protected set; } = -1;
        // 上一次的选择
        public int PreSelectIndex { get; protected set; } = Const.INT_Inv;
        #endregion

        #region life
        public override bool IsCollection => true;
        public override bool IsAutoFetchSubControls => false;
        protected override void Awake()
        {
            base.Awake();
            if (GetIsAutoSelect()) 
                CurSelectIndex = 0;
        }
        public override void OnBeFetched()
        {
            base.OnBeFetched();
            InitializesScroller();
        }
        public override void OnViewShow(bool b)
        {
            base.OnViewShow(b);
            if (!b)
                CurSelectIndex = -1;
            else
            {
                ScrollPosition = 0;
            }
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (!isShow)
            {
                CurSelectIndex = -1;
            }
        }
        void FixedUpdate()
        {
            if (updateSpacing)
            {
                UpdateSpacing(spacing);
            }

            //更新滑杆速度,防止速度不稳定,增加手感
            if (ScrollRect != null)
            {
                if (ScrollRectSize > cellOffsetArray.Last())
                    ScrollRect.scrollSensitivity = 2;
                else
                    ScrollRect.scrollSensitivity = 15;
            }

            // if the scroll rect size has changed and looping is on,
            // or the loop setting has changed, then we need to resize
            if (
                    (loop && lastScrollRectSize != ScrollRectSize) ||
                    (loop != lastLoop)
                )
            {
                Resize(true);
                lastScrollRectSize = ScrollRectSize;

                lastLoop = loop;
            }

            // update the scroll bar visibility if it has changed
            if (lastScrollbarVisibility != scrollbarVisibility)
            {
                ScrollbarVisibility = scrollbarVisibility;
                lastScrollbarVisibility = scrollbarVisibility;
            }

            // determine if the scroller has started or stopped scrolling
            // and call the delegate if so.
            if (LinearVelocity != 0 && !IsScrolling)
            {
                IsScrolling = true;
                Callback_ScrollerScrollingChanged?.Invoke(this, true);
            }
            else if (LinearVelocity == 0 && IsScrolling)
            {
                IsScrolling = false;
                Callback_ScrollerScrollingChanged?.Invoke(this, false);
            }

            if (Scrollbar && IsFixedScrollBar)
                Scrollbar.size = ScrollBarSize;

            if (Content != null && RefreshLayoutTimer.CheckOverOnce())
            {
                //刷新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
            }

            if (BaseInputMgr.IsScrollWheel)
            {
                if (ScrollRect) ScrollRect.velocity = Vector2.zero;
            }
            if (ScrollRect && BaseGuideView.Default)
                ScrollRect.enabled = !BaseGuideView.Default.IsInMask; //!IsBlockClick();
        }
        public override void Refresh()
        {
            base.Refresh();
            if (activeCells.data == null) return;

            if (CurSelectData != null)
            {
                CurSelectIndex = GetDataIndex(CurSelectData);
                CurSelectData = null;
            }
            else
            {
                bool needReset = true;
                foreach (var item in activeCells.data)
                {
                    if (item == null) continue;
                    if (item.Index == CurSelectIndex)
                        needReset = false;
                }
                if (needReset && GetIsAutoSelect())
                    CurSelectIndex = 0;
            }

            foreach (var item in activeCells.data)
            {
                if (item == null) continue;
                RefreshCell(item, true);
            }
            Callback_OnSelectChange?.Invoke(CurSelectIndex, PreSelectIndex);
            Data.OnSelectedChange?.Invoke(CurSelectIndex);
        }
        public override void RefreshData()
        {
            base.RefreshData();
            bool isHaveData = false;
            if (Data != null && Data.GetCustomDatas != null && Prefab != null)
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    BasePrefabSize = PrefabRect.sizeDelta.y;
                else
                    BasePrefabSize = PrefabRect.sizeDelta.x;

                //重新获取数据
                CacheCustomData = Data.GetCustomDatas.Invoke();
                //数据排序
                var sortCall = GetSortCall();
                if (sortCall != null && CacheCustomData is List<object> listCustomData)
                {
                    var sortBy = SortBy;
                    if (SortReversedList)
                        CacheCustomData = listCustomData.OrderByDescending(sortCall).ToList();
                    else
                        CacheCustomData = listCustomData.OrderBy(sortCall).ToList();
                }
                isHaveData = true;

                if (CacheCustomData.Count == 0)
                {
                    if (EmptyDesc != null)
                    {
                        EmptyDesc.gameObject.SetActive(true);
                        if (Data.EmptyDesc != null)
                            EmptyDesc.text = Data.EmptyDesc?.Invoke();
                        else
                            EmptyDesc.text = GetStr(Data.EmptyDescKey);
                        //if(EmptyDesc.supportRichText)
                        //    EmptyDesc.text = EmptyDesc.text.Replace("\\n", "\n");
                    }
                }
                else
                    EmptyDesc?.gameObject.SetActive(false);
            }

            //重新加载数据
            if (isHaveData)
            {
                //重载数据
                reloadData(NormalizedScrollPosition);
                if (Scrollbar != null)
                    Scrollbar.size = ScrollBarSize;
                LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
                Callback_OnRelaodedData?.Invoke();
            }
        }
        public override void RefreshCell()
        {
            base.RefreshCell();
            if (activeCells.data == null) return;
            foreach (var item in activeCells.data)
            {
                if (item == null) continue;
                RefreshCell(item);
            }
        }
        public virtual void RefreshCell(UControl control, bool onlyRefreshSelect = false)
        {
            if (control is UScrollGroup scrollGroup)
            {
                foreach (var item in scrollGroup.Cells)
                {
                    RefreshSingle(item);
                }
            }
            else
            {
                RefreshSingle(control);
            }

            void RefreshSingle(UControl single)
            {
                single.SetActive(single.DataIndex< CacheCustomData.Count);

                if (single.GO.activeSelf)
                {
                    //刷新基类
                    if (GetIsToggleGroup())
                    {
                        if (single is UCheckBox checkBox && checkBox.IsToggleGroup)
                            checkBox.RefreshStateAndActiveEffectBySelect();
                    }
                    if (onlyRefreshSelect)
                        return;
                    //通过用户自定义方发刷新
                    var customData = CacheCustomData[single.DataIndex];
                    single.SetCustomData(customData);
                    Data.OnRefresh?.Invoke(single, customData);
                }
            }
        }
        #endregion

        #region Init
        //public void Init(UScrollData data,Func<IList> getData, Callback<object, object> onRefresh, List<Func<object, object>> sorter = null, Callback<int> onItemClick = null, string emptyKey = "")
        //{
        //    data.GetCustomDatas = getData;
        //    data.OnRefresh = onRefresh;
        //    data.Sorter = sorter;
        //    data.OnSelectItem = onItemClick;
        //    data.EmptyDescStr = emptyKey;
        //    Init(data);
        //}
        public void Init(Func<IList> getData, Callback<object, object> onRefresh, string emptyKey)
        {
            var temp = new UScrollData { GetCustomDatas = getData, OnRefresh = onRefresh, EmptyDescKey = emptyKey };
            Init(temp);
        }
        public void Init(Func<IList> getData, Callback<object, object> onRefresh, List<Func<object, object>> sorter = null, Callback<int> onItemClick = null, string emptyKey = "")
        {
            var temp = new UScrollData { GetCustomDatas = getData, OnRefresh = onRefresh, Sorter = sorter, OnSelectItem = onItemClick, EmptyDescKey = emptyKey };
            Init(temp);
        }
        // 初始化Scroll
        public override void Init(UScrollData data)
        {
            base.Init(data);
            if (Prefab == null)
            {
                CLog.Error("Scroll:{0} 基础Prefab不能为Null", name);
                return;
            }
            if (Prefab.name.StartsWith(Const.STR_Base)) CLog.Error($"无法使用基础UI Prefab初始化:{Prefab.name}");
            if (Data.GetCustomDatas == null) CLog.Error("TableData 的 GetCustomDatas 必须设置");
            if (Data.OnRefresh == null) CLog.Error("TableData 的 OnRefresh 必须设置");
        }
        #endregion

        #region Set(Core)

        public void SortData(int by, bool isDirtyData = true)
        {
            if (by > Data.Sorter.Count)
                CLog.Error("ListSort,数组越界!!!");
            if (SortBy == by)
                SortReversedList = !SortReversedList;
            else if (SortBy == -1)
                SortReversedList = true;
            else
                SortReversedList = true;

            SelectIndex(-1);
            SortBy = by;
            if (isDirtyData)
                SetDirtyData();
        }
        public void SetDirtyLayout()
        {
            RefreshLayoutTimer.Restart();
        }
        public override void SetDirtyData()
        {
            base.SetDirtyData();
            SelectIndex(-1);
        }
        public void ToggleLoop() => Loop = !loop;
        #endregion

        #region Select
        object CurSelectData = null;
        public void SelectData(object data) => CurSelectData = data;
        public void SelectData(int index)
        {
            CurSelectData = GetData<object>(index);
        }
        public void SelectIndex(int index)
        {
            PreSelectIndex = CurSelectIndex;
            if (GetIsAutoSelect() && index <= -1) CurSelectIndex = 0;
            else CurSelectIndex = index;
            Callback_OnClickSelected?.Invoke(CurSelectIndex);
        }
        public void SelectItem(UControl arg1)
        {
            SelectIndex(arg1.Index);
            Data?.OnSelectItem?.Invoke(arg1.Index);
            if (arg1 is UCheckBox)
            {
                Refresh();
                //SetDirtyRefresh();
            }
        }
        #endregion

        #region get
        public Func<object, object> GetSortCall()
        {
            if (Data.Sorter == null)
                return null;
            if (Data.Sorter.Count <= SortBy)
                return null;
            if (SortBy == -1)
                return null;
            return Data.Sorter[SortBy];
        }
        // 获得控件数据
        public T GetData<T>(int dataIndex) where T : class
        {
            if (dataIndex < 0)
                return null;
            if (dataIndex >= CacheCustomData.Count)
                return null;
            return CacheCustomData[dataIndex] as T;
        }
        public T GetData<T>() where T : class
        {
            if (CurSelectIndex.IsInv())
                return default(T);
            return GetData<T>(CurSelectIndex);
        }
        public int GetDataIndex(object obj)
        {
            if (CacheCustomData == null)
                return 0;
            return CacheCustomData.IndexOf(obj);
        }
        public float GetExpendSize()
        {
            if (IsForceExpend)
                return BasePrefabSize;
            return ExpendSize;
        }
        public bool GetIsToggleGroup()=> IsToggleGroup;
        public bool GetIsAutoSelect()=> IsAutoSelect;
        //更具Index获得Cell
        public TCell GetCell<TCell>(int index) 
            where TCell :UControl
        {
            if (index < 0)
                return null;
            if (activeCells.Count > index)
            {
                //获得第一个
                var first = activeCells[0];
                if (first is UScrollGroup fistGroup)
                {
                    var groupCount = fistGroup.Cells.Count;
                    var groupIndex = (int)(index / groupCount);
                    var group = activeCells[groupIndex] as UScrollGroup;
                    var cellIndex = 0;
                    if (index >= groupCount)
                        cellIndex = index - groupCount;
                    else
                        cellIndex = index;
                    return group.Cells[cellIndex] as TCell;
                }
                else
                {
                    return activeCells[index] as TCell;
                }
            }
            return null;
        }
        public TCell FindCell<TCell>(Func<TCell,bool> condition)
            where TCell : UControl
        {
            foreach (var item in activeCells.data)
            {
                if (item == null)
                    continue;
                if (!(item is TCell))
                {
                    CLog.Error($"错误item 不是{typeof(TCell).Name}类型");
                    return null;
                }
                if (condition(item as TCell))
                    return item as TCell;
            }
            return null;
        }
        public TCell FindCell<TCell,TCustomData>(Func<TCell, TCustomData, bool> condition)
            where TCell : UControl
            where TCustomData : class,new()
        {
            foreach (var item in activeCells.data)
            {
                if (item == null)
                    continue;
                if (!(item is TCell))
                {
                    CLog.Error($"错误item 不是{typeof(TCell).Name}类型");
                    return null;
                }
                if (condition(item as TCell,item.CustomData as TCustomData))
                    return item as TCell;
            }
            return null;
        }
        #endregion

        #region Inspector
        bool Inspector_HideAutoSelect()
        {
            return !IsToggleGroup;
        }
        bool Inspector_IsPrefab()
        {
            return transform.childCount <= 0;
        }
        [Button("统一名称")]
        public void ModifyName()
        {
            Util.UnifyChildName(Content.gameObject);
        }
        #endregion
    }

}