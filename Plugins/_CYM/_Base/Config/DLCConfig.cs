//------------------------------------------------------------------------------
// AssetBundleConfig.cs
// Copyright 2018 2018/5/18 
// Created by CYM on 2018/5/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
namespace CYM.DLC
{
    public class DLCItemConfig
    {
        public string Name;
        public bool IsActive = true;
        public DLCItemConfig(string name)
        {
            Name = name;
        }
    }
    public sealed class DLCConfig : ScriptableObjectConfig<DLCConfig>
    {
        #region inspector
        //将所有的配置资源打成AssetBundle来读取，适合移动平台
        [SerializeField]
        public bool IsAssetBundleConfig = true;
        [SerializeField]
        public bool IsDiscreteShared = true; //是否为离散的共享包
        [SerializeField]
        public bool IsForceBuild = false;
        [SerializeField]
        public bool IsCompresse = true; //是否为压缩格式
        [SerializeField]
        public bool IsSimulationEditor = true;
        //是否初始化的时候加载所有的Directroy Bundle
        [SerializeField]
        public bool IsInitLoadDirBundle = true;
        //是否初始化的时候加载所有Shared Bundle
        [SerializeField]
        public bool IsInitLoadSharedBundle = true;
        [SerializeField]
        public bool IsWarmupAllShaders = true;
        #endregion

        #region private inspector
        [SerializeField]
        List<BuildRuleConfig> BuildRule = new List<BuildRuleConfig>();
        [SerializeField]
        List<string> IgnoreConst = new List<string>();
        [SerializeField]
        List<DLCItemConfig> DLC = new List<DLCItemConfig>();
        #endregion

        #region dlc config
        //内部DLC
        public DLCItemConfig ConfigInternal { get; private set; }
        //默认DLC
        public DLCItemConfig ConfigNative { get; private set; }
        //扩展DLC 不包含 Native
        public List<DLCItemConfig> ConfigExtend { get; private set; } = new List<DLCItemConfig>();
        public List<DLCItemConfig> ConfigAll { get; private set; } = new List<DLCItemConfig>();
        #endregion

        #region editor dlc item
        public DLCItem EditorInternal { get; private set; }
        public DLCItem EditorNative { get; private set; }
        public List<DLCItem> EditorExtend { get; private set; } = new List<DLCItem>();
        public List<DLCItem> EditorAll { get; private set; } = new List<DLCItem>();
        #endregion

        #region runtime
        public HashSet<string> AllDirectory { get; private set; } = new HashSet<string>();
        public List<BuildRuleConfig> Config { get; private set; } = new List<BuildRuleConfig>();
        public List<string> CopyDirectory { get; private set; } = new List<string>();
        public HashSet<string> IgnoreConstSet { get; private set; } = new HashSet<string>();
        #endregion

        #region is
        //是否为编辑器模式
        public bool IsEditorMode
        {
            get
            {
                if (!Application.isEditor) return false;
                if (Application.isEditor && IsSimulationEditor) return true;
                return false;
            }
        }
        //编辑器模式或者纯配置模式
        public bool IsEditorOrConfigMode=> IsEditorMode || !IsAssetBundleConfig;
        //编辑器模式或者AB配置模式
        public bool IsEditorOrAssetBundleMode=> IsEditorMode || IsAssetBundleConfig;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            RecreateDLC();
        }
        public override void OnInited()
        {
            base.OnInited();
            RefreshDLC();
        }
        public void RecreateDLC()
        {
            BuildRule.Clear();
            CopyDirectory.Clear();
            IgnoreConst.Clear();
            if (BuildRule.Count == 0)
            {
                //配置资源
                BuildRule.Add(new BuildRuleConfig ("Config",BuildRuleType.Directroy,true));
                BuildRule.Add(new BuildRuleConfig ("Lua", BuildRuleType.Directroy,true));
                BuildRule.Add(new BuildRuleConfig ("Language", BuildRuleType.Directroy,true));
                BuildRule.Add(new BuildRuleConfig ("Excel", BuildRuleType.Directroy,true));
                BuildRule.Add(new BuildRuleConfig ("Text", BuildRuleType.Directroy,true));
                BuildRule.Add(new BuildRuleConfig ("CSharp", BuildRuleType.Directroy,true));
                //图片资源
                BuildRule.Add(new BuildRuleConfig("Sprite", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("BG", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Icon", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Head", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Flag", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Texture", BuildRuleType.Directroy));
                //其他资源
                BuildRule.Add(new BuildRuleConfig("Audio", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("AudioMixer", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Material", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Music", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("PhysicsMaterial", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Video", BuildRuleType.Directroy));
                //Prefab资源
                BuildRule.Add(new BuildRuleConfig("Animator", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Prefab", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("Perform", BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("System",  BuildRuleType.Directroy));
                BuildRule.Add(new BuildRuleConfig("UI", BuildRuleType.Directroy));
                //场景资源
                BuildRule.Add(new BuildRuleConfig("Scene", BuildRuleType.File));
            }
            if (IgnoreConst.Count == 0)
            {
                //忽略的Const
                IgnoreConst.Add("CONFIG_DLCItem");
                IgnoreConst.Add("CONFIG_DLCManifest");
            }
        }
        //刷新DLC
        public void RefreshDLC()
        {
            Config.Clear();
            AllDirectory.Clear();
            CopyDirectory.Clear();
            IgnoreConstSet.Clear();

            ConfigExtend.Clear();
            ConfigAll.Clear();

            EditorExtend.Clear();
            EditorAll.Clear();

            foreach (var item in BuildRule)
                AddBuildConfig(item);
            foreach (var item in IgnoreConst)
                AddIgnoreConst(item);

            //加载内部dlc
            ConfigInternal = new DLCItemConfig(Const.STR_InternalDLC);
            EditorInternal = new DLCItem(ConfigInternal);

            //加载原生dlc
            ConfigNative = new DLCItemConfig(Const.STR_NativeDLC);
            EditorNative = new DLCItem(ConfigNative);

            //加载其他额外DLC
            foreach (var item in DLC)
            {
                var dlcItem = new DLCItemConfig(item.Name);
                ConfigExtend.Add(dlcItem);
                EditorExtend.Add(new DLCItem(dlcItem));
            }

            ConfigAll = new List<DLCItemConfig>(ConfigExtend);
            ConfigAll.Add(ConfigInternal);
            ConfigAll.Add(ConfigNative);

            EditorAll = new List<DLCItem>(EditorExtend);
            EditorAll.Add(EditorInternal);
            EditorAll.Add(EditorNative);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                foreach (var item in AllDirectory)
                {
                    FileUtil.EnsureDirectory(Path.Combine(Const.Path_NativeDLC, item));
                }

                foreach (var item in DLC)
                {
                    foreach (var dic in AllDirectory)
                    {
                        FileUtil.EnsureDirectory(Path.Combine(Const.Path_Bundles, item.Name, dic));
                    }
                }
            }
#endif
        }

        void AddBuildConfig(BuildRuleConfig config)
        {
            BuildRuleConfig data = config.Clone() as BuildRuleConfig;
            Config.Add(data);
            if (data.IsCopyDirectory)
            {
                CopyDirectory.Add(data.Name);
            }
            if (!AllDirectory.Contains(data.Name))
                AllDirectory.Add(data.Name);
        }
        void AddIgnoreConst(string name)
        {
            if (!IgnoreConstSet.Contains(name))
                IgnoreConstSet.Add(name);
        }
        public bool IsInIgnoreConst(string name)
        {
            return IgnoreConstSet.Contains(name);
        }
        #endregion

    }
}