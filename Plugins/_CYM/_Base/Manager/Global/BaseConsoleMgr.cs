//**********************************************
// Class Name	: BaseDevConsoleMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

//using SickDev.DevConsole;
using UnityEngine;

namespace CYM
{
    public class BaseConsoleMgr : BaseGFlowMgr
    {
        static bool IsStarted = false;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
        }
        public override void OnStart()
        {
            base.OnStart();
            //DevConsole.IsEnableCommand = IsGMMode;
            IsStarted = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (BaseGlobal.DiffMgr.IsGMMode() ||
                Application.isEditor)
            {
                if(BaseInputMgr.IsCanInput())
                    OnGMUpdate();
            }
        }

        public void Toggle()
        {
            //if (DevConsole.singleton != null)
            //{
            //    if (DevConsole.singleton.isOpen)
            //        DevConsole.singleton.Close();
            //    else
            //    {
            //        DevConsole.singleton.Open();
            //    }
            //}
        }
        public override void Enable(bool b)
        {
            base.Enable(b);
            //if (DevConsole.singleton != null)
            //    DevConsole.singleton.enabled = b;
        }

        protected virtual void OnGMUpdate()
        {

        }
        #region is

        public static bool IsShow
        {
            get
            {
                //if (!IsStarted)
                //    return false;
                //if (DevConsole.singleton != null) 
                //    return DevConsole.singleton.isOpen;
                return false;
            }
        }
        protected bool IsGMMode => BaseGlobal.DiffMgr.IsGMMode();
        #endregion
    }
}
