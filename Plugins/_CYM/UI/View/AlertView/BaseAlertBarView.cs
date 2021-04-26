//------------------------------------------------------------------------------
// BaseAlertBarView.cs
// Copyright 2019 2019/3/2 
// Created by CYM on 2019/3/2
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.Pool;
using CYM.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.UI
{
    public class BaseAlertBarView<TData> : UUIView where TData : TDBaseAlertData, new()
    {
        [SerializeField]
        bool IsInverseSort = false;
        [SerializeField]
        RectTransform AlertBar;
        [SerializeField]
        UDupplicate DP_AlertPoints;
        [SerializeField]
        GameObject Prefab;
        [SerializeField]
        RectTransform StartPos;

        #region prop
        HashList<BaseAlertItem> ActiveItems = new HashList<BaseAlertItem>();
        HashList<TDBaseAlertData> AlertDatasSet = new HashList<TDBaseAlertData>();
        GOPool GOPool;
        private CoroutineHandle delayCallTask;
        #endregion

        #region life
        private BaseAlertMgr<TData> AlertMgr { get; set; }
        protected virtual BaseAlertMgr<TData> NewAlertMgr => throw new NotImplementedException();
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            GOPool = new GOPool(Prefab, AlertBar);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateAlertPoint();
        }
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in ActiveItems)
            {
                item.Refresh();
            }
        }
        #endregion

        #region Alert
        void UpdateAlertPoint()
        {
            int IndexPoint = 0;
            for (int i = 0; i < ActiveItems.Count; ++i)
            {
                var item = ActiveItems[i];
                if (!item.IsShow)
                    continue;
                if (IndexPoint < DP_AlertPoints.GOCount)
                {
                    var lastIndex = IndexPoint;
                    if (IsInverseSort) lastIndex = DP_AlertPoints.GOCount - 1 - IndexPoint;
                    item.RectTrans.position = Vector3.LerpUnclamped(item.RectTrans.position, DP_AlertPoints.GOs[lastIndex].transform.position, 0.1f);
                    IndexPoint++;
                }
            }
        }
        void AddAlert(TDBaseAlertData alert)
        {
            if (AlertDatasSet.Contains(alert))
                return;
            GameObject go = GOPool.Spawn();
            BaseAlertItem alertItem = go.GetComponent<BaseAlertItem>();
            alertItem.PUIView = this;
            alertItem.CancleInit();
            alertItem.Init(new UButtonData
            {
                Icon = () => alert.GetIcon(),
                Bg = () => alert.GetBg(),
                OnClick = (x, y) => alert.DoClick(x, y),
                OnEnter = (x) => alert.DoEnter(),
                OnShowActive = OnAlertShow,
            });
            alertItem.SetID(alert.ID);
            alertItem.Show(true, true);
            alertItem.RectTrans.position = StartPos.position;
            ActiveItems.Add(alertItem);
            AlertDatasSet.Add(alert);
        }

        void RemoveAlert(TDBaseAlertData alert)
        {
            BaseAlertItem tempAlert = ActiveItems.Find((x) => { return x.ID == alert.ID; });
            if (tempAlert == null) return;
            tempAlert.Show(false);
            ActiveItems.Remove(tempAlert);
            AlertDatasSet.Remove(alert);
        }
        void ReCreateAlerts()
        {
            foreach (var alert in ActiveItems)
            {
                GOPool.Despawn(alert.gameObject);
            }
            ActiveItems.Clear();
            if (delayCallTask != null)
                BattleCoroutine.Kill(delayCallTask);
            delayCallTask = BattleCoroutine.Run(AddInitAlerts());

            IEnumerator<float> AddInitAlerts()
            {
                if (AlertMgr != null)
                {
                    for (int i = 0; i < AlertMgr.Data.Count; ++i)
                    {
                        if (i >= AlertMgr.Data.Count)
                            yield break;
                        yield return Timing.WaitForSeconds(0.1f);
                        AddAlert(AlertMgr.Data[i]);
                    }
                }
                yield break;
            }
        }
        #endregion

        #region Callback
        protected override void OnSetPlayerBase(BaseUnit oldPlayer, BaseUnit newPlayer)
        {
            base.OnSetPlayerBase(oldPlayer, newPlayer);
            if (AlertMgr != null)
            {
                AlertMgr.Callback_OnAdded -= OnAlertAdded;
                AlertMgr.Callback_OnRemoved -= OnAlertRemoved;
                AlertMgr.Callback_OnMerge -= OnAlertMerge;
                AlertMgr.Callback_OnCommingTimeOut -= OnAlertCommingTimeOut;
            }
            AlertMgr = NewAlertMgr;
            AlertMgr.Callback_OnAdded += OnAlertAdded;
            AlertMgr.Callback_OnRemoved += OnAlertRemoved;
            AlertMgr.Callback_OnMerge += OnAlertMerge;
            AlertMgr.Callback_OnCommingTimeOut += OnAlertCommingTimeOut;
            AlertDatasSet.Clear();
            ReCreateAlerts();
        }

        private void OnAlertCommingTimeOut(TDBaseAlertData arg1)
        {
        }

        private void OnAlertMerge(TDBaseAlertData arg1)
        {

        }

        private void OnAlertRemoved(TDBaseAlertData arg1)
        {
            RemoveAlert(arg1);
        }

        private void OnAlertAdded(TDBaseAlertData arg1)
        {
            AddAlert(arg1);
        }

        private void OnAlertShow(UControl p, bool arg1)
        {
            if (arg1 == false)
            {
                p.GO.transform.position = StartPos.position;
                GOPool.Despawn(p.GO);
            }
        }
        #endregion
    }
}