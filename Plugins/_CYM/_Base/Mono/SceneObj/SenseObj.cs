using UnityEngine;

namespace CYM
{
    public class SenseObj : BaseMono
    {
        #region prop
        IBaseSenseMgr SenseMgr;
        #endregion

        #region set
        public void Init(IBaseSenseMgr sense)
        {
            SenseMgr = sense;
        }
        #endregion

        protected virtual void OnTriggerEnter(Collider other)
        {
            SenseMgr.DoTestEnter(other);
        }

        protected void OnTriggerExit(Collider other)
        {
            SenseMgr.DoTestExit(other);
        }
    }

}