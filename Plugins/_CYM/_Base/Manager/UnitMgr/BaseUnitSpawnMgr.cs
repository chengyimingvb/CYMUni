//**********************************************
// Class Name	: BaseSpawnMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.Pool;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    /// <summary>
    /// 此类可以给单位使用也可以给全局对象使用
    /// </summary>
    /// <typeparam name="TUnit"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public class BaseUnitSpawnMgr<TUnit,TConfig> : BaseMgr, ISpawnMgr<TUnit> ,IUnitSpawnMgr
        where TUnit : BaseUnit
        where TConfig :  TDBaseData, new()
    {
        #region prop
        GameObject TempSpawnTrans = new GameObject("TempSpawnTrans");
        protected BasePoolMgr PoolMgr => BaseGlobal.PoolMgr;
        //重复数据检测
        public HashSet<TUnit> Sets { get; private set; } = new HashSet<TUnit>();
        #endregion

        #region type
        public Type UnitType { get; private set; }
        public Type ConfigType { get; private set; }
        #endregion

        #region mgr
        protected ITDLuaMgr TDLuaMgr { get; private set; }
        #endregion

        #region ISpawnMgr
        public TUnit Gold { get; protected set; }
        public IDDicList<TUnit> Data { get; protected set; } = new IDDicList<TUnit>();
        public event Callback<TUnit> Callback_OnAdd;
        public event Callback<TUnit> Callback_OnSpawnGold;
        public event Callback<TUnit> Callback_OnSpawn;
        public event Callback<TUnit> Callback_OnDespawn;
        public event Callback<TUnit> Callback_OnDataChanged;

        public Callback<TUnit> Callback_OnUnitMgrAdded { get; set; }
        public Callback<TUnit> Callback_OnUnitMgrRemoved { get; set; }
        public Callback<TUnit> Callback_OnUnitMgrOccupied { get; set; }
        #endregion

        #region life
        public override MgrType MgrType => MgrType.All;
        protected virtual bool IsCopyConfig => true;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGameLogicTurn = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            TempSpawnTrans.hideFlags = HideFlags.HideInHierarchy;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            UnitType = typeof(TUnit);
            ConfigType = typeof(TConfig);
            TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(ConfigType);
            if (IsGlobal)
            {
                BaseGlobal.BattleMgr.Callback_OnBattleUnLoaded += OnBattleUnLoaded;
                BaseGlobal.BattleMgr.Callback_OnBattleLoadedScene += OnBattleLoadedScene;
                BaseGlobal.LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            }
        }
        //可以重写,用于自定义数据
        protected virtual IEnumerable<TUnit> GetGameLogicTurnData()
        {
            return Data;
        }
        public override void OnGameLogicTurn()
        {
            //计算规范化的分数
            float maxScore = 1;
            foreach (var item in GetGameLogicTurnData())
            {
                item.Score = item.CalcScore();
                maxScore = Mathf.Max(maxScore, item.Score);
            }
            foreach (var item in GetGameLogicTurnData())
            {
                item.Score = item.Score/ maxScore;
                if (item.NeedGameLogicTurn)
                    item.OnGameLogicTurn();
            }
            base.OnGameLogicTurn();
        }
        public override void OnGameFrameTurn(int gameFramesPerSecond)
        {
            base.OnGameFrameTurn(gameFramesPerSecond);
            foreach (var item in Data)
            {
                item.OnGameFrameTurn(gameFramesPerSecond);
            }
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            foreach (var item in Data)
                item.OnGameStart1();
        }
        public override void OnGameStart2()
        {
            base.OnGameStart2();
            foreach (var item in Data)
                item.OnGameStart2();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            foreach (var item in Data)
                item.OnGameStarted1();
        }
        public override void OnGameStarted2()
        {
            base.OnGameStarted1();
            foreach (var item in Data)
                item.OnGameStarted2();
        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            foreach (var item in Data)
                item.OnGameStartOver();
        }
        public override void OnDeath()
        {
            base.OnDeath();
            Clear();
        }
        #endregion 

        #region set
        //和Spawn一样,只不过会触发OnBeNewSpawned回调
        public virtual TUnit SpawnNew(string tdid, Vector3? spwanPoint=null, Quaternion? quaternion = null, int? team = null, long? rtid = null, string prefab = null)
        {
            var ret = Spawn(tdid, spwanPoint, quaternion, team, rtid, prefab);
            ret?.OnBeNewSpawned();
            return ret;
        }
        public virtual TUnit Spawn(string tdid, Vector3? spwanPoint = null, Quaternion? quaternion = null, int? team = null, long? rtid = null, string prefab = null)
        {
            if (tdid.IsInv()) return null;

            //获得配置数据
            TConfig config = TDLuaMgr?.Get<TConfig>(tdid);
            if (config == null)
            {
                CLog.Error("配置错误，没有这个配置TDID：" + tdid);
                return null;
            }

            //获得prefab
            GameObject goPrefab;
            if (!prefab.IsInv()) goPrefab = BundleCacher.Get(prefab);
            else goPrefab = config?.GetPrefab();
            if (goPrefab == null)
            {
                if (prefab != null) 
                    CLog.Error("没有这个CustomPrefab:{0}", prefab);
                return null;
            }

            GameObject charaGO = SpawnPool.Spawn(goPrefab, spwanPoint, quaternion);
            charaGO.name = tdid;
            TUnit unitChara = BaseCoreMono.GetUnityComponet<TUnit>(charaGO);
            SpawnAdd(unitChara, tdid, rtid, team);
            Callback_OnSpawn?.Invoke(unitChara);
            return unitChara;
        }
        public virtual TUnit OnSpawSystem()
        {
            Gold = GetUnit(GoldID);
            if (Gold == null)
            {
                TempSpawnTrans.transform.position = GoldPos == null ? Const.VEC_FarawayPos : GoldPos.Value;
                Gold = Spawn(GoldID, TempSpawnTrans.transform.position, Quaternion.identity, int.MaxValue);
            }
            Callback_OnSpawnGold?.Invoke(Gold);
            return Gold;
        }
        // 执行Add操作,但是也会触发Spawn流程,适用于对已经存在于地图上的对象使用
        public virtual void SpawnAdd(TUnit chara, string tdid, long? rtid = null, int? team = null)
        {
            if (chara == null) return;
            if (!rtid.HasValue)chara.SetRTID(IDUtil.Gen());
            else chara.SetRTID(rtid.Value);

            TConfig config = TDLuaMgr?.Get<TConfig>(tdid);
            if (config == null)
            {
                config = new TConfig();
            }
            else if (IsCopyConfig)
            {
                config = config.Copy<TConfig>();
            }

            chara.SpawnMgr = this;
            chara.SetTDID(tdid);
            chara.SetTeam(team);
            chara.SetConfig(config);
            chara.OnInit();
            chara.OnBeSpawned();
            OnSpawned(tdid, chara.ID, chara);
            AddToData(chara);
        }
        public virtual void Despawn(TUnit chara,float delay=0)
        {
            if (chara == null)
            {
                CLog.Error("BaseSpawnMgr.Despawn:chara==null");
                return;
            }
            if (!IsHave(chara))
                return;
            SpawnPool.Despawn(chara, delay);
            RemoveFromData(chara);
        }
        public void Despawn(BaseUnit chara, float delay = 0)=> Despawn(chara as TUnit, delay);
        // 清空数据
        public virtual void Clear()
        {
            Data.Clear();
            Sets.Clear();
        }
        #endregion

        #region is
        public bool IsHave(BaseUnit unit)
        {
            return Data.Contains(unit as TUnit);
        }
        #endregion

        #region get
        public TUnit GetUnit(string tdid) => Data.Get(tdid);
        public TUnit GetUnit(long rtid) => Data.Get(rtid);
        public BaseUnit GetBaseUnit(long id) => GetUnit(id);
        public BaseUnit GetBaseUnit(string tdid) => GetUnit(tdid);
        #endregion

        #region op data
        protected virtual void AddToData(TUnit chara)
        {
            Data.Add(chara);
            OnDataChanged(chara);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnAdd?.Invoke(chara);
                Callback_OnDataChanged?.Invoke(chara);
            }
        }
        protected virtual void RemoveFromData(TUnit chara)
        {
            Data.Remove(chara);
            OnDataChanged(chara);
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnDespawn?.Invoke(chara);
                Callback_OnDataChanged?.Invoke(chara);
            }
        }
        #endregion

        #region virtual
        protected virtual string GoldID => "";
        protected virtual Vector3? GoldPos => null;
        protected virtual SpawnPool SpawnPool => throw new NotImplementedException();
        protected virtual BundleCacher<GameObject> BundleCacher => throw new NotImplementedException();
        protected virtual GameObject GetPrefab(string tdid, string prefab, params object[] ps)
        {
            if (!prefab.IsInv())
                return BundleCacher.Get(prefab);
            TConfig config = TDLuaMgr?.Get<TConfig>(tdid);
            return config?.GetPrefab();
        }
        public virtual void OnSpawned(string tdid, long rtid, TUnit unit) { }
        #endregion

        #region Callback
        public void OnDataChanged(TUnit data) { }
        protected virtual void OnBattleUnLoaded()
        {
            Clear();
        }
        protected virtual void OnBattleLoadedScene()
        {
        }
        protected virtual void OnAllLoadEnd2()
        {
        }
        #endregion

        #region DB
        public override void OnRead1<TDBData>(TDBData data)
        {
            OnSpawSystem();
            base.OnRead1(data);
            foreach (var item in Data)
                item.OnRead1(data);
        }

        public override void OnRead2<TDBData>(TDBData data)
        {
            base.OnRead2(data);
            foreach (var item in Data)
                item.OnRead2(data);
        }

        public override void OnRead3<TDBData>(TDBData data)
        {
            base.OnRead3(data);
            foreach (var item in Data)
                item.OnRead3(data);
        }
        public override void OnReadEnd<TDBData>(TDBData data)
        {
            base.OnReadEnd(data);
            foreach (var item in Data)
                item.OnReadEnd(data);
        }

        public override void OnWrite<TDBData>(TDBData data)
        {
            base.OnWrite(data);
            foreach (var item in Data)
                item.OnWrite(data);
        }
        #endregion
    }
}