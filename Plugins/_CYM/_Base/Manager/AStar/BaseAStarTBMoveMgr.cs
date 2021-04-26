//------------------------------------------------------------------------------
// BaseMoveMgr.cs
// Copyright 2019 2019/4/17 
// Created by CYM on 2019/4/17
// Owner: CYM
// 回合制游戏专用的移动组件,需要和BaseLogicTurn配合使用
//------------------------------------------------------------------------------

using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Serializable]
    public class DBBaseTDMove:DBBaseMove
    {
        public float CurMovePoint = 0;
    }
    public class BaseAStarTBMoveMgr<TState, TUnit, TTraversal,TModify> : BaseAStarMoveMgr<TState, TUnit, TTraversal, TModify> 
        where TUnit : BaseUnit 
        where TState : struct, Enum
        where TTraversal : BaseTraversal, new()
        where TModify:MonoModifier
    {
        #region Callback
        public Callback Callback_OnResetMovePoint { get; set; }
        #endregion

        #region Constant
        public virtual float PerMovePoint => MaxMovePoint == 0 ? 0 : CurMovePoint / MaxMovePoint;
        public virtual float CurMovePoint { get; set; } = 0;
        public virtual float MaxMovePoint { get; set; }
        protected override bool UseFollowCoroutine => true;
        public override bool IsCanMove => CurMovePoint > 0.0f;
        //路径展示的节点
        HashList<GraphNode> ConstantNodesDraw = new HashList<GraphNode>();
        //所有可移动的节点
        HashList<GraphNode> ConstantNodesMove = new HashList<GraphNode>();
        public bool IsForceBreak { get;private set; } = false;
        public bool IsFinalPosMove { get; private set; } = false;
        #endregion

        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGameLogicTurn = true;
        }

        #region set
        public HashList<GraphNode> CalcConstant()
        {
            float range = CurMovePoint;
            ConstantNodesDraw.Clear();
            ConstantNodesMove.Clear();
            ConstantNodesMove = AStarMgr.GetDistanceRange(SelfBaseUnit.Pos, range, x => Traversal.CanTraverse(null, x));

            foreach (var item in ConstantNodesMove)
            {
                //过滤掉占有单位得节点,并且这个节点不是自身,防止重复绕路
                if (AStarMgr.IsHaveUnit(item)) continue;
                ConstantNodesDraw.Add(item);
            }
            return ConstantNodesDraw;
        }
        public void SetForceBreak(bool b)
        {
            IsForceBreak = b;
        }
        public virtual void ResetMovePoint(float? movePoint = null)
        {
            if (movePoint == null)
                CurMovePoint = MaxMovePoint;
            else CurMovePoint = movePoint.Value;
            OnResetMovePoint();
            Callback_OnResetMovePoint?.Invoke();
        }
        #endregion

        #region is
        //单位是否处于移动状态种
        public override bool IsMoving => FollowPathCoroutine.IsRunning || base.IsMoving;
        //移动范围是否可以链接到目标
        public bool IsCanConstantConnection(BaseUnit unit)
        {
            if (ConstantNodesMove == null || ConstantNodesMove.Count == 0) return false;
            HashList<GraphNode> links = AStarMgr.GetConnectionsBlocker(unit);
            foreach (var item in links)
            {
                if (ConstantNodesMove.Contains(item))
                    return true;
            }

            return false;
        }
        #endregion

        #region Callback
        public override void OnGameLogicTurn()
        {
            base.OnGameLogicTurn();
            ResetMovePoint();
        }
        public override void OnBeNewSpawned()
        {
            base.OnBeNewSpawned();
            ResetMovePoint();
        }
        protected virtual void OnResetMovePoint()
        {

        }
        protected override Vector3 OnModifyFinalPos(Vector3 pos)
        {
            return GetFinalPos(pos);
        }
        protected virtual void OnMoveStep(float movedDistance, float toalDistance, float nodeSize, float segmentLength) { }
        protected virtual bool OnPreLerpMoveAlone(Vector3 nextPos, float moveStep, float movedDistance, float toalDistance, float nodeSize, float segmentLength, bool isFinalCorrect = false)
        {
            return true;
        }
        protected virtual void OnLerpMoveAlone(float moveStep, float movedDistance, float toalDistance, float nodeSize, float segmentLength, bool isFinalCorrect = false)
        {
            CurMovePoint -= moveStep;
        }
        #endregion

        #region IEnumerator

        protected override IEnumerator<float> OnFollowPathCoroutine()
        {
            IsMovingFlag = false;
            //是否为第一段移动
            bool isFirstMoved = false;
            //当前已经走过的路段
            float distanceAlongSegment = 0;
            //当前路段长度
            var segmentLength = 0.0f;
            //节点的大小
            float nodeSize = AStarMgr.Ins.data.gridGraph.nodeSize;
            //最大可以移动的距离
            float maxMoveDistance = nodeSize * MaxMovePoint;
            //已经移动的距离
            float movedDistance = 0;
            var moveStep = Time.smoothDeltaTime * RealMoveSpeed;
            IsForceBreak = false;
            IsFinalPosMove = false;
            for (int i = 0; i < ABPath.vectorPath.Count - 1; i++)
            {
                CurIndex = i;
                var p1 = ABPath.vectorPath[i];
                var p2 = ABPath.vectorPath[i + 1];
                segmentLength = Vector3.Distance(p1, p2);
                if (CurMovePoint <= segmentLength)
                {
                    p2 = (Vector3)AStarMgr.GetSafeNode(p2).position; ;
                }
                while (IsHaveMoveSegment() && !IsForceBreak)
                {
                    if (CurMovePoint < nodeSize &&
                        MathUtil.Approximately(SelfBaseUnit.Pos, p2))
                    {
                        IsForceBreak = true;
                    }
                    else LerpMove(p1, p2,1,true,false);

                    yield return Timing.WaitForOneFrame;
                    if (!isFirstMoved)
                    {
                        isFirstMoved = true;
                        Callback_OnFirstMovingAlone?.Invoke();
                    }
                }

                distanceAlongSegment -= segmentLength;
                OnMoveStep(movedDistance, maxMoveDistance, nodeSize, segmentLength);
                CurIndex++;
                if (!IsCanMove || IsForceBreak) 
                    break;
            }

            //计算最后的安全落点
            yield return Timing.WaitUntilDone(FinalPosMove());
            if (CurMovePoint < 1) CurMovePoint = 0;
            IsMovingFlag = true;
            Callback_OnMoveDestination?.Invoke();
            StopPath();

            //最后位置点的矫正
            IEnumerator<float> FinalPosMove()
            {
                IsFinalPosMove = true;
                Vector3 startPos = SelfBaseUnit.Pos;
                var finalPos = GetFinalPos(startPos);
                if (MathUtil.Approximately(startPos, finalPos)) yield break;
                //将单位移动到最终的目标节点
                distanceAlongSegment = 0;
                segmentLength = Vector3.Distance(startPos, finalPos);
                while (IsHaveMoveSegment())
                {
                    LerpMove(startPos, finalPos, 1, segmentLength > nodeSize,true);
                    yield return Timing.WaitForOneFrame;
                }
                SelfBaseUnit.Pos = finalPos;

                //如果目标无效,进入递归
                if (!MathUtil.Approximately(finalPos, GetFinalPos(SelfBaseUnit.Pos)))
                {
                    yield return Timing.WaitUntilDone(FinalPosMove());
                }
                IsForceBreak = false;
                IsFinalPosMove = false;
            }

            bool LerpMove(Vector3 p1, Vector3 p2, float speedMul = 1.0f, bool isRot = true,bool isFinalCorrect=false)
            {
                float aiSpeedMul = 1;
                if (BaseGlobal.FOWMgr != null &&
                    BaseGlobal.FOWMgr.IsInFog(p2) &&
                    SelfBaseUnit != null &&
                    SelfBaseUnit.IsAI() &&
                    SelfBaseUnit.FOWMgr != null &&
                    !SelfBaseUnit.FOWMgr.IsVisible &&
                    !SelfBaseUnit.FOWMgr.IsPreVisible)
                {
                    aiSpeedMul = 10;
                }
                if (!isFinalCorrect && IsForceBreak) return false;
                var tempMoveStep = moveStep * speedMul * aiSpeedMul;
                var interpolatedPoint = Vector3.Lerp(p1, p2, distanceAlongSegment / segmentLength);
                var targetRot = Quaternion.LookRotation((p2 - p1).SetY(0), Vector3.up);
                if (isRot) SelfBaseUnit.Rot = Quaternion.Slerp(SelfBaseUnit.Rot, targetRot, tempMoveStep);
                bool isValid = OnPreLerpMoveAlone(interpolatedPoint, tempMoveStep, movedDistance, maxMoveDistance, nodeSize, segmentLength, isFinalCorrect);
                if (!isValid) return false;
                SelfBaseUnit.Pos = interpolatedPoint;
                movedDistance += tempMoveStep;
                distanceAlongSegment += tempMoveStep;
                OnLerpMoveAlone(tempMoveStep, movedDistance, maxMoveDistance, nodeSize, segmentLength, isFinalCorrect);
                Callback_OnMovingAlone?.Invoke();
                return true;
            }
            //是否有移动的路段
            bool IsHaveMoveSegment()
            {
                return distanceAlongSegment < segmentLength;
            }

        }
        Vector3 GetFinalPos(Vector3 pos)
        {
            Vector3 finalPos = (Vector3)AStarMgr.GetSafeNode(pos).position;
            //单位可以和目标点重叠(finalPos必须是目标位置点才能重叠,所以需要Approximately判断,Approximately判断为了防止移动中途路径上的重叠)
            if (IsCanUnitOverlap && MathUtil.Approximately(Destination, finalPos)) { }
            else finalPos = (Vector3)AStarMgr.GetDistNode(SelfBaseUnit, finalPos,false, true, true, false,false).position;
            return finalPos;
        }
        #endregion

        #region DB
        public DBBaseTDMove GetDBData()
        {
            DBBaseTDMove dbData = new DBBaseTDMove();
            dbData.CurMovePoint = CurMovePoint;
            dbData.MoveTarget = !MoveTarget_Unit.IsInv() ? MoveTarget_Unit.ID : Const.INT_Inv;
            dbData.FaceTarget = !FaceTarget.IsInv() ? FaceTarget.ID : Const.INT_Inv;
            dbData.IsValidMoveTarget = MoveTarget_IsValid;
            dbData.MoveTargetState = Enum<TState>.Int(MoveTarget_State);
            dbData.MoveTargetPosPreview = MoveTarget_PosPreview.ToVec3();
            dbData.MoveTargetPosReal = MoveTarget_PosReal.ToVec3();
            dbData.CurMoveState = Enum<TState>.Int(StateMachine.CurState);
            return dbData;
        }
        public void Load(DBBaseTDMove data)
        {
            CurMovePoint = data.CurMovePoint;
            MoveTarget_Unit = GetEntity(data.MoveTarget);
            FaceTarget = GetEntity(data.FaceTarget);
            MoveTarget_IsValid = data.IsValidMoveTarget;
            MoveTarget_State = (TState)(object)data.MoveTargetState;
            StateMachine.SetCurState((TState)(object)data.CurMoveState, false);
            SetMoveTargetPosPreview(null, null, data.MoveTargetPosPreview.V3);
            SetMoveTargetPosReal(data.MoveTargetPosReal.V3);
        }
        #endregion
    }
}