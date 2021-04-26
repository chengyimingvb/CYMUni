//------------------------------------------------------------------------------
// BaseTDMgr.cs
// Copyright 2019 2019/5/14 
// Created by CYM on 2019/5/14
// Owner: CYM
// TableData 数据管理器,用来管理表格数据对象(产生继承于BaseConfig的对象)
// 只能作为Unit组件使用,需要配合BaseTDSpawnMgr
// 适合既需要被Unit管理又需要被Global管理的数据使用,比如(人物系统),全局有一套数据,本地也有一套数据
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public class BaseTDMgr<TData, TUnit> : BaseUFlowMgr<TUnit>
        where TUnit : BaseUnit
        where TData : TDBaseData, new()
    {

        #region Callback
        public event Callback<TData> Callback_OnDataChanged;
        #endregion

        #region prop
        public IDDicList<TData> Data { get; protected set; } = new IDDicList<TData>();
        #endregion

        #region life
        protected virtual BaseTDSpawnMgr<TData> GMgr => throw new System.NotImplementedException("此函数必须实现");
        #endregion

        #region set
        public virtual TData Spawn(string id)
        {
            TData temp = GMgr.Spawn(id, Vector3.zero, Quaternion.identity, 0);
            OnSpawned(temp);
            Add(temp);
            GMgr.Callback_OnUnitMgrAdded?.Invoke(temp);
            return temp;
        }
        public virtual void Despawn(TData data)
        {
            if (data == null) return;
            Remove(data);
            GMgr.Despawn(data);
            GMgr.Callback_OnUnitMgrRemoved?.Invoke(data);
        }
        public virtual TData Add(TData data)
        {
            Data.Add(data);
            Callback_OnDataChanged?.Invoke(data);
            OnDataChanged(data);
            return data;
        }
        public virtual void Remove(TData data)
        {
            Data.Remove(data);
            Callback_OnDataChanged?.Invoke(data);
            OnDataChanged(data);
        }
        #endregion

        #region is
        public bool IsOwn(TData data)
        {
            if (data == null)
                return false;
            return Data.ContainsID(data.ID);
        }
        #endregion

        #region Callback
        protected virtual void OnSpawned(TData data) { }
        public virtual void OnDataChanged(TData data) { }
        #endregion
    }
}