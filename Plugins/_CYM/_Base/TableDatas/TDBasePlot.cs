
using System;
using System.Collections.Generic;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public enum PlotModeType : int
    {
        Normal = 0,//剧情模式
        Manual = 1,//玩家手动暂停
    }
    [Serializable]
    public class TDBasePlotData : TDBaseData
    {
        #region prop
        protected int CurPlotIndex => BaseGlobal.PlotMgr.CurPlotIndex;
        protected IBasePlotMgr PlotMgr => BaseGlobal.PlotMgr;
        Coroutineter BattleCoroutineter => BaseGlobal.BattleCoroutineter;
        Timer updateTimer = new Timer(1.0f);
        #endregion

        #region set
        protected void StartPlot(string tdid)
        {
            PlotMgr.Start(tdid);
        }
        protected int PushPlotIndex(int? index=null)
        {
            return PlotMgr.PushIndex(index);
        }
        protected CoroutineHandle Run(IEnumerator<float> coroutine)
        {
            return BattleCoroutineter.Run(coroutine);
        }
        #endregion

        #region life
        //自动开始剧情流程
        public virtual bool AutoStarPlot => true;
        //设置剧情暂停标志
        protected virtual bool PlotPause => true;
        //自定义剧情更新频率
        protected virtual float UpdateTime => 1.0f;
        public virtual string CheckForNext() => Const.STR_Inv;
        public override void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            base.OnBeAdded(selfMono, obj);
            if (PlotMgr != null && PlotPause)
            {
                PlotMgr.SetPlotPause(true);
            }
            updateTimer = new Timer(UpdateTime);
        }
        public override void OnBeRemoved()
        {
            if (PlotMgr != null && PlotPause)
            {
                PlotMgr.SetPlotPause(false);
            }
            base.OnBeRemoved();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (updateTimer.CheckOver())
            {
                UpdateTimer();
            }
        }
        protected virtual void UpdateTimer()
        { 
        
        }
        public virtual IEnumerator<float> OnPlotStart()
        {
            yield break;
        }
        public virtual IEnumerator<float> OnPlotRun()
        {
            yield break;
        }
        public virtual void OnPlotEnd()
        {

        }
        public virtual IEnumerator<float> CustomStartBattleFlow()
        {
            yield break;
        }
        protected void RunMain()
        {
            PlotMgr?.RunMain();
        }
        #endregion
    }
}