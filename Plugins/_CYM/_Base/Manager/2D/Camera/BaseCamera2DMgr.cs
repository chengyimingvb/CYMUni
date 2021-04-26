//------------------------------------------------------------------------------
// BaseCamera2DMgr.cs
// Copyright 2019 2019/12/11 
// Created by CYM on 2019/12/11
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CYM
{
    public class BaseCamera2DMgr : BaseGFlowMgr, IBaseCameraMgr
    {
        #region Callback
        public event Callback<Camera> Callback_OnFetchCamera;
        #endregion

        #region interface
        public PostProcessVolume PostProcessVolume { get; private set; }
        public virtual float ScrollVal => 0;
        public float GetCustomScrollVal(float maxVal) => Mathf.Clamp(ScrollVal / maxVal, 0, 1.0f);
        public Camera MainCamera { get; private set; }
        public Transform MainCameraTrans { get; private set; }
        #endregion

        #region life
        protected override string ResourcePrefab => throw new System.Exception("此函数必须被实现");
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

        #region get
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
            if (MainCamera)
                MainCamera.enabled = b;
        }
        #endregion
    }
}