using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

using Sirenix.OdinInspector.Editor;
using CYM.Template;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using CYM.DLC;

namespace CYM
{
    public class BuildWindow : EditorWindow
    {

        [MenuItem("Tools/CYM/Build  #q")]
        public static void ShowWindow()
        {
            var ret = ShowWindow<BuildWindow>();
            ret.minSize = new Vector2(300,500);
        }
        [MenuItem("Tools/CYM/Option  %#q")]
        public static void ShowOptionWindow()
        {
            StaticInspectorWindow.InspectType(typeof(Options), StaticInspectorWindow.AccessModifierFlags.Public, StaticInspectorWindow.MemberTypeFlags.AllButObsolete);
        }

        #region prop
        static GUIStyle TitleStyle = new GUIStyle(); 
        static BuildConfig BuildConfig => BuildConfig.Ins;
        static LocalConfig LocalConfig => LocalConfig.Ins;
        static DLCConfig DLCConfig => DLCConfig.Ins;
        static UIConfig UIConfig => UIConfig.Ins;
        static LogConfig LogConfig => LogConfig.Ins;
        protected static Dictionary<string, string> SceneNames { get; private set; } = new Dictionary<string, string>();
        protected static string VerticalStyle = "HelpBox";
        protected static string ButtonStyle = "minibutton";
        protected static string FoldStyle = "FoldOutPreDrop;";
        protected static string SceneButtonStyle = "ButtonMid;";
        #endregion

        #region life
        void OnEnable()
        {
            RefreshData();
        }
        void RefreshData()
        {
            titleContent.text = "Build";
            TitleStyle.fixedWidth = 100;
            EnsureProjectFiles();
            RefreshSceneNames();
            Repaint();
            if (DLCConfig.Ins != null &&
                BuildConfig.Ins != null &&
                LocalConfig.Ins != null &&
                GameConfig.Ins != null &&
                CursorConfig.Ins != null &&
                UIConfig.Ins != null &&
                ImportConfig.Ins!=null &&
                LogConfig.Ins!=null 
                )
            {
                CLog.Info("创建所有的Config成功!!!");
            }
            else
            {
                CLog.Error("有Config没有创建成功");
            }
            CLog.Info("打开开发者界面");
        }
        void DrawGUI()
        {
            Present_Info();
            Present_Version();
            Present_Settings();
            Present_Build();
            Present_Explorer();
            Present_SubWindow();
            Present_LevelList();
            Present_Other();
        }
        #endregion

        #region info
        void Present_Info()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldInfo = EditorGUILayout.Foldout(LocalConfig.Ins.FoldInfo, "Info", true))
            {
                if (!BuildConfig.LastBuildTime.IsInv())
                    EditorGUILayout.LabelField("BuildTime:" + BuildConfig.LastBuildTime);
                EditorGUILayout.LabelField(string.Format("版本号预览:{0}", BuildConfig));
                EditorGUILayout.LabelField(string.Format("完整版本号预览:{0}", BuildConfig.FullVersionName));
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Version
        void Present_Version()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldVersion = EditorGUILayout.Foldout(LocalConfig.Ins.FoldVersion, "版本", true))
            {
                BuildConfig.Platform = (Platform)EditorGUILayout.Popup("目标", (int)BuildConfig.Platform, Enum.GetNames(typeof(Platform)));
                BuildConfig.Distribution = (Distribution)EditorGUILayout.EnumPopup("发布平台", BuildConfig.Distribution);
                BuildConfig.BuildType = (BuildType)EditorGUILayout.EnumPopup("打包版本", BuildConfig.BuildType);

                BuildConfig.Name = EditorGUILayout.TextField("名称", BuildConfig.Name);
                if (PlayerSettings.productName != BuildConfig.Name)
                {
                    PlayerSettings.productName = BuildConfig.Name;
                    RefreshAppIdentifier();
                }
                BuildConfig.CompanyName = EditorGUILayout.TextField("公司", BuildConfig.CompanyName);
                if (PlayerSettings.companyName != BuildConfig.CompanyName)
                {
                    PlayerSettings.companyName = BuildConfig.Name;
                    RefreshAppIdentifier();
                }

                BuildConfig.Major = EditorGUILayout.IntField("主版本", BuildConfig.Major);
                BuildConfig.Minor = EditorGUILayout.IntField("副版本", BuildConfig.Minor);
                BuildConfig.Data = EditorGUILayout.IntField("存档标", BuildConfig.Data);
                BuildConfig.Prefs = EditorGUILayout.IntField("Prefs", BuildConfig.Prefs);

                EditorGUILayout.BeginHorizontal();
                BuildConfig.Tag = (VersionTag)EditorGUILayout.EnumPopup("后缀", BuildConfig.Tag);
                BuildConfig.Suffix = EditorGUILayout.IntField(BuildConfig.Suffix);
                EditorGUILayout.EndHorizontal();


                if (PlayerSettings.bundleVersion != BuildConfig.ToString())
                    PlayerSettings.bundleVersion = BuildConfig.ToString();

                if (PlayerSettings.productName != BuildConfig.Name)
                    PlayerSettings.productName = BuildConfig.Name;

                if (PlayerSettings.companyName != BuildConfig.CompanyName)
                    PlayerSettings.companyName = BuildConfig.CompanyName;

                EditorGUILayout.BeginVertical();
                OnDrawSettings();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            void RefreshAppIdentifier()
            {
                PlayerSettings.applicationIdentifier = "com." + BuildConfig.CompanyName + "." + BuildConfig.Name;
            }
        }
        #endregion

