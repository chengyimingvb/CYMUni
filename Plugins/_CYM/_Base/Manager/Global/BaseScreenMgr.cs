using System;
using System.Collections.Generic;
//**********************************************
// Class Name	: CYMBaseScreenController
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class BaseScreenMgr<TUnit, TData> : BaseGFlowMgr, IBaseScreenMgr 
        where TUnit : BaseUnit 
        where TData : TDBaseData
    {
        #region Callback val
        /// <summary>
        /// 设置玩家的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public event Callback<TUnit, TUnit> Callback_OnSetPlayer;
        public event Callback<BaseUnit, BaseUnit> Callback_OnSetPlayerBase;
        // 本地玩家死亡
        public event Callback Callback_OnPlayerRealDeath;
        #endregion

        #region property
        protected IBaseDBMgr DBMgr => BaseGlobal.DBMgr;
        protected BaseInputMgr InputMgr => BaseGlobal.InputMgr;
        // 当前的玩家
        public TUnit Player { get; private set; }
        // 老玩家
        public TUnit PrePlayer { get; private set; }
        public BaseUnit BaseLocalPlayer { get; private set; }
        public BaseUnit BasePrePlayer { get; private set; }
        public BaseUnit TempPlayer { get; private set; }
        #endregion

        #region life
        public virtual BaseUnit GetUnit(string id) => BaseGlobal.GetUnit<TUnit>(id);
        public virtual BaseUnit GetUnit(int id) => BaseGlobal.GetUnit<TUnit>(id);
        #endregion

        #region Select chara
        // 可选择的对象
        public List<TData> SelectCharasItems = new List<TData>();
        // 选择的ID
        public string SelectedChara { get; protected set; }
        public TData SelectedCharaData { get; private set; }
        // 选择chara
        public virtual void SelectChara(TData data)
        {
            SelectedCharaData = data;
            SelectedChara = SelectedCharaData.TDID;
        }
        // 选择chara 通过index
        public void SelectChara(int index)
        {
            SelectChara(SelectCharasItems[index]);
        }
        // 选择chara 通过 tdid
        public void SelectChara(string tdid)
        {
            foreach (var item in SelectCharasItems)
            {
                if (tdid == item.TDID)
                {
                    SelectedCharaData = item;
                    break;
                }
            }
            SelectedChara = tdid;
        }
        // 随机选择
        public void RandSelectChara()
        {
            SelectChara(RandUtil.RandArray(SelectCharasItems));
        }
        #endregion

        #region set
        // 设置玩家
        // 默认会在OnBattleLoaded设置Player 
        public virtual void SetPlayer(TUnit unit, bool isSystem = false)
        {
            Player?.OnUnBeSetPlayer();
            PrePlayer = Player;
            BasePrePlayer = Player;
            Player = unit;
            BaseLocalPlayer = unit;
            Callback_OnSetPlayer?.Invoke(PrePlayer, Player);
            Callback_OnSetPlayerBase?.Invoke(PrePlayer, Player);

            if (PrePlayer != null)
                PrePlayer.Callback_OnRealDeath -= OnPlayerRealDeath;
            if (Player != null)
                Player.Callback_OnRealDeath += OnPlayerRealDeath;

            Player?.OnBeSetPlayer();
        }
        #endregion

        #region get
        protected virtual void LoadSelectItems(ref List<TData> items)
        {
            throw new NotImplementedException("此函数必须被实现");
        }
        #endregion

        #region is
        public bool IsPlayer(TUnit target)
        {
            return Player == target;
        }
        #endregion

        #region Callback

        protected virtual void OnPlayerRealDeath()
        {
            Callback_OnPlayerRealDeath?.Invoke();
        }
        protected override void OnAllLoadEnd1()
        {
            LoadSelectItems(ref SelectCharasItems);
            //创建一个临时的Player对象
            if (TempPlayer == null)
            {
                TempPlayer = Util.CreateGlobalObj<TUnit>("TempLocalPlayer");
            }
            //设置默认的Player
            if (Player == null)
            {
                Player = TempPlayer as TUnit;
                BaseLocalPlayer = Player;
            }
        }
        #endregion

        #region tempPlayer Callback
        public override void OnLoginInit1(object data)
        {
            base.OnLoginInit1(data);
            TempPlayer?.OnLoginInit1(data);
        }
        public override void OnLoginInit2(object data)
        {
            base.OnLoginInit2(data);
            TempPlayer?.OnLoginInit2(data);
        }
        public override void OnLoginInit3(object data)
        {
            base.OnLoginInit3(data);
            TempPlayer?.OnLoginInit3(data);
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            TempPlayer?.OnGameStart1();
            var player = GetUnit(DBMgr.CurBaseGameData.PlayerID) as TUnit;
            if (player != null)
                SetPlayer(player, true);
        }
        public override void OnGameStart2()
        {
            base.OnGameStart2();
            TempPlayer?.OnGameStart2();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            TempPlayer?.OnGameStarted1();
        }
        public override void OnGameStarted2()
        {
            base.OnGameStarted2();
            TempPlayer?.OnGameStarted2();
        }
        public override void OnGameStartOver()
        {
            base.OnGameStartOver();
            TempPlayer?.OnGameStartOver();
        }
        public override void OnRead1<TDBData>(TDBData data)
        {
            base.OnRead1(data);
            TempPlayer?.OnRead1(data);
        }
        public override void OnRead2<TDBData>(TDBData data)
        {
            base.OnRead2(data);
            TempPlayer?.OnRead2(data);
        }
        public override void OnRead3<TDBData>(TDBData data)
        {
            base.OnRead3(data);
            TempPlayer?.OnRead3(data);
        }
        public override void OnReadEnd<TDBData>(TDBData data)
        {
            base.OnReadEnd(data);
            TempPlayer?.OnReadEnd(data);
        }
        public override void OnWrite<TDBData>(TDBData data)
        {
            base.OnWrite(data);
            TempPlayer?.OnWrite(data);
        }
        #endregion
    }
}
