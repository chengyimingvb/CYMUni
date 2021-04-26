//------------------------------------------------------------------------------
// TakeCamera.cs
// Copyright 2019 2019/3/29 
// Created by CYM on 2019/3/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace CYM
{
    public class TakeCamera : MonoBehaviour
    {
        [Button("Capture")]
        public void Capture()
        {
            ScreenCapture.CaptureScreenshot(Const.Path_Screenshot + "/Screenshot.png", ScreenCapture.StereoScreenCaptureMode.BothEyes);
            FileUtil.OpenDev();
        }
    }
}