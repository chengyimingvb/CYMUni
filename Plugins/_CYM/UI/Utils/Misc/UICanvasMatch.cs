//------------------------------------------------------------------------------
// UICanvasMatch.cs
// Copyright 2021 2021/3/8 
// Created by CYM on 2021/3/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class UICanvasMatch : MonoBehaviour 
    {
        CanvasScaler CanvasScaler;
        [SerializeField]
        float Threshold = 0.5625f;
        private void Awake()
        {            
            CanvasScaler = GetComponent<CanvasScaler>();
        }

        private void Update()
        {
            if (CanvasScaler != null)
            {
                float radio = Screen.width / Screen.height;
                if (radio <= Threshold)
                    CanvasScaler.matchWidthOrHeight = 0.0f;
                else
                    CanvasScaler.matchWidthOrHeight = 1.0f;
            }
        }
    }
}