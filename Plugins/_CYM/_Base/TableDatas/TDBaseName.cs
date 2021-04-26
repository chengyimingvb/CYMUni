//------------------------------------------------------------------------------
// TDBaseName.cs
// Copyright 2019 2019/2/18 
// Created by CYM on 2019/2/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CYM
{
    //Person head icon part enum
    public enum PHIPart
    {
        PBare,
        PEye,
        PNose,
        PHair,
        PMouse,
        PBrow,

        PHelmet,
        PBody,
        PBeard,
        PDecorate,
        PBG,
        PFrame,
    }
    [Serializable]
    public class GenderInfo
    {
        BaseGRMgr GRMgr => BaseGlobal.GRMgr;

        #region Config
        public string Name { get; set; } = "";
        public HashList<PartInfo> PBare { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PEye { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PNose { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PHair { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PMouse { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PBrow { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PBeard { get; set; } = new HashList<PartInfo>();

        public HashList<PartInfo> PBody { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PDecorate { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PBG { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PHelmet { get; set; } = new HashList<PartInfo>();
        public HashList<PartInfo> PFrame { get; set; } = new HashList<PartInfo>();
        #endregion

        #region parsed
        Dictionary<string, HashList<PartInfo>> SBare { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SEye { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SNose { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SHair { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SMouse { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SBrow { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SBeard { get; set; } = new Dictionary<string, HashList<PartInfo>>();

        Dictionary<string, HashList<PartInfo>> SBody { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SDecorate { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SBG { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SHelmet { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        Dictionary<string, HashList<PartInfo>> SFrame { get; set; } = new Dictionary<string, HashList<PartInfo>>();
        HashList<string> Rope = new HashList<string>();
        public List<string> NameKeys { get; private set; } = new List<string>();
        #endregion

        #region get
        public HashList<PartInfo> GetSBare(string tag) => GetPartInfo(tag, PBare, SBare);
        public HashList<PartInfo> GetSEye(string tag) => GetPartInfo(tag, PEye, SEye);
        public HashList<PartInfo> GetSNose(string tag) => GetPartInfo(tag, PNose, SNose);
        public HashList<PartInfo> GetSHair(string tag) => GetPartInfo(tag, PHair, SHair);
        public HashList<PartInfo> GetSMouse(string tag) => GetPartInfo(tag, PMouse, SMouse);
        public HashList<PartInfo> GetSBrow(string tag) => GetPartInfo(tag, PBrow, SBrow);
        public HashList<PartInfo> GetSBeard(string tag) => GetPartInfo(tag, PBeard, SBeard);

        public HashList<PartInfo> GetSBody(string tag) => GetPartInfo(tag, PBody, SBody);
        public HashList<PartInfo> GetSDecorate(string tag) => GetPartInfo(tag, PDecorate, SDecorate);
        public HashList<PartInfo> GetSBG(string tag) => GetPartInfo(tag, PBG, SBG);
        public HashList<PartInfo> GetSHelmet(string tag) => GetPartInfo(tag, PHelmet, SHelmet);
        public HashList<PartInfo> GetSFrame(string tag) => GetPartInfo(tag, PFrame, SFrame);
        private HashList<PartInfo> GetPartInfo(string tag, HashList<PartInfo> pdata, Dictionary<string, HashList<PartInfo>> sdata)
        {
            if (tag == Const.PTag_Normal) return pdata;
            if (sdata.ContainsKey(tag)) return sdata[tag];
            return pdata;
        }
        #endregion

        #region parse
        public void Parse()
        {
            ParseTag(PBare, SBare);
            ParseTag(PEye, SEye);
            ParseTag(PNose, SNose);
            ParseTag(PHair, SHair);
            ParseTag(PMouse, SMouse);
            ParseTag(PBrow, SBrow);
            ParseTag(PBeard, SBeard);
            ParseTag(PBody, SBody);
            ParseTag(PDecorate, SDecorate);
            ParseTag(PBG, SBG);
            ParseTag(PHelmet, SHelmet);
            ParseTag(PFrame, SFrame);
            NameKeys = BaseLanguageMgr.GetCategory(Name);
        }
        void ParseTag(List<PartInfo> info, Dictionary<string, HashList<PartInfo>> sinfo)
        {
            foreach (var item2 in info)
            {
                //加入年龄Tag
                Enum<AgeRange>.For(x =>
                {
                    string tagAge = x.ToString();
                    HashList<PartInfo> temp;
                    if (sinfo.ContainsKey(tagAge)) temp = sinfo[tagAge];
                    else
                    {
                        temp = new HashList<PartInfo>();
                        sinfo.Add(tagAge, temp);
                    }
                    var addtionName = item2.Name + "_" + tagAge;
                    if (GRMgr.Head.IsHave(addtionName))
                    {
                        PartInfo newPartInfo = item2.Clone() as PartInfo;
                        newPartInfo.Name = addtionName;
                        temp.Add(newPartInfo);
                    }
                });

                //加入自定义Tag
                if (item2.Tag == null) continue;
                foreach (var tag in item2.Tag)
                {
                    HashList<PartInfo> temp;
                    if (sinfo.ContainsKey(tag)) temp = sinfo[tag];
                    else
                    {
                        temp = new HashList<PartInfo>();
                        sinfo.Add(tag, temp);
                    }
                    temp.Add(item2);

                    if (tag == Const.PTag_Rope)
                        Rope.Add(item2.Name);
                }
            }
            info.RemoveAll((x) =>
            {
                return !x.IsIn;
            });
        }
        #endregion

        #region is
        public bool IsRope(string id)
        {
            if (Rope.Contains(id))
                return true;
            return false;
        }
        #endregion
    }
    [Serializable]
    public class PartInfo : ICloneable
    {
        public string Name { get; set; } = Const.STR_Inv;
        //是否在随机列表中
        public bool IsIn { get; set; } = true;
        public HashList<string> Tag { get; set; } = new HashList<string>();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    [Serializable]
    public class TDBaseNameData : TDBaseData
    {
        #region config
        public string Splite { get; set; } = "";
        public string Last { get; set; } = "";
        public GenderInfo Male { get; set; } = new GenderInfo();
        public GenderInfo Female { get; set; } = new GenderInfo();
        #endregion

        #region prop
        List<string> All { get; set; } = new List<string>();
        public List<string> LastNameKeys { get; private set; } = new List<string>();
        #endregion

        #region life
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            LastNameKeys.Clear();
            LastNameKeys = BaseLanguageMgr.GetCategory(Last);
            All.Clear();
            All.AddRange(Male.NameKeys);
            All.AddRange(Female.NameKeys);
            All.AddRange(LastNameKeys);
            Male.Parse();
            Female.Parse();
        }


        #endregion

        #region get
        public string GetPersonName(TDBasePersonData person)
        {
            string ret = "";
            string splite = "";
            string firstName = "";
            if (!BaseLanguageMgr.Space.IsInv()) splite = BaseLanguageMgr.Space;
            else splite = Splite;

            if (CustomName.IsInv()) firstName = person.FirstName.GetName();
            else firstName = CustomName;

            ret = person.LastName.GetName() + splite + firstName;
            return ret;
        }
        public string RandFirstNameKey(Gender gender)
        {
            return GetInfo(gender).NameKeys.Rand();
        }
        public string RandLastNameKey()
        {
            return LastNameKeys.Rand();
        }
        // 只获得名称
        public string RandName(bool isTrans = false)
        {
            string ret = RandUtil.RandArray(All);
            if (isTrans) return BaseLanguageMgr.Get(ret);
            return ret;
        }
        public Dictionary<PHIPart, string> RandHeadIcon(Gender gender, string tag)
        {
            Dictionary<PHIPart, string> ret = new Dictionary<PHIPart, string>();
            RandPart(PHIPart.PBare, ref ret);
            RandPart(PHIPart.PEye, ref ret);
            RandPart(PHIPart.PNose, ref ret);
            RandPart(PHIPart.PHair, ref ret);
            RandPart(PHIPart.PMouse, ref ret);
            RandPart(PHIPart.PBrow, ref ret);
            RandPart(PHIPart.PBeard, ref ret);

            RandPart(PHIPart.PHelmet, ref ret);
            RandPart(PHIPart.PBody, ref ret);
            RandPart(PHIPart.PDecorate, ref ret);
            RandPart(PHIPart.PBG, ref ret);
            RandPart(PHIPart.PFrame, ref ret);
            return ret;

            void RandPart(PHIPart part, ref Dictionary<PHIPart, string> data)
            {
                List<PartInfo> parts = GetParts(part);
                var key = parts.Rand();
                if (key == null) key = new PartInfo();
                data.Add(part, key.Name);
            }

            List<PartInfo> GetParts(PHIPart part)
            {
                var info = GetInfo(gender);
                //Face
                if (part == PHIPart.PBare) return info.GetSBare(tag);
                else if (part == PHIPart.PEye) return info.GetSEye(tag);
                else if (part == PHIPart.PHair) return info.GetSHair(tag);
                else if (part == PHIPart.PNose) return info.GetSNose(tag);
                else if (part == PHIPart.PMouse) return info.GetSMouse(tag);
                else if (part == PHIPart.PBrow) return info.GetSBrow(tag);
                else if (part == PHIPart.PBeard) return info.GetSBeard(tag);
                //其他
                else if (part == PHIPart.PBG) return info.GetSBG(tag);
                else if (part == PHIPart.PBody) return info.GetSBody(tag);
                else if (part == PHIPart.PDecorate) return info.GetSDecorate(tag);
                else if (part == PHIPart.PFrame) return info.GetSFrame(tag);
                else if (part == PHIPart.PHelmet) return info.GetSHelmet(tag);
                return new List<PartInfo>();
            }
        }
        public GenderInfo GetInfo(Gender gender)
        {
            GenderInfo info = new GenderInfo();
            if (gender == Gender.Female) info = Female;
            else if (gender == Gender.Male) info = Male;
            return info;
        }
        #endregion
    }

    public class TDBaseName<TData> : TDBaseConfig<TData>
        where TData : TDBaseNameData, new()
    {
        public HashList<string> AllLastName { get; private set; } = new HashList<string>();
        public override void OnLuaParseEnd()
        {
            base.OnLuaParseEnd();

            AllLastName.Clear();
            foreach (var item in Data)
            {
                AllLastName.AddRange(item.Value.LastNameKeys);
            }
        }
    }
}