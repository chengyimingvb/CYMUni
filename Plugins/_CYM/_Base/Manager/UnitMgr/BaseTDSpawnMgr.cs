//------------------------------------------------------------------------------
// BaseTDMgr.cs
// Copyright 2018 2018/11/27 
// Created by CYM on 2018/11/27
// Owner: CYM
// TableData 数据管理器,用来产生虚拟对象(产生继承于BaseConfig的对象)
// 全局和本地都可以用
//------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace CYM
{
    public class BaseTDSpawnMgr<TData> : BaseMgr, ISpawnMgr<TData> , ITDSpawnMgr
        where TData : TDBaseData, new()
    {
        #region ISpawnMgr
        public TData Gold { get; protected set; }
        public IDDicList<TData> Data { get; protected set; } = new IDDicList<TData>();
        public event Callback<TData> Callback_OnAdd;
        public event Callback<TData> Callback_OnSpawnGold;
        public event Callback<TData> Callback_OnSpawn;
        public event Callback<TData> Callback_OnDespawn;
        public event Callback<TData> Callback_OnDataChanged;
        #endregion

        #region Callback 
        public Callback<TData> Callback_OnUnitMgrAdded { get; set; }
        public Callback<TData> Callback_OnUnitMgrRemoved { get; set; }
        public Callback<TData> Callback_OnUnitMgrOccupied { get; set; }
        #endregion

        #region prop
        public int Count => Data.Count;
        ITDLuaMgr TDLuaMgr;
        #endregion

        #region life
        public override MgrType MgrType => MgrType.All;

        public Type UnitType { get; private set; }

        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(typeof(TData));
            UnitType = typeof(TData);
            if (IsGlobal)
            {
                BaseGlobal.BattleMgr.Callback_OnBattleLoaded += OnBattleLoaded;
                BaseGlobal.BattleMgr.Callback_OnBattleUnLoaded += OnBattleUnLoaded;
            }
        }
        public override void OnGameLogicTurn()
        {
            base.OnGameLogicTurn();
            foreach (var item in Data)
                item.GameLogicTurn();
        }
        public override void OnGameFrameTurn(int gameFramesPerSecond)
        {
            base.OnGameFrameTurn(gameFramesPerSecond);
            foreach (var item in Data)
                item.GameFrameTurn(gameFramesPerSecond);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in Data)
                item.OnUpdate();
        }
        public override void OnDeath()
        {
            Clear();
            base.OnDeath();
        }
        #endregion

        #region set
        public virtual void Sort()
        {
            throw new NotImplementedException();
        }
        //加载到现有得对象中
        public void LoadAdd(TData config, DBBase data)
        {
            SpawnAdd(config, data.TDID, data.ID);
        }
        /// <summary>
        /// 加载对象
        /// </summary>
        /// <typeparam name="TDBData"></typeparam>
        /// <param name="data"></param>
        /// <param name="copyTable">拷贝表格配置的数值,而不是直接引用</param>
        /// <returns></returns>
        public TData Load<TDBData>(TDBData data, bool copyTable = false) where TDBData : DBBase
        {
            TData ret = new TData();
            if (copyTable)
            {
                ret = TDLuaMgr.Get<TData>(data.TDID).Copy<TData>();
            }
            Util.CopyToTD(data, ret);
            OnLoad(data, ref ret);
            SpawnAdd(ret, data.TDID, data.ID);
            return ret;
        }
        //保存对象
        public TDBData Save<TDBData>(TData config) where TDBData : DBBase, new()
        {
            TDBData ret = new TDBData();
            Util.CopyToDB(config, ret);
            OnSave(ref ret, config);
            return ret;
        }
        //游戏开始的时候创建一个新的对象
        public virtual TData SpawnNew(string tdid, Vector3? spwanPoint = null, Quaternion? quaternion = null, int? team = null, long? rtid = null, string prefab = null)
        {
            var ret = Spawn(tdid, spwanPoint, quaternion, team, rtid, prefab);
            ret.OnNewSpawn();
            return ret;
        }
        //创建对象
        public virtual TData Spawn(string tdid, Vector3? spwanPoint = null, Quaternion? quaternion = null, int? team = null, long? rtid = null, string prefab = null)
        {
            if (tdid.IsInv()) return null;
            TData data = TDLuaMgr.Get<TData>(tdid).Copy<TData>();
            SpawnAdd(data, tdid, rtid, team);
            Callback_OnSpawn?.Invoke(data);
            return data;
        }
        //添加,触发整个SpawnAdd流程
        public virtual void SpawnAdd(TData data, string tdid = null, long? rtid = null, int? team = null)
        {
            if (!tdid.IsInv()) data.TDID = tdid;
            else tdid = data.TDID;

            if (rtid != null)
            {
                data.ID = rtid.Value;
            }
            else
            {
                rtid = data.ID = IDUtil.Gen();
            }
            data.OnBeAdded(SelfMono);
            OnSpawned(tdid, rtid.Value, data);
            AddToData(data);
        }
        // despawn
        public virtual void Despawn(TData data,float delay=0)
        {
            data.OnBeRemoved();
            RemoveFromData(data);
        }
        // 清空数据
        public virtual void Clear()=> Data.Clear();
        public virtual void OnSpawned(string tdid, long rtid, TData unit) { }

        public void AddToData(TData data)
        {
            data.SetOwner(SelfBaseUnit);
            Data.Add(data);
            OnDataChanged(data);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnAdd?.Invoke(data);
                Callback_OnDataChanged?.Invoke(data);
            }
        }
        public void RemoveFromData(TData data)
        {
            Data.Remove(data);
            OnDataChanged(data);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnDespawn?.Invoke(data);
                Callback_OnDataChanged?.Invoke(data);
            }
        }
        #endregion

        #region is
        public bool IsHave() => Data.Count > 0;
        #endregion

        #region get
        public virtual TData GetUnit(long rtid) => Data.Get(rtid);
        public virtual TData GetUnit(string tdid) => Data.Get(tdid);
        #endregion

        #region Callback
        protected virtual void OnLoad<TDBData>(TDBData DBDta, ref TData TDData) { }
        protected virtual void OnSave<TDBData>(ref TDBData DBDta, TData TDData) { }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
        }
        protected virtual void OnBattleUnLoaded() => Clear();
        protected virtual void OnBattleLoaded() { }
        public virtual void OnDataChanged(TData data) { }
        #endregion

        #region ITDSpawnMgr
        public void Despawn(TDBaseData chara, float delay = 0) => Despawn(chara as TData, delay);
        public bool IsHave(TDBaseData unit)
        {
            if (unit == null) return false;
            return Data.ContainsID(unit.ID);
        }
        public TDBaseData GetBaseData(long rtid) => Data.Get(rtid);
        public TDBaseData GetBaseData(string tdid) => Data.Get(tdid);
        #endregion
    }
}