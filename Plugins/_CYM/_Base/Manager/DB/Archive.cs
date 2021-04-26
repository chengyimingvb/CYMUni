//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using Sirenix.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace CYM
{
    [Serializable]
    // 存档文件头，用于存储少量信息
    public class ArchiveHeader
    {
        // 存档TDID
        public string PlayerID;
        // 用来测试兼容性
        public int Version;
        // 游戏时间
        public int PlayTime;
        public int ContentLength;
        // 游戏日期，ticks
        public long SaveTimeTicks;
        // hash,用来检测文件是否在传输过程中出错或者被意外修改
        public string ContentHash;
        // 内容是否为压缩，格式为GZ
        public bool IsCompressed;
        //是否为隐藏
        public bool IsHide = false;

        public DateTime SaveTime => new DateTime(SaveTimeTicks);

        public ArchiveHeader()
        {
            Version = -1;
            PlayTime = 0;
        }
    }

    // 游戏存档
    public class ArchiveFile<T> : IBaseArchiveFile where T : DBBaseGame
    {
        public string Name { get; private set; }
        public DateTime SaveTime { get { return Header.SaveTime; } }
        public bool IsBroken { get; private set; }
        public ArchiveHeader Header { get; private set; }
        public DateTime FileTime { get; private set; }        // FileTime用于快速发现文件是否没变
        public bool IsLoadble => !IsBroken && IsCompatible;
        public bool IsCompatible => Header.Version == curVersion;
        public TimeSpan PlayTime => new TimeSpan(0, 0, Header.PlayTime);
        public bool IsInSaving { get; private set; } = false;

        // 当存档载入仅读取文件头时，GameDatas为空
        public T GameDatas { get; private set; }
        public DBBaseGame BaseGameDatas => GameDatas;
        byte[] Content;
        int curVersion;
        bool isHide = false;

        #region set
        public ArchiveFile(string name, int curVersion, DateTime fileTime = new DateTime(), bool isHide = false)
        {
            this.isHide = isHide;
            this.curVersion = curVersion;
            Name = name;
            FileTime = fileTime;
            Header = new ArchiveHeader();
        }

        // 载入存档
        public void Load(string path, string name, int curVersion, bool isReadContent, DateTime fileTime)
        {
            ArchiveFile<T> archive = this;
            archive.Name = name;
            archive.curVersion = curVersion;
            archive.FileTime = fileTime;
            using (Stream stream = File.OpenRead(path))
            {
                try
                {
                    BinaryReader reader = new BinaryReader(stream);
                    string headerStr = null;
                    //使用try防止无效的存档
                    headerStr = reader.ReadString();
                    if (string.IsNullOrEmpty(headerStr))
                    {
                        archive.IsBroken = true;
                    }
                    else
                    {
                        archive.Header = JsonUtility.FromJson<ArchiveHeader>(headerStr);
                        int contentSize = archive.Header.ContentLength;
                        if (contentSize <= 0)
                        {
                            archive.IsBroken = true;
                        }
                        else
                        {
                            archive.Content = reader.ReadBytes(contentSize);
                            if (!string.IsNullOrEmpty(archive.Header.ContentHash))
                            {
                                // 内容损坏
                                if (archive.Header.ContentHash != FileUtil.Hash(archive.Content))
                                {
                                    archive.IsBroken = true;
                                    return;
                                }
                            }
                            if (isReadContent && archive.IsCompatible && contentSize > 0)
                            {
                                byte[] toBeDeserialized = null;
                                if (archive.Header.IsCompressed)
                                {
                                    toBeDeserialized = FileUtil.GZDecompressToBytes(archive.Content);
                                }
                                else
                                {
                                    toBeDeserialized = archive.Content;
                                }
                                archive.GameDatas = SerializationUtility.DeserializeValue<T>(toBeDeserialized, DataFormat.Binary);
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    archive.IsBroken = true;
                    CLog.Error("读取存档{0}时出现异常:{1}, 因此认为是损坏的存档。", archive.Name, e.Message);
                }
            }
            return;
        }

        // 保存存档
        public void Save(T datas, string path)
        {
            if (IsInSaving) return;
            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                IsInSaving = true;
                GameDatas = datas ?? throw new ArgumentNullException("datas");
                bool IsHash = false;
                bool IsCompressed = false;
                //保存文件头
                Header.PlayTime = GameDatas.PlayTime;
                Header.PlayerID = GameDatas.PlayerID;
                Header.Version = curVersion;
                Header.IsHide = isHide;
                Header.SaveTimeTicks = DateTime.Now.Ticks;
                Header.IsCompressed = IsCompressed;
                if (IsHash) Header.ContentHash = FileUtil.Hash(Content);
                else Header.ContentHash = null;
                //保存内容
                Content = SerializationUtility.SerializeValue(GameDatas, DataFormat.Binary);
                if (IsCompressed) Content = FileUtil.GZCompressToBytes(Content);
                //写入长度
                Header.ContentLength = Content.Length;
                string headerStr = JsonUtility.ToJson(Header);
                using (BinaryWriter writer = new BinaryWriter(stream)) 
                {
                    writer.Write(headerStr);
                    writer.Write(Content);
                    writer.Close();
                }
                stream.Close();
                IsInSaving = false;
            }
        }
        public async void SaveAsyn(T datas, string path)
        {
            if (IsInSaving) return;
            await Task.Run(() =>
            {
                Save(datas, path);
            });
        }
        #endregion
    }

}