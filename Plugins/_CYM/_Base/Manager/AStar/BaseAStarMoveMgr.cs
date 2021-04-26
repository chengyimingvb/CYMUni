//------------------------------------------------------------------------------
// BaseMoveMgr.cs
// Copyright 2019 2019/4/17 
// Created by CYM on 2019/4/17
// Owner: CYM
// 单位身上的移动组件基类
//------------------------------------------------------------------------------

using CYM.AI.USM;
using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Serializable]
    public class DBBaseMove
    {
        public int CurMoveState = 0;
        public int MoveTargetState = 0;
        public long MoveTarget = Const.INT_Inv;
        public long FaceTarget = Const.INT_Inv;
        public Vec3 MoveTargetPosPreview;
        public Vec3 MoveTargetPosReal;
        public bool IsValidMoveTarget;
    }
    public class BaseTraversal : ITraversalProvider
    {
        protected BaseAStarMgr AStarPathMgr => BaseGlobal.AStarMgr;
        protected BaseUnit SelfUnit { get; private set; }

        public BaseTraversal()
        {

        }
        public void Init(BaseUnit legion)
        {
            SelfUnit = legion;
        }

        public bool CanTraverse(Path path, GraphNode node)
        {
            return Filter(node);
        }

        public uint GetTraversalCost(Path path, GraphNode node)
        {
            return DefaultITraversalProvider.GetTraversalCost(path, node);
        }

        public virtual bool Filter(GraphNode node)
        {
            bool defaulRet = node.Walkable;
            int customRet = 0;
            HashList<BaseUnit> blockerUnits = AStarPathMgr.GetBlockerUnits(node);
            if (blockerUnits != null)
            {
                foreach (var item in blockerUnits)
                {
                    if (item != null)
                    {
                        if (!OnFilter(item))
                            customRet++;
                    }
                }
            }
            return customRet==0 && defaulRet;
        }
        protected virtual bool OnFilter(BaseUnit item)
        {
            return item.IsSOF(SelfUnit) || item == SelfUnit;
        }
    }

    public class NoModifier : MonoModifier
    {
        public override int Order => 0;

        public override void Apply(Path path)
        {
            
        }
    }
    public abstract class BaseAStarMoveMgr<TState, TUnit, TTraversal, TModify> : BaseMgr, IBaseMoveMgr, IBaseAStarMoveMgr
        where TState : struct, Enum
        where TUnit : BaseUnit
        where TTraversal : BaseTraversal, new()
        where TModify : MonoModifier
    {
        #region Callback val
        public Callback Callback_OnMoveStart { get; set; }
        public Callback Callback_OnMovingAlone { get; set; }
        public Callback Callback_OnFirstMovingAlone { get; set; }
        public Callback Callback_OnMoveEnd { get; set; }
        public Callback Callback_OnMoveDestination { get; set; }
        #endregion

        #region val
        public BaseUnit FaceTarget { get; protected set; }
        public float SearchedSpeed { get; protected set; }
        public Vector3 SearchedPos { get; protected set; } = Const.VEC_FarawayPos;
        public Vector3 Destination { get; protected set; } = Const.VEC_FarawayPos;
        public float BaseMoveSpeed { get; private set; } = 10.0f;
        public float BaseRotSpeed { get; private set; } = 0.5f;
        public virtual float RealMoveSpeed => BaseMoveSpeed * BaseAStarMgr.MultipleSpeed;
        public virtual float RealRotSpeed=>BaseRotSpeed * BaseAStarMgr.MultipleSpeed;
        public int CurIndex { get; protected set; }
        Vector3 LastPos;
        Quaternion LastRot;
        #endregion

        #region Astar
        public GraphNode PreNode { get; protected set; }
        public GraphNode CurNode { get; protected set; }
        protected BaseNodeRoot RootBlockerNode;
        #endregion

        #region mgr
        protected Coroutineter BattleCoroutine => BaseGlobal.BattleCoroutineter;
        protected BaseAStarMgr AStarMgr => BaseGlobal.AStarMgr;
        protected CharaStateMachine<TState, TUnit, BaseMoveState> StateMachine { get; set; } = new CharaStateMachine<TState, TUnit, BaseMoveState>();
        #endregion

        #region prop
        protected TUnit SelfUnit => SelfBaseUnit as TUnit;
        protected ABPath ABPath;
        protected Seeker Seeker;
        protected Quaternion NewQuateration = Quaternion.identity;
        protected CoroutineHandle FollowPathCoroutine;
        public BaseTraversal Traversal { get;private set; }
        protected TModify PathModify { get; private set; }
        #endregion

        #region life
        protected virtual bool UseSeizeNode => true;
        protected virtual bool UseBlockNode => true;
        protected virtual bool UseFollowCoroutine => throw new NotImplementedException();
        public override MgrType MgrType => MgrType.Unit;
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            Traversal = new TTraversal();
            Traversal.Init(SelfBaseUnit);
            StateMachine.Init(SelfBaseUnit);
            Seeker = SelfMono.SetupMonoBehaviour<Seeker>();
            PathModify = SelfMono.SetupMonoBehaviour<TModify>();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            AStarMgr.Callback_OnSeizeNode += OnSeizeNode;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            AStarMgr.Callback_OnSeizeNode -= OnSeizeNode;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            RootBlockerNode = SelfMono.GetComponentInChildren<BaseNodeRoot>();
        }
        public override void OnStart()
        {
            base.OnStart();
            GrabNewQuateration();
        }
        public override void OnBirth()
        {
            base.OnBirth();
            IsLockRotationFlag = false;
        }
        public override void OnBirth3()
        {
            base.OnBirth3();
            CalcCurNode();
            CalcCurBlock();
        }
        public override void OnDeath()
        {
            base.OnDeath();
            ClearNode();
            ClearBlock();
            CancleMoveTarget();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (SelfBaseUnit != null)
            {
                //位置变化记录
                LastPos = SelfUnit.Pos;
                LastRot = SelfUnit.Rot;

                //执行旋转
                if (!IsLockRotation)
                {
                    if (!IsMoving)
                        SelfBaseUnit.Rot = Quaternion.Slerp(SelfBaseUnit.Rot, NewQuateration, Time.smoothDeltaTime * RealRotSpeed);
                    else
                        NewQuateration = SelfBaseUnit.Rot;
                }
                //使用携程
                if (UseFollowCoroutine) { }
                else
                {
                    OnFollowPathUpdate();
                }
                StateMachine?.OnUpdate();

                //位置变化记录
                IsPositionChange = !BaseMathUtil.Approximately(LastPos, SelfUnit.Pos);
                IsRotationChange = LastRot != SelfUnit.Rot;
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (SelfBaseUnit != null)
            {
                StateMachine?.OnFixedUpdate();
            }
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            SetState(StateMachine.CurState, false);
        }
        #endregion

        #region rotation
        public void LockRotation(bool b)
        {
            IsLockRotationFlag = b;
            GrabNewQuateration();
        }
        public void LookDir(Vector3 dir)
        {
            IsLockRotationFlag = false;
            dir.y = 0;
            var normalDir = dir.normalized;
            if (normalDir == Vector3.zero)
                return;
            NewQuateration = Quaternion.LookRotation(normalDir, Vector3.up);
        }
        public void Look(Vector3 pos)
        {
            if (pos == SelfBaseUnit.Pos)
                return;

            Vector3 dir = (pos- SelfBaseUnit.Pos);
            LookDir(dir);
        }
        public void Look(BaseUnit unit)
        {
            if (unit == null) return;
            Look(unit.Pos);
            FaceTarget = unit;
        }
        public void SetRotationY(float rot)
        {
            NewQuateration = Quaternion.AngleAxis(rot, Vector3.up);
        }
        public void GrabNewQuateration(Quaternion? qua = null)
        {
            if (qua != null)
                SelfBaseUnit.Rot = qua.Value;
            NewQuateration = SelfBaseUnit.Rot;
        }
        public void RandRotationY()
        {
            RandUtil.RandForwardY(SelfBaseUnit);
            GrabNewQuateration();
        }
        #endregion

        #region move
        public bool MoveToPos(Vector3 pos, float speed)
        {
            IsCanUnitOverlap = false;
            return StartPath(pos, speed);
        }
        // 移动到指定节点
        public bool MoveIntoNode(GraphNode node, float speed)
        {
            if (node == null) return false;
            IsCanUnitOverlap = false;
            return StartPath((Vector3)node.position, speed);
        }
        // 移动到指定单位
        public bool MoveIntoUnit(BaseUnit unit, float speed)
        {
            if (unit == null) return false;
            if (unit.Pos.IsInv()) return false;
            if (unit.MoveMgr == null) return false;
            var node = unit.AStarMoveMgr.CurNode;
            if (node == null) return false;
            IsCanUnitOverlap = true;
            return StartPath((Vector3)node.position, speed);
        }
        // 移动到指定单位边上
        public bool MoveToUnit(BaseUnit unit, float speed)
        {
            if (unit == null) return false;
            if (unit == SelfBaseUnit) return false;
            if (unit.Pos.IsInv()) return false;
            GraphNode closetNode = AStarMgr.GetClosedNode(SelfBaseUnit, unit.Pos);
            if (closetNode == null) return false;
            IsCanUnitOverlap = false;
            return StartPath((Vector3)closetNode.position, speed);
        }
        #endregion

        #region path
        public bool RePath()
        {
            if(IsMoving)
                return StartPath(SearchedPos, SearchedSpeed);
            return false;
        }
        public bool StartPath(Vector3 pos, float speed)
        {
            if (!IsCanMove) return false;
            if (pos.IsInv()) return false;
            SearchedPos = pos;
            SearchedSpeed = speed;
            if (MathUtil.Approximately(pos, SelfBaseUnit.Pos))
                return false;
            Destination = OnModifyFinalPos(pos);
            if (MathUtil.Approximately(Destination, SelfBaseUnit.Pos))
                return false;
            ABPath = ABPath.Construct(SelfBaseUnit.Pos, Destination);
            ABPath.traversalProvider = Traversal;
            //if (traversalProvider != null) ABPath.traversalProvider = traversalProvider;
            Seeker.StartPath(ABPath, (path) => OnPathFindCompleted(path, speed));
            return true;
        }
        public void StopPath()
        {
            OnMoveEnd();
        }
        public void PreviewPath(Vector3 pos)
        {
            ABPath ABPath;
            if (pos.IsInv()) return;
            if (pos == SelfBaseUnit.Pos) return;
            pos = OnModifyFinalPos(pos);
            ABPath = ABPath.Construct(SelfBaseUnit.Pos, pos);
            ABPath.traversalProvider = Traversal;
            Seeker.StartPath(ABPath);
            AstarPath.BlockUntilCalculated(ABPath);
            if (ABPath.error)
            {
                CLog.Error(ABPath.errorLog);
            }
        }
        public void ShowPath(bool b)
        {
            if (!b)
            {
                return;
            }
            if (!SelfBaseUnit.IsPlayer()) return;
            if (!IsHaveMoveTarget()) return;
            var state = StateMachine.GetState(MoveTarget_State);
            var curState = StateMachine.CurState;
            if (!IsMoveTargetState(curState))
            {
                Vector3 realPoint = Vector3.zero;
                if (MoveTarget_PosPreview == Vector3.zero) return;
                if (MoveTarget_PosPreview.IsInv()) return;
                realPoint = (Vector3)AStarMgr.GetDistNode(SelfBaseUnit, MoveTarget_PosPreview, false, true, false, false, false).position;
                PreviewPath(realPoint);
            }
        }
        void OnPathFindCompleted(Path p, float speed)
        {
            ABPath = p as ABPath;
            //无路可走,直接返回,不要报错
            if (ABPath.vectorPath.Count == 0)
            {
                StopPath();
                OnPathNoWay();
                return;
            }
            if (ABPath.error)
            {
                CLog.Error($"Name:{SelfBaseUnit.GetName()},Pos:{SelfBaseUnit.Pos},SearchedPos:{SearchedPos},\n{ABPath.errorLog}");
                StopPath();
                OnPathError();
                return;
            }
            SetMoveSpeed(speed);
            OnMoveStart();
        }
        #endregion

        #region set
        public virtual bool AdjToNode()
        {
            ClearNode();
            GraphNode node = AStarMgr.GetNode(SelfMono.Pos);
            if (node == null) return false;
            SelfMono.Pos = (Vector3)node.position;
            CalcCurNode();
            return true;
        }
        public void SetRotateSpeed(float speed)
        {
            BaseRotSpeed = speed;
        }
        public void SetMoveSpeed(float speed)
        {
            BaseMoveSpeed = speed;
        }
        public virtual void SetToNode(GraphNode node)
        {
            ClearBlock();
            ClearNode();
            SelfBaseUnit.Pos = (Vector3)node.position;
            CalcCurNode();
            CalcCurBlock();
        }
        public void ChangeState(TState state, bool isForce = false, bool isManual = true) => StateMachine.ChangeState(state, isForce, isManual);
        public void SetState(TState state, bool isManual = true) => StateMachine.SetCurState(state, isManual);
        #endregion

        #region get
        public List<Vector3> GetDetailPathPoints(ABPath path = null)
        {
            var realPath = path == null ? ABPath : path;
            List<Vector3> ret = new List<Vector3>();
            if (realPath == null) return ret;
            for (int i = 0; i < realPath.vectorPath.Count - 1; i++)
            {
                var p0 = realPath.vectorPath[i];
                var p1 = realPath.vectorPath[Mathf.Min(i + 1, realPath.vectorPath.Count - 1)];

                float step = 1f;
                int maxCount = 6;
                while (step < maxCount)
                {
                    var interpolatedPoint = Vector3.Lerp(p0, p1, step / maxCount);
                    ret.Add(interpolatedPoint);
                    step++;
                }
            }
            return ret;
        }
        protected virtual List<GraphNode> GetBlockerNode()
        {
            List<GraphNode> ret = new List<GraphNode>();
            if (CurNode != null) ret.Add(CurNode);
            if (RootBlockerNode != null)
            {
                RootBlockerNode.CalcNodes();
                ret.AddRange(RootBlockerNode.Nodes);
            }
            return ret;
        }
        #endregion

        #region is
        protected bool IsMovingFlag { get; set; } = false;
        protected bool IsCanUnitOverlap { get; set; } = false;
        public virtual bool IsLockRotation => IsLockRotationFlag;
        public virtual bool IsMoving => IsMovingFlag && IsPositionChange;
        public virtual bool IsCanMove => true;
        public bool IsInState(TState state) => Enum<TState>.Int(StateMachine.CurState) == Enum<TState>.Int(state);
        public bool IsCanTraversal(GraphNode node)
        {
            if (Traversal == null) return true;
            return Traversal.CanTraverse(null, node);
        }
        public bool IsLockRotationFlag { get; private set; } = false;
        public bool IsPositionChange { get; private set; } = false;
        public bool IsRotationChange { get; private set; } = false;
        public bool IsHaveMoveTarget() => MoveTarget_IsValid;
        public bool IsMoveTargetState(TState state) => MoveTargetStateDatas.ContainsKey(state);
        public bool IsCanAutoExcuteMoveTarget() => IsCanMove && IsHaveMoveTarget();
        //当前Unit的node是否可以连接到目标Unit的Blocker范围内,一般可以用来做攻击检测
        public bool IsInBlockerRange(BaseUnit unit)
        {
            HashList<GraphNode> connection = new HashList<GraphNode>();
            CurNode.GetConnections(connection.Add);
            HashList<GraphNode> targetUnitBlocker = AStarMgr.GetBlocker(unit);
            if (targetUnitBlocker != null)
            {
                foreach (var item in targetUnitBlocker)
                {
                    if (connection.Contains(item))
                        return true;
                }
            }
            else
            {
                CLog.Error("{0}:目标单位没有Blocker", unit.BaseConfig.GetName());
            }
            return false;
        }
        public bool IsInConnection(BaseUnit target)
        {
            //如果没有链接,就返回
            HashList<GraphNode> blockers = AStarMgr.GetBlocker(SelfBaseUnit);
            if (!AStarMgr.IsConnection(blockers, target.AStarMoveMgr.CurNode))
                return false;
            return true;
        }
        public bool IsInDestination()=> BaseMathUtil.Approximately(Destination, SelfUnit.Pos);
        public bool IsNoInDestination() => !IsInDestination();
        public bool IsInPos(Vector3 target,float k)=> BaseMathUtil.Approximately(SelfUnit.Pos, target, k);
        #endregion

        #region Cur Node
        private void CalcCurNode()
        {
            if (!UseSeizeNode)
                return;
            PreNode = CurNode;
            CurNode = AStarMgr.GetSafeNode(SelfBaseUnit.Pos);
            if (CurNode == null) CLog.Error("没有获取到寻路节点!!!{0}", SelfBaseUnit.GOName);
            else SelfBaseUnit.Pos = (Vector3)CurNode.position;
            AStarMgr.SetSeizeNode(CurNode, SelfBaseUnit);
        }
        private void ClearNode()
        {
            if (!UseSeizeNode)
                return;
            PreNode = CurNode;
            CurNode = AStarMgr.GetSafeNode(SelfBaseUnit.Pos);
            AStarMgr.ClearSeizeNode(CurNode, SelfBaseUnit);
        }
        private void CalcCurBlock()
        {
            if (!UseBlockNode)
                return;
            AStarMgr.SetBlockNode(SelfBaseUnit, GetBlockerNode());
        }
        private void ClearBlock()
        {
            if (!UseBlockNode)
                return;
            AStarMgr.SetBlockNode(SelfBaseUnit, null);
        }
        #endregion

        #region Callback
        protected virtual void OnPathNoWay()
        { 
        
        }
        protected virtual void OnPathError()
        {

        }
        protected virtual Vector3 OnModifyFinalPos(Vector3 pos)
        {
            return pos;
        }

        protected virtual void OnMoveStart()
        {
            BaseAStarMgr.GlobalMoveState.Add();
            StateMachine.CurStateData?.OnMoveStart();

            ClearNode();
            ClearBlock();

            GrabNewQuateration();
            Callback_OnMoveStart?.Invoke();
            if (UseFollowCoroutine)
            {
                BattleCoroutine.Kill(FollowPathCoroutine);
                FollowPathCoroutine = BattleCoroutine.Run(OnFollowPathCoroutine());
            }
        }
        protected virtual void OnMoveEnd()
        {
            BaseAStarMgr.GlobalMoveState.Remove();
            BaseAStarMgr.SpeedUp(1.0f);
            StateMachine.CurStateData?.OnMoveEnd();

            CalcCurNode();
            CalcCurBlock();

            GrabNewQuateration();
            ChangeToEndState();
            if (MoveTarget_IsInTarget())
                CancleMoveTarget();

            Callback_OnMoveEnd?.Invoke();
            if (UseFollowCoroutine)
                BattleCoroutine.Kill(FollowPathCoroutine);
        }
        protected virtual void OnSeizeNode(BaseUnit unit, GraphNode node)
        {

        }
        #endregion

        #region move target prop
        private HashSet<TState> MoveTargetStateToNode = new HashSet<TState>();
        private HashSet<TState> MoveTargetStateToUnit = new HashSet<TState>();
        //移动状态
        private Dictionary<TState, Tuple<TState, Func<bool>, Func<bool>>> MoveTargetStateDatas = new Dictionary<TState, Tuple<TState, Func<bool>, Func<bool>>>();
        //是否到达目标
        private bool MoveTarget_IsInTarget() => MathUtil.Approximately(SelfBaseUnit.Pos, MoveTarget_PosReal) || MoveTarget_Node == CurNode;
        //目标是否有效
        protected bool MoveTarget_IsValid { get; set; } = false;
        //期待的状态
        public TState MoveTarget_State { get; protected set; }
        //预览的目标位置
        public Vector3 MoveTarget_PosPreview { get; protected set; } = Vector3.zero;
        //期待的目标点
        public Vector3 MoveTarget_PosReal { get; protected set; } = Vector3.zero;
        public BaseUnit MoveTarget_Unit { get; protected set; }
        //目标节点
        public GraphNode MoveTarget_Node { get; protected set; }
        #endregion

        #region pub move target
        //执行MovaTarget,返回true:表示正在移动,返回false:表示移动完毕
        public bool ExcuteMoveTarget(bool isManual)
        {
            if (IsMoveTargetState(MoveTarget_State))
            {
                //自动执行的时候判断是否为默认状态(只有默认状态才能执行移动)
                if (!isManual && Enum<TState>.Int(StateMachine.CurState) != 0)
                {
                    CancleMoveTarget();
                    return false;
                }
                ChangeState(MoveTarget_State, true, true);
                //执行命令
                if (IsMoving)
                {
                    return true;
                }
                //执行移动后的Action
                else
                {
                    ChangeToEndState();
                    CancleMoveTarget();
                    return false;
                }
            }
            return false;
        }
        //设置MoveTarget
        public void SetMoveTarget(TState state, GraphNode node)
        {
            if (node == null) return;
            SetMoveTarget(state, null, node);
        }
        //设置MoveTarget
        public void SetMoveTarget(TState state, BaseUnit unit)
        {
            if (unit == null) return;
            SetMoveTarget(state, unit, null);
        }
        //取消移动目标
        public void CancleMoveTarget()
        {
            MoveTarget_IsValid = false;
            MoveTarget_State = Enum<TState>.Invert(0);
            MoveTarget_PosPreview = Vector3.zero;
            MoveTarget_PosReal = Vector3.zero;
            MoveTarget_Unit = null;
            MoveTarget_Node = null;
        }
        #endregion

        #region private move target
        protected void AddState(TState state, BaseMoveState stateData, TState? endState = null, Func<bool> isRange = null, Func<bool> isAction = null)
        {
            StateMachine.AddState(state, stateData);
            if (endState != null)
            {
                MoveTargetStateDatas.Add(state, new Tuple<TState, Func<bool>, Func<bool>>(endState.Value, isRange, isAction));
                if (isRange == null && isAction == null) MoveTargetStateToNode.Add(state);
                else MoveTargetStateToUnit.Add(state);
            }
        }
        private void SetMoveTarget(TState state, BaseUnit unit, GraphNode node)
        {
            //正确性判断
            if (unit != null && !MoveTargetStateToUnit.Contains(state))
            {
                CLog.Error("错误!SetMoveTarget,unit != null,但state确是:" + state.ToString());
                return;
            }
            else if (node != null && !MoveTargetStateToNode.Contains(state))
            {
                CLog.Error("错误!SetMoveTarget,node != null,但state确是:" + state.ToString());
                return;
            }
            //执行移动
            if (IsMoveTargetState(state))
            {
                MoveTarget_IsValid = true;
                MoveTarget_State = state;
                MoveTarget_Node = node;
                MoveTarget_Unit = unit;
                var endState = MoveTargetStateDatas[state].Item1;
                var isRange = MoveTargetStateDatas[state].Item2;
                var isAction = MoveTargetStateDatas[state].Item3;
                bool isInRange = isRange == null ? false : isRange.Invoke();
                bool isInAction = isAction == null ? false : isAction.Invoke();
                if (isInRange && isInAction)
                {
                    ChangeState(endState, true, true);
                }
                else if (IsCanMove && !isInRange)
                {
                    ExcuteMoveTarget(true);
                    SetMoveTargetPosReal(Destination);
                    SetMoveTargetPosPreview(null, null, Destination);
                }
                else
                {
                    SetMoveTargetPosPreview(unit, node, null);
                    ShowPath(true);
                }
            }
        }
        private void ChangeToEndState()
        {
            if (IsHaveMoveTarget())
            {
                var endState = MoveTargetStateDatas[MoveTarget_State].Item1;
                var isRange = MoveTargetStateDatas[MoveTarget_State].Item2;
                var isAction = MoveTargetStateDatas[MoveTarget_State].Item3;
                bool isInRange = isRange == null ? false : isRange.Invoke();
                bool isInAction = isAction == null ? false : isAction.Invoke();
                if (isInRange && isInAction)
                {
                    ChangeState(endState, true, true);
                }
                else
                {
                    ChangeState((TState)(object)0, true, true);
                }
            }
        }
        protected void SetMoveTargetPosPreview(BaseUnit unit, GraphNode node, Vector3? pos)
        {
            if (unit != null) MoveTarget_PosPreview = (unit.Pos);
            if (node != null) MoveTarget_PosPreview = ((Vector3)node.position);
            if (pos != null) MoveTarget_PosPreview = pos.Value;
        }
        protected void SetMoveTargetPosReal(Vector3 pos)
        {
            MoveTarget_PosReal = pos;
            MoveTarget_Node = AStarMgr.GetSafeNode(pos);
        }
        #endregion

        #region IEnumerator
        protected virtual IEnumerator<float> OnFollowPathCoroutine()
        {
            yield break;
        }

        protected virtual void OnFollowPathUpdate()
        {
            return;
        }
        #endregion

        #region state
        public class BaseMoveState : CharaState<TState, TUnit>
        {
            #region mgr
            protected IBaseMoveMgr MoveMgr => SelfUnit.MoveMgr;
            #endregion

            #region life
            public virtual Color Color => Color.black;
            protected virtual bool MustPlayerDrawPath => true;
            public override void OnBeAdded()
            {
                base.OnBeAdded();
            }
            public override void Enter()
            {
                base.Enter();
            }
            public override void Exit()
            {
                base.Exit();
            }
            #endregion

            #region set
            protected void DrawPath(Color col)
            {

            }
            protected void ClearPath()
            {

            }
            #endregion

            #region Callback
            public virtual void OnMoveStart() { DrawPath(Color); }
            public virtual void OnMoveEnd() { ClearPath(); }
            #endregion
        }
        #endregion

        //#region DB
        //public DBBaseMove GetDBData()
        //{
        //    DBBaseMove dbData = new DBBaseMove();
        //    dbData.CurMovePoint = CurMovePoint;
        //    dbData.MoveTarget = !MoveTarget_Unit.IsInv() ? MoveTarget_Unit.ID : Const.INT_Inv;
        //    dbData.FaceTarget = !FaceTarget.IsInv() ? FaceTarget.ID : Const.INT_Inv;
        //    dbData.IsValidMoveTarget = MoveTarget_IsValid;
        //    dbData.MoveTargetState = Enum<TState>.Int(MoveTarget_State);
        //    dbData.MoveTargetPosPreview = MoveTarget_PosPreview.ToVec3();
        //    dbData.MoveTargetPosReal = MoveTarget_PosReal.ToVec3();
        //    dbData.CurMoveState = Enum<TState>.Int(StateMachine.CurState);
        //    return dbData;
        //}
        //public void Load(DBBaseMove data)
        //{
        //    CurMovePoint = data.CurMovePoint;
        //    MoveTarget_Unit = GetEntity(data.MoveTarget);
        //    FaceTarget = GetEntity(data.FaceTarget);
        //    MoveTarget_IsValid = data.IsValidMoveTarget;
        //    MoveTarget_State = (TState)(object)data.MoveTargetState;
        //    StateMachine.SetCurState((TState)(object)data.CurMoveState, false);
        //    SetMoveTargetPosPreview(null, null, data.MoveTargetPosPreview.V3);
        //    SetMoveTargetPosReal(data.MoveTargetPosReal.V3);
        //}
        //#endregion

    }
}