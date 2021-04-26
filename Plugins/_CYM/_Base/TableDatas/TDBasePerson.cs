//------------------------------------------------------------------------------
// TDBasePerson.cs
// Copyright 2019 2019/5/14 
// Created by CYM on 2019/5/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    //性别
    public enum Gender : int
    {
        Male = 0,//男
        Female,//女
    }
    //年龄阶段
    public enum AgeRange : int
    {
        Child = 0,//1-18
        Adult = 1,//18-30
        Middle = 2,//30-50
        Old = 3,//50-60
    }
    public enum PState
    {
        None, //正常
        Prisoner,//囚犯
        Captive,//俘虏
    }
    [Serializable]
    public class TDBasePersonData : TDBaseData
    {
        #region data
        public static readonly RangeDic<AgeRange> AgeRangeData = new RangeDic<AgeRange>()
        {
            { AgeRange.Child,new Range(1,16) },
            { AgeRange.Adult,new Range(16,40) },
            { AgeRange.Middle,new Range(40,55) },
            { AgeRange.Old,new Range(55,80) },
        };
        public static readonly Dictionary<AgeRange, float> DeathProb = new Dictionary<AgeRange, float>()
        {
            { AgeRange.Child,0.002f },
            { AgeRange.Adult,0.001f },
            { AgeRange.Middle,0.0025f },
            { AgeRange.Old,0.015f },
        };
        #endregion

        #region Config
        public string NameLib { get; set; } = "Name_中原";
        //并非Key,而是翻译后的字符串
        public string FirstName { get; set; } = Const.STR_None;
        //并非Key,而是翻译后的字符串
        public string LastName { get; set; } = Const.STR_None;
        public Gender Gender { get; set; } = Gender.Male;
        public PState PState { get; set; } = PState.None;
        public int Age { get; set; } = Const.INT_Inv;
        public bool IsCelebrity { get; set; } = false;
        public Dictionary<PHIPart, string> HeadIcon { get; set; } = new Dictionary<PHIPart, string>();
        #endregion

        #region prop
        public AgeRange AgeRange { get; set; } = AgeRange.Adult;
        protected Dictionary<PHIPart, string> LastPartIDs { get; set; } = new Dictionary<PHIPart, string>();
        public TDBaseNameData NameData { get; private set; }
        #endregion

        #region life
        protected virtual TDBaseNameData GetNameData(string id) => throw new NotImplementedException("此函数必须实现!!!");
        public override void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            base.OnBeAdded(selfMono, obj);
            NameData = GetNameData(NameLib);
            AgeRange = GetAgeRange();
        }
        #endregion

        #region rand
        public void RandInfo(TDBaseNameData nameData, AgeRange range = AgeRange.Adult, Gender gender = Gender.Male, string lastName = null)
        {
            IsCelebrity = false;
            AgeRange = range;
            Gender = gender;
            NameLib = nameData.TDID;
            Age = RandUtil.RangeInt(AgeRangeData[AgeRange]);
            NameData = GetNameData(NameLib);
            HeadIcon = NameData.RandHeadIcon(Gender, Const.PTag_Normal);
            if (lastName.IsInv())
            {
                LastName = NameData.RandLastNameKey();
            }
            else
            {
                if (BaseLanguageMgr.AllLastNames.Contains(lastName)) LastName = lastName;
                else throw new Exception("没有这个姓氏:" + lastName);
            }
            if (FirstName.IsInv())
            {
                FirstName = NameData.RandFirstNameKey(Gender);
            }
            OnRandInfo();
        }
        public void Generate()
        {
            IsCelebrity = true;
            AgeRange = GetAgeRange();
            NameData = GetNameData(NameLib);
            HeadIcon = NameData.RandHeadIcon(Gender, Const.PTag_Normal);
            OnGenerate();
        }
        #endregion

        #region Set
        public void Growup()
        {
            Age++;
            AgeRange = GetAgeRange();
        }
        #endregion

        #region Get
        public string GetTDID()
        {
            if (TDID.IsInv()) return LastName + FirstName;
            return TDID;
        }
        public override string GetName()
        {
            if (FirstName.IsInv() || LastName.IsInv()) return base.GetName();
            return NameData.GetPersonName(this);
        }
        public virtual string GetFirstName()
        {
            if (FirstName.IsInv()) return CustomName;
            return FirstName.GetName();
        }
        public virtual int GetStar() => 1;
        public Sprite GetStarIcon() => GRMgr.Icon.Get(Const.Prefix_Star + GetStar());
        public Sprite GetPSprite(PHIPart part)
        {
            Sprite ret = null;
            var tempHeadIcon = GetHeadIcon();
            if (tempHeadIcon.ContainsKey(part))
            {
                //获得原始Key
                string temp = tempHeadIcon[part];
                if (temp.IsInv()) return null;
                //身份(职业)加工(自定义处理)身份加工
                string sourcePartKey = OnProcessIdentity(part, temp);
                string addPartKey = sourcePartKey;
                //隐藏设置
                if (OnProcessHide(part)) return null;
                //加工年龄
                addPartKey = OnProcessAge(part, addPartKey);
                //获得图片
                ret = GRMgr.Head.Get(addPartKey, false);
                if (ret == null) ret = GRMgr.Head.Get(sourcePartKey, true);
                //记录ID
                if (ret != null)
                {
                    if (LastPartIDs.ContainsKey(part)) LastPartIDs[part] = sourcePartKey;
                    else LastPartIDs.Add(part, sourcePartKey);
                }
                else
                {
                    LastPartIDs.Remove(part);
                }

                return ret;
            }
            else
            {
                throw new NotImplementedException("没有这个部分:" + part.ToString());
            }
        }
        public AgeRange GetAgeRange()
        {
            foreach (var item in AgeRangeData)
            {
                if (item.Value.IsIn(Age))
                    return item.Key;
            }
            return AgeRange.Old;
        }
        public void SetAge(int age)
        {
            Age = age;
            AgeRange = GetAgeRange();
        }
        public virtual bool IsRopeHelmet()
        {
            if (NameData == null)
                return false;
            if (!LastPartIDs.ContainsKey(PHIPart.PHelmet))
                return false;
            if (NameData.GetInfo(Gender).IsRope(LastPartIDs[PHIPart.PHelmet]))
            {
                return true;
            }
            return false;
        }
        public string GetAgeStr(bool haveAgeStr = true)
        {
            return haveAgeStr ? "Text_Age".GetName() + ":" + Age.ToString() : Age.ToString();
        }
        public override string GetDesc(params object[] ps)
        {
            return base.GetDesc(ps);
        }
        //获得人物评价
        public string GetEvaluation()
        {
            if (IsCelebrity) return BaseLanguageMgr.Get("Text_历史名人");
            else return BaseLanguageMgr.Get("Text_无名鼠辈");
        }
        public virtual float GetDeathProp()
        {
            float deathAge = 65;
            float add = 0;
            float mul = 1;
            //正常的死亡几率
            float ageFac = 0.00005f;
            //名人的死亡几率降低
            if (IsCelebrity) mul = 0.5f;
            //人过60,死亡几率扩大
            if (Age > deathAge) add = 0.1f + (Age - deathAge) * 0.001f;
            //快速死亡
            if (Options.IsFastPersonDeath) add = 0.5f;
            //计算最终的死亡概率
            float final = DeathProb[AgeRange] + (Age * ageFac) + add;
            return final * mul;
        }
        public GenderInfo GetGenderInfo() => NameData.GetInfo(Gender);
        protected virtual Dictionary<PHIPart, string> GetHeadIcon() => HeadIcon;
        #endregion

        #region is
        public bool IsMale => Gender == Gender.Male;
        public bool IsFemale => Gender == Gender.Female;
        public bool IsJail => PState == PState.Captive || PState == PState.Prisoner;
        public bool IsCaptive => PState == PState.Captive;
        public bool IsWomenOrChildren => AgeRange == AgeRange.Child || IsFemale;
        #endregion

        #region virtual
        //自定义身份加工
        protected virtual string OnProcessIdentity(PHIPart part, string source)
        {
            return source;
        }
        //自定义隐藏(比如年轻的时候隐藏胡子)
        protected virtual bool OnProcessHide(PHIPart part)
        {
            if (part == PHIPart.PBeard)
            {
                if (AgeRange == AgeRange.Child)
                    return true;
                if (Gender == Gender.Female)
                    return true;
            }
            else if (part == PHIPart.PDecorate)
            {
                if (AgeRange == AgeRange.Child)
                    return true;
            }
            return false;
        }
        protected virtual string OnProcessAge(PHIPart part, string inputStr)
        {
            inputStr = inputStr + "_" + AgeRange.ToString();
            return inputStr;
        }
        protected virtual void OnRandInfo()
        {

        }
        protected virtual void OnGenerate()
        {

        }
        #endregion
    }
}