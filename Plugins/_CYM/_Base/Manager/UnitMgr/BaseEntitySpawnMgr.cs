//------------------------------------------------------------------------------
// BaseEntitySpawnMgr.cs
// Copyright 2019 2019/1/17 
// Created by CYM on 2019/1/17
// Owner: CYM
// 全局的实体生成管理器,可以和本地实体管理器配合使用(BaseEntityMgr)
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CYM
{
    public class BaseEntitySpawnMgr<TUnit, TConfig, TDBData, TOwner> : BaseUnitSpawnMgr<TUnit, TConfig>
        where TUnit : BaseEntity<TUnit, TConfig, TDBData, TOwner>
        where TConfig : TDBaseData, new()
        where TDBData : DBBaseUnit, new()
        where TOwner : BaseUnit
    {
        #region Callback val
        //失去所有实体,TOwner1:造成方,TOwner2:承受放
        public Callback<TOwner, TOwner> Callback_OnLoseAll { get; set; }
        #endregion

        #region life
        public override MgrType MgrType => MgrType.All;
        public override bool IsGlobal => true;
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Callback_OnLoseAll += OnLoseAll;
        }
        public override void OnDisable()
        {
            Callback_OnLoseAll -= OnLoseAll;
            base.OnDisable();
        }
        #endregion

        #region set
        //不会创建新的,加载地图上以有的对象,类似于SpawnAdd
        public void LoadAdd(TUnit unit, TDBData dbData)
        {
            if (unit == null || dbData == null)
                return;
            SpawnAdd(unit, dbData.TDID, dbData.ID);
            unit.SetDBData(dbData);
        }
        //加载,并且创建
        public void Load(TDBData dbData)
        {
            if (dbData == null)
                return;
            var unit = Spawn(dbData.TDID, dbData.Position.V3, dbData.Rotation.Q, 0, dbData.ID);
            unit.SetDBData(dbData);
        }
        #endregion

        #region Callback
        protected virtual void OnLoseAll(TOwner caster, TOwner underParty) { }
        #endregion
    }
}