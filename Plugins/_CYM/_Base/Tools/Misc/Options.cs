//------------------------------------------------------------------------------
// Options.cs
// Copyright 2020 2020/7/14 
// Created by CYM on 2020/7/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
namespace CYM
{
    [Unobfus]
    public partial class Options:Singleton<Options>
    {
        [FoldoutGroup("Base")]
        public static bool IsHavePlot { get; set; } = true;
        [FoldoutGroup("Base")]
        public static bool IsIgnoreCondition { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsFastPersonDeath { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsNoEvent { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsMustEvent { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsAllAlert { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsOnlyPlayerAI { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsNoMilitaryAI { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsAutoEndTurn { get; set; } = false;
        [FoldoutGroup("Base")]
        public static bool IsLockCamera { get; set; } = false;
    }
}