//------------------------------------------------------------------------------
// BaseScrollCore.cs
// Copyright 2019 2019/5/27 
// Created by CYM on 2019/5/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    public partial class UScroll
    {
        #region const
        const string NameRecycledCells = "RecycledCells";
        const string NameFirstPadder = "FirstPadder";
        const string NameLastPadder = "LastPadder";
        #endregion

        #region life
        void InitializesScroller()
        {
            if (initialized)
                return;
            ClearChilds();
            GameObject contentGO = Content.gameObject;
            ScrollRect = GO.SafeAddComponet<ScrollRect>();
            ScrollRect.content = Content;
            ScrollRect.inertia = true;
            ScrollRect.decelerationRate = 0.2f;
            ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            ScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            ScrollRect.movementType = MovementType;
            ScrollRect.scrollSensitivity = 20;
            scrollRectTransform = ScrollRect?.GetComponent<RectTransform>();
            if (Scrollbar != null)
                ScrollBarSize = Scrollbar.size;

            // set up the last values for updates
            lastScrollbarVisibility = scrollbarVisibility;
            lastScrollRectSize = ScrollRectSize;
            lastLoop = loop;

            // 设置背景阻挡,这个很重要,因为ScrollView需要相应Drag事件
            Image bg = GetComponent<Image>();
            if (bg != null) bg.raycastTarget = true;

            // 删除任何Layout
            LayoutGroup tempLayoutGroup = contentGO.GetComponent<LayoutGroup>();
            if (tempLayoutGroup != null)
            {
                if (tempLayoutGroup is HorizontalOrVerticalLayoutGroup hvLayout)
                {
                    spacing = hvLayout.spacing;
                    padding = hvLayout.padding;
                    Anchor = hvLayout.childAlignment;
                }
                DestroyImmediate(tempLayoutGroup);
            }

            // 添加Layout
            if (scrollDirection == ScrollDirectionEnum.Vertical) 
                layoutGroup = contentGO.SafeAddComponet<VerticalLayoutGroup>();
            else 
                layoutGroup = contentGO.SafeAddComponet<HorizontalLayoutGroup>();

            // 设置 Layout 参数
            layoutGroup.spacing = spacing;
            layoutGroup.padding = padding;
            layoutGroup.childAlignment = Anchor;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                ScrollRect.verticalScrollbar = Scrollbar;
                ScrollRect.vertical = true;
                ScrollRect.horizontal = false;
            }
            else
            {
                ScrollRect.horizontalScrollbar = Scrollbar;
                ScrollRect.vertical = false;
                ScrollRect.horizontal = true;
            }

            // create the recycled cell view container
            contentGO = new GameObject(NameRecycledCells, typeof(RectTransform));
            contentGO.transform.SetParent(ScrollRect.transform, false);
            recycledCellContainer = contentGO.GetComponent<RectTransform>();
            recycledCellContainer.gameObject.SetActive(false);

            // 获得所有的子节点
            List<Transform> childTrans = new List<Transform>();
            for (int i = 0; i < Content.childCount; ++i)
            {
                childTrans.Add(Content.GetChild(i));
            }
            // 回收所有有效的节点
            foreach(var item in childTrans)
            {
                var control = item.GetComponent<UControl>();
                if (control == null) continue;
                if (Prefab == null)
                {
                    Prefab = control;
                    UpdatePrefabChildCell();
                }
                RecycleCell(control);
            }

            // 判断Prefab是否为空
            if (Prefab == null)
            {
                CLog.Error("Scroll的BasePrefab不能为空,Error gameobject:{0}", gameObject.name);
                return;
            }
            else UpdatePrefabChildCell();

            // create the padder objects
            contentGO = new GameObject(NameFirstPadder, typeof(RectTransform), typeof(LayoutElement));
            contentGO.transform.SetParent(Content, false);
            firstPadder = contentGO.GetComponent<LayoutElement>();

            contentGO = new GameObject(NameLastPadder, typeof(RectTransform), typeof(LayoutElement));
            contentGO.transform.SetParent(Content, false);
            lastPadder = contentGO.GetComponent<LayoutElement>();

            // 设置BasePrefabSize
            PrefabRect = Prefab.transform as RectTransform;
            if (scrollDirection == ScrollDirectionEnum.Vertical) 
                BasePrefabSize = PrefabRect.sizeDelta.y;
            else 
                BasePrefabSize = PrefabRect.sizeDelta.x;

            // 创建Item
            float needCount = (ScrollRectSize / BasePrefabSize) + 5 - recycledCells.Count;
            for (int i = 0; i <= needCount; ++i)
            {
                RecycleCell(CreateCell(Prefab));
            }

            initialized = true;
        }
        void UpdatePrefabChildCell()
        {
            if (Prefab is UScrollGroup scrollGroup)
            {
                scrollGroup.FetchChildCell();
                NumOfGroupCell = Mathf.Clamp(scrollGroup.Count, 1, int.MaxValue);
            }
        }
        protected override void OnEnable()
        {
            ScrollRect?.onValueChanged.AddListener(ScrollRect_OnValueChanged);
        }

        protected override void OnDisable()
        {
            ScrollRect?.onValueChanged.RemoveListener(ScrollRect_OnValueChanged);
        }
        new void OnValidate()
        {
            // if spacing changed, update it
            if (initialized && spacing != layoutGroup.spacing)
            {
                updateSpacing = true;
            }
        }
        #endregion

        #region data
        // This resets the internal size list and refreshes the cell views
        void reloadData(float scrollPositionFactor = 0)
        {
            // recycle all the active cells so
            // that we are sure to get fresh views
            RecycleAllCells();

            // if we have a delegate handling our data, then
            // call the resize
            //if (_delegate != null)
            Resize(false);

            if (ScrollRect == null || scrollRectTransform == null || Content == null)
            {
                scrollPosition = 0f;
                return;
            }

            scrollPosition = Mathf.Clamp(scrollPositionFactor * ScrollSize, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionEnum.Before));
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                // set the vertical position
                ScrollRect.verticalNormalizedPosition = 1 - scrollPositionFactor;
            }
            else
            {
                // set the horizontal position
                ScrollRect.horizontalNormalizedPosition = scrollPositionFactor;
            }
        }
        public void Move(float val = 0)
        {
            float target = ScrollPosition + val;
            if (tweenMove != null) DOTween.Kill(tweenMove);
            tweenMove = DOTween.To(() => ScrollPosition, x => ScrollPosition = x, target, 0.5f);
            //ScrollPosition += val;
        }
        public void MoveNegative()
        {
            Move(-1 * BasePrefabSize);
        }
        public void MovePositive()
        {
            Move(BasePrefabSize);
        }
        public void JumpToData(object obj,
            float scrollerOffset = 0,
            float cellOffset = 0,
            bool useSpacing = true,
            TweenType tweenType = TweenType.immediate,
            float tweenTime = 0f,
            Action jumpComplete = null,
            LoopJumpDirectionEnum loopJumpDirection = LoopJumpDirectionEnum.Closest)
        {
            var index = GetDataIndex(obj);
            JumpToDataIndex(index, scrollerOffset, cellOffset, useSpacing, tweenType, tweenTime, jumpComplete, loopJumpDirection);
        }
        /// <summary>
        /// Jump to a position in the scroller based on a dataIndex. This overload allows you
        /// to specify a specific offset within a cell as well.
        /// </summary>
        /// <param name="dataIndex">he data index to jump to</param>
        /// <param name="scrollerOffset">The offset from the start (top / left) of the scroller in the range 0..1.
        /// Outside this range will jump to the location before or after the scroller's viewable area</param>
        /// <param name="cellOffset">The offset from the start (top / left) of the cell in the range 0..1</param>
        /// <param name="useSpacing">Whether to calculate in the spacing of the scroller in the jump</param>
        /// <param name="tweenType">What easing to use for the jump</param>
        /// <param name="tweenTime">How long to interpolate to the jump point</param>
        /// <param name="jumpComplete">This delegate is fired when the jump completes</param>
        public void JumpToDataIndex(int dataIndex,
            float scrollerOffset = 0,
            float cellOffset = 0,
            bool useSpacing = true,
            TweenType tweenType = TweenType.immediate,
            float tweenTime = 0f,
            Action jumpComplete = null,
            LoopJumpDirectionEnum loopJumpDirection = LoopJumpDirectionEnum.Closest
            )
        {
            var cellOffsetPosition = 0f;

            if (cellOffset != 0)
            {
                // calculate the cell offset position

                // get the cell's size
                var cellSize = GetCellSize(dataIndex);

                if (useSpacing)
                {
                    // if using spacing add spacing from one side
                    cellSize += spacing;

                    // if this is not a bounday cell, then add spacing from the other side
                    if (dataIndex > 0 && dataIndex < (GetNumberOfCells() - 1)) cellSize += spacing;
                }

                // calculate the position based on the size of the cell and the offset within that cell
                cellOffsetPosition = cellSize * cellOffset;
            }

            if (scrollerOffset == 1f)
            {
                cellOffsetPosition += padding.bottom;
            }

            // cache the offset for quicker calculation
            var offset = -(scrollerOffset * ScrollRectSize) + cellOffsetPosition;

            var newScrollPosition = 0f;

            if (loop)
            {
                // if looping, then we need to determine the closest jump position.
                // we do that by checking all three sets of data locations, and returning the closest one

                // get the scroll positions for each data set.
                // Note: we are calculating the position based on the cell view index, not the data index here
                var set1Position = GetScrollPositionForCellIndex(dataIndex, CellViewPositionEnum.Before) + offset;
                var set2Position = GetScrollPositionForCellIndex(dataIndex + GetNumberOfCells(), CellViewPositionEnum.Before) + offset;
                var set3Position = GetScrollPositionForCellIndex(dataIndex + (GetNumberOfCells() * 2), CellViewPositionEnum.Before) + offset;

                // get the offsets of each scroll position from the current scroll position
                var set1Diff = (Mathf.Abs(scrollPosition - set1Position));
                var set2Diff = (Mathf.Abs(scrollPosition - set2Position));
                var set3Diff = (Mathf.Abs(scrollPosition - set3Position));

                switch (loopJumpDirection)
                {
                    case LoopJumpDirectionEnum.Closest:

                        // choose the smallest offset from the current position (the closest position)
                        if (set1Diff < set2Diff)
                        {
                            if (set1Diff < set3Diff)
                            {
                                newScrollPosition = set1Position;
                            }
                            else
                            {
                                newScrollPosition = set3Position;
                            }
                        }
                        else
                        {
                            if (set2Diff < set3Diff)
                            {
                                newScrollPosition = set2Position;
                            }
                            else
                            {
                                newScrollPosition = set3Position;
                            }
                        }

                        break;

                    case LoopJumpDirectionEnum.Up:

                        newScrollPosition = set1Position;
                        break;

                    case LoopJumpDirectionEnum.Down:

                        newScrollPosition = set3Position;
                        break;

                }
            }
            else
            {
                // not looping, so just get the scroll position from the dataIndex
                newScrollPosition = GetScrollPositionForDataIndex(dataIndex, CellViewPositionEnum.Before) + offset;
            }

            // clamp the scroll position to a valid location
            newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionEnum.Before));

            // if spacing is used, adjust the final position
            if (useSpacing)
            {
                // move back by the spacing if necessary
                newScrollPosition = Mathf.Clamp(newScrollPosition - spacing, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionEnum.Before));
            }

            // ignore the jump if the scroll position hasn't changed
            if (newScrollPosition == scrollPosition)
            {
                jumpComplete?.Invoke();
                return;
            }

            // start tweening
            StartCoroutine(TweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete));
        }
        #endregion

        #region protected
        protected virtual UControl GetCell(int dataIndex, int cellIndex)
        {
            if (Prefab == null)
            {
                throw new NotImplementedException("没有BasePrefab");
            }
            UControl cellPresenter = GetCell(Prefab);
            return cellPresenter;
        }

        protected virtual float GetCellSize(int dataIndex)
        {
            return BasePrefabSize;
        }

        protected virtual int GetNumberOfCells()
        {
            return Mathf.CeilToInt((float)CacheCustomData.Count / (float)NumOfGroupCell);
            //return CacheCustomData.Count / NumOfGroupCell;
        }
        #endregion

        #region private
        /// <summary>
        /// The absolute position in pixels from the start of the scroller
        /// </summary>
        float ScrollPosition
        {
            get
            {
                return scrollPosition;
            }
            set
            {
                // make sure the position is in the bounds of the current set of views
                value = Mathf.Clamp(value, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionEnum.Before));
                // only if the value has changed
                if (scrollPosition != value)
                {
                    scrollPosition = value;
                    if (scrollDirection == ScrollDirectionEnum.Vertical)
                        ScrollRect.verticalNormalizedPosition = 1 - (scrollPosition / ScrollSize);
                    else
                        ScrollRect.horizontalNormalizedPosition = (scrollPosition / ScrollSize);
                }
            }
        }
        /// <summary>
        /// The size of the active cell view container minus the visibile portion
        /// of the scroller
        /// </summary>
        float ScrollSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    return Mathf.Max(Content.rect.height - scrollRectTransform.rect.height, 0);
                else
                    return Mathf.Max(Content.rect.width - scrollRectTransform.rect.width, 0);
            }
        }

        /// <summary>
        /// The normalized position of the scroller between 0 and 1
        /// </summary>
        float NormalizedScrollPosition
        {
            get
            {
                var scrollPosition = ScrollPosition;
                return (scrollPosition <= 0 ? 0 : this.scrollPosition / ScrollSize);
            }
        }

        /// <summary>
        /// Whether the scroller should loop the resulting cell views.
        /// Looping creates three sets of internal size data, attempting
        /// to keep the scroller in the middle set. If the scroller goes
        /// outside of this set, it will jump back into the middle set,
        /// giving the illusion of an infinite set of data.
        /// </summary>
        bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                // only if the value has changed
                if (loop != value)
                {
                    // get the original position so that when we turn looping on
                    // we can jump back to this position
                    var originalScrollPosition = scrollPosition;

                    loop = value;

                    // call resize to generate more internal elements if loop is on,
                    // remove the elements if loop is off
                    Resize(false);

                    if (loop)
                    {
                        // set the new scroll position based on the middle set of data + the original position
                        ScrollPosition = loopFirstScrollPosition + originalScrollPosition;
                    }
                    else
                    {
                        // set the new scroll position based on the original position and the first loop position
                        ScrollPosition = originalScrollPosition - loopFirstScrollPosition;
                    }

                    // update the scrollbars
                    ScrollbarVisibility = scrollbarVisibility;
                }
            }
        }

        /// <summary>
        /// Sets how the visibility of the scrollbars should be handled
        /// </summary>
        ScrollbarVisibilityEnum ScrollbarVisibility
        {
            get
            {
                return scrollbarVisibility;
            }
            set
            {
                scrollbarVisibility = value;

                // only if the scrollbar exists
                if (Scrollbar != null)
                {
                    // make sure we actually have some cell views
                    if (cellOffsetArray != null && cellOffsetArray.Count > 0)
                    {
                        if (cellOffsetArray.Last() < ScrollRectSize || loop)
                        {
                            // if the size of the scrollable area is smaller than the scroller
                            // or if we have looping on, hide the scrollbar unless the visibility
                            // is set to Always.
                            Scrollbar.gameObject.SetActive(scrollbarVisibility == ScrollbarVisibilityEnum.Always);
                        }
                        else
                        {
                            // if the size of the scrollable areas is larger than the scroller
                            // or looping is off, then show the scrollbars unless visibility
                            // is set to Never.
                            Scrollbar.gameObject.SetActive(scrollbarVisibility != ScrollbarVisibilityEnum.Never);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is the velocity of the scroller.
        /// </summary>
        Vector2 Velocity
        {
            get
            {
                return ScrollRect.velocity;
            }
            set
            {
                ScrollRect.velocity = value;
            }
        }

        /// <summary>
        /// The linear velocity is the velocity on one axis.
        /// The scroller should only be moving one one axix.
        /// </summary>
        float LinearVelocity
        {
            get
            {
                if (ScrollRect == null)
                    return 0;
                // return the velocity component depending on which direction this is scrolling
                return (scrollDirection == ScrollDirectionEnum.Vertical ? ScrollRect.velocity.y : ScrollRect.velocity.x);
            }
            set
            {
                if (ScrollRect == null)
                    return ;
                // set the appropriate component of the velocity
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    ScrollRect.velocity = new Vector2(0, value);
                }
                else
                {
                    ScrollRect.velocity = new Vector2(value, 0);
                }
            }
        }

        /// <summary>
        /// Whether the scroller is scrolling or not
        /// </summary>
        bool IsScrolling
        {
            get; set;
        }

        /// <summary>
        /// Whether the scroller is tweening or not
        /// </summary>
        bool IsTweening
        {
            get; set;
        }

        /// <summary>
        /// This is the first cell view index showing in the scroller's visible area
        /// </summary>
        int StartCellIndex { get; set; }

        /// <summary>
        /// This is the last cell view index showing in the scroller's visible area
        /// </summary>
        int EndCellIndex { get; set; }

        /// <summary>
        /// This is the first data index showing in the scroller's visible area
        /// </summary>
        int StartDataIndex
        {
            get
            {
                return StartCellIndex % GetNumberOfCells();
            }
        }

        /// <summary>
        /// This is the last data index showing in the scroller's visible area
        /// </summary>
        int EndDataIndex
        {
            get
            {
                return EndCellIndex % GetNumberOfCells();
            }
        }

        /// <summary>
        /// This is a convenience link to the scroller's scroll rect
        /// </summary>
        [ReadOnly]
        public ScrollRect ScrollRect { get;private set; }

        /// <summary>
        /// The size of the visible portion of the scroller
        /// </summary>
        float ScrollRectSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    return scrollRectTransform.rect.height;
                else
                    return scrollRectTransform.rect.width;
            }
        }

        Timer RefreshLayoutTimer { get; set; } = new Timer(0.02f);

        /// <summary>
        /// Create a cell view, or recycle one if it already exists
        /// </summary>
        /// <param name="cellPrefab">The prefab to use to create the cell view</param>
        /// <returns></returns>
        UControl GetCell(UControl cellPrefab)
        {
            // see if there is a view to recycle
            var cell = GetRecycledCell(cellPrefab);
            if (cell == null)
            {
                cell = CreateCell(cellPrefab);
            }
            else
            {
                // call the reused callback
                Callback_CellReused?.Invoke(this, cell);
            }

            return cell;
        }
        UControl CreateCell(UControl cellPrefab)
        {
            UControl cell = null;
            // no recyleable cell found, so we create a new view
            // and attach it to our container
            var go = Instantiate(cellPrefab.gameObject);
            go.name = cellPrefab.name;
            cell = go.GetComponent<UControl>();
            cell.transform.SetParent(Content);
            cell.transform.localPosition = Vector3.zero;
            cell.transform.localRotation = Quaternion.identity;

            // call the instantiated callback
            Callback_CellInstantiated?.Invoke(this, cell);
            if (AddCellChild(cell))
            {
                cell.PScroll = this;
                //if (cell is BaseCheckBox checkBox)
                //    checkBox.SetToggleGroup(IsToggleGroup);
            }
            return cell;
        }
        #endregion

        #region event
        /// <summary>
        /// int1 =当前的index
        /// int2 =上次的index
        /// </summary>
        public event Callback<int, int> Callback_OnSelectChange;
        /// <summary>
        /// 当前选得index
        /// </summary>
        public event Callback<int> Callback_OnClickSelected;
        /// <summary>
        /// This delegate is called when a cell view is hidden or shown
        /// </summary>
        public event Callback<UControl> Callback_CellVisibilityChanged;

        /// <summary>
        /// This delegate is called just before a cell view is hidden by recycling
        /// </summary>
        public event Callback<UControl> Callback_CellWillRecycle;

        /// <summary>
        /// This delegate is called when the scroll rect scrolls
        /// </summary>
        public event Callback<UScroll, Vector2, float> Callback_ScrollerScrolled;

        /// <summary>
        /// This delegate is called when the scroller has snapped to a position
        /// </summary>
        public event Callback<UScroll, int, int, UControl> Callback_ScrollerSnapped;

        /// <summary>
        /// This delegate is called when the scroller has started or stopped scrolling
        /// </summary>
        public event Callback<UScroll, bool> Callback_ScrollerScrollingChanged;

        /// <summary>
        /// This delegate is called when the scroller has started or stopped tweening
        /// </summary>
        public event Callback<UScroll, bool> Callback_ScrollerTweeningChanged;

        /// <summary>
        /// This delegate is called when the scroller creates a new cell view from scratch
        /// </summary>
        public event Callback<UScroll, UControl> Callback_CellInstantiated;

        /// <summary>
        /// This delegate is called when the scroller reuses a recycled cell view
        /// </summary>
        public event Callback<UScroll, UControl> Callback_CellReused;

        #endregion

        #region private method
        /// <summary>
        /// Removes all cells, both active and recycled from the scroller.
        /// This will call garbage collection.
        /// </summary>
        void ClearAll()
        {
            ClearActive();
            ClearRecycled();
        }

        /// <summary>
        /// Removes all the active cell views. This should only be used if you want
        /// to get rid of cells because of settings set by Unity that cannot be
        /// changed at runtime. This will call garbage collection.
        /// </summary>
        void ClearActive()
        {
            for (var i = 0; i < activeCells.Count; i++)
            {
                DestroyImmediate(activeCells[i].gameObject);
            }
            activeCells.Clear();
        }
        /// <summary>
        /// Removes all the recycled cell views. This should only be used after you
        /// load in a completely different set of cell views that will not use the 
        /// recycled views. This will call garbage collection.
        /// </summary>
        void ClearRecycled()
        {
            for (var i = 0; i < recycledCells.Count; i++)
            {
                DestroyImmediate(recycledCells[i].gameObject);
            }
            recycledCells.Clear();
        }
        /// <summary>
        /// Snaps the scroller on command. This is called internally when snapping is set to true and the velocity
        /// has dropped below the threshold. You can use this to manually snap whenever you like.
        /// </summary>
        void Snap()
        {
            if (GetNumberOfCells() == 0) return;

            // set snap jumping to true so other events won't process while tweening
            snapJumping = true;

            // stop the scroller
            LinearVelocity = 0;

            // cache the current inertia state and turn off inertia
            snapInertia = ScrollRect.inertia;
            ScrollRect.inertia = false;

            // calculate the snap position
            var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01(snapWatchOffset));

            // get the cell view index of cell at the watch location
            snapCellViewIndex = GetCellViewIndexAtPosition(snapPosition);

            // get the data index of the cell at the watch location
            snapDataIndex = snapCellViewIndex % GetNumberOfCells();

            // jump the snapped cell to the jump offset location and center it on the cell offset
            JumpToDataIndex(snapDataIndex, snapJumpToOffset, snapCellCenterOffset, snapUseCellSpacing, snapTweenType, snapTweenTime, SnapJumpComplete);
        }
        /// <summary>
        /// Gets the scroll position in pixels from the start of the scroller based on the cellViewIndex
        /// </summary>
        /// <param name="cellViewIndex">The cell index to look for. This is used instead of dataIndex in case of looping</param>
        /// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
        /// <returns></returns>
        float GetScrollPositionForCellIndex(int cellViewIndex, CellViewPositionEnum insertPosition)
        {
            if (GetNumberOfCells() == 0) return 0;
            if (cellViewIndex < 0) cellViewIndex = 0;

            if (cellViewIndex == 0 && insertPosition == CellViewPositionEnum.Before)
            {
                return 0;
            }
            else
            {
                if (cellViewIndex < cellOffsetArray.Count)
                {
                    // the index is in the range of cell view offsets

                    if (insertPosition == CellViewPositionEnum.Before)
                    {
                        // return the previous cell view's offset + the spacing between cell views
                        return cellOffsetArray[cellViewIndex - 1] + spacing + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left);
                    }
                    else
                    {
                        // return the offset of the cell view (offset is after the cell)
                        return cellOffsetArray[cellViewIndex] + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left);
                    }
                }
                else
                {
                    // get the start position of the last cell (the offset of the second to last cell)
                    return cellOffsetArray[cellOffsetArray.Count - 2];
                }
            }
        }
        /// <summary>
        /// Gets the scroll position in pixels from the start of the scroller based on the dataIndex
        /// </summary>
        /// <param name="dataIndex">The data index to look for</param>
        /// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
        /// <returns></returns>
        float GetScrollPositionForDataIndex(int dataIndex, CellViewPositionEnum insertPosition)
        {
            return GetScrollPositionForCellIndex(loop ? GetNumberOfCells() + dataIndex : dataIndex, insertPosition);
        }
        /// <summary>
        /// Gets the index of a cell view at a given position
        /// </summary>
        /// <param name="position">The pixel offset from the start of the scroller</param>
        /// <returns></returns>
        int GetCellViewIndexAtPosition(float position)
        {
            // call the overrloaded method on the entire range of the list
            return GetCellIndexAtPosition(position, 0, cellOffsetArray.Count - 1);
        }
        /// <summary>
        /// Get a cell view for a particular data index. If the cell view is not currently
        /// in the visible range, then this method will return null.
        /// Note: this is against MVC principles and will couple your controller to the view
        /// more than this paradigm would suggest. Generally speaking, the view can have knowledge
        /// about the controller, but the controller should not know anything about the view.
        /// Use this method sparingly if you are trying to adhere to strict MVC design.
        /// </summary>
        /// <param name="dataIndex">The data index of the cell view to return</param>
        /// <returns></returns>
        UControl GetCellAtDataIndex(int dataIndex)
        {
            for (var i = 0; i < activeCells.Count; i++)
            {
                if (activeCells[i].DataIndex == dataIndex)
                {
                    return activeCells[i];
                }
            }

            return null;
        }
        UControl GetCellAtIndex(int index)
        {
            for (var i = 0; i < activeCells.Count; i++)
            {
                if (activeCells[i].Index == index)
                {
                    return activeCells[i];
                }
            }

            return null;
        }
        #endregion

        #region Private
        /// <summary>
        /// Set after the scroller is first created. This allwos
        /// us to ignore OnValidate changes at the start
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Set when the spacing is changed in the inspector. Since we cannot
        /// make changes during the OnValidate, we have to use this flag to
        /// later call the _UpdateSpacing method from Update()
        /// </summary>
        private bool updateSpacing = false;

        /// <summary>
        /// Cached reference to the scrollRect's transform
        /// </summary>
        private RectTransform scrollRectTransform;

        /// <summary>
        /// Cached reference to the layout group that handles view positioning
        /// </summary>
        private HorizontalOrVerticalLayoutGroup layoutGroup;

        /// <summary>
        /// Flag to tell the scroller to refresh the active list of cell views
        /// </summary>
        private bool refreshActive;

        /// <summary>
        /// List of views that have been recycled
        /// </summary>
        private SmallList<UControl> recycledCells = new SmallList<UControl>();

        /// <summary>
        /// Cached reference to the element used to offset the first visible cell view
        /// </summary>
        private LayoutElement firstPadder;

        /// <summary>
        /// Cached reference to the element used to keep the cell views at the correct size
        /// </summary>
        private LayoutElement lastPadder;

        /// <summary>
        /// Cached reference to the container that holds the recycled cell views
        /// </summary>
        private RectTransform recycledCellContainer;

        /// <summary>
        /// Internal list of cell view sizes. This is created when the data is reloaded 
        /// to speed up processing.
        /// </summary>
        private SmallList<float> cellSizeArray = new SmallList<float>();

        /// <summary>
        /// Internal list of cell view offsets. Each cell view offset is an accumulation 
        /// of the offsets previous to it.
        /// This is created when the data is reloaded to speed up processing.
        /// </summary>
        private SmallList<float> cellOffsetArray = new SmallList<float>();

        /// <summary>
        /// The scrollers position
        /// </summary>
        private float scrollPosition;

        /// <summary>
        /// The list of cell views that are currently being displayed
        /// </summary>
        private SmallList<UControl> activeCells = new SmallList<UControl>();

        /// <summary>
        /// The index of the first element of the middle section of cell view sizes.
        /// Used only when looping
        /// </summary>
        private int loopFirstCellIndex;

        /// <summary>
        /// The index of the last element of the middle seciton of cell view sizes.
        /// used only when looping
        /// </summary>
        private int loopLastCellIndex;

        /// <summary>
        /// The scroll position of the first element of the middle seciotn of cell views.
        /// Used only when looping
        /// </summary>
        private float loopFirstScrollPosition;

        /// <summary>
        /// The scroll position of the last element of the middle section of cell views.
        /// Used only when looping
        /// </summary>
        private float loopLastScrollPosition;

        /// <summary>
        /// The position that triggers the scroller to jump to the end of the middle section
        /// of cell views. This keeps the scroller in the middle section as much as possible.
        /// </summary>
        private float loopFirstJumpTrigger;

        /// <summary>
        /// The position that triggers the scroller to jump to the start of the middle section
        /// of cell views. This keeps the scroller in the middle section as much as possible.
        /// </summary>
        private float loopLastJumpTrigger;

        /// <summary>
        /// The cached value of the last scroll rect size. This is checked every frame to see
        /// if the scroll rect has resized. If so, it will refresh.
        /// </summary>
        private float lastScrollRectSize;

        /// <summary>
        /// The cached value of the last loop setting. This is checked every frame to see
        /// if looping was toggled. If so, it will refresh.
        /// </summary>
        private bool lastLoop;

        /// <summary>
        /// The cell view index we are snapping to
        /// </summary>
        private int snapCellViewIndex;

        /// <summary>
        /// The data index we are snapping to
        /// </summary>
        private int snapDataIndex;

        /// <summary>
        /// Whether we are currently jumping due to a snap
        /// </summary>
        private bool snapJumping;

        /// <summary>
        /// What the previous inertia setting was before the snap jump.
        /// We cache it here because we need to turn off inertia while
        /// manually tweeing.
        /// </summary>
        private bool snapInertia;

        /// <summary>
        /// The cached value of the last scrollbar visibility setting. This is checked every
        /// frame to see if the scrollbar visibility needs to be changed.
        /// </summary>
        private ScrollbarVisibilityEnum lastScrollbarVisibility;

        /// <summary>
        /// Where in the list we are
        /// </summary>
        private enum ListPositionEnum
        {
            First,
            Last
        }

        /// <summary>
        /// This function will create an internal list of sizes and offsets to be used in all calculations.
        /// It also sets up the loop triggers and positions and initializes the cell views.
        /// </summary>
        /// <param name="keepPosition">If true, then the scroller will try to go back to the position it was at before the resize</param>
        private void Resize(bool keepPosition)
        {
            // cache the original position
            var originalScrollPosition = scrollPosition;

            // clear out the list of cell view sizes and create a new list
            cellSizeArray.Clear();
            var offset = AddCellSizes();

            // if looping, we need to create three sets of size data
            if (loop)
            {
                // if the cells don't entirely fill up the scroll area, 
                // make some more size entries to fill it up
                if (offset < ScrollRectSize)
                {
                    int additionalRounds = Mathf.CeilToInt(ScrollRectSize / offset);
                    DuplicateCellSizes(additionalRounds, cellSizeArray.Count);
                }

                // set up the loop indices
                loopFirstCellIndex = cellSizeArray.Count;
                loopLastCellIndex = loopFirstCellIndex + cellSizeArray.Count - 1;

                // create two more copies of the cell sizes
                DuplicateCellSizes(2, cellSizeArray.Count);
            }

            // calculate the offsets of each cell view
            CalculateCellOffsets();

            // set the size of the active cell view container based on the number of cell views there are and each of their sizes
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                if (ScrollRectSize > cellOffsetArray.Last())//没有充满
                {
                    Content.sizeDelta = new Vector2(Content.sizeDelta.x, ScrollRectSize  + padding.top + padding.bottom + GetExpendSize());
                }
                else //充满
                {
                    Content.sizeDelta = new Vector2(Content.sizeDelta.x, cellOffsetArray.Last() + GetExpendSize() + padding.top + padding.bottom);
                }
            }
            else
            {
                if (ScrollRectSize > cellOffsetArray.Last())//没有充满
                {
                    Content.sizeDelta = new Vector2(ScrollRectSize + padding.left + padding.right + GetExpendSize(), Content.sizeDelta.y);
                }
                else //充满
                {
                    Content.sizeDelta = new Vector2(cellOffsetArray.Last() + GetExpendSize() + padding.left + padding.right, Content.sizeDelta.y);
                }
            }
            // if looping, set up the loop positions and triggers
            if (loop)
            {
                loopFirstScrollPosition = GetScrollPositionForCellIndex(loopFirstCellIndex, CellViewPositionEnum.Before) + (spacing * 0.5f);
                loopLastScrollPosition = GetScrollPositionForCellIndex(loopLastCellIndex, CellViewPositionEnum.After) - ScrollRectSize + (spacing * 0.5f);

                loopFirstJumpTrigger = loopFirstScrollPosition - ScrollRectSize;
                loopLastJumpTrigger = loopLastScrollPosition + ScrollRectSize;
            }

            // create the visibile cells
            ResetVisibleCells();

            // if we need to maintain our original position
            if (keepPosition)
            {
                ScrollPosition = originalScrollPosition;
            }
            else
            {
                if (loop)
                {
                    ScrollPosition = loopFirstScrollPosition;
                }
                else
                {
                    ScrollPosition = 0;
                }
            }

            // set up the visibility of the scrollbar
            ScrollbarVisibility = scrollbarVisibility;
        }

        /// <summary>
        /// Updates the spacing on the scroller
        /// </summary>
        /// <param name="spacing">new spacing value</param>
        private void UpdateSpacing(float spacing)
        {
            updateSpacing = false;
            layoutGroup.spacing = spacing;
            reloadData(NormalizedScrollPosition);
        }

        /// <summary>
        /// Creates a list of cell view sizes for faster access
        /// </summary>
        /// <returns></returns>
        private float AddCellSizes()
        {
            var offset = 0f;
            // add a size for each row in our data based on how many the delegate tells us to create
            for (var i = 0; i < GetNumberOfCells(); i++)
            {
                // add the size of this cell based on what the delegate tells us to use. Also add spacing if this cell isn't the first one
                cellSizeArray.Add(GetCellSize(i) + (i == 0 ? 0 : layoutGroup.spacing));
                offset += cellSizeArray[cellSizeArray.Count - 1];
            }

            return offset;
        }

        /// <summary>
        /// Create a copy of the cell view sizes. This is only used in looping
        /// </summary>
        /// <param name="numberOfTimes">How many times the copy should be made</param>
        /// <param name="cellCount">How many cells to copy</param>
        private void DuplicateCellSizes(int numberOfTimes, int cellCount)
        {
            for (var i = 0; i < numberOfTimes; i++)
            {
                for (var j = 0; j < cellCount; j++)
                {
                    cellSizeArray.Add(cellSizeArray[j] + (j == 0 ? layoutGroup.spacing : 0));
                }
            }
        }

        /// <summary>
        /// Calculates the offset of each cell, accumulating the values from previous cells
        /// </summary>
        private void CalculateCellOffsets()
        {
            cellOffsetArray.Clear();
            var offset = 0f;
            for (var i = 0; i < cellSizeArray.Count; i++)
            {
                offset += cellSizeArray[i];
                cellOffsetArray.Add(offset);
            }
        }

        /// <summary>
        /// Get a recycled cell with a given identifier if available
        /// </summary>
        /// <param name="cellPrefab">The prefab to check for</param>
        /// <returns></returns>
        private UControl GetRecycledCell(UControl cellPrefab)
        {
            for (var i = 0; i < recycledCells.Count; i++)
            {
                if (recycledCells[i].GOName == cellPrefab.GOName)
                {
                    // the cell view was found, so we use this recycled one.
                    // we also remove it from the recycled list
                    var cellView = recycledCells.RemoveAt(i);
                    return cellView;
                }
            }

            return null;
        }

        /// <summary>
        /// This sets up the visible cells, adding and recycling as necessary
        /// </summary>
        private void ResetVisibleCells()
        {
            int startIndex;
            int endIndex;

            // calculate the range of the visible cells
            CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            // go through each previous active cell and recycle it if it no longer falls in the range
            var i = 0;
            SmallList<int> remainingCellIndices = new SmallList<int>();
            while (i < activeCells.Count)
            {
                if (activeCells[i].Index < startIndex || activeCells[i].Index > endIndex)
                {
                    RecycleCell(activeCells[i]);
                }
                else
                {
                    // this cell index falls in the new range, so we add its
                    // index to the reusable list
                    remainingCellIndices.Add(activeCells[i].Index);
                    i++;
                }
            }

            if (remainingCellIndices.Count == 0)
            {
                // there were no previous active cells remaining, 
                // this list is either brand new, or we jumped to 
                // an entirely different part of the list.
                // just add all the new cell views

                for (i = startIndex; i <= endIndex; i++)
                {
                    AddCell(i, ListPositionEnum.Last);
                }
            }
            else
            {
                // we are able to reuse some of the previous
                // cell views

                // first add the views that come before the 
                // previous list, going backward so that the
                // new views get added to the front
                for (i = endIndex; i >= startIndex; i--)
                {
                    if (i < remainingCellIndices.First())
                    {
                        AddCell(i, ListPositionEnum.First);
                    }
                }

                // next add teh views that come after the
                // previous list, going forward and adding
                // at the end of the list
                for (i = startIndex; i <= endIndex; i++)
                {
                    if (i > remainingCellIndices.Last())
                    {
                        AddCell(i, ListPositionEnum.Last);
                    }
                }
            }

            // update the start and end indices
            StartCellIndex = startIndex;
            EndCellIndex = endIndex;

            // adjust the padding elements to offset the cell views correctly
            SetPadders();
        }

        /// <summary>
        /// Recycles all the active cells
        /// </summary>
        private void RecycleAllCells()
        {
            while (activeCells.Count > 0) RecycleCell(activeCells[0]);
            StartCellIndex = 0;
            EndCellIndex = 0;
        }

        /// <summary>
        /// Recycles one cell view
        /// </summary>
        /// <param name="cell"></param>
        private void RecycleCell(UControl cell)
        {
            Callback_CellWillRecycle?.Invoke(cell);

            // remove the cell view from the active list
            activeCells.Remove(cell);

            // add the cell view to the recycled list
            recycledCells.Add(cell);

            // move the GameObject to the recycled container
            cell.transform.SetParent(recycledCellContainer);

            // reset the cellView's properties
            cell.SetDataIndex(0);
            cell.SetIndex(0);

            //RemoveChild(cellView);

            Callback_CellVisibilityChanged?.Invoke(cell);
        }

        /// <summary>
        /// Creates a cell view, or recycles if it can
        /// </summary>
        /// <param name="cellIndex">The index of the cell view</param>
        /// <param name="listPosition">Whether to add the cell to the beginning or the end</param>
        private void AddCell(int cellIndex, ListPositionEnum listPosition)
        {
            if (GetNumberOfCells() == 0) return;

            // get the dataIndex. Modulus is used in case of looping so that the first set of cells are ignored
            var dataIndex = cellIndex % GetNumberOfCells();
            // request a cell view from the delegate
            var cell = GetCell(dataIndex, cellIndex);

            // set the cell's properties
            cell.SetIndex(cellIndex);
            cell.SetDataIndex(dataIndex);

            //设置BaseCheckBox
            RefreshCell(cell);

            // add the cell view to the active container
            cell.transform.SetParent(Content, false);
            cell.transform.localScale = Vector3.one;

            // add a layout element to the cellView
            if (cell.LayoutElement == null) //cell.LayoutElement = cell.gameObject.AddComponent<LayoutElement>();
                cell.EnsureLayoutElement();
            LayoutElement layoutElement = cell.LayoutElement;

            // set the size of the layout element
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                layoutElement.minHeight = cellSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);
                layoutElement.minWidth = PrefabRect.sizeDelta.x;
            }
            else
            {
                layoutElement.minWidth = cellSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);
                layoutElement.minHeight = PrefabRect.sizeDelta.y;
            }

            // add the cell to the active list
            if (listPosition == ListPositionEnum.First)
                activeCells.AddStart(cell);
            else
                activeCells.Add(cell);

            // set the hierarchy position of the cell view in the container
            if (listPosition == ListPositionEnum.Last)
                cell.transform.SetSiblingIndex(Content.childCount - 2);
            else if (listPosition == ListPositionEnum.First)
                cell.transform.SetSiblingIndex(1);

            // call the visibility change delegate if available
            Callback_CellVisibilityChanged?.Invoke(cell);
        }

        /// <summary>
        /// This function adjusts the two padders that control the first cell view's
        /// offset and the overall size of each cell.
        /// </summary>
        private void SetPadders()
        {
            if (GetNumberOfCells() == 0) return;

            // calculate the size of each padder
            var firstSize = cellOffsetArray[StartCellIndex] - cellSizeArray[StartCellIndex];
            var lastSize = cellOffsetArray.Last() - cellOffsetArray[EndCellIndex];

            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                // set the first padder and toggle its visibility
                firstPadder.minHeight = firstSize;
                firstPadder.gameObject.SetActive(firstPadder.minHeight > 0);

                // set the last padder and toggle its visibility
                lastPadder.minHeight = lastSize;
                lastPadder.gameObject.SetActive(lastPadder.minHeight > 0);
            }
            else
            {
                // set the first padder and toggle its visibility
                firstPadder.minWidth = firstSize;
                firstPadder.gameObject.SetActive(firstPadder.minWidth > 0);

                // set the last padder and toggle its visibility
                lastPadder.minWidth = lastSize;
                lastPadder.gameObject.SetActive(lastPadder.minWidth > 0);
            }
        }

        /// <summary>
        /// This function is called if the scroller is scrolled, updating the active list of cells
        /// </summary>
        private void RefreshActive()
        {
            //_refreshActive = false;

            int startIndex;
            int endIndex;
            var velocity = Vector2.zero;

            // if looping, check to see if we scrolled past a trigger
            if (loop)
            {
                if (scrollPosition < loopFirstJumpTrigger)
                {
                    velocity = ScrollRect.velocity;
                    ScrollPosition = loopLastScrollPosition - (loopFirstJumpTrigger - scrollPosition) + spacing;
                    ScrollRect.velocity = velocity;
                }
                else if (scrollPosition > loopLastJumpTrigger)
                {
                    velocity = ScrollRect.velocity;
                    ScrollPosition = loopFirstScrollPosition + (scrollPosition - loopLastJumpTrigger) - spacing;
                    ScrollRect.velocity = velocity;
                }
            }

            // get the range of visibile cells
            CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            // if the index hasn't changed, ignore and return
            if (startIndex == StartCellIndex && endIndex == EndCellIndex) return;

            // recreate the visibile cells
            ResetVisibleCells();
        }

        /// <summary>
        /// Determines which cells can be seen
        /// </summary>
        /// <param name="startIndex">The index of the first cell visible</param>
        /// <param name="endIndex">The index of the last cell visible</param>
        private void CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = 0;

            // get the positions of the scroller
            var startPosition = scrollPosition;
            var endPosition = scrollPosition + (scrollDirection == ScrollDirectionEnum.Vertical ? scrollRectTransform.rect.height : scrollRectTransform.rect.width);

            // calculate each index based on the positions
            startIndex = GetCellViewIndexAtPosition(startPosition);
            endIndex = GetCellViewIndexAtPosition(endPosition);
        }

        /// <summary>
        /// Gets the index of a cell at a given position based on a subset range.
        /// This function uses a recursive binary sort to find the index faster.
        /// </summary>
        /// <param name="position">The pixel offset from the start of the scroller</param>
        /// <param name="startIndex">The first index of the range</param>
        /// <param name="endIndex">The last index of the rnage</param>
        /// <returns></returns>
        private int GetCellIndexAtPosition(float position, int startIndex, int endIndex)
        {
            // if the range is invalid, then we found our index, return the start index
            if (startIndex >= endIndex) return startIndex;

            // determine the middle point of our binary search
            var middleIndex = (startIndex + endIndex) / 2;

            // if the middle index is greater than the position, then search the last
            // half of the binary tree, else search the first half
            if ((cellOffsetArray[middleIndex] + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left)) >= position)
                return GetCellIndexAtPosition(position, startIndex, middleIndex);
            else
                return GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
        }

        /// <summary>
        /// Handler for when the scroller changes value
        /// </summary>
        /// <param name="val">The scroll rect's value</param>
        private void ScrollRect_OnValueChanged(Vector2 val)
        {
            // set the internal scroll position
            if (scrollDirection == ScrollDirectionEnum.Vertical)
                scrollPosition = (1f - val.y) * ScrollSize;
            else
                scrollPosition = val.x * ScrollSize;
            //_refreshActive = true;
            scrollPosition = Mathf.Clamp(scrollPosition, 0, GetScrollPositionForCellIndex(cellSizeArray.Count - 1, CellViewPositionEnum.Before));

            // call the handler if it exists
            Callback_ScrollerScrolled?.Invoke(this, val, scrollPosition);

            // if the snapping is turned on, handle it
            if (snapping && !snapJumping)
            {
                // if the speed has dropped below the threshhold velocity
                if (Mathf.Abs(LinearVelocity) <= snapVelocityThreshold && LinearVelocity != 0)
                {
                    // Make sure the scroller is not on the boundary if not looping
                    var normalized = NormalizedScrollPosition;
                    if (loop || (!loop && normalized > 0 && normalized < 1.0f))
                    {
                        // Call the snap function
                        Snap();
                    }
                }
            }

            RefreshActive();

            if (Scrollbar && IsFixedScrollBar)
                Scrollbar.size = ScrollBarSize;
        }

        /// <summary>
        /// This is fired by the tweener when the snap tween is completed
        /// </summary>
        private void SnapJumpComplete()
        {
            // reset the snap jump to false and restore the inertia state
            snapJumping = false;
            ScrollRect.inertia = snapInertia;

            UControl cellView = null;
            for (var i = 0; i < activeCells.Count; i++)
            {
                if (activeCells[i].DataIndex == snapDataIndex)
                {
                    cellView = activeCells[i];
                    break;
                }
            }

            // fire the scroller snapped delegate
            Callback_ScrollerSnapped?.Invoke(this, snapCellViewIndex, snapDataIndex, cellView);
        }

        #endregion

        #region Tweening
        private float _tweenTimeLeft;
        private object tweenMove;

        /// <summary>
        /// Moves the scroll position over time between two points given an easing function. When the
        /// tween is complete it will fire the jumpComplete delegate.
        /// </summary>
        /// <param name="tweenType">The type of easing to use</param>
        /// <param name="time">The amount of time to interpolate</param>
        /// <param name="start">The starting scroll position</param>
        /// <param name="end">The ending scroll position</param>
        /// <param name="jumpComplete">The action to fire when the tween is complete</param>
        /// <returns></returns>
        IEnumerator TweenPosition(TweenType tweenType, float time, float start, float end, Action tweenComplete)
        {
            if (tweenType == TweenType.immediate || time == 0)
            {
                // if the easing is immediate or the time is zero, just jump to the end position
                ScrollPosition = end;
            }
            else
            {
                // zero out the velocity
                ScrollRect.velocity = Vector2.zero;

                // fire the delegate for the tween start
                IsTweening = true;
                Callback_ScrollerTweeningChanged?.Invoke(this, true);

                _tweenTimeLeft = 0;
                var newPosition = 0f;

                // while the tween has time left, use an easing function
                while (_tweenTimeLeft < time)
                {
                    newPosition = MathUtil.Tween(tweenType, start, end, (_tweenTimeLeft / time));

                    if (loop)
                    {
                        // if we are looping, we need to make sure the new position isn't past the jump trigger.
                        // if it is we need to reset back to the jump position on the other side of the area.

                        if (end > start && newPosition > loopLastJumpTrigger)
                        {
                            //Debug.Log("name: " + name + " went past the last jump trigger, looping back around");
                            newPosition = loopFirstScrollPosition + (newPosition - loopLastJumpTrigger);
                        }
                        else if (start > end && newPosition < loopFirstJumpTrigger)
                        {
                            //Debug.Log("name: " + name + " went past the first jump trigger, looping back around");
                            newPosition = loopLastScrollPosition - (loopFirstJumpTrigger - newPosition);
                        }
                    }

                    // set the scroll position to the tweened position
                    ScrollPosition = newPosition;

                    // increase the time elapsed
                    _tweenTimeLeft += Time.unscaledDeltaTime;

                    yield return null;
                }

                // the time has expired, so we make sure the final scroll position
                // is the actual end position.
                ScrollPosition = end;
            }

            // the tween jump is complete, so we fire the delegate
            tweenComplete?.Invoke();

            // fire the delegate for the tween ending
            IsTweening = false;
            Callback_ScrollerTweeningChanged?.Invoke(this, false);
        }




        #endregion

        #region enum
        public enum LoopJumpDirectionEnum
        {
            Closest,
            Up,
            Down
        }
        /// <summary>
        /// The direction this scroller is handling
        /// </summary>
        public enum ScrollDirectionEnum
        {
            Vertical,
            Horizontal
        }

        /// <summary>
        /// Which side of a cell to reference.
        /// For vertical scrollers, before means above, after means below.
        /// For horizontal scrollers, before means to left of, after means to the right of.
        /// </summary>
        public enum CellViewPositionEnum
        {
            Before,
            After
        }

        /// <summary>
        /// This will set how the scroll bar should be shown based on the data. If no scrollbar
        /// is attached, then this is ignored. OnlyIfNeeded will hide the scrollbar based on whether
        /// the scroller is looping or there aren't enough items to scroll.
        /// </summary>
        public enum ScrollbarVisibilityEnum
        {
            OnlyIfNeeded,
            Always,
            Never
        }

        #endregion
    }
}