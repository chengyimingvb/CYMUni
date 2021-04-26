//------------------------------------------------------------------------------
// BaseNextMgr.cs
// Copyright 2021 2021/3/20 
// Created by CYM on 2021/3/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;

namespace CYM
{
    public class BaseNextMgr<TData> : BaseGFlowMgr 
        where TData:TDBaseData
    {
        #region prop
        ITDLuaMgr TDLuaMgr;
        List<TData> Group;
        int Index = -1;
        public bool IsHave => Index != -1;
        #endregion

        #region Callback
        public event Callback<TData> Callback_OnStart;
        public event Callback<TData, int> Callback_OnNext;
        public event Callback Callback_OnEnd;
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(typeof(TData));
        }
        #endregion

        public void Start(string group)
        {
            Index = 0;
            Group = TDLuaMgr.GetRawGroup(group) as List<TData>;
            if (Index >= Group.Count)
            {
                return;
            }
            Callback_OnStart?.Invoke(Group[Index]);
        }
        public void Next()
        {
            Index++;
            if (Index >= Group.Count)
            {
                Callback_OnEnd?.Invoke();
                Index = -1;
                return;
            }
            Callback_OnNext?.Invoke(Group[Index], Index);
        }
    }
}