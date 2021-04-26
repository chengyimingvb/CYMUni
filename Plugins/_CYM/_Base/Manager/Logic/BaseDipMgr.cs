//------------------------------------------------------------------------------
// BaseDipMgr.cs
// Copyright 2019 2019/11/14 
// Created by CYM on 2019/11/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseDipMgr<TUnit,TAlert> : BaseUFlowMgr<TUnit>, IBaseDipMgr
        where TUnit : BaseUnit
        where TAlert : TDBaseAlertData
    {
        #region event
        public event Callback<BaseUnit> Callback_OnChangeRelation;
        public event Callback<BaseUnit, int> Callback_OnChangeRelationShip;
        public event Callback<BaseUnit, bool> Callback_OnChangeAlliance;
        public event Callback<BaseUnit, bool> Callback_OnChangeMarriage;
        public event Callback<BaseUnit, bool> Callback_OnChangeSubsidiary;
        public event Callback<BaseUnit, int> Callback_OnChangeArmistice;
        public event Callback<object> Callback_OnAddToAttacker;
        public event Callback<object> Callback_OnAddToDefensers;
        public event Callback<object> Callback_OnRemoveFromWar;
        public event Callback<object, BaseUnit> Callback_OnDecalarWar;
        public event Callback<object, BaseUnit> Callback_OnBeDecalarWar;
        public event Callback<TDBaseAlertData, bool> Callback_OnBeAccept;
        public event Callback<TDBaseAlertData, bool> Callback_OnAccept;
        #endregion

        #region prop
        IBaseRelationMgr RelationMgr => BaseGlobal.RelationMgr;
        public HashList<TUnit> Sents { get; private set; } = new HashList<TUnit>();
        public Dictionary<TUnit, CD> ChangeRelationShipCD { get; private set; } = new Dictionary<TUnit, CD>();
        public Dictionary<TUnit, CD> Provocative { get; private set; } = new Dictionary<TUnit, CD>();
        public HashList<TUnit> Neighbor { get; protected set; } = new HashList<TUnit>();
        //邻居的邻居
        public HashList<TUnit> ExNeighbor { get; protected set; } = new HashList<TUnit>();
        #endregion

        #region life
        protected virtual IDDicList<BaseUnit> CastleData => throw new System.NotImplementedException();
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGameLogicTurn = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Sents.Clear();
            ChangeRelationShipCD.Clear();
            Provocative.Clear();
        }
        public override void OnBirth3()
        {
            base.OnBirth3();
            CalcNeighbor();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            CalcNeighbor();
        }
        public override void OnGameLogicTurn()
        {
            base.OnGameLogicTurn();

            //调整关系
            foreach (var item in ChangeRelationShipCD)
                item.Value.Update();

            //挑衅
            List<TUnit> clearProvocative = new List<TUnit>();
            foreach (var item in Provocative)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    clearProvocative.Add(item.Key);
            }
            foreach (var item in clearProvocative)
                Provocative.Remove(item);
            Sents.Clear();
            CalcNeighbor();
        }
        // 计算邻接国家
        protected void CalcNeighbor()
        {

        }
        #endregion

        #region set
        public virtual void Accept(TAlert data, bool b)
        {
            OnAccept(data,b);
            data.Cast.DipMgr.OnBeAccept(data,b);
        }
        public void AddSent(TUnit other)
        {
            Sents.Add(other);
        }
        public void ResetChangeRelationCD(TUnit other, int cd)
        {
            if (!ChangeRelationShipCD.ContainsKey(other))
            {
                ChangeRelationShipCD.Add(other, new CD());
            }
            ChangeRelationShipCD[other].Reset(cd);
        }
        public void AddProvocative(TUnit other, int cd)
        {
            if (Provocative.ContainsKey(other))
            {
                Provocative[other].Reset(cd);
            }
            else
            {
                Provocative.Add(other, new CD(cd));
            }
        }
        public void RemoveProvocative(TUnit other)
        {
            Provocative.Remove(other);
        }
        #endregion

        #region get
        public int GetChangeRelationShipCD(TUnit other)
        {
            if (ChangeRelationShipCD.ContainsKey(other))
                return (int)ChangeRelationShipCD[other].CurCount;
            return 0;
        }
        public int GetRelationShip(TUnit target)
        {
            return RelationMgr.GetRelationShip(SelfUnit, target);
        }
        #endregion

        #region is
        public bool IsNeighbor(TUnit other)
        {
            return Neighbor.Contains(other);
        }
        public bool IsInChangeRelationShipCD(TUnit other)
        {
            return GetChangeRelationShipCD(other) > 0;
        }
        public bool IsBeProvocative()
        {
            return Provocative.Count > 0;
        }
        public bool IsSent(TUnit other)
        {
            if (other == null) return false;
            if (SelfBaseUnit == other) return false;
            return Sents.Contains(other);
        }
        public bool IsInWar()
        {
            return RelationMgr.IsInWarfare(SelfBaseUnit);
        }
        public bool IsInProvocater(TUnit unit)
        {
            return Provocative.ContainsKey(unit);
        }
        public bool IsFriend(TUnit target)
        {
            return RelationMgr.IsFriend(SelfUnit, target);
        }
        public bool IsHaveWarFriend()
        {
            return IsHaveAlliance() || IsHaveVassal() || IsHaveSuzerain();
        }
        public bool IsHaveAlliance() => RelationMgr.IsHaveAlliance(SelfUnit);
        public bool IsHaveMarriage() => RelationMgr.IsHaveMarriage(SelfUnit);
        public bool IsHaveVassal() => RelationMgr.IsHaveVassal(SelfUnit);
        public bool IsHaveSuzerain() => RelationMgr.IsHaveSuzerain(SelfUnit);
        #endregion

        #region DB
        protected void LoadSent(List<string> db)
        {
            if (db == null) return;
            Sents.Clear();
            db.ForEach(x => Sents.Add(GetEntity<TUnit>(x)));
        }
        protected void LoadChangeRelationShipCD(Dictionary<long, CD> db)
        {
            if (db == null) return;
            db.ForEach((k, v) => ChangeRelationShipCD.Add(GetEntity<TUnit>(k), v));
        }
        protected void LoadProvocative(Dictionary<long, CD> db)
        {
            if (db == null) return;
            db.ForEach((k, v) => Provocative.Add(GetEntity<TUnit>(k), v));
        }
        protected List<string> GetSentDB()
        {
            List<string> ret = new List<string>();
            Sents.ForEach(x => ret.Add(x.TDID));
            return ret;
        }
        protected Dictionary<long, CD> GetChangeRelationShipCDDB()
        {
            Dictionary<long, CD> ret = new Dictionary<long, CD>();
            ChangeRelationShipCD.ForEach((k, v) => ret.Add(k.ID, v));
            return ret;
        }
        protected Dictionary<long, CD> GetProvocativeDB()
        {
            Dictionary<long, CD> ret = new Dictionary<long, CD>();
            Provocative.ForEach((k, v) => ret.Add(k.ID, v));
            return ret;
        }
        #endregion

        #region Callback
        public virtual void OnChangeRelation(BaseUnit other)
        {
            Callback_OnChangeRelation?.Invoke(other);
        }
        public virtual void OnChangeRelationShip(BaseUnit other, int relShip)
        {
            Callback_OnChangeRelationShip?.Invoke(other, relShip);
        }
        public virtual void OnChangeAlliance(BaseUnit other, bool b)
        {
            Callback_OnChangeAlliance?.Invoke(other, b);
        }
        public virtual void OnChangeMarriage(BaseUnit other, bool b)
        {
            Callback_OnChangeMarriage?.Invoke(other, b);
        }
        public virtual void OnChangeSubsidiary(BaseUnit other, bool b)
        {
            Callback_OnChangeSubsidiary?.Invoke(other, b);
        }
        public virtual void OnChangeArmistice(BaseUnit other, int count)
        {
            Callback_OnChangeArmistice?.Invoke(other, count);
        }
        public virtual void OnAddToAttacker(object warData)
        {
            Callback_OnAddToAttacker?.Invoke(warData);
        }
        public virtual void OnAddToDefensers(object warData)
        {
            Callback_OnAddToDefensers?.Invoke(warData);
        }
        public virtual void OnRemoveFromWar(object warData)
        {
            Callback_OnRemoveFromWar?.Invoke(warData);
        }
        public virtual void OnDecalarWar(object warData, BaseUnit other)
        {
            Callback_OnDecalarWar?.Invoke(warData, other);
        }
        public virtual void OnBeDecalarWar(object warData, BaseUnit caster)
        {
            Callback_OnBeDecalarWar?.Invoke(warData, caster);
        }
        public virtual void OnDeclarePeace(object warData, BaseUnit other)
        {
        }
        protected virtual void OnAddToNeighbor(TUnit unit)
        {

        }
        public virtual void OnBeAccept(TDBaseAlertData data, bool b)
        {
            Callback_OnBeAccept?.Invoke(data,b);
        }
        public virtual void OnAccept(TDBaseAlertData data, bool b)
        {
            Callback_OnAccept?.Invoke(data, b);
        }
        #endregion
    }
}