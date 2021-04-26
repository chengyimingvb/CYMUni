//------------------------------------------------------------------------------
// BaseSLGCamera.cs
// Copyright 2018 2018/11/13 
// Created by CYM on 2018/11/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Cam;
using CYM.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BaseSLGCameraMgr : BaseCameraMgr
    {
        #region prop
        RTSCamera RTSCamera;
        DBBaseSettings SettingsData => BaseGlobal.SettingsMgr.GetBaseSettings();
        public override float ScrollVal => RTSCamera.ScrollValue;
        public override float HightPercent => (CameraHight - RTSCamera.MinHight) / (RTSCamera.MaxHight - RTSCamera.MinHight);
        #endregion

        #region life
        bool isScrollControl = true;
        bool isDragControl = true;
        bool isScreenEdgeControl = true;
        bool isControlDisabled = false;

        protected virtual float SpeedFaction => 1;
        public virtual bool IsScrollControl=> 
                BaseInputMgr.IsEnablePlayerInput &&
                !BaseInputMgr.IsStayInUIWithoutHUD &&
                !BaseInputMgr.IsIMUIShow &&
                isScrollControl;
        public virtual bool IsDragControl =>
                BaseInputMgr.IsEnablePlayerInput &&
                !BaseInputMgr.IsStayInUIWithoutHUD &&
                !BaseInputMgr.IsIMUIShow &&
                isDragControl;
        public virtual bool IsScreenEdgeControl => isScreenEdgeControl;
        public virtual bool IsControlDisabled=>
                 Options.IsLockCamera ||
                 BaseInputMgr.IsFullScreen ||
                 BaseInputMgr.IsDevConsoleShow ||
                 !BaseInputMgr.IsEnablePlayerInput ||
                 BattleMgr.IsLoadingBattle ||
                 isControlDisabled;
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (RTSCamera == null) return;
            if (BattleMgr != null && 
                BattleMgr.IsInBattle)
            {
                var data = SettingsData;
                var zoomPercent = ZoomPercent;
                RTSCamera.DesktopMoveDragSpeed = (zoomPercent * RTSCamera.desktopMoveDragSpeed) * data.CameraMoveSpeed * SpeedFaction;
                RTSCamera.DesktopMoveSpeed = (zoomPercent * RTSCamera.desktopMoveSpeed) * data.CameraMoveSpeed * SpeedFaction;
                RTSCamera.DesktopScrollSpeed = (zoomPercent * RTSCamera.desktopScrollSpeed) * data.CameraScrollSpeed * SpeedFaction;
                RTSCamera.DesktopRotateSpeed = RTSCamera.desktopRotateSpeed;

                RTSCamera.TouchMoveDragSpeed = RTSCamera.touchMoveDragSpeed * data.CameraMoveSpeed * SpeedFaction;
                RTSCamera.TouchMoveSpeed = RTSCamera.touchMoveSpeed * data.CameraMoveSpeed * SpeedFaction;
                RTSCamera.TouchScrollSpeed = RTSCamera.touchScrollSpeed * data.CameraScrollSpeed * SpeedFaction;
                RTSCamera.TouchRotateSpeed = RTSCamera.touchRotateSpeed;

                RTSCamera.ScreenEdgeMoveControl(IsScreenEdgeControl);
                RTSCamera.ScrollControl(IsScrollControl);
                RTSCamera.DragControl(IsDragControl);
                RTSCamera.ControlDisabled.Set(IsControlDisabled);
            }
            else
            {
                RTSCamera.ControlDisabled.Set(true);
            }
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
        }
        public override void FetchCamera()
        {
            base.FetchCamera();
            if (MainCamera != null)
            {
                RTSCamera = MainCamera.GetComponentInChildren<RTSCamera>();
                if(RTSCamera==null)
                    RTSCamera = MainCamera.gameObject.AddComponent<RTSCamera>();
            }
        }
        #endregion

        #region jump
        public override void Jump(BaseUnit target, float? heightPercent = null)
        {
            base.Jump(target, heightPercent);
        }
        public override void Jump(Transform target, float? heightPercent = 0.05f)
        {
            if (target == null) return;
            RTSCamera.JumpToTarget(target);
            if (heightPercent != null)
                RTSCamera.SetScroll(heightPercent.Value);
        }
        #endregion

        #region set
        public void SetScrollControl(bool b)
        {
            isScrollControl = b;
        }
        public void SetDragControl(bool b)
        {
            isDragControl = b;
        }
        public void SetScreenEdgeControl(bool b)
        {
            isScreenEdgeControl = b;
        }
        public void Lock(bool b)
        {
            isControlDisabled = b;
        }
        public override void Enable(bool b)
        {
            base.Enable(b);
            if (RTSCamera)
            {
                RTSCamera.enabled = b;
            }
        }
        public void SetMinMaxHeight(float min, float max)
        {
            RTSCamera?.SetMinMaxHeight(min, max);
        }
        public void SetGroundTest(bool isTest)
        {
            RTSCamera?.SetGroundTest(isTest);
        }
        public void Move(Vector3 dir)
        {
            if (RTSCamera == null)
                return;
            RTSCamera.Move(dir);
        }
        public void Follow(BaseUnit target)
        {
            RTSCamera.Follow(target.Trans);
        }
        public void CancleFollow()
        {
            RTSCamera.CancelFollow();
        }
        public void SetCameraMoveSpeed(float v)
        {
            SettingsData.CameraMoveSpeed = v;
        }
        public void SetCameraScrollSpeed(float v)
        {
            SettingsData.CameraScrollSpeed = v;
        }
        public void SetBound(float minX,float maxX,float minY,float maxY)
        {
            RTSCamera?.SetBound(new List<float>() { minX, minY, maxX, maxY });
        }
        public void SetBound(List<float> data)
        {
            RTSCamera?.SetBound(data);
        }
        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            isScrollControl = true;
            isDragControl = true;
            isScreenEdgeControl = true;
            isControlDisabled = false;
        }
        #endregion
    }
}