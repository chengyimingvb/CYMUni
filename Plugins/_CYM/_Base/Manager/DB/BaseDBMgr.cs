//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CYM
{
    // 自动存储类型
    public enum AutoSaveType
    {
        None,
        Monthly,
        Yearly,
    }
    public class BaseDBMgr<TGameData> : BaseGFlowMgr, IBaseDBMgr 
        where TGameData : DBBaseGame, new()
    {
        #region 存档
        public ArchiveMgr<TGameData> CurArchiveMgr;
        public ArchiveMgr<TGameData> LocalArchiveMgr = new ArchiveMgr<TGameData>();
        public ArchiveMgr<TGameData> RemoteArchiveMgr = new ArchiveMgr<TGameData>();
        #endregion

        #region prop
        // 当前游戏存储的数据
        public TGameData CurGameData { get; protected set; } = new TGameData();
        // 是有拥有snapshot
        public bool HasSnapshot { get; protected set; } = false;
        // 开始自动存档
        public event Callback<AutoSaveType> Callback_OnStartAutoSave;
        // 结束自动存档
        public event Callback<AutoSaveType> Callback_OnEndAutoSave;
        // 游戏存储
        public event Callback<ArchiveFile<TGameData>> Callback_OnSaveGame;
        // 玩的时间
        public int PlayTime { get; protected set; } = 0;
        #endregion

        #region mgr
        IBaseScreenMgr ScreenMgr => BaseGlobal.ScreenMgr;
        new IBaseBattleMgr BattleMgr => BaseGlobal.BattleMgr;
        IBaseSettingsMgr SettingsMgr => BaseGlobal.SettingsMgr;
        BaseUnit BaseLocalPlayer => ScreenMgr.BaseLocalPlayer;
        #endregion

        #region 生命周期
        public override void OnEnable()
        {
            base.OnEnable();
            try
            {
                if (LocalArchiveMgr != null)
                {
                    LocalArchiveMgr.Init(GetLocalArchivePath());
                }

                if (RemoteArchiveMgr != null)
                {
                    RemoteArchiveMgr.Init(GetCloudArchivePath());
                }
            }
            catch (Exception e)
            {
                if (e != null)
                    CLog.Error(e.ToString());
            }
            UseRemoteArchives(!Prefers.GetLastAchiveLocal());
        }
        public override void OnStart()
        {
            base.OnStart();
            LocalArchiveMgr.RefreshArchiveList();
            RemoteArchiveMgr.RefreshArchiveList();
        }
        #endregion

        #region is
        //本机上是否有存档
        public bool IsHaveArchives()
        {
            var local = GetAchieveMgr(true);
            if (local.IsHaveArchive())
                return true;
            local = GetAchieveMgr(false);
            if (local.IsHaveArchive())
                return true;
            return false;
        }
        // 是否存在当前的存档
        public bool IsHaveSameArchives(string ID)
        {
            return CurArchiveMgr.IsHaveArchive(ID);
        }
        // 是否有游戏数据
        public bool IsHaveGameData()
        {
            return CurGameData != null;
        }
        // 是否可以使用云存档
        public bool IsCanUseRemoteArchives()
        {
            return BaseGlobal.PlatSDKMgr.IsSuportCloudArchive();
        }
        // 是否为本地存档
        public bool IsCurArchivesLocal()
        {
            return CurArchiveMgr == LocalArchiveMgr;
        }
        // 是否可以继续游戏
        public virtual bool IsCanContinueGame()
        {
            string id = Prefers.GetLastAchiveID();
            var tempAchive = GetAchieveMgr(Prefers.GetLastAchiveLocal());
            return tempAchive.IsHaveArchive(id) && tempAchive.IsArchiveValid(id);
        }
        #endregion

        #region Get
        // 获得基础数据
        public DBBaseGame CurBaseGameData => CurGameData;
        // 获得默认的存档名称
        public string GetDefaultSaveName() => string.Format($"{BuildConfig.Ins.Name}{DateTime.Today.Year}{DateTime.Today.Month}{DateTime.Today.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}");
        // 获得默认的自动存档名称
        public string GetDefaultAutoSaveName() => Const.Prefix_AutoSave + GetDefaultSaveName();
        // 获取所有的存档
        public List<ArchiveFile<TGameData>> GetAllArchives(bool isRefresh = false) => CurArchiveMgr.GetAllArchives(isRefresh);
        // 云存档的路径
        public virtual string GetCloudArchivePath() => Const.Path_CloudDB;
        // 本地存档路劲
        public virtual string GetLocalArchivePath() => Const.Path_LocalDB;
        public IBaseArchiveMgr GetBaseAchieveMgr(bool isLocal = true) => GetAchieveMgr(isLocal);
        public ArchiveMgr<TGameData> GetAchieveMgr(bool isLocal = true)
        {
            if (isLocal)
                return LocalArchiveMgr;
            return RemoteArchiveMgr;
        }
        public string GetTempSavePath()
        {
            return Path.Combine(Const.Path_LocalDB, Const.STR_DBTempSaveName+Const.Extention_Save);
        }
        #endregion

        #region Set
        // 开始新游戏
        public DBBaseGame StartNewGame()
        {
            CurGameData = OnGenerateNewGameData();
            return CurGameData;
        }
        // 加载游戏
        public DBBaseGame LoadGame(string ID)
        {
            var archive = CurArchiveMgr.LoadArchive(ID, true);
            CurGameData = archive.GameDatas;
            CurGameData = OnModifyGameData(CurGameData);
            return CurGameData;
        }
        // 设置使用远程云存档
        public void UseRemoteArchives(bool isUse)
        {
            if (IsCanUseRemoteArchives() && isUse)
                CurArchiveMgr = RemoteArchiveMgr;
            else
                CurArchiveMgr = LocalArchiveMgr;
        }
        // 另存当前游戏
        // isSetDirty=true 刷新存储文件(会卡) ,否则不刷新,比如自动存储的时候不需要刷新
        // isSnapshot=true 通过最近的一次快照存储游戏
        public virtual DBBaseGame SaveCurGameAs(string id, bool useSnapshot, bool isAsyn,bool isDirtyList, bool isHide , bool forceLocalArchive)
        {
            //保存
            if (id != Const.STR_DBTempSaveName)
            {
                Prefers.SetLastAchiveID(id);
                Prefers.SetLastAchiveLocal(IsCurArchivesLocal());
            }

            ArchiveFile<TGameData> archiveFile;
            if (useSnapshot)
            {
                //使用最近的一次快照
                if (!HasSnapshot)
                {
                    throw new NotImplementedException("最近一次没有快照,请手动调用Sanpshot");
                }
            }
            else
            {
                //临时快照
                Snapshot(false);
            }

            if (forceLocalArchive) archiveFile = LocalArchiveMgr.SaveData(id, CurGameData, isAsyn, isDirtyList, isHide);
            else archiveFile = CurArchiveMgr.SaveData(id, CurGameData, isAsyn, isDirtyList, isHide);
            Callback_OnSaveGame?.Invoke(archiveFile);
            return CurGameData;
        }
        // 自动存储
        public DBBaseGame AutoSave(bool useSnapshot = false)
        {
            AutoSaveType saveType = SettingsMgr.GetBaseSettings().AutoSaveType;
            if (saveType == AutoSaveType.None) return null;
            Callback_OnStartAutoSave?.Invoke(saveType);
            var ret = SaveCurGameAs(Const.Prefix_AutoSave + Util.GetStr(BaseLocalPlayer.TDID),useSnapshot, true,false ,false,true);
            Callback_OnEndAutoSave?.Invoke(saveType);
            return ret;
        }
        public DBBaseGame SaveTemp(bool useSnapshot = false)
        {
            var ret = SaveCurGameAs(Const.STR_DBTempSaveName, useSnapshot, false,false,true ,true);
            return ret;
        }
        // 快照
        // isSnapshot=true 设置快照标记,否则表示临时快照表示内部使用
        public DBBaseGame Snapshot(bool isSnapshot = true)
        {
            CurGameData = new TGameData();
            WriteGameDBData();
            HasSnapshot = isSnapshot;
            return CurGameData;
        }
        // 删除存档
        public void DeleteArchives(string ID) => CurArchiveMgr.DeleteArchives(ID);
        #endregion

        #region DB 读取和写入函数
        // 统一读取:手动调用
        public void ReadGameDBData()
        {
            var data = CurGameData;
            OnReadGameDataStart(data);
            OnReadGameData(data);
            SelfBaseGlobal.OnRead1(data);
            SelfBaseGlobal.OnRead2(data);
            SelfBaseGlobal.OnRead3(data);
            SelfBaseGlobal.OnReadEnd(data);
            OnReadGameDataEnd(data);
        }
        // 统一写入:手动调用
        public void WriteGameDBData()
        {
            CurGameData = new TGameData();
            var data = CurGameData;
            OnWriteGameData(ref data);
            SelfBaseGlobal.OnWrite(data);
        }
        #endregion

        #region DB 主流程
        // 创建自定义存档
        protected virtual TGameData OnGenerateNewGameData()
        {
            var data = new TGameData();
            data.PlayerID = ScreenMgr.SelectedChara;
            data.BattleID = BattleMgr.BattleID;
            return data;
        }
        protected virtual TGameData OnModifyGameData(TGameData data)
        {
            return data;
        }
        // 读取存档
        protected virtual void OnReadGameData(TGameData data)
        {
            PlayTime = data.PlayTime;
            ScreenMgr.SelectChara(data.PlayerID);
        }
        protected virtual void OnReadGameDataStart(TGameData data) { }
        protected virtual void OnReadGameDataEnd(TGameData data) { }
        // 写入存档
        protected virtual void OnWriteGameData(ref TGameData data)
        {
            data.PlayTime = PlayTime + (int)Time.realtimeSinceStartup;
            data.PlayerID = BaseLocalPlayer.TDID;
        }
        #endregion
    }

}