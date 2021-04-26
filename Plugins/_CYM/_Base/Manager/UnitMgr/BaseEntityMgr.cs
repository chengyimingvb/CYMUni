//------------------------------------------------------------------------------
// BaseEntityMgr.cs
// Copyright 2019 2019/4/8 
// Created by CYM on 2019/4/8
// Owner: CYM
// 附加在Entity下面地组件,用来管理子Entity对象,
// 比如附加再国家下面地军团管理器,需要和全局实体管理器配合使用(BaseGEntitySpawnMgr)
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public class BaseEntityMgr<TUnit, TConfig, TDBData, TOwner> : BaseUnitMgr<TUnit,TConfig,TOwner>, IUnitMgr
        where TUnit     : BaseEntity<TUnit, TConfig, TDBData, TOwner>
        where TConfig   : TDBaseData, new()
        where TDBData   : DBBaseUnit, new()
        where TOwner    : BaseUnit
    {

        #region Callback
        // 占领实体,TUnit:实体
        public event Callback<TUnit> Callback_OnOccupied;
        //失去所有的实体,TOwner:造成方
        public event Callback<TOwner> Callback_OnLoseAll;
        #endregion

        #region mgr
        public BaseEntitySpawnMgr<TUnit, TConfig, TDBData, TOwner> GEntityMgr => GMgr as BaseEntitySpawnMgr<TUnit, TConfig, TDBData, TOwner>;
        #endregion

        #region set
        public virtual void Occupied(TUnit unit)
        {
            if (unit == null) return;
            if (unit.Owner == null) return;
            if (IsHave(unit))
                CLog.Error("{0},已经拥有此实体:{1}", SelfBaseUnit.TDID, unit.TDID);
            else
            {
                if (unit.IsHaveOwner())
                {
                    var oldOwner = unit.Owner;
                    var targetMgr = unit.Owner.GetUnitMgr(UnitType) as BaseEntityMgr<TUnit, TConfig, TDBData, TOwner>;
                    if (targetMgr == null)
                    {
                        CLog.Error("错误!GetEntityMgr 为null.{0}", UnitType);
                        return;
                    }
                    targetMgr.RemoveFromData(unit);
                    targetMgr.OnBeOccupied(unit);
                    AddToData(unit);
                    OnOccupied(unit);
                    Callback_OnOccupied?.Invoke(unit);
                    unit.Callback_OnBeOccupied?.Invoke(oldOwner, unit.Owner);
                    GMgr.Callback_OnUnitMgrOccupied?.Invoke(unit);
                    if (targetMgr.Count <= 0)
                    {
                        targetMgr.Callback_OnLoseAll?.Invoke(SelfUnit);
                        GEntityMgr.Callback_OnLoseAll?.Invoke(SelfUnit, targetMgr.SelfUnit);
                    }
                    targetMgr.OnBePostOccupied(unit);
                    OnPostOccupied(unit);
                }
            }
        }
        #endregion
    }
}