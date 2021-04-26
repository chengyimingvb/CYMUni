//------------------------------------------------------------------------------
// GameConfig.cs
// Copyright 2018 2018/12/14 
// Created by CYM on 2018/12/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public sealed class GameConfig : ScriptableObjectConfig<GameConfig>
    {
        #region reference
        [SerializeField, FoldoutGroup("Reference")]
        Dictionary<string, Texture2D> RefTexture2D = new Dictionary<string, Texture2D>();
        [SerializeField, FoldoutGroup("Reference")]
        Dictionary<string, GameObject> RefGameObject = new Dictionary<string, GameObject>();
        [SerializeField, FoldoutGroup("Reference")]
        Dictionary<string, AnimationCurve> RefAnimationCurve = new Dictionary<string, AnimationCurve>();
        #endregion

        #region url
        [FoldoutGroup("URL")]
        public string URLWiki = "https://www.baidu.com/";
        [FoldoutGroup("URL")]
        public string URLCommunity = "https://www.baidu.com/";
        [FoldoutGroup("URL")]
        public string URLWebsite = "https://www.baidu.com/";
        #endregion

        #region game
        [FoldoutGroup("Steam")]
        public uint SteamAppID;
        [FoldoutGroup("Steam")]
        public string SteamWebAPI;
        #endregion

        #region ref
        public Texture2D GetTexture2D(string id)
        {
            if (RefTexture2D.ContainsKey(id)) return RefTexture2D[id];
            return null;
        }
        public GameObject GetGameObject(string id)
        {
            if (RefGameObject.ContainsKey(id)) return RefGameObject[id];
            return null;
        }
        public AnimationCurve GetAnimationCurve(string id)
        {
            if (RefAnimationCurve.ContainsKey(id)) return RefAnimationCurve[id];
            return null;
        }
        #endregion
    }
}