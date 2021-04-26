//------------------------------------------------------------------------------
// AdjTransToTerrain.cs
// Copyright 2019 2019/10/23 
// Created by CYM on 2019/10/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [ExecuteInEditMode]
    public class AdjTransToTerrain : MonoBehaviour
    {
        List<Transform> Childs = new List<Transform>();
        BaseSceneRoot SceneObj => BaseSceneRoot.Ins;
        void Awake()
        {

        }
        private void Start()
        {
            Parse();
        }
        [Button("Parse")]
        void Parse()
        {
            Childs.Clear();
            for (int i = 0; i < transform.childCount; ++i)
            {
                var item = transform.GetChild(i);
                Childs.Add(item);
                item.position = item.position.SetY(SampleHeight(item.position));

            }

            float SampleHeight(Vector3 point)
            {
                if (Terrain.activeTerrain == null)
                    return 0;
                if (Terrain.activeTerrain.terrainData == null)
                    return 0;
                return Terrain.activeTerrain.SampleHeight(point) + Terrain.activeTerrain.transform.position.y;
            }
        }
    }
}