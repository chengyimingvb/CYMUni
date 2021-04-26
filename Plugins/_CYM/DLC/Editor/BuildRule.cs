//------------------------------------------------------------------------------
// BuildRule.cs
// Copyright 2019 2019/6/5 
// Created by CYM on 2019/6/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace CYM.DLC
{
    public partial class Builder
    {
        #region build rule
        public abstract class BuildRule
        {
            protected DLCItem DLCItem;
            #region utile
            public BuildRuleConfig Config { get;private set; }
            public BuildRule(DLCItem dlc, BuildRuleConfig config)
            {
                DLCItem = dlc;
                Config = config;
            }
            public abstract void Build();
            #endregion
        }
        class BuildAssetsWithDirectroy : BuildRule
        {
            public BuildAssetsWithDirectroy(DLCItem dlc, BuildRuleConfig config) : base(dlc, config)
            {
            }
            public override void Build()
            {
                List<string> packedAsset = new List<string>();
                var files = GetFilesWithoutDirectories(Config.FullSearchPath);
                Tuple<string, string> bundleName = GetABNameDirectory(DLCItem, Config);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    if (!IsContainInPackedAssets(item))
                    {
                        packedAsset.Add(item);
                        AddToPackedAssets(item);
                    }
                    AddToAllAssets(item, bundleName);
                    AddToAllBundles(bundleName);
                }

                if (packedAsset.Count == 0)
                    return;
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = bundleName.Item1;
                build.assetNames = packedAsset.ToArray();
                Builds.Add(build);
            }
        }
        class BuildAssetsWithPath : BuildRule
        {
            public BuildAssetsWithPath(DLCItem dlc, BuildRuleConfig config) : base(dlc, config)
            {
            }
            public override void Build()
            {
                var files = GetFilesWithoutDirectories(Config.FullSearchPath);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    Tuple<string, string> bundleName = GetABNamePath(DLCItem, item);
                    if (!IsContainInPackedAssets(item))
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundleName.Item1;
                        build.assetNames = new string[] { item };
                        Builds.Add(build);
                        AddToPackedAssets(item);
                    }
                    AddToAllAssets(item, bundleName);
                    AddToAllBundles(bundleName);
                }
            }
        }
        class BuildAssetsWithFile : BuildRule
        {
            public BuildAssetsWithFile(DLCItem dlc, BuildRuleConfig config) : base(dlc, config)
            {
            }
            public override void Build()
            {
                var files = GetFilesWithoutDirectories(Config.FullSearchPath);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    Tuple<string, string> bundleName = GetABNameFile(DLCItem, Config, item);
                    if (!IsContainInPackedAssets(item))
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundleName.Item1;
                        build.assetNames = new string[] { item };
                        Builds.Add(build);
                        AddToPackedAssets(item);
                    }
                    AddToAllAssets(item, bundleName);
                    AddToAllBundles(bundleName);
                }
            }
        }
        #endregion
    }
}