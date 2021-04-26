//------------------------------------------------------------------------------
// BuildToolbar.cs
// Copyright 2020 2020/7/7 
// Created by CYM on 2020/7/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEditor;

namespace CYM
{
    public static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold
            };
        }
    }
    [InitializeOnLoad]
    public static class BuildToolbar
    {
        static BuildToolbar()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }
        static void OnToolbarGUI()
        {
            //GUILayout.Space(70);
            //var tex1 = EditorGUIUtility.IconContent(@"UnityEditor.GameView").image;
            //if (GUILayout.Button(new GUIContent(null, tex1, "打开Build界面"), ToolbarStyles.commandButtonStyle))
            //{
            //    BuildWindow.ShowWindow(); 
            //}
            //var tex2 = EditorGUIUtility.IconContent(@"slider thumb").image;
            //if (GUILayout.Button(new GUIContent(null, tex2, "打开Option界面"), ToolbarStyles.commandButtonStyle))
            //{
            //    BuildWindow.ShowOptionWindow();
            //}
        }
    }
}