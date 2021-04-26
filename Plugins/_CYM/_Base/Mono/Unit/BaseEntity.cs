//------------------------------------------------------------------------------
// Unit.cs
// Copyright 2018 2018/11/3 
// Created by CYM on 2018/11/3
// Owner: CYM
// 游戏的基础对象
// 实体对象,用于模拟具体得实物对象,比如国家,城市,军团,完整角色
//------------------------------------------------------------------------------

using System;

namespace CYM
{
    public class BaseEntity<TUnit, TConfig, TDBData, TOwner> : BaseUnit
        where TUnit : BaseEntity<TUnit, TConfig, TDBData, TOwner>
        where TConfig : TDBaseData, new()
        where TDBData : DBBaseUnit, new()
        where TOwner : BaseUnit
    {
        #region Callback
        /// <summary>
        /// 设置玩家的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public event Callback<TOwner, TOwner> Callback_OnSetOwner;
        /// <summary>
        /// 被占领的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public Callback<TOwner, TOwner> Callback_OnBeOccupied { get; set; }
        public Callback<bool> Callback_OnBeCapital { get; set; }
        #endregion

        #region prop
        //OnSpawned 赋值
        public TConfig Config => BaseConfig as TConfig;
        public TDBData DBData => DBBaseData as TDBData;
        public TOwner PreOwner => BasePreOwner as TOwner;
        //AddToData 赋值
        public TOwner Owner => BaseOwner as TOwner;
        #endregion

        #region mgr
        protected ITDLuaMgr TDLuaMgr { get; private set; }
        #endregion

        #region type
        public Type UnitType { get; private set; }
        public Type ConfigType { get; private set; }
        #endregion

        #region life
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            UnitType = typeof(TUnit);
            ConfigType = typeof(TConfig);
            TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(ConfigType);
        }
        public override void OnDeath()
        {
            base.OnDeath();
        }
        #endregion

        #region set
        // 设置父对象
        public virtual void SetOwner(TOwner unit)
        {
            base.SetOwner(unit);
            if (unit == null)
            {
                CLog.Error("错误SetOwner:Unit为null,{0}", GetName());
                return;
            }
            Callback_OnSetOwner?.Invoke(PreOwner, Owner);
        }
        #endregion

        #region is
        public bool IsHaveOwner() => Owner != null;
        public override bool IsPlayer() => ScreenMgr.BaseLocalPlayer == Owner;
        #endregion

        #region DB
        public override void OnRead1<TGData>(TGData data)
        {
            base.OnRead1(data);
            if (Config!=null)
            {
                Config.CustomName = DBData.CustomName;
            }
        }
        public override void OnWrite<TGData>(TGData data)
        {
            DBBaseData = new TDBData();
            DBData.ID = ID;
            DBData.TDID = TDID;
            DBData.Position.Fill(Pos);
            DBData.Rotation.Fill(Rot);
            DBData.CustomName = Config.CustomName;
            DBData.IsNewAdd = false;
            base.OnWrite(data);
        }
        #endregion

        #region operate
        public static explicit operator BaseEntity<TUnit, TConfig, TDBData, TOwner>(long data)
        {
            return BaseGlobal.GetUnit<TUnit>(data);
        }
        public static explicit operator BaseEntity<TUnit, TConfig, TDBData, TOwner>(string data)
        {
            return BaseGlobal.GetUnit<TUnit>(data);
        }
        #endregion
    }
}