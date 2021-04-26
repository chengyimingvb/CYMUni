//**********************************************
// Class Name	: Unit
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************


using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class DeathParam
    {
        public bool IsDelayDespawn = true;
        public BaseUnit Caster = null;
    }
    /// <summary>
    /// 1.BaseUnit 的 NeedUpdate 和 NeedFixedUpdate 默认必须关掉,
    /// 2.因为其他的物件可能会继承BaseUnit,而很多物件不需要Update和FixedUpdate
    /// 3.OnEnable 时候会自动归位缩放
    /// </summary>
    public class BaseUnit : BaseCoreMono
    {
        #region list Componet
        public List<IBaseSenseMgr> SenseMgrs { get; protected set; } = new List<IBaseSenseMgr>();
        public Dictionary<Type, IUnitMgr> UnitMgrs { get; private set; } = new Dictionary<Type, IUnitMgr>();
        #endregion

        #region entity mgr
        public IUnitMgr UnitMgr { get; set; }
        public IUnitSpawnMgr SpawnMgr { get; set; }
        #endregion

        #region Componet
        public IBaseSenseMgr SenseMgr { get; protected set; }
        public IBaseAttrMgr AttrMgr { get; protected set; }
        public IBaseBuffMgr BuffMgr { get; protected set; }
        public IBaseHUDMgr HUDMgr { get; protected set; }
        public IBaseMoveMgr MoveMgr { get; protected set; }
        public IBaseAStarMoveMgr AStarMoveMgr => MoveMgr as IBaseAStarMoveMgr;
        public IBaseDipMgr DipMgr { get; protected set; }
        public IBaseCastleStationedMgr CastleStationedMgr { get; protected set; }
        public IBaseLegionStationedMgr LegionStationedMgr { get; protected set; }
        public BaseDetectionMgr DetectionMgr { get; protected set; }
        public BaseNodeMgr NodeMgr { get; protected set; }
        public BaseAnimMgr AnimMgr { get; protected set; }
        public BaseVoiceMgr AudioMgr { get; protected set; }
        public BaseAIMgr AIMgr { get; protected set; }
        public BaseFOWRevealerMgr FOWMgr { get; protected set; }
        public BaseSurfaceMgr SurfMgr { get; protected set; }
        public BasePerformMgr PerformMgr { get; protected set; }
        public BaseAStarMove2DMgr Move2DMgr { get; protected set; }
        public BaseMarkMgr MarkMgr { get; protected set; }
        #endregion

        #region inspector
        [FoldoutGroup("Base"), SerializeField, TextArea, Tooltip("用户自定义描述")]
        protected string Desc = "";
        [FoldoutGroup("Base"), SerializeField, Tooltip("单位的TDID")]
        protected new string TDID = "";
        [FoldoutGroup("Base"), MinValue(0), SerializeField, Tooltip("单位的队伍")]
        public int Team = 0;
        #endregion

        #region base
        public TDBaseData BaseConfig { get; protected set; } = new TDBaseData();
        public DBBaseUnit DBBaseData { get; protected set; } = new DBBaseUnit();
        public BaseUnit BaseOwner { get; protected set; }
        public BaseUnit BasePreOwner { get; protected set; }
        public DeathParam DeathParam { get; private set; } = null;
        #endregion

        #region prop
        protected bool IsCanGamePlayInput => BaseInputMgr.IsCanGamePlayInput();
        public EventMgr<string> TriggerMgr { get; private set; } = new EventMgr<string>();
        #endregion

        #region timer
        Timer DeathRealTimer = new Timer();
        Timer DeathEffStartTimer = new Timer();
        #endregion

        #region global mgr
        protected BaseInputMgr InputMgr => BaseGlobal.InputMgr;
        protected IBaseScreenMgr ScreenMgr => BaseGlobal.ScreenMgr;
        #endregion

        #region Callback
        public event Callback<bool> Callback_OnTurnStart;
        public event Callback Callback_OnTurnEnd;
        public event Callback Callback_OnCantEndTurn;
        public event Callback Callback_OnPreEndTurn;
        public event Callback Callback_OnTurnOperating;
        public event Callback Callback_OnUnBeSetPlayer;
        public event Callback Callback_OnBeSetPlayer;
        public event Callback Callback_OnMouseDown;
        public event Callback Callback_OnMouseUp;
        public event Callback Callback_OnMouseEnter;
        public event Callback Callback_OnMouseExit;
        public event Callback<bool> Callback_OnBeSelected;
        public event Callback Callback_OnUnBeSelected;
        public static Callback<BaseUnit> Callback_OnRealDeathG { get; internal set; }
        public static Callback<BaseUnit> Callback_OnDeathG { get; internal set; }
        #endregion

        #region time
        public virtual float DeathDespawnTime => DeathRealTime + 0.1f; // 彻底消除的时间
        public virtual float DeathRealTime => 3.0f; // 从Death到RealDeath的时间
        public virtual float DeathEffStartTime => 1.0f;// 死亡效果开始的时间
        #endregion

        #region life
        public override MonoType MonoType => MonoType.Unit;
        public override void OnEnable()
        {
            Trans.localScale = Vector3.one;
            base.OnEnable();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            TriggerMgr.Clear();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateRendered();
            if (!IsLive && !IsRealDeath)
            {
                if (DeathRealTimer.CheckOverOnce())
                    OnRealDeath();
                else if (DeathEffStartTimer.CheckOverOnce())
                    OnDissolve();
            }
        }
        public void OnTurnStart(bool isForce)
        {
            Callback_OnTurnStart?.Invoke(isForce);
        }
        public void OnTurnEnd()
        {
            Callback_OnTurnEnd?.Invoke();
        }
        public void OnCantEndTurn()
        {
            Callback_OnCantEndTurn?.Invoke();
        }
        public void OnPreEndTurn()
        {
            Callback_OnPreEndTurn?.Invoke();
        }
        public void OnTurnOperating()
        {
            Callback_OnTurnOperating?.Invoke();
        }
        #endregion

        #region unit life
        public override void OnInit()
        {
            IsLive = false;
            base.OnInit();
            if (DeathEffStartTime > DeathRealTime)
                throw new Exception("溶解的时间不能大于死亡时间");
        }
        public override void OnDeath()
        {
            if (!IsLive) return;
            IsLive = false;
            Callback_OnDeathG?.Invoke(this);
            DeathRealTimer.Restart(DeathRealTime);
            DeathEffStartTimer.Restart(DeathEffStartTime);
            UnitMgr?.Despawn(this);
            base.OnDeath();
        }
        public override void OnBirth()
        {
            if (IsLive) return;
            IsLive = true;
            IsRealDeath = false;
            base.OnBirth();
        }
        public override void OnRealDeath()
        {
            Callback_OnRealDeathG?.Invoke(this);
            IsRealDeath = true;
            base.OnRealDeath();
        }
        protected virtual void UpdateRendered()
        {
            if (BaseGlobal.CameraMgr == null)
                return;
            if (!BaseGlobal.CameraMgr.IsEnable)
                return;
            if (BaseGlobal.CameraMgr.MainCamera == null)
                return;
            Vector3 pos = BaseGlobal.CameraMgr.MainCamera.WorldToViewportPoint(Trans.position);
            IsRendered = (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
            if (IsRendered != IsLastRendered)
            {
                if (IsRendered) OnBeRender();
                else OnBeUnRender();

                IsLastRendered = IsRendered;
            }
        }

        protected virtual void OnBecameInvisible() => IsVisible = false;
        protected virtual void OnBecameVisible() => IsVisible = true;
        protected virtual void OnDissolve() { }
        #endregion

        #region life set
        public override T AddComponent<T>()
        {
            var ret = base.AddComponent<T>();
            //加入组件列表
            if (ret is IUnitMgr entityMgr) UnitMgrs.Add(entityMgr.UnitType, entityMgr);
            if (ret is IBaseSenseMgr senseMgr) SenseMgrs.Add(senseMgr);
            //添加组件
            if (ret is IBaseSenseMgr && SenseMgr == null) SenseMgr = ret as IBaseSenseMgr;
            else if (ret is IBaseAttrMgr && AttrMgr == null) AttrMgr = ret as IBaseAttrMgr;
            else if (ret is IBaseBuffMgr && BuffMgr == null) BuffMgr = ret as IBaseBuffMgr;
            else if (ret is IBaseHUDMgr && HUDMgr == null) HUDMgr = ret as IBaseHUDMgr;
            else if (ret is IBaseMoveMgr && MoveMgr == null) MoveMgr = ret as IBaseMoveMgr;
            else if (ret is IBaseDipMgr && DipMgr == null) DipMgr = ret as IBaseDipMgr;
            else if (ret is IBaseLegionStationedMgr && LegionStationedMgr == null) LegionStationedMgr = ret as IBaseLegionStationedMgr;
            else if (ret is IBaseCastleStationedMgr && CastleStationedMgr == null) CastleStationedMgr = ret as IBaseCastleStationedMgr;
            else if (ret is BaseDetectionMgr && DetectionMgr == null) DetectionMgr = ret as BaseDetectionMgr;
            else if (ret is BaseNodeMgr && NodeMgr == null) NodeMgr = ret as BaseNodeMgr;
            else if (ret is BaseAnimMgr && AnimMgr == null) AnimMgr = ret as BaseAnimMgr;
            else if (ret is BaseVoiceMgr && AudioMgr == null) AudioMgr = ret as BaseVoiceMgr;
            else if (ret is BaseAIMgr && AIMgr == null) AIMgr = ret as BaseAIMgr;
            else if (ret is BaseFOWRevealerMgr && FOWMgr == null) FOWMgr = ret as BaseFOWRevealerMgr;
            else if (ret is BaseSurfaceMgr && SurfMgr == null) SurfMgr = ret as BaseSurfaceMgr;
            else if (ret is BasePerformMgr && PerformMgr == null) PerformMgr = ret as BasePerformMgr;
            else if (ret is BaseAStarMove2DMgr && Move2DMgr == null) Move2DMgr = ret as BaseAStarMove2DMgr;
            else if (ret is BaseMarkMgr && MarkMgr == null) MarkMgr = ret as BaseMarkMgr;
            return ret;
        }
        public virtual void DoDeath()
        {
            if (DeathParam == null)
            {
                DeathParam = new DeathParam();
            }
            OnDeath();
            Clear();
            SpawnMgr?.Despawn(this, DeathParam.IsDelayDespawn ? DeathDespawnTime : 0);
            DeathParam = null;
        }
        public void DoDeath(DeathParam param)
        {
            SetDeathParam(param);
            DoDeath();
        }
        #endregion

        #region set
        // 设置小队
        public void SetTeam(int? team)
        {
            if (team.HasValue)
                Team = team.Value;
        }
        // 设置TDID
        public void SetTDID(string tdid)
        {
            if (tdid.IsInv()) base.TDID = TDID = gameObject.name;
            else base.TDID = TDID = tdid;
        }
        public virtual void SetRTID(long rtid) => ID = rtid;
        public virtual void SetConfig(TDBaseData config)
        {
            if (config == null) config = new TDBaseData();
            BaseConfig = config;
            BaseConfig.OnBeAdded(this);
        }
        public virtual void SetOwner(BaseUnit owner)
        {
            BasePreOwner = BaseOwner;
            BaseOwner = owner;
        }
        public virtual void SetDBData(DBBaseUnit dbData)
        {
            DBBaseData = dbData;
        }
        public void SetDeathParam(DeathParam param)
        {
            DeathParam = param;
        }
        #endregion

        #region message set
        public void AddListener(string rMessageType, Callback<object> callback)
        {
            if (!isActiveAndEnabled) return;
            TriggerMgr.Add(rMessageType, callback);
        }
        public void TriggerListener(string rType, object rData)
        {
            if (!isActiveAndEnabled) return;
            TriggerMgr.Trigger(rType, rData);
        }
        #endregion

        #region get
        public virtual float CalcScore()
        {
            return 0;
        }
        public virtual string GetTDID() => TDID;
        //获得综合评分
        public float Score { get; set; }=0;
        public override string ToString()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetName();
        }
        public string GetName()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetName();
        }
        public string GetDesc()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetDesc();
        }
        public Sprite GetIcon()
        {
            if (BaseConfig == null) return null;
            return BaseConfig.GetIcon();
        }
        public IUnitMgr GetUnitMgr(Type unitType)
        {
            if (UnitMgrs.ContainsKey(unitType))
            {
                return UnitMgrs[unitType];
            }
            return null;
        }
        #endregion

        #region is
        // 是否为系统类型
        public virtual bool IsSystem => BaseConfig.IsSystem;
        // 是否为荒野类型
        public virtual bool IsWild => BaseConfig.IsWild;
        // 是否死亡
        public bool IsLive { get; protected set; } = false;
        // 是否真的死亡
        public bool IsRealDeath { get; protected set; } = false;
        // 是否被渲染(计算位置是否在摄像机中)
        public bool IsRendered { get; private set; } = false;
        // 是否被摄像机渲染
        public bool IsVisible { get; private set; } = false;
        // 上一帧被渲染
        public bool IsLastRendered { get; private set; } = false;
        // 是否为本地玩家
        public virtual bool IsPlayer() => ScreenMgr.BaseLocalPlayer == this;
        // 是否为其他玩家
        public virtual bool IsPlayerCtrl() => IsPlayer();
        public virtual bool IsAI() => !IsPlayerCtrl();
        // 是否是敌人
        public virtual bool IsEnemy(BaseUnit other)
        {
            if (other == null)
                return false;
            return other.Team != Team;
        }
        // 是否是友军
        public virtual bool IsFriend(BaseUnit other)
        {
            if (other == null)
                return false;
            return other.Team == Team;
        }
        // Self or Friend
        public virtual bool IsSOF(BaseUnit other)
        {
            if (other == null)
                return false;
            return IsFriend(other) || IsSelf(other);
        }
        // 是否为本地玩家的对立面
        public virtual bool IsOpposite() => false;
        // 是否为自己
        public virtual bool IsSelf(BaseUnit other)
        {
            if (other == null)
                return false;
            return this == other;
        }
        // 是否为中立怪
        public virtual bool IsNeutral() => Team == 2;
        // 非中立怪 敌人
        public virtual bool IsUnNeutralEnemy(BaseUnit other)
        {
            if (other == null)
                return false;
            if (other.IsNeutral())
                return false;
            return IsEnemy(other);
        }
        #endregion

        #region other
        public float Importance { get; set; }
        #endregion

        #region Callback
        protected virtual void OnBeRender() { }
        protected virtual void OnBeUnRender() { }
        protected virtual void OnMouseDown()
        {
            if (!IsCanGamePlayInput)
                return;
            Callback_OnMouseDown?.Invoke();
        }

        protected virtual void OnMouseEnter()
        {
            Callback_OnMouseEnter?.Invoke();
            InputMgr.OnEnterUnit(this);
        }

        protected virtual void OnMouseExit()
        {
            Callback_OnMouseExit?.Invoke();
            InputMgr.OnExitUnit(this);
        }
        protected virtual void OnMouseUp()
        {
            if (!IsCanGamePlayInput)
                return;
            Callback_OnMouseUp?.Invoke();
        }
        public virtual void OnBeSelected(bool isRepeat)
        {
            if (!IsCanGamePlayInput)
                return;
            Callback_OnBeSelected?.Invoke(isRepeat);
        }
        public virtual void OnUnBeSelected()
        {
            Callback_OnUnBeSelected?.Invoke();
        }
        public virtual void OnUnBeSetPlayer()
        {
            Callback_OnUnBeSetPlayer?.Invoke();
        }
        public virtual void OnBeSetPlayer()
        {
            Callback_OnBeSetPlayer?.Invoke();
        }

        #endregion

        #region inspector
        [Button("CopyName")]
        void CopyName()
        {
            Util.CopyTextToClipboard(GOName);
        }

        #endregion


    }
}