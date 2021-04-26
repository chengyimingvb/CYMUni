using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    [ExecuteInEditMode,HideMonoScript]
    public class BaseSceneRoot : BaseMono
    {
        #region inspector
        [FoldoutGroup("Active"), SerializeField, SceneObjectsOnly]
        protected GameObject[] EnableOnPlay;
        [FoldoutGroup("Active"), SerializeField, SceneObjectsOnly]
        protected GameObject[] DisableOnPlay;

        [FoldoutGroup("Root"), SerializeField, SceneObjectsOnly]
        public Transform RootSound;
        [FoldoutGroup("Root"), SerializeField, SceneObjectsOnly]
        public Transform RootAnimal;
        [FoldoutGroup("Root"), SerializeField, SceneObjectsOnly]
        public Transform RootPoints;
        #endregion

        #region prop
        public static BaseSceneRoot Ins { get; protected set; }
        public static Terrain ActiveTerrain => Terrain.activeTerrain;
        public List<Transform> Points { get; private set; } = new List<Transform>();
        protected Dictionary<string, Transform> PointsDic { get; private set; } = new Dictionary<string, Transform>();
        #endregion

        #region life
        public override void Awake()
        {
            Ins = this;
            base.Awake();
            Parse();
        }
        #endregion

        #region set
        [Button("Parse")]
        protected virtual void Parse()
        {
            if (Application.isPlaying)
            {
                if (EnableOnPlay != null)
                {
                    foreach (var item in EnableOnPlay)
                    {
                        if (item == null)
                            continue;
                        item.SetActive(true);
                    }
                }
                if (DisableOnPlay != null)
                {
                    foreach (var item in DisableOnPlay)
                    {
                        if (item == null)
                            continue;
                        item.SetActive(false);
                    }
                }
            }
            if (RootPoints != null)
            {
                Points.Clear();
                PointsDic.Clear();
                Points.AddRange(RootPoints.GetComponentsInChildren<Transform>());
                if (Points.Count > 0) Points.RemoveAt(0);
                foreach (var item in Points)
                {
                    if (PointsDic.ContainsKey(item.name)) continue;
                    PointsDic.Add(item.name, item);
                }
            }
        }
        #endregion

        #region get
        public Vector3 GetInterpolatedNormal(int x, int z)
        {
            if (ActiveTerrain == null)
                throw new Exception("ActiveTerrain == null");
            return ActiveTerrain.terrainData.GetInterpolatedNormal(x, z);
        }
        /// <summary>
        /// 获得出身点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Transform GetPoint(int index = 0)
        {
            if (Points.Count <= 0) return null;
            if (Points.Count <= index) return Points[0];
            return Points[index];
        }
        /// <summary>
        /// 获得位置点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Transform GetPoint(string name)
        {
            if (!PointsDic.ContainsKey(name)) return null;
            return PointsDic[name];
        }
        /// <summary>
        /// 获得高度
        /// </summary>
        /// <returns></returns>
        public float GetHeight(float x, float z)
        {
            if (ActiveTerrain == null) return 0;
            if (ActiveTerrain.terrainData == null) return 0;
            return ActiveTerrain.terrainData.GetHeight((int)x, (int)z) + ActiveTerrain.transform.position.y;
        }
        public float GetAbsHeight(float x, float z)
        {
            if (ActiveTerrain == null) return 0;
            if (ActiveTerrain.terrainData == null) return 0;
            return ActiveTerrain.terrainData.GetHeight((int)x, (int)z);
        }
        public float SampleHeight(Vector3 point)
        {
            if (ActiveTerrain == null) return 0;
            if (ActiveTerrain.terrainData == null) return 0;
            return ActiveTerrain.SampleHeight(point) + ActiveTerrain.transform.position.y;
        }
        #endregion

    }

}