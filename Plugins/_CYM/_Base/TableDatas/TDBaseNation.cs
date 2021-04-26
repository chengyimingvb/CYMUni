//------------------------------------------------------------------------------
// TDBaseNation.cs
// Copyright 2019 2019/5/16 
// Created by CYM on 2019/5/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Serializable]
    public class TDBaseNation<TType> : TDBaseData where TType : Enum
    {
        #region prop
        DBBaseSettings Settings => BaseGlobal.SettingsMgr.GetBaseSettings();
        public string Capital
        {
            get
            {
                if (Castles == null || Castles.Count == 0)
                    return Const.STR_Inv;
                return Castles[0];
            }
        }
        public Color NationColor { get; private set; }
        #endregion

        #region lua config
        public string Flag { get; set; } = "";
        //颜色
        public string Color { get; set; } = "bbb004";
        public string LastName { get; set; } = "S_姬";
        //城市,第一位为首都
        public List<string> Castles { get; set; } = new List<string>();
        //特质
        public List<string> Traits { get; set; } = new List<string>();
        //属性
        public Dictionary<TType, float> Attrs { get; set; } = new Dictionary<TType, float>();
        #endregion

        #region get
        string GetFlagID()
        {
            string flagID = Flag;
            if (flagID.IsInv()) flagID = TDID;
            return flagID;
        }
        public Sprite GetFlag()
        {
            string flagID = GetFlagID();
            Sprite ret = null;
            if (ret == null)
                ret = GRMgr.Flag.Get(flagID, false);
            if (ret == null)
                ret = GRMgr.Flag.Get(Const.RES_FlagEmpty, false);
            return ret;
        }
        public Sprite GetFlagCircle()
        {
            string flagID = GetFlagID();
            Sprite ret = null;
            if (ret == null)
                ret = GRMgr.Flag.Get(flagID + Const.Suffix_FlagCircle, false);
            if (ret == null)
                ret = GRMgr.Flag.Get(Const.RES_FlagEmpty + Const.Suffix_FlagCircle, false);
            return ret;
        }
        public Sprite GetFlagSquare()
        {
            string flagID = GetFlagID();
            Sprite ret = null;
            if (ret == null)
                ret = GRMgr.Flag.Get(flagID + Const.Suffix_FlagSquare, false);
            if (ret == null)
                ret = GRMgr.Flag.Get(Const.RES_FlagEmpty + Const.Suffix_FlagSquare, false);
            return ret;
        }
        #endregion

        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            NationColor = UIUtil.FromHex(Color);
        }
    }
}