using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class EffectFlicker : BaseMono
    {
        public float pauzeBeforeStart = 1.3f;
        public float flickerSpeedStart = 15f;
        public float flickerSpeedEnd = 35f;
        public float Duration = 2f;
        public bool DestroyOnFinish = false;

        public MeshRenderer[] GFX;

        CoroutineHandle CoroutineHandle;

        public override void Awake()
        {
            GFX = GetComponentsInChildren<MeshRenderer>();
        }

        public void StopEffect()
        {
            BaseGlobal.BattleCoroutineter.Kill(CoroutineHandle);
            foreach (var g in GFX) g.enabled = (true);
        }

        public void StartEffect(float pauzeBeforeStart = 0.0f, float duration = 1.0f)
        {
            this.pauzeBeforeStart = pauzeBeforeStart;
            this.Duration = duration;
            BaseGlobal.BattleCoroutineter.Kill(CoroutineHandle);
            CoroutineHandle = BaseGlobal.BattleCoroutineter.Run(FlickerCoroutine());
        }

        IEnumerator<float> FlickerCoroutine()
        {

            //pause before start
            yield return Timing.WaitForSeconds(pauzeBeforeStart); 

            //flicker
            float t = 0;
            while (t < 1)
            {
                float speed = Mathf.Lerp(flickerSpeedStart, flickerSpeedEnd, MathUtil.Coserp(0, 1, t));
                float i = Mathf.Sin(Time.time * speed);
                foreach (var g in GFX) g.enabled = (i > 0);
                t += Time.deltaTime / Duration;
                yield return Timing.WaitForOneFrame;
            }

            //destroy
            if (DestroyOnFinish)
            {
                BaseGlobal.PoolMgr.Perform.Despawn(this);
            }
        }
    }

}