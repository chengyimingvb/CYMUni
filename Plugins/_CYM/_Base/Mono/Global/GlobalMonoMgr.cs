using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    // mono 的类型
    public enum MonoType
    {
        None = 0,
        Unit = 1,
        Global = 2,
        View = 4,
        Normal = 8,
    }

    public class MonoUpdateData
    {
        public List<BaseCoreMono> UpdateIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> FixedUpdateIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> LateUpdateIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> GUIIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> GizmosIns = new List<BaseCoreMono>();
        public List<BaseCoreMono> TurnLogicIns = new List<BaseCoreMono>();

        List<BaseCoreMono> update_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> update_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> fixedupdate_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> fixedupdate_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> lateupdate_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> lateupdate_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> gui_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> gui_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> gizmos_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> gizmos_removeList = new List<BaseCoreMono>();

        List<BaseCoreMono> turnLogic_addList = new List<BaseCoreMono>();
        List<BaseCoreMono> turnLogic_removeList = new List<BaseCoreMono>();

        bool IsPause(BaseCoreMono mono)
        {
            if (mono.IsEnable == false) return true;
            return (GlobalMonoMgr.PauseType & mono.MonoType) != 0;
        }

        void RemoveList(List<BaseCoreMono> ins, List<BaseCoreMono> list)
        {
            if (list.Count <= 0) return;
            foreach (var temp in list) ins.Remove(temp);
            list.Clear();
        }
        void AddList(List<BaseCoreMono> ins, List<BaseCoreMono> list)
        {
            if (list.Count <= 0) return;
            foreach (var temp in list) ins.Add(temp);
            list.Clear();
        }

        public void AddMono(BaseCoreMono mono)
        {
            if (mono.NeedUpdate) update_addList.Add(mono);
            if (mono.NeedLateUpdate) lateupdate_addList.Add(mono);
            if (mono.NeedGUI) gui_addList.Add(mono);
            if (mono.NeedFixedUpdate) fixedupdate_addList.Add(mono);
            if (mono.NeedGameLogicTurn) turnLogic_addList.Add(mono);
        }
        public void RemoveMono(BaseCoreMono mono)
        {
            if (mono.NeedUpdate) update_removeList.Add(mono);
            if (mono.NeedLateUpdate) lateupdate_removeList.Add(mono);
            if (mono.NeedGUI) gui_removeList.Add(mono);
            if (mono.NeedFixedUpdate) fixedupdate_removeList.Add(mono);
            if (mono.NeedGameLogicTurn) turnLogic_removeList.Add(mono);
        }
        public void RemoveAllNull()
        {
            UpdateIns.RemoveAll((p) => p == null);
            FixedUpdateIns.RemoveAll((p) => p == null);
            LateUpdateIns.RemoveAll((p) => p == null);
            GUIIns.RemoveAll((p) => p == null);
            GizmosIns.RemoveAll((p) => p == null);
            TurnLogicIns.RemoveAll(p => p == null);
        }

        public void LateUpdate()
        {
            AddList(UpdateIns, update_addList);
            RemoveList(UpdateIns, update_removeList);

            AddList(FixedUpdateIns, fixedupdate_addList);
            RemoveList(FixedUpdateIns, fixedupdate_removeList);

            AddList(LateUpdateIns, lateupdate_addList);
            RemoveList(LateUpdateIns, lateupdate_removeList);

            AddList(GUIIns, gui_addList);
            RemoveList(GUIIns, gui_removeList);

            AddList(GizmosIns, gizmos_addList);
            RemoveList(GizmosIns, gizmos_removeList);

            AddList(TurnLogicIns, turnLogic_addList);
            RemoveList(TurnLogicIns, turnLogic_removeList);

            foreach (var item in LateUpdateIns)
            {
                if (IsPause(item)) continue;
                item.OnLateUpdate();
            }
        }

        public void Update()
        {
            foreach (var item in UpdateIns)
            {
                if (IsPause(item)) continue;
                item.OnUpdate();
            }
        }

        public void FixedUpdate()
        {
            foreach (var item in FixedUpdateIns)
            {
                if (IsPause(item)) continue;
                item.OnFixedUpdate();
            }
        }

        public void OnGUI()
        {
            foreach (var item in GUIIns)
            {
                if (IsPause(item)) continue;
                item.OnGUIPaint();
            }
        }

        public void OnDestroy()
        {
            UpdateIns.Clear();
            FixedUpdateIns.Clear();
            LateUpdateIns.Clear();
            GUIIns.Clear();
            GizmosIns.Clear();
            TurnLogicIns.Clear();
        }
    }
    [HideMonoScript]
    public sealed class GlobalMonoMgr : MonoBehaviour
    {
        #region global mono
        Timer GCTimer = new Timer(1000);
        public static MonoType PauseType { get; private set; } = MonoType.None;

        public static MonoUpdateData Unit = new MonoUpdateData();
        public static MonoUpdateData Global = new MonoUpdateData();
        public static MonoUpdateData View = new MonoUpdateData();
        public static MonoUpdateData Normal = new MonoUpdateData();

        // 设置暂停类型
        public static void SetPauseType(MonoType type) => PauseType = type;
        public static void ActiveMono(BaseCoreMono mono) => mono.IsEnable = true;
        public static void DeactiveMono(BaseCoreMono mono) => mono.IsEnable = false;
        public static void AddMono(BaseCoreMono mono)
        {
            if (mono.MonoType == MonoType.Unit) Unit.AddMono(mono);
            else if (mono.MonoType == MonoType.Global) Global.AddMono(mono);
            else if (mono.MonoType == MonoType.View) View.AddMono(mono);
            else if (mono.MonoType == MonoType.Normal) Normal.AddMono(mono);
        }
        public static void RemoveMono(BaseCoreMono mono)
        {
            if (mono.MonoType == MonoType.Unit) Unit.RemoveMono(mono);
            else if (mono.MonoType == MonoType.Global) Global.RemoveMono(mono);
            else if (mono.MonoType == MonoType.View) View.RemoveMono(mono);
            else if (mono.MonoType == MonoType.Normal) Normal.RemoveMono(mono);
        }
        public static void RemoveAllNull()
        {
            Normal.RemoveAllNull();
            Unit.RemoveAllNull();
            Global.RemoveAllNull();
            View.RemoveAllNull();
        }
        void Update()
        {
            Normal.Update();
            Unit.Update();
            Global.Update();
            View.Update();

            if (GCTimer.CheckOver())
                GC.Collect();
        }

        private void FixedUpdate()
        {
            Normal.FixedUpdate();
            Unit.FixedUpdate();
            Global.FixedUpdate();
            View.FixedUpdate();
        }


        public void LateUpdate()
        {
            Normal.LateUpdate();
            Unit.LateUpdate();
            Global.LateUpdate();
            View.LateUpdate();
        }

        void OnGUI()
        {
            Normal.OnGUI();
            Unit.OnGUI();
            Global.OnGUI();
            View.OnGUI();
        }

        private void OnDestroy()
        {
            Normal.OnDestroy();
            Unit.OnDestroy();
            Global.OnDestroy();
            View.OnDestroy();
        }
        #endregion
    }
}