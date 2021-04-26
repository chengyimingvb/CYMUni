//------------------------------------------------------------------------------
// UnitLOD.cs
// Copyright 2019 2019/10/24 
// Created by CYM on 2019/10/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public enum LODType
    {
        Custom,
        Height,
    }
    public class UnitLOD : MonoBehaviour
    {
        public LODType LODType = LODType.Custom;
        public int curLod = 0;
        public float[] LODHeight;
        public Mesh[] LODMeshs;
        SkinnedMeshRenderer skin;
        bool isDis = false;

        BaseCameraMgr CameraMgr => BaseGlobal.CameraMgr;
        void Awake()
        {
            skin = GetComponentInChildren<SkinnedMeshRenderer>();
        }
        private void Update()
        {
            if (skin != null && CameraMgr != null)
            {
                if (LODType == LODType.Height)
                {
                    if (LODHeight == null || LODHeight.Length <= 0)
                        return;
                    float hight = CameraMgr.CameraHight;
                    if (LODHeight.Length >= 1 && hight <= LODHeight[0])
                        SetLod(0);
                    else if (LODHeight.Length >= 2 && hight <= LODHeight[1])
                        SetLod(1);
                    else if (LODHeight.Length >= 3 && hight <= LODHeight[2])
                        SetLod(2);
                    else
                        DisLod();
                }
            }
        }
        public void SetLod(int level)
        {
            if (curLod == level)
                return;
            if (curLod >= LODMeshs.Length || curLod < 0)
                curLod = 0;
            curLod = level;
            skin.sharedMesh = LODMeshs[level];
        }
        public void DisLod()
        {
            if (curLod == -1) return;
            curLod = -1;
            skin.sharedMesh = null;
        }
    }
}