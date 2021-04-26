using System;
using UnityEngine;
namespace CYM
{
    public class BaseSenseMgr<TUnit> : BaseMgr, IBaseSenseMgr 
        where TUnit : BaseUnit
    {
        #region prop
        protected Collider SelfCol;
        protected GameObject SenseGameObj;
        protected SphereCollider SenseCollider;
        protected SenseObj SenseObject;
        protected Timer Timer = new Timer();
        protected Collider[] ColliderResults;
        protected virtual float UpdateTimer => float.MaxValue;
        public virtual float Radius => 4;
        protected virtual int MaxColliderResults => 20;
        #endregion

        #region mgr
        IBaseRelationMgr RelationMgr => BaseGlobal.RelationMgr;
        #endregion

        #region list
        public HashList<TUnit> Units { get; private set; } = new HashList<TUnit>();//视野中的Unit
        public HashList<TUnit> UnitsEnemy { get; private set; } = new HashList<TUnit>();//并不安全
        public HashList<TUnit> UnitsAlly { get; private set; } = new HashList<TUnit>();//并不安全
        public HashList<TUnit> UnitsSelf { get; private set; } = new HashList<TUnit>();//并不安全
        #endregion

        #region life
        public virtual string SenseName => throw new NotImplementedException();
        protected virtual LayerData CheckLayer => throw new NotImplementedException("必须重载");
        public override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            SelfCol = SelfBaseUnit.Collider;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (RelationMgr != null)
                RelationMgr.Callback_OnBaseChangeRelation += OnBaseChangeRelation;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            if (RelationMgr != null)
                RelationMgr.Callback_OnBaseChangeRelation -= OnBaseChangeRelation;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            Timer = new Timer(UpdateTimer);
            SenseGameObj = new GameObject("SenseObj");
            SenseGameObj.layer = (int)Const.Layer_Sense;
            SenseGameObj.transform.SetParent(SelfMono.Trans);
            SenseGameObj.transform.localPosition = Vector3.zero;
            SenseGameObj.transform.localScale = Vector3.one;
            SenseGameObj.transform.localRotation = Quaternion.identity;
            SenseObject = BaseMono.GetUnityComponet<SenseObj>(SenseGameObj);
            SenseObject.Init(this);
            SenseCollider = SenseGameObj.AddComponent<SphereCollider>();
            SenseCollider.isTrigger = true;
            SenseCollider.radius = Radius;
            ColliderResults = new Collider[MaxColliderResults];
            SenseGameObj.SetActive(false);
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (Timer.CheckOver())
                DoCollect();
            if (SenseCollider != null && SenseCollider.radius != Radius)
            {
                SenseCollider.radius = Mathf.Lerp(Radius, SenseCollider.radius, Time.deltaTime*2);
            }
        }
        public override void OnBirth()
        {
            base.OnBirth();
            Clear();
            SenseGameObj.SetActive(true);

        }

        public override void OnDeath()
        {
            foreach (var item in SelfBaseUnit.DetectionMgr.Units.ToArray())
            {
                foreach (var sense in item.SenseMgrs)
                    sense.DoTestExit(SelfCol);
            }
            Clear();
            SenseGameObj.SetActive(false);
            base.OnDeath();
        }
        #endregion

        #region set
        public void Clear()
        {
            foreach (var item in Units)
            {
                item.DetectionMgr.Remove(this,SelfBaseUnit);
            }
            Units.Clear();
            UnitsEnemy.Clear();
            UnitsAlly.Clear();
            UnitsSelf.Clear();
            for (int i = 0; i < ColliderResults.Length; i++)
            {
                ColliderResults[i] = null;
            }
        }
        public void RefreshEnemyAlly()
        {
            UnitsEnemy.Clear();
            UnitsAlly.Clear();
            UnitsSelf.Clear();
            foreach (var item in Units)
            {
                if (SelfBaseUnit.IsEnemy(item)) UnitsEnemy.Add(item);
                if (SelfBaseUnit.IsSOF(item)) UnitsAlly.Add(item);
                if (SelfBaseUnit.IsSelf(item)) UnitsSelf.Add(item);
            }
        }
        #endregion

        #region is
        public bool IsInSense(BaseUnit unit) => Units.Contains(unit as TUnit);
        public bool IsInSensePos(TUnit unit) => SenseCollider.bounds.Contains(unit.Pos);
        public bool IsHaveEnemy() => UnitsEnemy.Count > 0;
        public bool IsHave() => Units.Count > 0;
        #endregion

        #region utile
        public void DoCollect()
        {
            Clear();
            int count = Physics.OverlapSphereNonAlloc(SelfBaseUnit.Pos, Radius, ColliderResults, (LayerMask)CheckLayer, QueryTriggerInteraction.Collide);
            if (count > 0)
            {
                if (ColliderResults == null) return;
                foreach (var item in ColliderResults)
                    DoTestEnter(item);
            }
        }
        public void DoTestEnter(Collider col)
        {
            if (!SelfBaseUnit.IsLive)
                return;
            if (col == null) return;
            TUnit unit = col.GetComponent<TUnit>();
            if (unit != null)
            {
                if (!unit.IsLive)
                    return;
                Units.Add(unit);
                if (SelfBaseUnit.IsEnemy(unit)) UnitsEnemy.Add(unit);
                if (SelfBaseUnit.IsSOF(unit)) UnitsAlly.Add(unit);
                if (SelfBaseUnit.IsSelf(unit)) UnitsSelf.Add(unit);
                unit.DetectionMgr.Add(this,SelfBaseUnit);
                OnEnter(unit);
            }
            OnEnterObject(col);
        }
        public void DoTestExit(Collider col)
        {
            if (!SelfBaseUnit.IsLive)
                return;
            TUnit unit = col.GetComponent<TUnit>();
            if (unit != null)
            {
                Units.Remove(unit);
                UnitsEnemy.Remove(unit);
                UnitsAlly.Remove(unit);
                UnitsSelf.Remove(unit);
                unit.DetectionMgr.Remove(this,SelfBaseUnit);
                OnExit(unit);
            }
            OnExitObject(col);
        }
        public void RefreshUnitState()
        {
            if (!SelfBaseUnit.IsLive)
                return;
            UnitsEnemy.Clear();
            UnitsAlly.Clear();
            UnitsSelf.Clear();
            foreach (var item in Units)
            {
                if (SelfBaseUnit.IsEnemy(item)) UnitsEnemy.Add(item);
                if (SelfBaseUnit.IsSOF(item)) UnitsAlly.Add(item);
                if (SelfBaseUnit.IsSelf(item)) UnitsSelf.Add(item);
            }
        }
        #endregion

        #region Callback
        protected virtual void OnEnter(TUnit col) { }
        protected virtual void OnExit(TUnit col) { }
        protected virtual void OnEnterObject(Collider col) { }
        protected virtual void OnExitObject(Collider col) { }

        private void OnBaseChangeRelation()
        {
            RefreshUnitState();
        }
        #endregion
    }

}