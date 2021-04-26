//------------------------------------------------------------------------------
// BaseSimpleMoveCtrl.cs
// Copyright 2018 2018/9/25 
// Created by CYM on 2018/9/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public class SimpleMoverCtrl : BaseMono
    {
        #region mgr
        IBasePlotMgr PlotMgr => BaseGlobal.PlotMgr;
        #endregion

        public Vector3 TargetPos = Vector3.zero;
        public float TranslationSpeed = 1;
        public Vector3 RotationAxis = Vector3.up;
        public float RotSpeed = 10;
        public Vector3 OscillationAxis = Vector3.zero;
        public float OscillationPeriod = 10;
        public float OscillationSpeed = 10;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        bool isForward = true;
        float curTime = 0.0f;

        public override void Start()
        {
            _originalPosition = Trans.position;
            _originalRotation = Trans.rotation;
        }

        protected virtual void Update()
        {
            if (PlotMgr!=null && PlotMgr.IsInPlotPause())
                return;
            curTime += Time.smoothDeltaTime * TranslationSpeed;
            if (isForward)
            {
                Trans.position = Vector3.LerpUnclamped(_originalPosition, TargetPos, curTime);
                if (curTime >= 1.0f)
                {
                    curTime = 0.0f;
                    isForward = false;
                }
            }
            else
            {
                Trans.position = Vector3.LerpUnclamped(TargetPos, _originalPosition, curTime);
                if (curTime >= 1.0f)
                {
                    curTime = 0.0f;
                    isForward = true;
                }
            }

            Quaternion targetRotForOscillation = Quaternion.Euler(OscillationAxis.normalized * (Mathf.Sin(Time.time * OscillationSpeed) * OscillationPeriod)) * _originalRotation;
            Trans.rotation = Quaternion.Euler(RotationAxis * RotSpeed * Time.time) * targetRotForOscillation;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, TargetPos);
            Gizmos.DrawSphere(TargetPos, 0.1f);
        }
    }
}