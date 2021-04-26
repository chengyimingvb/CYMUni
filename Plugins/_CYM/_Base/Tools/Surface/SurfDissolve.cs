//**********************************************
// Class Name	: Surface_Dissolve
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
namespace CYM
{
    public class SurfDissolve : BaseSurf
    {
        #region prop
        private float delay = 0.0f;
        private float speed = 1.0f;
        private Timer delayTimer = new Timer();
        private float curAmount = 0.0f;
        #endregion

        #region set
        /// <summary>
        /// param1=延时时间
        /// param2=速度
        /// </summary>
        /// <param name="param"></param>
        public override void SetParam(params object[] param)
        {
            if (param.Length > 0) this.delay = (float)param[0];
            if (param.Length > 1) this.speed = (float)param[1];
        }
        public override void Use()
        {
            base.Use();
            delayTimer.Restart();
        }
        protected override void CopyProp(Material preMat, Material mat)
        {
            Texture mainTex = preMat.GetTexture("_MainTex");
            mat.SetTexture("_MainTex", mainTex);
            curAmount = 0.0f;
            mat.SetFloat("_Amount", curAmount);
        }
        #endregion

        #region get
        public override string GetDefaultMatName()
        {
            return "DissolveFire";
        }
        #endregion

        #region update
        public override void Update()
        {
            base.Update();
            UpdateDissolve();
        }
        void UpdateDissolve()
        {
            if (delayTimer.Elapsed() < delay) return;
            if (GetRenderers() != null)
            {
                curAmount += Time.deltaTime * speed;
                curAmount = Mathf.Clamp01(curAmount);
                ForeachMaterial((x) => x.SetFloat("_Amount", curAmount));
            }
        }
        #endregion

    }
}