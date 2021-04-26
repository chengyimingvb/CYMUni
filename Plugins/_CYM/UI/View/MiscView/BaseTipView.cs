//------------------------------------------------------------------------------
// BaseErrorTipView.cs
// Copyright 2020 2020/7/1 
// Created by CYM on 2020/7/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BaseTipView : BaseStaticUIView<BaseTipView>
    {
        [SerializeField]
        UText Desc;

        CoroutineHandle Coroutine;
        public void ShowError(string key, params object[] ps)
        {
            Show(true);
            Desc.Refresh(UIUtil.Red(Util.GetStr(key, ps)));
            StartEnumerator();
        }
        public void Show(string key, params object[] ps)
        {
            Show(true);
            Desc.Refresh(Util.GetStr(key, ps));
            StartEnumerator();
        }
        void StartEnumerator()
        {
            BaseGlobal.CommonCoroutineter.Kill(Coroutine);
            Coroutine = BaseGlobal.CommonCoroutineter.Run(_Close());
        }
        IEnumerator<float> _Close()
        {
            yield return Timing.WaitForSeconds(2f);
            Close();
        }
    }
}