//------------------------------------------------------------------------------
// BaseTable.cs
// Copyright 2018 2018/10/28 
// Created by CYM on 2018/10/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class UTableData : UScrollData
    {
        public Callback<int> OnTitleClick;
        public UCheckBoxData[] TitleDatas;
    }
    [AddComponentMenu("UI/Control/UTable")]
    [HideMonoScript]
    public class UTable : UPresenter<UTableData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField, Required, ChildGameObjectsOnly, SceneObjectsOnly]
        UDupplicate DP;
        [FoldoutGroup("Inspector"), SerializeField, Required, ChildGameObjectsOnly, SceneObjectsOnly]
        UScroll Scroll;
        #endregion

        #region prop
        UCheckBox[] Titles;
        #endregion

        #region set
        public void Init(Func<IList> getData, Callback<object, object> onRefresh, List<UCheckBoxData> titleBnts, Callback<int> onItemClick = null, Callback<int> onTitleClick = null)
        {
            Init(new UTableData
            {
                GetCustomDatas = getData,
                OnRefresh = onRefresh,
                TitleDatas = titleBnts.ToArray(),
                Sorter = GetSorters(titleBnts),
                OnSelectItem = onItemClick,
                OnTitleClick = onTitleClick,
            });
        }
        // 初始化Table menu
        public override void Init(UTableData tableData)
        {
            base.Init(tableData);

            if (DP == null) CLog.Error("没有BaseDupplicate组件");
            if (Scroll == null) CLog.Error("没有BaseScroll组件");
            if (Data.GetCustomDatas == null) CLog.Error("TableData 的 GetCustomDatas 必须设置");
            if (Data.OnRefresh == null) CLog.Error("TableData 的 OnRefresh 必须设置");

            Scroll.Init(tableData);
            Titles = DP.Init<UCheckBox, UCheckBoxData>(Data.TitleDatas);
            foreach (var item in Titles)
                item.Data.OnClick += OnBntClick;
        }
        #endregion

        #region Callback
        void OnBntClick(UControl presenter, PointerEventData data)
        {
            Data?.OnTitleClick?.Invoke(presenter.Index);
            Scroll.SortData(presenter.Index, false);
            SetDirtyData();
        }
        #endregion

        #region set
        public void SetSelectData(object data)
        {
            Scroll.SelectData(data);
        }
        public void SetSelectData(int index)
        {
            Scroll.SelectData(index);
        }
        #endregion

        #region get
        List<Func<object, object>> GetSorters(List<UCheckBoxData> datas)
        {
            List<Func<object, object>> ret = new List<Func<object, object>>();
            if (datas == null)
                return ret;
            foreach (var item in datas)
            {
                ret.Add(item.OnSorter);
            }
            return ret;
        }
        public T GetData<T>(int dataIndex) where T : class
        {
            return Scroll.GetData<T>(dataIndex);
        }
        #endregion
    }
}