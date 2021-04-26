//------------------------------------------------------------------------------
// BaseTarget.cs
// Copyright 2019 2019/8/3 
// Created by CYM on 2019/8/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

namespace CYM
{
    #region 条件
    public class BaseTarget : ILuaObj
    {
        #region config
        public ACCType ACCType { get; set; } = ACCType.And;
        public float Value { get; set; } = 0;
        #endregion

        #region prop
        protected BaseConditionMgr ACM => BaseGlobal.ACM;
        #endregion

        #region set
        public virtual void DoCondition(BaseUnit unit)
        {
            if (unit == null)
            {
                CLog.Error("错误!没有设置SelfUnit");
            }
        }
        #endregion

        #region Callback
        public virtual void OnBeCreated()
        {

        }
        #endregion
    }
    #endregion
}