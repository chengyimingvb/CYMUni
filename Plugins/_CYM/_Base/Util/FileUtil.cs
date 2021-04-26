using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Writers;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace CYM
{
    public partial class FileUtil:BaseFileUtil
    {
        #region MD5
        /// <summary>
        /// 从字符串获取MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = Encoding.UTF8.GetBytes(str);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToUpper();
        }

        /// <summary>
        /// 从文件中获取MD5
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string MD5File(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString().ToUpper();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        #endregion

        #region 压缩
        // 字符串会转换成utf8存储
        public static byte[] GZCompressToBytes(string content)
        {
            return GZCompressToBytes(Encoding.UTF8.GetBytes(content));
        }

        // 假定字符串存储是utf8格式
        public static string GZDecompressToString(byte[] data)
        {
            return Encoding.UTF8.GetString(GZDecompressToBytes(data));
        }

        public static byte[] GZCompressToBytes(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return GZCompressToBytes(ms);
            }
        }

        public static byte[] GZCompressToBytes(Stream inStream)
        {
            var archive = GZipArchive.Create();
            archive.AddEntry("content", inStream, false);
            MemoryStream ms = new MemoryStream();
            archive.SaveTo(ms, new WriterOptions(SharpCompress.Common.CompressionType.Deflate));
            return ms.ToArray();
        }

        public static byte[] GZDecompressToBytes(byte[] data)
        {
            using (MemoryStream ms = GZDecompressToMemoryStream(data))
            {
                return ms.ToArray();
            }
        }

        static MemoryStream GZDecompressToMemoryStream(byte[] data)
        {
            using (MemoryStream inMs = new MemoryStream(data))
            {
                var archive = GZipArchive.Open(inMs);
                var entry = archive.Entries.First();
                MemoryStream ms = new MemoryStream();
                entry.WriteTo(ms);
                ms.Position = 0;
                return ms;
            }
        }
        #endregion

        #region hash
        public static string Hash(string input)
        {
            return Hash(Encoding.UTF8.GetBytes(input));
        }

        static string HashToString(byte[] hash)
        {
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static string Hash(byte[] input)
        {
            var hash = (new SHA1Managed()).ComputeHash(input);
            return HashToString(hash);
        }

        public static string HashFile(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            string hash = HashStream(fs);
            fs.Close();
            return hash;
        }

        internal static string HashStream(Stream s)
        {
            return HashToString((new SHA1Managed()).ComputeHash(s));
        }
        #endregion
    }
}