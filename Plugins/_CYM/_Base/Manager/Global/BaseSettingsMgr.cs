//**********************************************
// Class Name	: CYMBaseSettingsManager
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public enum WindowType
    {
        Fullscreen,
        Windowed,
    }
    [Serializable]
    public class DBBaseSettings
    {
        /// <summary>
        /// 是否为简单地形
        /// </summary>
        public bool IsSimpleTerrin = false;
        /// <summary>
        /// 语言类型
        /// </summary>
        public LanguageType LanguageType = LanguageType.Chinese;
        /// <summary>
        /// 禁止背景音乐
        /// </summary>
        public bool MuteMusic = false;
        /// <summary>
        /// 禁止音效
        /// </summary>
        public bool MuteSFX = false;
        /// <summary>
        /// 静止所有音乐
        /// </summary>
        public bool Mute = false;
        /// <summary>
        /// 是否静止环境音效
        /// </summary>
        public bool MuteAmbient = false;
        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float VolumeMusic = 0.2f;
        /// <summary>
        /// 音效音量
        /// </summary>
        public float VolumeSFX = 1.0f;
        /// <summary>
        /// 主音量
        /// </summary>
        public float Volume = 1.0f;
        /// <summary>
        /// 语音音量
        /// </summary>
        public float VolumeVoice = 0.5f;
        /// <summary>
        /// 自动存储类型
        /// </summary>
        public AutoSaveType AutoSaveType = AutoSaveType.None;
        /// <summary>
        /// 开启HUD
        /// </summary>
        public bool EnableHUD = true;
        /// <summary>
        /// 开启MSAA
        /// </summary>
        public bool EnableMSAA = false;
        /// <summary>
        /// 开启SSAO
        /// </summary>
        public bool EnableSSAO = true;
        /// <summary>
        /// 显示FPS
        /// </summary>
        public bool ShowFPS = true;
        /// <summary>
        /// 开启Bloom效果
        /// </summary>
        public bool EnableBloom = true;
        /// <summary>
        /// 是否开启后期特效
        /// </summary>
        public bool EnablePostProcess = true;
        /// <summary>
        /// 开启shadow
        /// </summary>
        public bool EnableShadow = true;
        /// <summary>
        /// 游戏画质
        /// </summary>
        public GamePropType Quality = GamePropType.Hight;
        /// <summary>
        /// 游戏分辨率,通常选择小一号的窗口模式
        /// </summary>
        public int Resolution = 1;
        /// <summary>
        /// 全屏
        /// </summary>
        public WindowType WindowType = WindowType.Windowed;
        /// <summary>
        /// 地形精度
        /// </summary>
        public bool TerrainAccuracy = true;
        /// <summary>
        /// 摄像机移动速度
        /// </summary>
        public float CameraMoveSpeed = 0.5f;
        /// <summary>
        /// 摄像机滚动速度
        /// </summary>
        public float CameraScrollSpeed = 0.5f;
    }

    public class BaseSettingsMgr<TDBSetting> : BaseGFlowMgr, IBaseSettingsMgr 
        where TDBSetting : DBBaseSettings, new()
    {
        #region prop
        public TDBSetting Settings { get; protected set; } = new TDBSetting();
        public bool IsFirstCreateSettings { get; set; } = false;
        #endregion

        #region Callback Val
        /// <summary>
        /// 还原设置
        /// </summary>
        public event Callback<TDBSetting> Callback_OnRevert;
        /// <summary>
        /// 设置初始化
        /// </summary>
        public event Callback Callback_OnInitSettings;
        /// <summary>
        /// 第一次创建设置文件回调
        /// </summary>
        public event Callback<TDBSetting> Callback_OnFirstCreateSetting;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            InitAllResolutions();
            Callback_OnRevert += OnRevert;
            Callback_OnInitSettings += OnInitSetting;
            Callback_OnFirstCreateSetting += OnFirstCreateSetting;
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();
        }
        protected override void OnAllLoadEnd2()
        {
            base.OnAllLoadEnd2();
            RefreshScreenSettings();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            string fullpath = Const.Path_Settings;
            TDBSetting settings = default;
            if (File.Exists(fullpath))
            {
                using (Stream stream = File.OpenRead(fullpath))
                {
                    if (stream != null)
                    {
                        try
                        {
                            settings = FileUtil.LoadJson<TDBSetting>(stream);
                        }
                        catch (Exception e)
                        {
                            settings = default;
                            CLog.Error("载入settings出错{0}", e);
                        }
                    }

                }
            }
            if (settings == null)
            {
                IsFirstCreateSettings = true;
                settings = new TDBSetting();
                Save();
            }
            SetSettings(settings);
        }
        public override void OnStart()
        {
            base.OnStart();
            if (IsFirstCreateSettings)
                Callback_OnFirstCreateSetting?.Invoke(Settings);
            Callback_OnInitSettings?.Invoke();
        }
        /// <summary>
        /// mono的OnDisable
        /// </summary>
        public override void OnDisable()
        {
            Callback_OnRevert -= OnRevert;
            Callback_OnInitSettings -= OnInitSetting;
            Callback_OnFirstCreateSetting -= OnFirstCreateSetting;
            base.OnDisable();
        }
        #endregion

        #region set
        /// <summary>
        /// 还原设置
        /// </summary>
        public virtual void Revert()
        {
            Settings = new TDBSetting();
            Callback_OnRevert?.Invoke(Settings);
        }
        /// <summary>
        /// 设置设置
        /// </summary>
        /// <param name="data"></param>
        public void SetSettings(TDBSetting data)
        {
            Settings = data;
        }
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            using (Stream stream = new FileStream(Const.Path_Settings, FileMode.Create))
            {
                FileUtil.SaveJson(stream, Settings);
                stream.Close();
            }
        }
        /// <summary>
        /// 设置分辨率
        /// </summary>
        public virtual void SetResolution(int index)
        {

            Settings.Resolution = index;
            RefreshScreenSettings();
        }
        /// <summary>
        /// 设置全屏
        /// </summary>
        public virtual void SetWindowType(WindowType type)
        {
            Settings.WindowType = type;
            if (type == WindowType.Fullscreen)
                Settings.Resolution = 0;
            RefreshScreenSettings();
        }
        /// <summary>
        /// 设置画质
        /// </summary>
        public virtual void SetQuality(int index)
        {
            QualitySettings.SetQualityLevel(index);
            Settings.Quality = (GamePropType)index;
        }
        public void SetTerrainAccuracy(bool b)
        {
            Settings.TerrainAccuracy = b;
            if (ActiveTerrain != null)
            {
                ActiveTerrain.heightmapPixelError = b ? 1 : 3;
            }
        }
        public void SetAmbientSound(bool val)
        {
            Settings.MuteAmbient = val;
            if (BaseSceneObject != null && BaseSceneObject.RootSound != null)
            {
                BaseSceneObject.RootSound.gameObject.SetActive(val);
            }
        }
        public void SetAutoSaveType(AutoSaveType autoSaveType)
        {
            Settings.AutoSaveType = autoSaveType;
        }
        public void SetSimpleTerrain(bool b)
        {
            Settings.IsSimpleTerrin = b;
        }
        /// <summary>
        /// 刷新屏幕设置
        /// </summary>
        public void RefreshScreenSettings()
        {
            if (!BaseGlobal.LoaderMgr.IsLoadEnd) return;
            var index = Settings.Resolution;
            if (Resolutions.Count <= index) return;

            if (Settings.WindowType == WindowType.Fullscreen)
            {
                Screen.SetResolution(Resolutions[index].width, Resolutions[index].height, FullScreenMode.MaximizedWindow);
            }
            else if (Settings.WindowType == WindowType.Windowed)
            {
                Screen.SetResolution(Resolutions[index].width, Resolutions[index].height, FullScreenMode.Windowed);
            }
        }
        protected virtual void InitAllResolutions()
        {
            ResolutionsKey.Clear();
            Resolutions.Clear();
            foreach (var item in Screen.resolutions)
            {
                string customKey = string.Format($"{item.width}x{item.height}");
                if (!ResolutionsKey.Contains(customKey))
                {
                    ResolutionsKey.Add(customKey);
                    Resolutions.Add(item);
                }
            }

            Resolutions.Sort((x, y) =>
            {

                if (x.width > y.width)
                    return -1;
                else
                    return 1;
            });
        }
        #endregion

        #region get
        protected HashSet<string> ResolutionsKey = new HashSet<string>();
        protected List<Resolution> Resolutions = new List<Resolution>();
        public virtual string[] GetResolutionStrs()
        {
            return Resolutions.Select(x => x.ToString()).ToArray();
        }
        public DBBaseSettings GetBaseSettings()
        {
            return Settings;
        }
        #endregion

        #region Callback
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            SetTerrainAccuracy(Settings.TerrainAccuracy);
            SetAmbientSound(Settings.MuteAmbient);

        }
        protected virtual void OnRevert(TDBSetting data)
        {
            BaseGlobal.LangMgr.Switch(data.LanguageType);

            BaseGlobal.AudioMgr.MuteMusic(data.MuteMusic);
            BaseGlobal.AudioMgr.MuteSFX(data.MuteSFX);
            BaseGlobal.AudioMgr.SetVolumeMusic(data.VolumeMusic);
            BaseGlobal.AudioMgr.SetVolumeSFX(data.VolumeSFX);
            BaseGlobal.AudioMgr.SetVolume(data.Volume);

            BaseGlobal.CameraMgr.EnableHUD(data.EnableHUD);
            BaseGlobal.CameraMgr.EnableMSAA(data.EnableMSAA);
            BaseGlobal.CameraMgr.EnableBloom(data.EnableBloom);
            BaseGlobal.CameraMgr.EnableSSAO(data.EnableSSAO);

            FPSCounter.Show(data.ShowFPS);

            SetQuality((int)data.Quality);
            SetTerrainAccuracy(data.TerrainAccuracy);
            SetAmbientSound(data.MuteAmbient);
            SetResolution(data.Resolution);
            SetWindowType(data.WindowType);
            SetAutoSaveType(data.AutoSaveType);
            SetSimpleTerrain(data.IsSimpleTerrin);
        }
        protected virtual void OnInitSetting()
        {
            OnRevert(Settings);
        }
        protected virtual void OnFirstCreateSetting(TDBSetting arg1)
        {

        }
        #endregion
    }
}