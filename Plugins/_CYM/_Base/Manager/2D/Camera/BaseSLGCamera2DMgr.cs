//------------------------------------------------------------------------------
// BaseSLGCamera2DMgr.cs
// Copyright 2019 2019/12/11 
// Created by CYM on 2019/12/11
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Cam;
using CYM.UI;
using UnityEngine;

namespace CYM
{
    public class BaseSLGCamera2DMgr : BaseCamera2DMgr
    {
        #region prop
        public override float ScrollVal => RTSCamera.scrollValue;
        RTSCamera2D RTSCamera;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (RTSCamera == null) return;
            if (BattleMgr != null)
            {
                if (BattleMgr.IsInBattle)
                {
                    RTSCamera.MouseControl(
                        !BaseInputMgr.IsStayInUI &&
                        !BaseInputMgr.IsStayHUDBar &&
                        !BaseInputMgr.IsFullScreen &&
                        !BaseInputMgr.IsDevConsoleShow &&
                        !BattleMgr.IsLoadingBattle
                        );
                }
                else
                {
                    RTSCamera.MouseControl(false);
                }
            }
        }
        public override void FetchCamera()
        {
            base.FetchCamera();
            if (MainCamera != null)
            {
                RTSCamera = MainCamera.GetComponentInChildren<RTSCamera2D>();
                if (RTSCamera == null)
                    RTSCamera = MainCamera.gameObject.AddComponent<RTSCamera2D>();
            }
        }
        #endregion

        #region set
        public override void Enable(bool b)
        {
            base.Enable(b);
            RTSCamera.enabled = b;
        }
        public void SetScroll(float scroll)
        {
            RTSCamera.scrollValue = scroll;
        }
        public void SetPos(BaseMono mono)
        {
            if (mono == null) return;
            RTSCamera.transform.position = mono.transform.position - Vector3.forward;
            JumpTo(mono);
        }
        public void JumpTo(BaseMono mono)
        {
            RTSCamera.JumpTo(mono.Trans);
        }
        #endregion
    }
}