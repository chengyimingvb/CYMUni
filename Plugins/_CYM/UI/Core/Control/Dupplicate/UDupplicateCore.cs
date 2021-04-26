//------------------------------------------------------------------------------
// BaseDupplicateCore.cs
// Copyright 2019 2019/6/9 
// Created by CYM on 2019/6/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public partial class UDupplicate
    {
        #region utile
        public TP[] InitCountAndData<TP, TD>(int count, TD data = null) 
            where TP : UPresenter<TD>, new() 
            where TD : UPresenterData, new()
        {
            InitCount(count);
            return InitData<TP, TD>(data);
        }
        public TP[] InitCountAndData<TP, TD>(int count, TD[] data) 
            where TP : UPresenter<TD>, new() 
            where TD : UPresenterData, new()
        {
            InitCount(count);
            return InitData<TP, TD>(data);
        }

        // 通过数量初始化
        private void InitCount(int count)
        {
            if (IsInitedCount)
            {
                CLog.Error("InitCount 无法初始化2次!!!! " + Path);
                return;
            }
            IsInitedCount = true;
            GOs.Clear();
            for (int i = 0; i < Trans.childCount; ++i)
            {
                Transform temp = Trans.GetChild(i);
                var ele = temp.GetComponent<LayoutElement>();
                if (ele != null && ele.ignoreLayout)
                {
                    continue;
                }
                var ignore = temp.GetComponent<UIgnore>();
                if (ignore != null)
                {
                    continue;
                }
                GOs.Add(temp.gameObject);
            }
            if (Prefab == null && GOs.Count > 0)
            {
                Prefab = GOs[0];
            }
            if (Prefab == null)
            {
                CLog.Error("{0}: Prefab == null", Path);
                return;
            }
            if (Prefab.name.StartsWith(Const.STR_Base))
                CLog.Error($"不能使用基础UI Prefab 初始化:{Prefab.name}");

            //差值
            int subCount = count - GOs.Count;

            if (subCount > 0)
            {
                //生成剩余的游戏对象
                for (int i = 0; i < subCount; ++i)
                {
                    GameObject temp = GameObject.Instantiate(Prefab, this.RectTrans.position, this.RectTrans.rotation);
                    (temp.transform as RectTransform).SetParent(this.RectTrans);
                    (temp.transform as RectTransform).localScale = Vector3.one;
                    GOs.Add(temp);
                }
            }

            Controls.Clear();
            for (int i = 0; i < GOs.Count; ++i)
            {
                var tempPresenter = GOs[i].GetComponent<UControl>();
                if (tempPresenter == null)
                {
                    CLog.Error("Item:没有添加组件");
                    return;
                }
                Controls.Add(tempPresenter);

                if (i < count)
                    tempPresenter.Show(true);
                else
                    tempPresenter.Show(false);

                if (tempPresenter is UCheckBox checkBox)
                {
                    ToggleGroupCheckBoxs.Add(checkBox);
                }
            }
        }
        private TP[] InitData<TP, TD>(TD data = null) 
            where TP : UPresenter<TD> 
            where TD : UPresenterData, new()
        {
            ClearChilds();
            if (data == null)
            {
                data = new TD();
            }
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            TP[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<TP>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0}", typeof(TP)));
                    break;
                }
                else
                {
                    if (AddDynamicChild(ts[i]))
                    {
                        ts[i].SetIndex(i);
                        ts[i].SetDataIndex(i);
                        ts[i].PDupplicate = this;
                    }
                    //if (data != null &&
                    //    Data.OnRefresh == null &&
                    //    Data.OnFixedRefresh == null)
                    {
                        ts[i].Init(data);
                    }
                }
            }
            return ts;
        }
        private TP[] InitData<TP, TD>(TD[] data) 
            where TP : UPresenter<TD> 
            where TD : UPresenterData, new()
        {
            ClearChilds();
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            TP[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<TP>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0},如果想要忽略,请添加IgnoreElement组件", typeof(TP)));
                    break;
                }
                else
                {
                    if (AddDynamicChild(ts[i]))
                    {
                        ts[i].SetIndex(i);
                        ts[i].SetDataIndex(i);
                        ts[i].PDupplicate = this;
                    }
                    //if (
                    //    Data.OnRefresh == null &&
                    //    Data.OnFixedRefresh == null)
                    {
                        if (data != null)
                        {
                            if (i < data.Length)
                                ts[i].Init(data[i]);
                        }
                        else
                        {
                            ts[i].Init(new TD());
                        }
                    }
                }
            }
            return ts;
        }
        private void InitData()
        {
            ClearChilds();
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            UControl[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<UControl>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0},如果想要忽略,请添加IgnoreElement组件", typeof(UControl)));
                    break;
                }
                else
                {
                    if (AddDynamicChild(ts[i]))
                    {
                        ts[i].SetIndex(i);
                        ts[i].SetDataIndex(i);
                        ts[i].PDupplicate = this;
                    }
                }
            }
        }
        #endregion

        #region callback
        // 鼠标进入
        public override void OnPointerEnter(PointerEventData eventData) { }
        // 鼠标退出
        public override void OnPointerExit(PointerEventData eventData) { }
        // 鼠标点击
        public override void OnPointerClick(PointerEventData eventData) { }
        // 鼠标按下
        public override void OnPointerDown(PointerEventData eventData) { }
        // 鼠标按下
        public override void OnPointerUp(PointerEventData eventData) { }
        // 点击状态变化
        public override void OnInteractable(bool b) { }
        #endregion

        #region Inspector
        [Button("统一名称")]
        public void ModifyName()
        {
            Util.UnifyChildName(GO);
        }
        bool Inspector_IsInitCount()
        {
            return IsInitOnStart;
        }
        bool Inspector_IsPrefab()
        {
            return transform.childCount<=0;
        }
        bool IsShowCheckbox()
        {
            if (Prefab == null)
                return false;
            return Prefab.GetComponent<UCheckBox>() != null;
        }
        #endregion

    }
}