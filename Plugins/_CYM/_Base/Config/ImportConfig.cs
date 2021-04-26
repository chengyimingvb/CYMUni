using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace CYM
{
    public enum SpriteDirRoot
    { 
        Bundle,
        Art,
    }
    public class TextureImportSettings
    {
        public List<string> Directory = new List<string>();
        public SpriteDirRoot DirRoot = SpriteDirRoot.Bundle;
        public bool CrunchedCompression = false;
        public int CompressionQuality = 100;
        public string PackingTag = "";
        public FilterMode FilterMode = FilterMode.Bilinear;
#if UNITY_EDITOR
        public TextureImporterType TextureImporterType = TextureImporterType.Default;
        public TextureImporterCompression TextureCompression = TextureImporterCompression.Uncompressed;
#endif
        public bool IsContainInDirectoryTag(string path)
        {
            HashSet<string> split = new HashSet<string>( path.Split('/'));
            if (DirRoot == SpriteDirRoot.Bundle && !split.Contains(SysConst.Dir_Bundles))
            {
                return false;
            }
            if (DirRoot == SpriteDirRoot.Art && !split.Contains(SysConst.Dir_Art))
            {
                return false;
            }

            bool ret = true;
            foreach (var item in Directory)
            {
                if (!split.Contains(item))
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }
    }
    public class AudioImportSettings
    {
        public List<string> Path = new List<string>();
        public AudioClipLoadType LoadType = AudioClipLoadType.DecompressOnLoad;
        public AudioCompressionFormat CompressionFormat = AudioCompressionFormat.Vorbis;
        public float Quality = 0.01f;
    }

    public sealed class ImportConfig : ScriptableObjectConfig<ImportConfig>
    {
        [FoldoutGroup("Textures")]
        public List<TextureImportSettings> Textures = new List<TextureImportSettings>();
        [FoldoutGroup("Textures")]
        public HashList<string> TexturesExclusiveSuffix = new HashList<string>();

        [FoldoutGroup("Audios")]
        public List<AudioImportSettings> Audios = new List<AudioImportSettings>();
        [FoldoutGroup("Audios")]
        public HashList<string> AudiosExclusiveSuffix = new HashList<string>();

        public override void OnCreate()
        {
            base.OnCreate();
        }

    }
}
