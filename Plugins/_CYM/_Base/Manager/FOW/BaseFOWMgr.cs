//------------------------------------------------------------------------------
// BaseFOWMgr.cs
// Copyright 2018 2018/11/11 
// Created by CYM on 2018/11/11
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System;
namespace CYM
{
    public class BaseFOWMgr : BaseGFlowMgr
    {
        #region prop
        //private FogOfWarTeam FOW { get; set; }
        public bool IsFogShow { get; private set; } = true;
        public HashList<BaseFOWRevealerMgr> FOWRevealerList { get; private set; } = new HashList<BaseFOWRevealerMgr>();
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            //FOW = SelfMono.SetupMonoBehaviour<FogOfWarTeam>();
            EnableFOW(false);
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            EnableFOW(true);
            //FOW.SetAll(255);
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            SetDirty();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            //EnableFOW(false);
        }
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in FOWRevealerList)
            {
                item.Refresh();
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 设置所有
        /// </summary>
        /// <param name="val"></param>
        public virtual void SetAll(byte val)
        {
            throw new NotImplementedException();
            //FOW.SetAll(val);
        }
        public virtual void EnableFOW(bool b)
        {
            throw new NotImplementedException();
            //FOW.enabled = b;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否在迷雾内
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="minFog"></param>
        /// <returns></returns>
        public bool IsInFog(Vector3 pos, byte minFog = 70)
        {
            throw new NotImplementedException();
            //return FOW.GetFogValue(pos) > minFog;
        }
        public bool IsInFog(Vector3 pos)
        {
            throw new NotImplementedException();
            //return FOW.GetFogValue(pos) > 0;
        }
        #endregion
    }
}