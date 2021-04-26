//**********************************************
// Class Name	: CYMConstans
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public partial class Const:SysConst
    {
        #region config
        public const int Val_MaxTalkOptionCount = 4;
        public static string NameSpace => BuildConfig.Ins.NameSpace;
        #endregion

        #region Person tag
        public const string PTag_Normal = "N";
        public const string PTag_Rope = "Rope";
        #endregion

        #region ucd
        public const string UCD_NoBreakingSpace = "\u00A0";
        #endregion

        #region extention
        public static readonly string Extention_Save = ".dbs";
        #endregion

        #region res
        public const string RES_FlagEmpty = "Nation_Empty";
        public const string RES_IconEmpty = "Attr_Empty";
        #endregion

        #region scene
        public const string SCE_Start = "Start";
        public const string SCE_Preview = "Preview";
        public const string SCE_Test = "Test";
        #endregion
    }

}

