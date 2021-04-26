//------------------------------------------------------------------------------
// BasePersonMgr.cs
// Copyright 2019 2019/5/15 
// Created by CYM on 2019/5/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;

namespace CYM
{
    public class BasePersonMgr<TData> : BaseTDSpawnMgr<TData> where TData : TDBasePersonData, new()
    {
        #region prop
        public IDDicList<TData> CacheData { get; protected set; } = new IDDicList<TData>();
        protected virtual int CacheTurnCount => 3;
        int CurTurnCount = 0;
        #endregion

        #region life
        protected virtual bool IsCanAddToCache(TData data) => false;
        public override void OnGameLogicTurn()
        {
            base.OnGameLogicTurn();
            if (CurTurnCount >= CacheTurnCount)
            {
                CurTurnCount = 0;
                CacheData.Clear();
            }
            else
            {
                CurTurnCount++;
            }
        }
        #endregion

        #region Rand
        public virtual TData RandPerson(TDBaseNameData nameData, AgeRange range = AgeRange.Adult, Gender gender = Gender.Male, string lastName = null, Func<TData, TData> onProcessInfo = null)
        {
            TData person = new TData();
            if (onProcessInfo != null) person = onProcessInfo.Invoke(person);
            person.RandInfo(nameData, range, gender, lastName);
            SpawnAdd(person, person.GetTDID());
            return person;
        }
        public virtual TData GeneratePerson(TData config, Func<TData, TData> onProcessInfo = null)
        {
            TData person = config.Copy<TData>();
            if (onProcessInfo != null) person = onProcessInfo.Invoke(person);
            person.Generate();
            SpawnAdd(person, person.GetTDID());
            return person;
        }
        #endregion

        #region set
        public override void Despawn(TData data,float delay=0)
        {
            base.Despawn(data, delay);
            if (IsCanAddToCache(data))
                CacheData.Add(data);
        }
        #endregion

        #region get
        public override TData GetUnit(long rtid)
        {
            var ret = base.GetUnit(rtid);
            if (ret == null)
                ret = CacheData.Get(rtid);
            return ret;
        }
        public override TData GetUnit(string tdid)
        {
            var ret = base.GetUnit(tdid);
            if (ret == null)
                ret = CacheData.Get(tdid);
            return ret;
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            Data.Clear();
            CacheData.Clear();
        }
        #endregion

        #region db
        public TData LoadCache<TDBData>(TDBData data) where TDBData : DBBase
        {
            TData ret = new TData();
            Util.CopyToTD(data, ret);
            CacheData.Add(ret);
            return ret;
        }
        #endregion
    }
}