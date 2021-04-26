//------------------------------------------------------------------------------
// StarRate.cs
// Created by CYM on 2021/2/22
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CYM.UI
{
    public class UStarRateData : UPresenterData
    {
        public Func<float> GetRate;
    }
    [AddComponentMenu("UI/Control/UStarRate")]
    [HideMonoScript]
    public class UStarRate : UPresenter<UStarRateData>
    {
        [SerializeField]
        Image StarRateFull;
        [SerializeField]
        Image StarRateHalf;
        [SerializeField]
        int MaxRate = 5;

        public override void Refresh()
        {
            base.Refresh();

            float step = 1.0f / MaxRate;
            if (Data != null)
            {
                if (Data.GetRate != null)
                {
                    float curRate = Data.GetRate.Invoke();
                    curRate = Mathf.Clamp(curRate,0,MaxRate);
                    if (StarRateFull != null)
                    {
                        StarRateFull.fillAmount = (int)(curRate) * step;
                    }
                    if (StarRateHalf != null)
                    {
                        StarRateHalf.fillAmount = Mathf.CeilToInt(curRate) * step;
                    }
                }
            }
        }
    }
}