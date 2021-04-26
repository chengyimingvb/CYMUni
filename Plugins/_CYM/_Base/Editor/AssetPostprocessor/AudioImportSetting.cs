//------------------------------------------------------------------------------
// AudioImportSetting.cs
// Copyright 2018 2018/3/23 
// Created by CYM on 2018/3/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEditor;

namespace CYM
{
    public class AudioImportSetting : AssetPostprocessor
    {
        ImportConfig Config => ImportConfig.Ins;

        void OnPreprocessAudio()
        {
            //特殊路径跳过
            if (assetImporter.assetPath.Contains("Plugins"))
                return;
            if (Config.AudiosExclusiveSuffix.Contains(assetImporter.name))
                return;

            //自定义配置处理
            AudioImporter importer = (AudioImporter)assetImporter;
            bool isProcessed = false;
            foreach (var item in Config.Audios)
            {
                foreach (var path in item.Path)
                {
                    if (assetImporter.assetPath.Contains(path))
                    {
                        importer.defaultSampleSettings = new AudioImporterSampleSettings
                        {
                            loadType = item.LoadType,
                            compressionFormat = item.CompressionFormat,
                            quality = item.Quality,

                        };
                        isProcessed = true;
                    }
                }
            }

            //默认处理
            if (!isProcessed)
            {
            }

        }
    }
}