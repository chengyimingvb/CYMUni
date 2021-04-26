//------------------------------------------------------------------------------
// BaseEventMgr.cs
// Copyright 2019 2019/8/5 
// Created by CYM on 2019/8/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseEventMgr<TData> : BaseMgr, IDBListConverMgr<DBBaseEvent> where TData : TDBaseEventData, new()
    {
        #region Callback
        public event Callback<TData> Callback_OnEventAdded;
        public event Callback<TData> Callback_OnEventRemoved;
        public event Callback<TData> Callback_OnEventChange;
        #endregion

        #region prop
        BaseConditionMgr ACMgr => BaseGlobal.ACM;
        public IDDicList<TData> Data { get; private set; } = new IDDicList<TData>();
        public Dictionary<string, CD> EventCD { get; protected set; } = new Dictionary<string, CD>();
        static int MaxRandCount = 5;
        int CurRandCount = 0;
        ITDLuaMgr TDLuaMgr;
        List<string> clearEventss = new List<string>();
        protected virtual float GlobalProp => 1.0f;
        #endregion

        #region life
        public override MgrType MgrType => MgrType.Unit;
        public TData CurData { get; set; }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGameLogicTurn = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(typeof(TData));
        }
        public override void OnGameLogicTurn()
        {
            base.OnGameLogicTurn();
            Rand();
            clearEventss.Clear();
            foreach (var item in EventCD)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    clearEventss.Add(item.Key);
            }
            foreach (var item in clearEventss)
                EventCD.Remove(item);
        }
        #endregion

        #region get
        public TData Get(int id)
        {
            if (!Data.ContainsID(id))
                return null;
            return Data[id];
        }
        public TData First()
        {
            if (Data.Count > 0)
                return Data[0];
            return null;
        }
        #endregion

        #region set
        int CurEventIndex = 0;
        public TData TestNext()
        {
            if (TDLuaMgr.Keys.Count == 0) return null;
            if (CurEventIndex >= TDLuaMgr.Keys.Count) return null;
            string key = TDLuaMgr.Keys[CurEventIndex];
            CurEventIndex++;
            return Add(key);
        }
        public TData Rand()
        {
            if (!RandUtil.Rand(GlobalProp))
                return null;
            if (CurRandCount >= MaxRandCount)
            {
                CurRandCount = 0;
                return null;
            }
            CurRandCount++;
            if (Options.IsNoEvent) return null;
            if (!SelfBaseUnit.IsPlayerCtrl()) return null;
            if (TDLuaMgr.Keys.Count == 0) return null;

            string key = TDLuaMgr.Keys.Rand();
            TData config = TDLuaMgr.Get<TData>(key);
            //判断事件的触发条件
            if (IsInTarget(config))
            {
                //判断事件的概率
                if (IsInProp(config))
                {
                    CurRandCount = 0;
                    if (config.CD > 0)
                    {
                        if (!EventCD.ContainsKey(key))
                            EventCD.Add(key, new CD());
                        EventCD[key] = new CD(config.CD);
                    }
                    return Add(config.TDID);
                }
                else
                {
                    return null;
                }
            }
            return Rand();
        }
        public TData Add(string eventDlgName)
        {
            if (!TDLuaMgr.Contains(eventDlgName)) return null;
            TData tempEventDlg = TDLuaMgr.Get<TData>(eventDlgName).Copy<TData>();
            if (tempEventDlg == null)
            {
                CLog.Error("未找到EventDlg errorId=" + eventDlgName);
                return null;
            }
            tempEventDlg.ID = CYM.IDUtil.Gen();
            tempEventDlg.OnBeAdded(SelfBaseUnit);
            Data.Add(tempEventDlg);
            Callback_OnEventAdded?.Invoke(tempEventDlg);
            Callback_OnEventChange?.Invoke(tempEventDlg);
            return tempEventDlg;
        }
        public void Remove(TData eventDlg)
        {
            if (eventDlg == null) return;
            Data.Remove(eventDlg);
            Callback_OnEventRemoved?.Invoke(eventDlg);
            Callback_OnEventChange?.Invoke(eventDlg);
        }
        public void SelOption(TData eventData, BaseEventOption option)
        {
            SelOption(eventData, option.Index);
        }
        public void SelOption(TData eventData, int index)
        {
            eventData.DoSelOption(index);
            Remove(eventData);
        }
        #endregion

        #region is
        public bool IsHaveEvent() => Data.Count > 0;
        // 是否可以触发
        bool IsInProp(TDBaseEventData eventData)
        {
            if (Options.IsMustEvent) return true;
            if (RandUtil.Rand(eventData.Prob)) return true;
            return false;
        }
        bool IsInTarget(TDBaseEventData eventData)
        {
            if (eventData.Targets == null) return false;
            if (EventCD.ContainsKey(eventData.TDID))
            {
                if (!EventCD[eventData.TDID].IsOver())
                    return false;
            }

            if (eventData.Targets.Count > 0)
            {
                ACMgr.Reset(SelfBaseUnit);
                ACMgr.Add(eventData.Targets);
                if (!ACMgr.IsTrue())
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region DB utile
        public List<DBBaseEvent> GetDBData()
        {
            List<DBBaseEvent> ret = new List<DBBaseEvent>();
            foreach (var item in Data)
            {
                DBBaseEvent temp = new DBBaseEvent();
                temp.ID = item.ID;
                temp.TDID = item.TDID;
                temp.CD = item.CD;
                ret.Add(temp);
            }
            return ret;
        }
        public void LoadDBData(List<DBBaseEvent> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                var temp = Add(item.TDID);
                temp.TDID = item.TDID;
                temp.ID = item.ID;
                temp.CD = item.CD;
            }
        }
        public Dictionary<string, CD> GetEventCDDBData()
        {
            return EventCD;
        }
        public void LoadEventCDDBData(Dictionary<string, CD> eventCD)
        {
            EventCD = eventCD;
        }
        #endregion
    }
}