        #region setting
        void Present_Settings()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.FoldSetting = EditorGUILayout.Foldout(LocalConfig.FoldSetting, "设置", true))
            {
                EditorGUILayout.BeginVertical();
                //Build
                BuildConfig.NameSpace = EditorGUILayout.TextField("命名空间", BuildConfig.NameSpace);
                BuildConfig.MainUIView = EditorGUILayout.TextField("MainUIView", BuildConfig.MainUIView);

                if (LocalConfig.FoldSettingBuild = EditorGUILayout.Foldout(LocalConfig.FoldSettingBuild, "Build"))
                {
                    BuildConfig.TouchDPI = EditorGUILayout.IntField("Touch DPI", BuildConfig.TouchDPI);
                    BuildConfig.DragDPI = EditorGUILayout.IntField("Drag DPI", BuildConfig.DragDPI);
                    LogConfig.Enable = EditorGUILayout.Toggle("Is Log", LogConfig.Enable);
                    UIConfig.IsShowLogo = EditorGUILayout.Toggle("Is Show Logo", UIConfig.IsShowLogo);
                    BuildConfig.IgnoreChecker = EditorGUILayout.Toggle("Is Ignore Checker", BuildConfig.IgnoreChecker);
                    bool preDevelopmentBuild = BuildConfig.IsUnityDevelopmentBuild;
                    BuildConfig.IsUnityDevelopmentBuild = EditorGUILayout.Toggle("Is Dev Build", BuildConfig.IsUnityDevelopmentBuild);
                    if (preDevelopmentBuild != BuildConfig.IsUnityDevelopmentBuild)
                    {
                        EditorUserBuildSettings.development = BuildConfig.IsUnityDevelopmentBuild;
                    }
                }

                //其他
                if (LocalConfig.FoldSettingDLC = EditorGUILayout.Foldout(LocalConfig.FoldSettingDLC, "DLC"))
                {
                    DLCConfig.IsSimulationEditor = EditorGUILayout.Toggle("Is Simulation Editor", DLCConfig.IsSimulationEditor);
                    DLCConfig.IsInitLoadDirBundle = EditorGUILayout.Toggle("Is Init Load Dir Bundle", DLCConfig.IsInitLoadDirBundle);
                    DLCConfig.IsInitLoadSharedBundle = EditorGUILayout.Toggle("Is Init Load Shared Bundle", DLCConfig.IsInitLoadSharedBundle);
                    DLCConfig.IsWarmupAllShaders = EditorGUILayout.Toggle("Is Warmup All Shaders", DLCConfig.IsWarmupAllShaders);
                    DLCConfig.IsAssetBundleConfig = EditorGUILayout.Toggle("Is Asset Bundle Config", DLCConfig.IsAssetBundleConfig);
                    DLCConfig.IsDiscreteShared = EditorGUILayout.Toggle("Is Discrete Shared", DLCConfig.IsDiscreteShared);
                    DLCConfig.IsForceBuild = EditorGUILayout.Toggle("Is Force Build DLC", DLCConfig.IsForceBuild);
                    DLCConfig.IsCompresse = EditorGUILayout.Toggle("Is Compresse", DLCConfig.IsCompresse);
                }

                OnDrawSettings();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 构建
        void Present_Build()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.FoldDLC = EditorGUILayout.Foldout(LocalConfig.FoldDLC, "构建", true))
            {
                EditorGUILayout.BeginVertical();

                foreach (var item in DLCConfig.EditorAll)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(item.Name, GUILayout.MaxWidth(100));

                    if (DLCConfig.IsSimulationEditor)
                    {
                        if (GUILayout.Button("Build Config"))
                        {
                            Builder.BuildDLCConfig(item);
                            AssetDatabase.Refresh();
                        }
                        if (GUILayout.Button("Build Bundle"))
                        {
                            Builder.BuildBundle(item);
                            EditorUtility.DisplayDialog("提示!", "恭喜! 已经打包完成!!", "确定");
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("请勾选编辑器模式");
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("RrfreshDLC"))
                {
                    DLCConfig.RefreshDLC();
                }
                if (GUILayout.Button("RecreateDLC"))
                {
                    DLCConfig.RecreateDLC();
                    DLCConfig.RefreshDLC();
                }
                if (DLCConfig.IsSimulationEditor)
                {
                    if (GUILayout.Button("One click build"))
                    {
                        if (CheckEorr()) return;
                        if (!CheckDevBuildWarring()) return;
                        if (!CheckAuthority()) return;
                        RefreshData();
                        foreach (var item in DLCConfig.EditorAll)
                        {
                            Builder.BuildBundle(item);
                        }
                        Builder.BuildEXE();
                        EditorUtility.DisplayDialog("提示!", "恭喜! 已经打包完成!!", "确定");
                        EditorApplication.Beep();
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Build"))
                {
                    if (CheckEorr()) return;
                    if (!CheckDevBuildWarring()) return;
                    if (!CheckAuthority()) return;
                    RefreshData();
                    Builder.BuildEXE();
                    EditorUtility.DisplayDialog("提示!", "恭喜! 已经打包完成!!", "确定");
                }

                if (GUILayout.Button("Run game"))
                {
                    FileUtil.OpenFile(BuildConfig.ExePath);
                    CLog.Info("Run:{0}", BuildConfig.ExePath);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 资源管理器
        void Present_Explorer()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldExplorer = EditorGUILayout.Foldout(LocalConfig.Ins.FoldExplorer, "链接", true))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Persistent"))
                {
                    FileUtil.OpenExplorer(Application.persistentDataPath);
                }
                else if (GUILayout.Button("删除 Persistent"))
                {
                    FileUtil.DeletePath(Application.persistentDataPath);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Project File"))
                {
                    FileUtil.OpenExplorer(Const.Path_Project);
                }
                else if (GUILayout.Button("Bin"))
                {
                    FileUtil.OpenExplorer(Const.Path_Build);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                OnDrawPresentExplorer();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 关卡列表
        [HideInInspector]
        public Vector2 scrollSceneList = Vector2.zero;
        void Present_LevelList()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldSceneList = EditorGUILayout.Foldout(LocalConfig.Ins.FoldSceneList, "场景", true))
            {
                if (SceneNames.Count > 5)
                    scrollSceneList = EditorGUILayout.BeginScrollView(scrollSceneList, GUILayout.ExpandHeight(true), GUILayout.MinHeight(300));
                else
                    scrollSceneList = EditorGUILayout.BeginScrollView(scrollSceneList, GUILayout.ExpandHeight(true), GUILayout.MinHeight(SceneNames.Count * 50));

                EditorGUILayout.BeginHorizontal();
                OnDrawPresentScene();
                EditorGUILayout.EndHorizontal();
                if (SceneNames != null)
                {
                    foreach (var item in SceneNames)
                    {
                        if (item.Key == Const.SCE_Preview ||
                            item.Key == Const.SCE_Start ||
                            item.Key == Const.SCE_Test)
                            continue;
                        DrawGoToBundleSceneButton(item.Key, item.Value);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 其他
        void Present_Other()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldOther = EditorGUILayout.Foldout(LocalConfig.Ins.FoldOther, "其他", true))
            {
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("预览场景"))
                {
                    GoToScene(GetScenesPath(Const.SCE_Preview), OpenSceneMode.Additive);
                }
                else if (GUILayout.Button("保存"))
                {
                    EditorUtility.SetDirty(UIConfig);
                    EditorUtility.SetDirty(DLCConfig);
                    EditorUtility.SetDirty(LocalConfig);
                    EditorUtility.SetDirty(BuildConfig);
                    AssetDatabase.SaveAssets();
                }
                else if (GUILayout.Button("编译"))
                {
                    AssetDatabase.Refresh();
                }
                else if (GUILayout.Button("刷新"))
                {
                    RefreshData();
                }
                else if (GUILayout.Button("运行"))
                {
                    AssetDatabase.Refresh();
                    GoToScene(GetSystemScenesPath(Const.SCE_Start), OpenSceneMode.Single);
                    EditorApplication.isPlaying = true;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                OnDrawPresentScriptTemplate();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region sub Window
        void Present_SubWindow()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (LocalConfig.Ins.FoldSubWindow = EditorGUILayout.Foldout(LocalConfig.Ins.FoldSubWindow, "窗口", true))
            {
                if (GUILayout.Button("Shelf")) ShelfEditor.ShowWindow();
                else if (GUILayout.Button("Prefs")) PrefsWindow.ShowWindow();
                else if (GUILayout.Button("GUIStyle")) GUIStyleWindow.ShowWindow();
                else if (GUILayout.Button("Template")) TemplateSettingsWindow.ShowWindow();
                else if (GUILayout.Button("ColorPicker")) ColorPickerWindow.ShowWindow();
                else if (GUILayout.Button("DebugOptions")) ShowOptionWindow();
                else if (GUILayout.Button("Dependencies")) DependencyWindow.ShowWindow();
                else if (GUILayout.Button("ParticleScaler")) ParticleScalerWindow.ShowWindow();
                else if (GUILayout.Button("UnityTexture")) UnityTextureWindow.ShowWindow();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region utile
        protected static bool CheckDevBuildWarring()
        {
            if (BuildConfig.IsDevBuild)
            {
                return EditorUtility.DisplayDialog("警告!", "您确定要构建吗?因为当前是Dev版本", "确定要构建Dev版本", "取消");
            }
            return true;
        }
        protected static bool CheckAuthority()
        {
            CLog.Info("打包:"+SystemInfo.deviceName);
            return true;
        }
        protected bool CheckEorr()
        {
            if (BuildConfig.IgnoreChecker)
                return false;

            if (CheckIsHaveError())
                return true;
            return false;
        }
        protected static bool DoCheckWindow<T>() where T : CheckerWindow
        {
            T window = GetWindow<T>();
            window.CheckAll();
            window.Close();
            return window.IsHaveError();
        }
        protected static EditorWindow ShowWindow<T>() where T : EditorWindow
        {
            var ret = GetWindow<T>();
            ret.ShowPopup();
            return ret;
        }
        protected static string GetScenesPath(string name)
        {
            return string.Format(Const.Format_BundleScenesPath, name);
        }
        protected static string GetSystemScenesPath(string name)
        {
            return string.Format(Const.Format_BundleSystemScenesPath, name);
        }
        protected static void DrawGoToBundleSystemSceneButton(string name)
        {
            if (GUILayout.Button(name))
            {
                GoToScene(GetSystemScenesPath(name));
            }
        }
        protected static void DrawGoToBundleSceneButton(string name, string fullPath)
        {
            if (GUILayout.Button(name))
            {
                GoToSceneByFullPath(fullPath);
            }
        }
        protected static void DrawButton(string name, Callback doAction)
        {
            if (GUILayout.Button(name))
            {
                doAction?.Invoke();
            }
        }
        protected static void GoToScene(string path, OpenSceneMode mode = OpenSceneMode.Single)
        {
            GoToSceneByFullPath(Application.dataPath + path, mode);
        }
        protected static void GoToSceneByFullPath(string path, OpenSceneMode mode = OpenSceneMode.Single)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(path, mode);
        }
        protected static void LookAtPos(Vector3 pos)
        {
            SceneView view = SceneView.lastActiveSceneView;
            view.LookAt(pos, view.rotation, 20);
        }
        protected static void SafeOpenJsonFile<T>(string path, T data) where T : class
        {
            FileUtil.UpdateFile(path, data);
            FileUtil.OpenFile(path);
        }
        protected static void RefreshSceneNames()
        {
            var paths = AssetDatabase.GetAssetPathsFromAssetBundle(Const.BN_Scene);
            SceneNames.Clear();
            foreach (var item in paths)
            {
                string tempName = Path.GetFileNameWithoutExtension(item);
                if (!SceneNames.ContainsKey(tempName))
                    SceneNames.Add(tempName, item);
            }
        }
        /// <summary>
        /// 确保标准项目文件夹存在
        /// </summary>
        public static void EnsureProjectFiles()
        {
            FileUtil.EnsureDirectory(Const.Path_Arts);
            FileUtil.EnsureDirectory(Path.Combine(Const.Path_Arts, "Scene"));

            FileUtil.EnsureDirectory(Const.Path_Bundles);

            FileUtil.EnsureDirectory(Const.Path_Resources);
            FileUtil.EnsureDirectory(Const.Path_ResourcesConfig);
            FileUtil.EnsureDirectory(Const.Path_ResourcesTemp);
            FileUtil.EnsureDirectory(Const.Path_ResourcesText);
            FileUtil.EnsureDirectory(Const.Path_ResourcesScriptTemplate);
            FileUtil.EnsureDirectory(Const.Path_ResourcesConst);

            FileUtil.EnsureDirectory(Const.Path_Funcs);
            FileUtil.EnsureDirectory(Path.Combine(Const.Path_Funcs, "GlobalMgr"));
            FileUtil.EnsureDirectory(Path.Combine(Const.Path_Funcs, "Main"));
            FileUtil.EnsureDirectory(Path.Combine(Const.Path_Funcs, "Table"));
            FileUtil.EnsureDirectory(Path.Combine(Const.Path_Funcs, "UI"));
            FileUtil.EnsureDirectory(Path.Combine(Const.Path_Funcs, "UnitMgr"));
            FileUtil.EnsureDirectory(Path.Combine(Const.Path_Funcs, "UnitMono"));
            FileUtil.EnsureDirectory(Const.Path_StreamingAssets);
            TemplateTool.RefreshTemplates(Const.Path_Editor, false);
        }
        #endregion

        #region Override
        [HideInInspector]
        public Vector2 scrollPosition = Vector2.zero;
        protected void OnGUI()
        {
            if (BuildConfig == null)
                return;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawGUI();
            EditorGUILayout.EndScrollView();
        }
        protected virtual void OnDrawPresentScene()
        {
            //e.x.
            DrawGoToBundleSystemSceneButton(Const.SCE_Start);
            DrawGoToBundleSystemSceneButton(Const.SCE_Preview);
            DrawGoToBundleSystemSceneButton(Const.SCE_Test);
        }
        protected virtual void OnDrawPresentScriptTemplate()
        {
        }
        protected virtual void OnDrawPresentExplorer()
        {
        }
        protected virtual void OnDrawSubwindow()
        {

        }
        protected virtual void OnDrawSettings()
        {

        }
        protected virtual bool CheckIsHaveError()
        {
            //e.x.
            return DoCheckWindow<CheckerWindow>();
        }
        #endregion

        #region Callback

        #endregion
    }
}