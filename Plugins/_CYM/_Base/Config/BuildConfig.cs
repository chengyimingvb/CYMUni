using System;
using System.IO;
using UnityEngine;

namespace CYM
{
    public enum VersionTag
    {
        Preview,//预览版本
        Beta,//贝塔版本
        Release,//发行版本
    }

    public enum Platform
    {
        Windows64,
        Android,
        IOS,
    }

    public enum Distribution
    {
        Steam,//Steam平台
        Rail,//腾讯WeGame平台
        Turbo,//多宝平台
        Trial,//试玩平台
        Gaopp,//版署版本
        Usual,//通用版本
    }

    /// <summary>
    /// 发布类型
    /// </summary>
    public enum BuildType
    {
        Develop,//内部开发版本
        Public,//上传发行版本
    }

    public sealed class BuildConfig : ScriptableObjectConfig<BuildConfig>
    {
        public override void OnCreated()
        {
            int pathCount = Enum.GetNames(typeof(Distribution)).Length;
        }

        public string LastBuildTime;
        public Platform Platform = Platform.Windows64;

        public string FullVersionName
        {
            get
            {
                return string.Format("{0} {1} {2}", FullName, ToString(), Platform);
            }
        }

        public string DirPath
        {
            get
            {
                if (IsPublic)
                {
                    if (IsTrial)
                    {
                        return Path.Combine(Const.Path_Build, Platform.ToString()) + " " + Distribution;//xxxx/Windows_x64 Trail
                    }
                    else
                        return Path.Combine(Const.Path_Build, Platform.ToString());//xxxx/Windows_x64
                }
                else
                {
                    return Path.Combine(Const.Path_Build, FullVersionName);//xxxx/BloodyMary v0.0 Preview1 Windows_x64 Steam
                }
            }
        }

        public string ExePath
        {
            get
            {
                if(Platform == Platform.Windows64)
                    return Path.Combine(DirPath, FullName + ".exe");
                else if(Platform == Platform.Android)
                    return Path.Combine(DirPath, FullName + ".apk");
                else if(Platform == Platform.IOS)
                    return Path.Combine(DirPath, FullName + ".ipa");
                throw new Exception();
            }
        }
        public string GameVersion { get { return ToString(); } }


        #region Inspector
        public string Name = "MainTitle";
        public string CompanyName = "Yiming";
        public Distribution Distribution;
        public string NameSpace = "~~~";
        public string MainUIView = "~~~";
        public int TouchDPI = 1;
        public int DragDPI = 800;
        public string FullName => Name ;
        #endregion

        #region version data
        public int Major;
        public int Minor;
        public int Data;
        public int Suffix = 1;
        public int Prefs = 0;
        public VersionTag Tag = VersionTag.Preview;
        public bool IsUnityDevelopmentBuild;
        public bool IsObfuse=true;
        public bool IgnoreChecker;
        public BuildType BuildType = BuildType.Develop;

        public bool IsDevBuild => BuildType == BuildType.Develop;
        public bool IsPublic => BuildType == BuildType.Public;
        public bool IsTrial => Distribution == Distribution.Trial;
        public override string ToString()
        {
            string str = string.Format("v{0}.{1} {2}{3} {4}", Major, Minor, Tag, Suffix, Distribution);
            if (IsDevBuild)
            {
                str += " Dev";
            }
            return str;
        }
        #endregion

        #region Build data
        [NonSerialized]
        public string Username;
        [NonSerialized]
        public string Password;
        public BuildData GetBuildData(Distribution type)
        {
            if (type == Distribution.Steam)
                return new BuildSteamData();
            else
                return new BuildData();
            throw new Exception("GetBuildData:错误的平台!");
        }
        #endregion

        #region is
        public static bool IsWindowsPlayer => Application.platform == RuntimePlatform.WindowsPlayer;
        /// <summary>
        /// 数据库版本是否兼容
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsInData(int data)
        {
            return Data == data;
        }
        #endregion
    }

    #region Base
    [System.Serializable]
    public class BuildData
    {
        protected BuildConfig BuildConfig { get { return BuildConfig.Ins; } }
        public virtual void PostBuilder()
        {

        }
    }
    #endregion

    #region Steam Data
    [System.Serializable]
    public class BuildSteamData : BuildData
    {
        public override void PostBuilder()
        {
            FileUtil.CopyFileToDir("steam_appid.txt", BuildConfig.DirPath);
        }
    }
    #endregion

}