//------------------------------------------------------------------------------
// InspectorBaseGlobalMonoMgr.cs
// Copyright 2018 2018/6/1 
// Created by CYM on 2018/6/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

namespace CYM
{
    [CustomEditor(typeof(BaseGlobal), true)]
    public class InspectorBaseGlobal : OdinEditor
    {
        BaseGlobal BaseGlobal;

        protected override void OnEnable()
        {
            base.OnEnable();
            BaseGlobal = (BaseGlobal)target;
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            if (BaseGlobal == null) return;
            //UpdateUI("Normal", BaseGlobalMonoMgr.Normal);
            UpdateUI("Unit", GlobalMonoMgr.Unit);
            //UpdateUI("Global", BaseGlobalMonoMgr.Global);
            UpdateUI("View", GlobalMonoMgr.View);
        }

        void UpdateUI(string tite, MonoUpdateData updateData)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(tite);
            EditorGUILayout.LabelField("Update Count:", updateData.UpdateIns.Count.ToString());
            EditorGUILayout.LabelField("FixedUpdate Count:", updateData.FixedUpdateIns.Count.ToString());
            EditorGUILayout.LabelField("LateUpdate Count:", updateData.LateUpdateIns.Count.ToString());
            EditorGUILayout.LabelField("GUI Count:", updateData.GUIIns.Count.ToString());
            EditorGUILayout.EndVertical();
        }
    }
}