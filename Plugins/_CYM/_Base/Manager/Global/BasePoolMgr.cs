using CYM.Pool;
using UnityEngine;

//**********************************************
// Class Name	: CYMPoolManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class BasePoolMgr : BaseGFlowMgr
    {
        #region member variable
        public SpawnPool Common { get; private set; }
        public SpawnPool Unit { get; private set; }
        public SpawnPool Perform { get; private set; }
        public SpawnPool HUD { get; private set; }
        public SpawnPool JumpText { get; private set; }
        #endregion

        #region methon
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);

        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        public virtual void CreateBasePool()
        {
            Common = PoolManager.Create("Common");
            Unit = PoolManager.Create("Units");
            Perform = PoolManager.Create("Perform");
            HUD = PoolManager.Create("HUD");
            JumpText = PoolManager.Create("JumpText");
        }
        public void DestroyBasePool()
        {
            PoolManager.DestroyAll();
            Common = null;
            Unit = null;
            Perform = null;
            HUD = null;
            JumpText = null;
        }
        public SpawnPool CreatePool(string name)
        {
            return PoolManager.Pools.Create(name);
        }
        public GameObject Spawn(string name)
        {
            return Common.Spawn(GRMgr.Prefab.Get(name));
        }
        public void Despawn(GameObject go)
        {
            Common.Despawn(go);
        }
        public GameObject SpawnFX(string name)
        {
            return Common.Spawn(GRMgr.Perfome.Get(name));
        }
        public void DespawnFX(GameObject go)
        {
            Common.Despawn(go);
        }
        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
            CreateBasePool();
        }
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            DestroyBasePool();
        }
        #endregion


    }

}

