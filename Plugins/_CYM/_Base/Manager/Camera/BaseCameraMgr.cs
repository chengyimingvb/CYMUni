using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
namespace CYM
{
    public class BaseCameraMgr : BaseGFlowMgr, IBaseCameraMgr
    {
        #region prop
        float LastedHight = 0;
        public virtual float MostHight => 600.0f;
        public virtual float TopHight => 300.0f;
        public virtual float MidHight => 137.0f;
        public virtual float NearHight => 80.0f;
        public float CameraHight { get; private set; } = 1;
        public bool IsLessNearHight => CameraHight <= NearHight;
        public bool IsInLowHight => CameraHight <= MidHight;
        public bool IsInMiddleHight => CameraHight > MidHight && CameraHight <= TopHight;
        public bool IsMoreMiddleHight => CameraHight > MidHight;
        public bool IsMoreTopHight => CameraHight >= TopHight;
        public bool IsMoreMostHight => CameraHight >= MostHight;
        public float ZoomPercent => CameraHight / MostHight;
        #endregion

        #region Interface
        public PostProcessVolume PostProcessVolume { get; private set; }
        public virtual float HightPercent => 0;
        public float GetCustomHightPercent(float maxVal) => Mathf.Clamp(HightPercent / maxVal, 0, 1.0f);
        public virtual float ScrollVal => 0;
        public float GetCustomScrollVal(float maxVal) => Mathf.Clamp(ScrollVal / maxVal, 0, 1.0f);
        public Camera MainCamera { get; private set; }
        public Transform MainCameraTrans { get; private set; }
        #endregion

        #region Callback
        public event Callback<bool> Callback_OnIsTopHight;
        public event Callback<bool> Callback_OnIsMostHight;
        public event Callback<Camera> Callback_OnFetchCamera;
        #endregion

        #region mgr
        protected IBaseSettingsMgr SettingsMgr => BaseGlobal.SettingsMgr;
        protected DBBaseSettings BaseSettings => BaseGlobal.SettingsMgr.GetBaseSettings();
        #endregion

        #region life
        protected override string ResourcePrefab => "BaseSLGCamera";
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            FetchCamera();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (MainCamera == null) return;
            if (BattleMgr!=null && BattleMgr.IsInBattle)
            {
                LastedHight = CameraHight;
                bool preIsTopHight = IsMoreTopHight;
                bool preIsMostHight = IsMoreMostHight;
                CameraHight = MainCameraTrans.position.y;
                if (IsMoreTopHight != preIsTopHight)
                {
                    Callback_OnIsTopHight?.Invoke(IsMoreTopHight);
                }
                if (IsMoreMostHight != preIsMostHight)
                {
                    Callback_OnIsMostHight?.Invoke(IsMoreMostHight);
                }
            }
        }
        //重场景中重新获得Camera
        public virtual void FetchCamera()
        {
            if (ResourcePrefab.IsInv())
            {
                ResourceObj = CameraObj.GO;
            }
            if (ResourceObj!=null)
            {
                MainCamera = ResourceObj.GetComponent<Camera>();
                MainCameraTrans = MainCamera.transform;
                PostProcessVolume = MainCamera.GetComponentInChildren<PostProcessVolume>();
                Callback_OnFetchCamera?.Invoke(MainCamera);
            }
        }
        #endregion

        #region Is
        public bool IsInScroll()
        {
            return LastedHight != CameraHight;
        }
        #endregion

        #region get
        public Vector3 CameraPos
        {
            get
            {
                if (MainCameraTrans == null)
                    return Vector3.zero;
                return MainCameraTrans.position;
            }
        }
        public T GetPostSetting<T>() where T : PostProcessEffectSettings
        {
            T ret;
            if (PostProcessVolume == null)
                return null;
            PostProcessVolume.profile.TryGetSettings(out ret);
            return ret;
        }
        #endregion

        #region set
        public override void Enable(bool b)
        {
            base.Enable(b);
            if (MainCamera != null)
            {
                MainCamera.enabled = b;
            }
        }
        public void EnableSkyBox(bool b)
        {
            if (MainCamera != null)
            {
                if (b) MainCamera.clearFlags = CameraClearFlags.Skybox;
                else MainCamera.clearFlags = CameraClearFlags.SolidColor;
            }
        }
        public virtual void EnableHUD(bool b)
        {
            if (MainCamera != null)
            {
                MainCamera.allowHDR = b;
                BaseSettings.EnableHUD = b;
            }
        }
        public virtual void EnableMSAA(bool b)
        {
            if (MainCamera != null)
            {
                MainCamera.allowMSAA = b;
                BaseSettings.EnableMSAA = b;
            }
        }
        public virtual void EnableBloom(bool b)
        {
            BaseSettings.EnableBloom = b;
        }
        public virtual void EnableSSAO(bool b)
        {
            BaseSettings.EnableSSAO = b;
        }
        public virtual void EnableShadow(bool b)
        {
            BaseSettings.EnableShadow = b;
        }
        #endregion

        #region jump
        int CurJumpIndex = 0;
        IList LastJumpList;
        public virtual void Jump(Transform target, float? heightPercent = null)
        {
            throw new NotImplementedException();
        }
        public void Jump(Vector3 pos, float? heightPercent = null)
        {
            Jump(BaseGlobal.GetTransform(pos),heightPercent);
        }
        public virtual void Jump(BaseUnit target, float? heightPercent = null)
        {
            if (target == null) return;
            Jump(target.Trans, heightPercent);
        }
        public virtual void SetPos(Transform target)
        {
            MainCameraTrans.transform.position = target.position;
            Jump(target);
        }
        public virtual void Jump<TUnit>(List<TUnit> list, float? heightPercent = null, Func<TUnit, Transform> customTrans = null) where TUnit : BaseUnit
        {
            if (list.Count <= 0) return;
            if (LastJumpList != list) CurJumpIndex = 0;
            LastJumpList = list;
            if (list.Count <= CurJumpIndex) CurJumpIndex = 0;
            if (customTrans == null)
            {
                Jump(list[CurJumpIndex], heightPercent);
            }
            else
            {
                Jump(customTrans(list[CurJumpIndex]), heightPercent);
            }
            CurJumpIndex++;
        }
        #endregion
    }

}