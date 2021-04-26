
using System;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    [Serializable]
    public class TDBaseBattleData : TDBaseData
    {
        #region mgr
        IBaseSettingsMgr SettingsMgr => BaseGlobal.SettingsMgr;
        DBBaseSettings BaseSettingData => SettingsMgr.GetBaseSettings();
        #endregion

        public string SceneName { get; set; } = Const.STR_Inv;
        public virtual string GetSceneName()
        {
            var ret = SceneName;
            if (ret.IsInv())
                ret = TDID.Replace(Const.Prefix_Battle, "");
            if (BaseSettingData.IsSimpleTerrin)
                ret = ret + Const.Suffix_Simple;
            return ret;
        }
        public string GetRawSceneName()
        {
            var ret = SceneName;
            if (ret.IsInv())
                ret = TDID.Replace(Const.Prefix_Battle, "");
            return ret;
        }

    }
}
