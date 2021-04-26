//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public enum GameNetMode
    {
        PVP,
        PVE,
    }
    public enum GamePlayStateType
    {
        NewGame,//新游戏
        LoadGame,//加载游戏
    }

    [Serializable]
    public class DBBaseGame
    {
        public string PlayerID = Const.STR_Inv;
        public string BattleID = Const.STR_Inv;
        public int PlayTime = 0;
        public int LoadBattleCount = 0;
        public int TurnCount = 0;
        public GameNetMode GameNetMode = GameNetMode.PVE;
        public GamePlayStateType GamePlayStateType = GamePlayStateType.NewGame;
        public bool IsNewGame() => GamePlayStateType == GamePlayStateType.NewGame;
        public bool IsLoadGame() => GamePlayStateType == GamePlayStateType.LoadGame;
        public bool IsFirstLoadBattle() => LoadBattleCount == 1;
    }

    #region base
    [Serializable]
    public class DBBase
    {
        public long ID;
        public string TDID;
        public string CustomName;
    }
    [Serializable]
    public class DBBaseUnit : DBBase
    {
        public Vec3 Position = new Vec3(new Vector3(99999.0f, 99999.0f, 99999.0f));
        public Qua Rotation = new Qua(Quaternion.identity);
        public bool IsNewAdd = false;
    }
    [Serializable]
    public class DBBaseBuff : DBBase
    {
        public float CD = 0;
        public float Input;
        public bool Valid = true;
    }
    [Serializable]
    public class DBBaseWar : DBBase
    {
        public List<string> Attackers = new List<string>();
        public List<string> Defensers = new List<string>();
        public HashList<string> AllowOccupy = new HashList<string>();
        public int WarDay = 0;
        public float AttackersWarPoint;
        public float DefensersWarPoint;
    }
    [Serializable]
    public class DBBaseAlert : DBBase
    {
        public float CurTurn;
        public bool IsCommingTimeOutFalg;
        public long Cast;
        public string TipStr;
        public string DetailStr;
        public string TitleStr;
        public string Illustration;
        public AlertType Type = AlertType.Continue;
        public List<long> SelfArticle = new List<long>();
        public List<long> TargetArticle = new List<long>();
        public long War = Const.INT_Inv;
        public string StartSFX;
        public bool IsAutoTrigger;
        public string Bg;
        public string Icon;
    }
    [Serializable]
    public class DBBaseArticle : DBBase
    {
        public long Self;
        public long Target;

        public float Float1;
        public float Float2;
        public float Float3;
        public int Int1;
        public int Int2;
        public int Int3;
        public string Str1;
        public string Str2;
        public string Str3;
        public bool Bool1;
        public bool Bool2;
        public bool Bool3;
        public long Long1;
        public long Long2;
        public long Long3;

        public ArticleObjType ArticleObjType = ArticleObjType.Self;
    }
    [Serializable]
    public class DBBaseEvent : DBBase
    {
        public int CD;
    }
    [Serializable]
    public class DBBaseTransact : DBBase
    {
        public string Type;
        public int CD;
        public long Self;
        public long Target;
        public float Value;
    }
    [Serializable]
    public class DBBaseLegionStationed : DBBase
    {
        public long DefendCastle = Const.INT_Inv;
        public long PreDefendCastle = Const.INT_Inv;
        public long SiegeCastle = Const.INT_Inv;
        public long PreSiegeCastle = Const.INT_Inv;
    }
    [Serializable]
    public class DBBaseCastleStationed : DBBase
    {
        public long DefendLegion = Const.INT_Inv;
        public long PreDefendLegion = Const.INT_Inv;
        public List<long> AttackLegion = new List<long>();
    }
    [Serializable]
    public class DBBaseLoan : DBBase
    {
        public List<LoanData> Loan = new List<LoanData>();
        public float CurLoan = 0;
    }
    [Serializable]
    public class DBBaseTargetMarker : DBBase
    {
        public long Unit = Const.INT_Inv;
        public long Target = Const.INT_Inv;
        public float CD =Const.FLOAT_Inv;
    }
    [Serializable]
    public class DBBaseNarration : DBBase
    {
        public HashList<string> Showed = new HashList<string>();
    }
    #endregion
}