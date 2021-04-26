//------------------------------------------------------------------------------
// UIConfig.cs
// Copyright 2018 2018/11/29 
// Created by CYM on 2018/11/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM.UI;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Reflection;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CYM
{

    public enum FontType
    {
        None=-1,
        Normal=0,
        Title=1,
    }

    public enum LogoType
    {
        Image,
        Video,
    }

    [Serializable]
    public class LogoData
    {
        public LogoType Type = LogoType.Image;
        [HideIf("IsVideo")]
        public Sprite Logo;
        [HideIf("IsImage")]
        public VideoClip Video;
        [HideIf("IsVideo")]
        public float WaitTime = 1.0f;
        [HideIf("IsVideo")]
        public float InTime = 0.5f;
        [HideIf("IsVideo")]
        public float OutTime = 0.5f;

        public bool IsImage()
        {
            return Type == LogoType.Image;
        }
        public bool IsVideo()
        {
            return Type == LogoType.Video;
        }
    }
    [Serializable]
    public class PresenterStateColor
    {
        public Color Normal = Color.white;
        public Color Enter = Color.grey;
        public Color Press = Color.white * 0.8f;
        public Color Disable = Color.grey;
        public Color Selected = Color.white;
    }

    public sealed class UIConfig : ScriptableObjectConfig<UIConfig>
    {
        #region prop
        public bool IsEditorMode()
        {
            if (Application.isEditor)
                return IsShowLogo;
            return true;
        }
        #endregion

        #region inspector
        [FoldoutGroup("Fonts"),SerializeField]
        public bool EnableSharpText = true;
        [FoldoutGroup("Fonts"), SerializeField]
        public Font DefaultFont;
        [FoldoutGroup("Fonts"), SerializeField]
        public Font DefaultNormalFont;
        [FoldoutGroup("Fonts"), SerializeField]
        public Font DefaultTitleFont;
        [FoldoutGroup("Fonts"), SerializeField]
        public Dictionary<LanguageType, Font> OverrideNormalFonts = new Dictionary<LanguageType, Font>();
        [FoldoutGroup("Fonts"), SerializeField]
        public Dictionary<LanguageType, Font> OverrideTitleFonts = new Dictionary<LanguageType, Font>();

        [FoldoutGroup("Logo"), SerializeField]
        public bool IsShowLogo;
        [FoldoutGroup("Logo"), SerializeField]
        public Texture2D StartLogo;
        [FoldoutGroup("Logo"), SerializeField]
        public List<LogoData> Logos = new List<LogoData>();

        [FoldoutGroup("Progress"), SerializeField]
        public float ProgressWidth = 100;
        [FoldoutGroup("Progress"), SerializeField]
        public float ProgressHeight = 5;
        [FoldoutGroup("Progress"), SerializeField]
        public Texture2D ProgressBG;
        [FoldoutGroup("Progress"), SerializeField]
        public Texture2D ProgressFill;
        #endregion

        #region dyn
        [FoldoutGroup("Dyn"), SerializeField]
        public Dictionary<string, MethodInfo> DynStrFuncs = new Dictionary<string, MethodInfo>();
        [FoldoutGroup("Dyn"), ReadOnly, SerializeField]
        private List<string> DynStrName = new List<string>();
        [FoldoutGroup("Dyn"), ReadOnly, SerializeField]
        private List<MethodInfo> DynStrMethodInfo = new List<MethodInfo>();
        [FoldoutGroup("Dyn"), ReadOnly, SerializeField]
        public string MonoTypeName = "";
#if UNITY_EDITOR
        [FoldoutGroup("Dyn"), SerializeField]
        public MonoScript DynamicFuncScript;
#endif

        [FoldoutGroup("Misc"), SerializeField]
        public List<SpriteConfig> SpriteGroupConfigs;
        [FoldoutGroup("Misc"), SerializeField]
        public Dictionary<string, PresenterStateColor> PresenterStateColors = new Dictionary<string, PresenterStateColor>();
        #endregion

        #region get
        public PresenterStateColor GetStateColor(string key)
        {
            if (PresenterStateColors.ContainsKey(key))
                return PresenterStateColors[key];
            CLog.Error("没有这个StateColor:{0}", key);
            return new PresenterStateColor();
        }
        public Font GetFont(FontType fonttype = FontType.Normal)
        {
            if (fonttype == FontType.None)
                return null;
            LanguageType type = BaseLanguageMgr.LanguageType;
            if (fonttype == FontType.Title)
            {
                if (OverrideTitleFonts.ContainsKey(type)) return OverrideTitleFonts[type];
                else return DefaultTitleFont;
            }
            else if (fonttype == FontType.Normal)
            {
                if (OverrideNormalFonts.ContainsKey(type)) return OverrideNormalFonts[type];
                else return DefaultNormalFont;
            }
            return DefaultFont;
        }
        #endregion

        #region life
        public override void OnInited()
        {
            base.OnInited();
            if (!PresenterStateColors.ContainsKey(Const.STR_Custom))
            {
                PresenterStateColors.Add(Const.STR_Custom, new PresenterStateColor());
            }
        }
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (DynamicFuncScript != null)
            {
                MonoTypeName = DynamicFuncScript.GetClass().FullName;
            }
#endif
            DynStrName.Clear();
            DynStrMethodInfo.Clear();
            var type = Type.GetType(MonoTypeName);
            if (type == null)
                return;
            var array = type.GetMethods();
            foreach (var item in array)
            {
                var attrArray = item.GetCustomAttributes(true);
                foreach (var attr in attrArray)
                {
                    if (attr is DynStr)
                    {
                        DynStrName.Add(item.Name);
                        DynStrMethodInfo.Add(item);
                    }
                }
            }
            DynStrFuncs.Clear();
            for (int i = 0; i < DynStrName.Count; ++i)
            {
                DynStrFuncs.Add(DynStrName[i], DynStrMethodInfo[i]);
            }
        }
        #endregion
    }
}