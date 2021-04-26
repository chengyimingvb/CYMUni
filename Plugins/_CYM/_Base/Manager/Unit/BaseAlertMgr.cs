//------------------------------------------------------------------------------
// BaseAlertMgr.cs
// Copyright 2019 2019/3/1 
// Created by CYM on 2019/3/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace CYM
{
    public enum AlertType
    {
        Continue,         //持续
        Interaction,      //交互
        Disposable,       //一次性
    }
    public class BaseAlertMgr<TData> : BaseMgr, IBaseAlertMgr, IDBListConverMgr<DBBaseAlert> 
        where TData : TDBaseAlertData, new()
    {
        #region Callback
        public event Callback<TData> Callback_OnAdded;
        public event Callback<TData> Callback_OnRemoved;
        public event Callback<TData> Callback_OnMerge;
        public event Callback<TData> Callback_OnCommingTimeOut;
        public event Callback<TData> Callback_OnInteractionChange;
        public event Callback<TData> Callback_DisposableChange;
        public event Callback<TData> Callback_ContinueChange;
        #endregion

        #region prop
        IBaseArticleMgr ArticleMgr => BaseGlobal.ArticleMgr;
        IBaseRelationMgr RelationMgr => BaseGlobal.RelationMgr;
        public List<TData> Data { get; private set; } = new List<TData>();
        public List<TData> InteractionData { get; private set; } = new List<TData>();
        public List<TData> DisposableData { get; private set; } = new List<TData>();
        public IDDicList<TData> ContinueData { get; private set; } = new IDDicList<TData>();
        public List<TData> TDContinueData { get; private set; } = new List<TData>();
        protected BaseUnit LocalPlayer => BaseGlobal.ScreenMgr.BaseLocalPlayer;
        protected List<TData> ClearData = new List<TData>();
        protected virtual string CommonAlert => "Alert_Common";
        protected bool IsDirtyUpdateAlert = false;
        Timer UpdateAlertTimer = new Timer(1.0f);
        ITDLuaMgr TDLuaMgr;
        #endregion

        #region life
        public override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGameLogicTurn = true;
            NeedFixedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            TDLuaMgr = BaseLuaMgr.GetTDLuaMgr(typeof(TData));
            foreach (var item in TDLuaMgr.ObjValues)
            {
                var data = item as TData;
                if (data.Type == AlertType.Continue)
                    TDContinueData.Add(data);
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            SelfBaseUnit.Callback_OnBeSetPlayer += OnBeSetPlayer;
            SelfBaseUnit.Callback_OnUnBeSetPlayer += OnUnBeSetPlayer;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            SelfBaseUnit.Callback_OnBeSetPlayer -= OnBeSetPlayer;
            SelfBaseUnit.Callback_OnUnBeSetPlayer -= OnUnBeSetPlayer;
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (SelfBaseUnit.IsPlayer())
            {
                if (IsDirtyUpdateAlert || UpdateAlertTimer.CheckOver())
                {
                    OnUpdateContinueAlert();
                    IsDirtyUpdateAlert = false;
                }
            }
        }
        public override void OnGameLogicTurn()
        {
            base.OnGameLogicTurn();
            if (SelfBaseUnit.IsPlayer())
                SetAlertDirty();

            foreach (var item in Data)
            {
                item.GameLogicTurn();
                if (!item.IsValid())
                {
                    ClearData.Add(item);
                }
                if (item.IsCommingTimeOut())
                {
                    item.OnCommingTimeOut();
                    Callback_OnCommingTimeOut?.Invoke(item);
                }
            }
            for (int i = 0; i < ClearData.Count; ++i)
            {
                ClearData[i].OnTimeOut();
                Remove(ClearData[i]);
            }
            ClearData.Clear();
        }
        protected virtual void OnUpdateContinueAlert()
        {
            foreach (var item in TDContinueData)
            {
                SetContinueAlert(item.TDID, item.IsActiveContinue);
            }
        }
        public void SetAlertDirty()
        {
            IsDirtyUpdateAlert = true;
        }
        #endregion

        #region add common
        public TData AddAutoTrigger(Callback<TData> action = null)
        {
            return AddCommon(action, true);
        }
        public TData AddAutoTrigger(string baseKey, string illustration, string sfx, params object[] descPS)
        {
            return AddAutoTrigger((x) => x.SetAuto(baseKey, illustration, sfx, descPS));
        }
        public TData AddCommon(string baseKey, string illustration, string sfx, params object[] descPS)
        {
            return AddCommon((x) => x.SetAuto(baseKey, illustration, sfx, descPS),false);
        }
        public TData AddCommon(Callback<TData> action = null, bool isAutoTrigger = false)
        {
            return AddCommon(action,null, isAutoTrigger);
        }
        public TData AddCommon(Callback<TData> action = null, BaseUnit cast = null, bool isAutoTrigger = false)
        {
            if (!IsCanAddDisposableAlert()) return null;
            return Add(CommonAlert,cast,action,isAutoTrigger);
        }
        #endregion

        #region Add other
        public TData AddInteraction(string tdid, BaseUnit cast = null, Callback<TData> action = null)
        {
            return Add(tdid, cast, action);
        }
        public TData AddDisposable(string tdid, BaseUnit cast = null)
        {
            if (!IsCanAddDisposableAlert()) return null;
            return Add(tdid, cast);
        }
        #endregion

        #region set
        public void TriggerFirstDisposable()
        {
            var data = GetDisposable();
            data.DoLeftClickTrigger();
        }
        protected void SetContinueAlert(string tdid, Func<bool> isTrigger)
        {
            if (isTrigger == null) return;
            if (isTrigger.Invoke())
            {
                if (!ContinueData.ContainsTDID(tdid))
                    Add(tdid);
            }
            else Remove(tdid);
        }
        private TData Add(string tdid, BaseUnit cast = null, Callback<TData> action = null, bool isAutoTrigger = false)
        {
            if (!TDLuaMgr.Contains(tdid))
            {
                if (CommonAlert == tdid)
                    CLog.Error("没有:{0},请手动添加CommonAlert", tdid);
                else
                    CLog.Error("没有:{0},请手动添加Alert", tdid);
                return null;
            }

            TData sourceAlert = TDLuaMgr.Get<TData>(tdid);
            sourceAlert.Cast = cast ? cast : LocalPlayer;
            sourceAlert.AlertMgr = this;

            //判断通知是否可以被合并
            var finalAlert = CanMerge(sourceAlert);
            if (finalAlert != null)
            {
                finalAlert.OnMerge();
                Callback_OnMerge?.Invoke(finalAlert);
            }
            else
            {
                finalAlert = sourceAlert.Copy<TData>();
                finalAlert.ID = IDUtil.Gen();
                finalAlert.OnBeAdded(SelfBaseUnit);
                Data.Add(finalAlert);
                if (finalAlert.Type == AlertType.Interaction)
                {
                    //推送最近一次的谈判信息
                    if (ArticleMgr.IsStarNegotiation)
                    {
                        ArticleMgr.PushNagotiationToAlert(finalAlert);
                    }
                    InteractionData.Add(finalAlert);
                    Callback_OnInteractionChange?.Invoke(finalAlert);
                }
                else if (finalAlert.Type == AlertType.Disposable)
                {
                    DisposableData.Add(finalAlert);
                    Callback_DisposableChange?.Invoke(finalAlert);
                }
                else if (finalAlert.Type == AlertType.Continue)
                {
                    ContinueData.Add(finalAlert);
                    Callback_ContinueChange?.Invoke(finalAlert);
                }
                action?.Invoke(finalAlert);
                if (BaseGlobal.IsUnReadData)
                    Callback_OnAdded?.Invoke(finalAlert);
            }
            if (finalAlert.IsAutoTrigger || isAutoTrigger)
            {
                finalAlert.DoLeftClickTrigger();
            }
            return finalAlert;
        }
        public void Remove(TData alert)
        {
            if (alert == null) return;
            alert.OnBeRemoved();
            Data.Remove(alert);
            if (alert.Type == AlertType.Interaction)
            {
                InteractionData.Remove(alert);
                Callback_OnInteractionChange?.Invoke(alert);
            }
            else if (alert.Type == AlertType.Disposable)
            {
                DisposableData.Remove(alert);
                Callback_DisposableChange?.Invoke(alert);
            }
            else if (alert.Type == AlertType.Continue)
            {
                ContinueData.Remove(alert);
                Callback_ContinueChange?.Invoke(alert);
            }
            //移除Article
            foreach (var item in alert.SelfArticle)
                ArticleMgr.RemoveArticle(item);
            foreach (var item in alert.TargetArticle)
                ArticleMgr.RemoveArticle(item);
            if (BaseGlobal.IsUnReadData)
                Callback_OnRemoved?.Invoke(alert);
        }
        public void Remove(long id) => Remove(Data.Find((x) => { return id == x.ID; }));
        public void Remove(string tdid) => Remove(Data.Find((x) => { return tdid == x.TDID; }));
        public TData GetAlert(long id) => Data.Find((x) => { return id == x.ID; });
        #endregion

        #region get
        public TData GetInteraction()
        {
            if (!IsHaveInteraction()) return null;
            return InteractionData[0];
        }
        public TData GetDisposable()
        {
            if (!IsHaveDisposable()) return null;
            return DisposableData[0];
        }
        #endregion

        #region is
        private bool IsCanAddDisposableAlert()
        {
            if (SelfBaseUnit == null) return false;
            if (Options.IsAllAlert) return true;
            if (!SelfBaseUnit.IsPlayer()) return false;
            return true;
        }
        public bool IsHaveInteraction()
        {
            return InteractionData.Count > 0;
        }
        public bool IsHaveDisposable()
        {
            return DisposableData.Count > 0;
        }

        #endregion

        #region Cache
        // 是否可以被合并，相同的Alert将会被合并
        private TData CanMerge(TData alert)
        {
            for (int i = 0; i < Data.Count; ++i)
            {
                var item = Data[i];
                //普通的通知id相同就合并
                if (alert.Type == AlertType.Continue &&
                    item.Type == AlertType.Continue)
                {
                    if (alert.TDID == item.TDID)
                        return item;
                }
                //外交alert要国家相同才行
                else if (alert.Type == AlertType.Interaction &&
                    item.Type == AlertType.Interaction)
                {
                    if (alert.TDID == item.TDID &&
                        alert.Cast == item.Cast)
                        return item;
                }
                //回信Alert不做合并
                else if (alert.Type == AlertType.Disposable &&
                    item.Type == AlertType.Disposable)
                {
                    return null;
                }
            }
            return null;
        }
        #endregion

        #region Callback
        protected virtual void OnBeSetPlayer()
        {
            SetAlertDirty();
            BaseUIMgr.Callback_OnControlClick += OnControlClick;
        }
        private void OnUnBeSetPlayer()
        {
            BaseUIMgr.Callback_OnControlClick -= OnControlClick;
        }

        private void OnControlClick(UControl arg1, PointerEventData arg2)
        {
            SetAlertDirty();
        }
        #endregion

        #region DB utile
        public List<DBBaseAlert> GetDBData()
        {
            List<DBBaseAlert> ret = new List<DBBaseAlert>();
            foreach (var item in Data)
            {
                if (item.Type == AlertType.Continue)
                    continue;
                DBBaseAlert temp = new DBBaseAlert();
                temp.ID = item.ID;
                temp.TDID = item.TDID;
                temp.Cast = item.Cast.ID;
                temp.CurTurn = item.CurTurn;
                temp.IsCommingTimeOutFalg = item.IsCommingTimeOutFalg;
                temp.TipStr = item.TipStr;
                temp.DetailStr = item.DetailStr;
                temp.TitleStr = item.TitleStr;
                temp.Illustration = item.Illustration;
                temp.Type = item.Type;
                temp.StartSFX = item.StartSFX;
                temp.Bg = item.Bg;
                temp.Icon = item.Icon;
                temp.IsAutoTrigger = item.IsAutoTrigger;
                if (item.WarfareData != null) temp.War = item.WarfareData.ID;
                if (item.SelfArticle != null) item.SelfArticle.ForEach(x => temp.SelfArticle.Add(x.ID));
                if (item.TargetArticle != null) item.TargetArticle.ForEach(x => temp.TargetArticle.Add(x.ID));
                ret.Add(temp);
            }
            return ret;
        }
        public void LoadDBData(List<DBBaseAlert> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                if (item.Type == AlertType.Continue)
                    continue;
                var alert = Add(item.TDID, GetEntity(item.Cast));
                alert.TipStr = item.TipStr;
                alert.DetailStr = item.DetailStr;
                alert.TitleStr = item.TitleStr;
                alert.Illustration = item.Illustration;
                alert.CurTurn = item.CurTurn;
                alert.ID = item.ID;
                alert.IsCommingTimeOutFalg = item.IsCommingTimeOutFalg;
                alert.StartSFX = item.StartSFX;
                alert.IsAutoTrigger = item.IsAutoTrigger;
                alert.Bg = item.Bg;
                alert.Icon = item.Icon;
                alert.WarfareData = RelationMgr.GetWarfareData<IBaseWarfareData>(item.War);
                item.SelfArticle.ForEach(x => alert.SelfArticle.Add(ArticleMgr.Get<TDBaseArticleData>(x)));
                item.TargetArticle.ForEach(x => alert.TargetArticle.Add(ArticleMgr.Get<TDBaseArticleData>(x)));
            }
        }
        #endregion
    }
}