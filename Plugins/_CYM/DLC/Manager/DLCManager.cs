//------------------------------------------------------------------------------
// AssetBundleMono.cs
// Copyright 2018 2018/5/21 
// Created by CYM on 2018/5/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CYM.DLC
{
    public class DLCManager : MonoBehaviour
    {
        #region prop
        readonly internal static Dictionary<string,AssetBundle> RawBundles = new Dictionary<string, AssetBundle>();
        readonly internal static Dictionary<string, UnityEngine.Object> RawAssets = new Dictionary<string, UnityEngine.Object>();
        readonly internal static Dictionary<string, Bundle> Bundles = new Dictionary<string, Bundle>();
        readonly internal static Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();
        readonly internal static Dictionary<string, DLCItem> LoadedDLCItems = new Dictionary<string, DLCItem>();
        readonly internal static List<Asset> AsyncAssets = new List<Asset>();
        readonly internal static List<Asset> clearAsyncAssets = new List<Asset>();
        #endregion

        #region 映射
        /// <summary>
        /// 资源路径映射
        /// string:资源路径
        /// int:bundle的index ,这里使用Index可以节省资源
        /// </summary>
        static internal readonly Dictionary<string, int> amap = new Dictionary<string, int>();
        /// <summary>
        /// Bundle路径映射
        /// string:bundle的名字
        /// List<int>:资源的路径,这里用Index可以节省资源
        /// </summary>
        static internal readonly Dictionary<string, List<int>> bmap = new Dictionary<string, List<int>>();
        /// <summary>
        /// Bundle/AssetName 映射表
        /// string1:bundle 名称
        /// T:int:资源路径的index,这里使用Index可以节省资源
        /// T:string:DLC的名称
        /// </summary>
        static internal readonly Dictionary<string, Tuple<int, string>> bamap = new Dictionary<string, Tuple<int, string>>();
        /// <summary>
        /// 所有的资源数组,完整的路径
        /// </summary>
        static internal readonly List<string> allAssets = new List<string>();
        /// <summary>
        /// 所有的Bundle数组,bundle名称
        /// </summary>
        static internal readonly List<string> allBundles = new List<string>();
        /// <summary>
        /// 所有bundle+asset名称组成的id,用来检测资源是否纯在
        /// </summary>
        static internal readonly HashSet<string> allBundlesAssetsMap = new HashSet<string>();
        /// <summary>
        /// 所有共享的Bundle
        /// </summary>
        static internal readonly SafeDic<string,List<string>> allSharedBundles = new SafeDic<string, List<string>>();
        #endregion

        static DLCConfig DLCConfig => DLCConfig.Ins;

        #region life
        /// <summary>
        /// update
        /// </summary>
        void FixedUpdate()
        {
            if (AsyncAssets.Count > 0)
            {
                foreach (var item in AsyncAssets)
                {
                    item.OnUpdate();
                    if (item.IsDone)
                    {
                        clearAsyncAssets.Add(item);
                    }
                }
                if (clearAsyncAssets.Count > 0)
                {
                    AsyncAssets.RemoveAll((x) => x.IsDone);
                    clearAsyncAssets.Clear();
                }
            }
        }
        #endregion

        #region set
        /// <summary>
        /// For all dlc
        /// </summary>
        /// <param name="callback"></param>
        public static void ForAllDLC(Callback<DLCItem> callback)
        {
            foreach (var item in LoadedDLCItems.Values)
            {
                callback?.Invoke(item);
            }
        }
        /// <summary>
        /// 加载DLC
        /// </summary>
        /// <param name="config"></param>
        public static bool LoadDLC(DLCItemConfig config)
        {
            if (LoadedDLCItems.ContainsKey(config.Name))
            {
                CLog.Error("重复加载!!!!");
                return false;
            }

            //DLC名称
            string dlcName = config.Name;
            //AB清单
            AssetBundleManifest abManifest = LoadAssetBundleManifest(dlcName);
            //if (!DLCConfig.Ins.IsEditorMode)
            //{
                //加载Shared包
                //if(DLCConfig.IsInitLoadSharedBundle)
                //    LoadAllSharedBundle(dlcName);
            //}
            DLCItem item = new DLCItem(config, abManifest);
            //加载自定义Manifest            
            LoadManifest(item);
            LoadedDLCItems.Add(dlcName, item);         
            CLog.Info("[DLC] Load dlc:{0} succeed", dlcName);
            return true;

            void LoadManifest(DLCItem dlc)
            {
                DLCManifest reader = dlc.GetManifestFile();

                if (reader == null)
                {
                    CLog.Error("错误:reader 为 null");
                    return;
                }


                foreach (var bundleDataItem in reader.Data)
                {
                    string bundle = bundleDataItem.BundleName;
                    allBundles.Add(bundle);
                    if (!bmap.ContainsKey(bundle))
                        bmap.Add(bundle, new List<int>());
                    if (bundleDataItem.IsShared)
                    {
                        if (!allSharedBundles.ContainsKey(dlc.Name))
                            allSharedBundles.Add(dlc.Name, new List<string>());
                        allSharedBundles[dlc.Name].Add(bundle);
                    }

                    foreach (var assetPathData in bundleDataItem.AssetFullPaths)
                    {
                        //完整路径
                        string fullpath = assetPathData.FullPath;
                        //添加完整路径到资源表
                        allAssets.Add(fullpath);
                        //计算资源路径Index
                        int assetPathIndex = allAssets.Count - 1;
                        //计算资源bunleIndex
                        int bundleNameIndex = allBundles.Count - 1;
                        //添加完整资源路径的Index到映射表中
                        bmap[bundle].Add(assetPathIndex);
                        //添加完整的Bundle名称的Index到映射表中
                        amap[fullpath] = bundleNameIndex;
                        //如果资源有自己的Bundle设定,则添加到映射表中
                        if (!assetPathData.SourceBundleName.IsInv())
                        {
                            string bampKey = assetPathData.SourceBundleName + assetPathData.FileName;
                            bamap[bampKey] = new Tuple<int, string>(assetPathIndex, bundleDataItem.DLCName);
                            allBundlesAssetsMap.Add(bampKey);
                        }
                    }
                }
            }
        }
        public static AssetBundle LoadRawBundle(string name, DLCItem dlc)
        {
            string fullName = dlc.Name + name;
            if (RawBundles.ContainsKey(fullName))
                return RawBundles[fullName];
            string path = Path.Combine(Application.streamingAssetsPath, dlc.Name) + "/" + dlc.Name.ToLower() + "_" + name;
            AssetBundle bundle = null;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (File.Exists(path))
#endif
                bundle = AssetBundle.LoadFromFile(path);
            if (bundle != null)
            {
                RawBundles.Add(fullName, bundle);
            }
            return bundle;
        }
        public static T LoadRawAsset<T>(string bundleName,string assetName,DLCItem dlc) 
            where T : UnityEngine.Object
        {
            string fullAssetsName = bundleName + assetName + dlc.Name;
            if (RawAssets.ContainsKey(fullAssetsName))
                return RawAssets[fullAssetsName] as T;
            var bundle = LoadRawBundle(bundleName,dlc);
            if (bundle != null)
            {
                var obj = bundle.LoadAsset<T>(assetName);
                if(obj!=null)
                    RawAssets.Add(fullAssetsName,obj);
                return obj;
            }
            return default;
        }
        /// <summary>
        /// 加载Manifest
        /// </summary>
        /// <param name="reader"></param>

        #endregion

        #region is
        public static bool IsHave(string bundle, string asset)
        {
            return allBundlesAssetsMap.Contains(bundle + asset);
        }
        #endregion

        #region get
        /// <summary>
        /// 获得DLC
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DLCItem GetDLCItem(string name)
        {
            if (!LoadedDLCItems.ContainsKey(name))
            {
                throw new Exception("没有这个DLC:" + name);
            }
            //从外部已经加载进来的DLC中获取
            return LoadedDLCItems[name];
        }
        /// <summary>
        /// 根据资源路径获得Bundle名称
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetBundleName(string assetPath)
        {
            if (assetPath == null)
            {
                return "";
            }
            if (!amap.ContainsKey(assetPath))
            {
                Error("没有这个资源:{0}", assetPath);
            }
            return allBundles[amap[assetPath]];
        }
        /// <summary>
        /// 根据完整路径获得资源名称
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetAssetName(string assetPath)
        {
            return Path.GetFileName(assetPath);
        }
        /// <summary>
        /// 根据Bundle名称和资源名称获得其完整路径
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetAssetPath(string bundleName, string assetName)
        {
            string key = bundleName.ToLower() + assetName;
            if (!bamap.ContainsKey(key))
            {
                //Error("GetAssetPath没有这个资源,Bundle:{0},Asset:{1},Key:{2}", bundleName, assetName, key);
                return null;
            }
            return allAssets[bamap[key].Item1];
        }
        /// <summary>
        /// 获得DLC的名称
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetDLCName(string bundleName, string assetName)
        {
            string key = bundleName.ToLower() + assetName;
            if (!bamap.ContainsKey(key))
            {
                //Error("GetDLCName没有这个资源,Bundle:{0},Asset:{1}", bundleName, assetName);
                return null;
            }
            return bamap[key].Item2;
        }
        /// <summary>
        /// 获得Bundle/Asset映射值
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static Tuple<int, string> GetBAValue(string bundleName, string assetName)
        {
            string key = bundleName.ToLower() + assetName;
            if (!bamap.ContainsKey(key))
            {
                Error("没有这个资源,Bundle:{0},Asset:{1}", bundleName, assetName);
                return null;
            }
            return bamap[key];
        }
        #endregion

        #region scene
        public static Asset LoadScene(string bundleName)
        {
            return LoadAssetInternal("scene/" + bundleName, bundleName, typeof(Scene), AssetBundleLoadType.Scene);
        }
        public static void UnloadScene(Asset asset)
        {
            if (!asset.IsDone)
            {
                Error("错误!资源没有加载完毕不能被卸载");
                return;
            }
            asset.Release();
            asset.Unload(true);
            Assets.Remove(asset.Mapkey);
            AsyncAssets.Remove(asset);
            asset = null;
        }
        #endregion

        #region Asset
        /// <summary>
        /// 根据Bundle名称和资源名称加载资源
        /// 例子: Prefab,Chara_xxx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static Asset LoadAsset<T>(string bundleName, string assetName)
        {
            return LoadAssetInternal(bundleName, assetName, typeof(T), AssetBundleLoadType.Normal);
        }
        public static Asset LoadAssetAsync<T>(string bundleName, string assetName)
        {
            return LoadAssetInternal(bundleName, assetName, typeof(T), AssetBundleLoadType.Async);
        }
        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="asset"></param>
        public static void UnloadAsset(Asset asset,bool all)
        {
            if (!asset.IsDone)
            {
                Error("错误!资源没有加载完毕不能被卸载");
                return;
            }
            asset.Release();
            if (asset.References <= 0)
            {
                asset.Unload(all);
                Assets.Remove(asset.Mapkey);
                AsyncAssets.Remove(asset);
            }
        }
        static Asset LoadAssetInternal(string bundleName, string assetName, Type type, AssetBundleLoadType loadType)
        {
            if (bundleName == null || assetName == null) return null;
            string mapkey = bundleName + assetName;
            Asset asset = null;
            if (Assets.ContainsKey(mapkey))
            {
                if (asset is BundleAssetScene)
                {
                    CLog.Error("同一个场景资源不能重复加载,请先卸载资源!");
                    return null;
                }
                asset = Assets[mapkey];
            }
            else
            {
                if (loadType == AssetBundleLoadType.Normal)
                    asset = new BundleAsset(bundleName, assetName, type);
                else if (loadType == AssetBundleLoadType.Async)
                    asset = new BundleAssetAsync(bundleName, assetName, type);
                else if (loadType == AssetBundleLoadType.Scene)
                    asset = new BundleAssetScene(bundleName, assetName, type);
                asset.Load();
                Assets.Add(mapkey, asset);
            }
            asset.Retain();
            if (loadType == AssetBundleLoadType.Async ||
                loadType == AssetBundleLoadType.Scene)
            {
                AsyncAssets.Add(asset);
            }
            return asset;
        }
        #endregion

        #region Bundle
        public static void UnloadBundle(Bundle bundle, bool all)
        {
            if (bundle == null || !bundle.IsDone)
            {
                CLog.Error("错误!不能再资源没有加载完毕的时候卸载");
                return;
            }
            bundle.Release();
            if (bundle.References <= 0)
            {
                bundle.Unload(all);

                UnloadDependencies(bundle,all);

                Bundles.Remove(bundle.Name);
                bundle = null;
            }
        }
        static void UnloadDependencies(Bundle bundle, bool all)
        {
            foreach (var item in bundle.Dependencies)
            {
                item.Release();
                if (item.References <= 0)
                {
                    item.Unload(all);
                    UnloadDependencies(item,all);
                    Bundles.Remove(item.Name);
                }
            }
            bundle.Dependencies.Clear();
        }
        public static AssetBundleManifest LoadAssetBundleManifest(string dlc)
        {
            if (DLCConfig.Ins.IsEditorMode)
                return null;
            //加载Unity Assetbundle Manifest
            Bundle tempBundle = LoadBundle(dlc, dlc, false, false);
            if (tempBundle == null)
                return null;
            return tempBundle.LoadAsset<AssetBundleManifest>(Const.STR_ABManifest);
        }
        public static Bundle LoadBundle(string bundleName, string dlc, bool asyncRequest = false,bool isPreload=false)
        {
            if (bundleName.IsInv() || dlc.IsInv())
                return null;
            //如果bundleName == dlc 表示LoadingAssetBundleManifest
            bool isLoadABManifest = bundleName == dlc;
            var url = Path.Combine(Const.Path_StreamingAssets, dlc) + "/" + bundleName;
            Bundle bundle = null;
            if (Bundles.ContainsKey(bundleName))
            {
                bundle = Bundles[bundleName];
            }
            else
            {
                if (asyncRequest)
                    bundle = new BundleAsync(url);
                else
                    bundle = new Bundle(url);
                bundle.Name = bundleName;
                Bundles.Add(bundleName, bundle);
                bundle.Load();

                //加载依赖的的Bundle
                if (!isLoadABManifest &&
                    !isPreload)
                {
                    DLCItem dlcItem = GetDLCItem(dlc);
                    var dependencies = dlcItem.GetAllDependencies(bundleName);
                    if (dependencies != null && dependencies.Length > 0)
                    {
                        foreach (var item in dependencies)
                        {
                            var dependencieBundle = LoadBundle(item, dlc, asyncRequest);
                            //dependencieBundle.Parent = bundle;
                            //bundle.Dependencies.Add(dependencieBundle);

                            bundle.AddDependencies(dependencieBundle);
                        }
                    }
                }
            }
            bundle.Retain();
            return bundle;
        }
        #endregion

        #region load all
        public static List<Bundle> LoadAllDirBundle()
        {
            List<Bundle> ret = new List<Bundle>();
            if (LoadedDLCItems.Count == 0)
            {
                CLog.Error("没有加载完成DLC...");
                return ret;
            }

            foreach (var dlc in LoadedDLCItems)
            {
                foreach (var builData in dlc.Value.BuildRuleData)
                {
                    if (builData.BuildRuleType == BuildRuleType.Directroy)
                    {
                        IsNextLogError = false;
                        var bundle = LoadBundle(builData.Name.ToLower(), dlc.Key, true, true);
                        if (bundle != null)
                        {
                            ret.Add(bundle);
                        }
                    }
                }
            }
            return ret;
        }
        public static List<Bundle> LoadAllSharedBundle()
        {
            List<Bundle> ret = new List<Bundle>();
            if (LoadedDLCItems.Count == 0)
            {
                CLog.Error("没有加载完成DLC...");
                return ret;
            }
            foreach (var dlc in LoadedDLCItems.Keys)
            {
                var sharedBundle = allSharedBundles[dlc];
                if (sharedBundle == null)
                    continue;
                foreach (var item in sharedBundle)
                {
                    var bundle = LoadBundle(item, dlc, true, true);
                    if (bundle != null)
                    {
                        ret.Add(bundle);
                    }
                }
            }
            return ret;
        }
        #endregion

        #region utile
        public static bool IsNextLogError { get; set; } = true;
        public static void Error(string str, params object[] ps)
        {
            if (IsNextLogError)
                CLog.Error(str, ps);
        }
        #endregion
    }
}