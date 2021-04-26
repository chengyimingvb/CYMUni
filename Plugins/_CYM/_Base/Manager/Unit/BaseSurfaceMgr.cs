//**********************************************
// Class Name	: CYMBaseSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CYM
{
    public class BaseSurfaceMgr : BaseMgr
    {
        #region member variable
        public Renderer[] ModelRenders { get; protected set; }//模型自身的渲染器
        public SkinnedMeshRenderer[] SkinnedMeshRenderers { get; protected set; }//蒙皮渲染
        public SkinnedMeshRenderer MainSkinnedMesh { get; protected set; }//主要的蒙皮
        public GameObject Model { get; protected set; }//模型自身的渲染器的跟节点
        public bool IsEnableRenders { get; private set; }
        protected virtual bool IsUseSurfaceMaterial { get; } //禁用材质效果,这样可以使用GPUInstance
        protected BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        #endregion

        #region property
        public BaseSurf CurSurface { get; set; }
        public SurfSource SurfSource { get; private set; } = new SurfSource();
        #endregion

        #region life
        public override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            AssianModel();
            if (Model == null)
            {
                CLog.Error("Unit 没有model");
                return;
            }
            ModelRenders = Model.GetComponentsInChildren<Renderer>();
            {
            }
            SkinnedMeshRenderers = Model.GetComponentsInChildren<SkinnedMeshRenderer>();
            {
                float lastSize = 0.0f;
                float curSize = 0.0f;
                foreach (var item in SkinnedMeshRenderers)
                {
                    Vector3 extents = item.bounds.extents;
                    curSize = extents.x + extents.y + extents.z;
                    if (curSize > lastSize)
                    {
                        lastSize = curSize;
                        MainSkinnedMesh = item;
                    }
                }
            }
            IsEnableRenders = true;
            if (IsUseSurfaceMaterial)
                SurfSource.InitByMgr(this);
        }
        public override void OnBirth()
        {
            base.OnBirth();
            if (IsUseSurfaceMaterial)
                SurfSource.Use();
            CloseHighlight();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();

        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (IsUseSurfaceMaterial)
                CurSurface?.Update();
        }
        protected virtual void AssianModel()
        {
            Model = SelfMono.GO;
        }
        public virtual void EnableRender(bool enable)
        {
            if (IsEnableRenders == enable)
                return;
            if (ModelRenders != null)
            {
                for (int i = 0; i < ModelRenders.Length; ++i)
                    ModelRenders[i].enabled = enable;
                IsEnableRenders = enable;
            }
        }
        public virtual void SetShadowMode(ShadowCastingMode mode)
        {
            if (ModelRenders != null)
            {
                foreach (var item in ModelRenders)
                {
                    item.shadowCastingMode = mode;
                }
            }
        }
        public void EnableReceiveShadows(bool b)
        {
            if (ModelRenders != null)
            {
                foreach (var item in ModelRenders)
                {
                    item.receiveShadows = b;
                }
            }
        }
        #endregion

        #region set
        public virtual void ShowSelectEffect(bool b)
        {

        }
        #endregion

        #region highlight
        public void SetHighlighted(Color col)
        {

        }
        public void CloseHighlight()
        {

        }
        #endregion
    }
}
