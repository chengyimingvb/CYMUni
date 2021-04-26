using System;
using System.Collections.Generic;
using System.IO;
namespace CYM
{
    public class ArchiveMgr<T> : IBaseArchiveMgr where T : DBBaseGame, new()
    {
        //默认刷新一次
        bool _isArchiveListDirty = true;
        List<ArchiveFile<T>> _allArchives = new List<ArchiveFile<T>>();
        List<ArchiveFile<T>> _lastArchives = new List<ArchiveFile<T>>();
        string BasePath;
        public void Init(string path)
        {
            BasePath = path;
            FileUtil.EnsureDirectory(path);
        }
        // 当前存档
        public string CurArchives { get; set; }
        public int CurArchiveVersion => BuildConfig.Ins.Data;
        // 得到存档
        public ArchiveFile<T> GetArchive(string id) => _allArchives.Find(ar => ar.Name == id);
        // 得到最后修改的时间
        public DateTime GetLastWriteTime(string name) => new FileInfo(Path.Combine(BasePath, name)).LastWriteTime;
        // 获取所有存档
        public List<ArchiveFile<T>> GetAllArchives(bool isRefresh = false)
        {
            if (isRefresh)
                SetArchiveListDirty();
            if (_isArchiveListDirty)
            {
                RefreshArchiveList();
            }
            return _allArchives;
        }
        public List<IBaseArchiveFile> GetAllBaseArchives(bool isRefresh = false)
        {
            if (isRefresh)
                SetArchiveListDirty();
            if (_isArchiveListDirty)
            {
                RefreshArchiveList();
            }
            List<IBaseArchiveFile> ret = new List<IBaseArchiveFile>();
            foreach (var item in _allArchives)
            {
                ret.Add(item);
            }
            return ret;
        }
        // 得到存档路径
        string GetArchivePath(string name) => Path.Combine(BasePath, name + Const.Extention_Save);
        // 得到所有文件
        public string[] GetFiles() => FileUtil.GetFiles(BasePath, "*" + Const.Extention_Save, SearchOption.AllDirectories);

        #region set
        // 设置dirty
        public void SetArchiveListDirty()
        {
            _isArchiveListDirty = true;
        }
        // 从运行中的游戏保存
        public ArchiveFile<T> SaveData(string id, T GameData, bool isAsyn = true,bool isDirtyList=true, bool isHide = false)
        {
            if (isAsyn && isDirtyList)
            {
                CLog.Error("错误!isAsyn && isDirtyList 不能同时出现");
            }
            ArchiveFile<T> archive = new ArchiveFile<T>(id, CurArchiveVersion, default, isHide);
            if (isAsyn)
            {
                archive.SaveAsyn(GameData, GetArchivePath(id));
            }
            else
            {
                archive.Save(GameData, GetArchivePath(id));
                if(isDirtyList)
                    SetArchiveListDirty();
            }
            return archive;
        }
        // 刷新存档列表
        public void RefreshArchiveList()
        {
            foreach (var item in _allArchives)
            {
                if (item.IsInSaving)
                {
                    CLog.Error("错误:有文件正在存储,无发刷新");
                    break;
                }
            }
            _lastArchives.Clear();
            _lastArchives.AddRange(_allArchives);
            _allArchives.Clear();
            foreach (var file in GetFiles())
            {
                if (Path.GetExtension(file) == Const.Extention_Save)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    DateTime fileTime = GetLastWriteTime(file);
                    ArchiveFile<T> a = null;
                    // 如果以前就存在这个存档的，而且修改时间符合，则使用以前的
                    a = _lastArchives.Find(ac => ac.Name == name && ac.FileTime == fileTime);
                    if (a == null)
                    {
                        a = LoadArchive(name, false);
                    }
                    else
                    {
                    }
                    _allArchives.Add(a);
                }
            }

            // 按时间排序
            _allArchives.Sort((a1, a2) => -a1.SaveTime.CompareTo(a2.SaveTime));
            _isArchiveListDirty = false;
        }
        // 删除指定存档
        public void DeleteArchives(string ID)
        {
            if (!IsHaveArchive(ID))
            {
                CLog.Error("没有这个存档,错误id=" + ID);
                return;
            }
            else
            {
                File.Delete(GetArchivePath(ID));
            }
            SetArchiveListDirty();
        }
        // 加载存档
        public ArchiveFile<T> LoadArchive(string ID, bool isReadContnet)
        {
            string path = GetArchivePath(ID);
            ArchiveFile<T> archive = new ArchiveFile<T>(ID, CurArchiveVersion);
            archive.Load(path, ID, CurArchiveVersion, isReadContnet, GetLastWriteTime(path));
            return archive;
        }
        #endregion

        #region is
        // 存档是否可以载入
        public bool IsArchiveValid(string id)
        {
            ArchiveFile<T> a = GetArchive(id);
            return a != null && a.IsLoadble;
        }
        // 是否存在相同的存档
        public bool IsHaveArchive(string ID) => GetArchive(ID) != null;
        public bool IsHaveArchive() => _allArchives.Count > 0;
        #endregion
    }

}