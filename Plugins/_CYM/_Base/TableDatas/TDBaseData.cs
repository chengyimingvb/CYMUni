using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//**********************************************
// Class Name	: Battle_BaseLuaBattle
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public enum CloneType
    {
        Memberwise, //浅层拷贝,拷贝所有值字段
        Deep,       //拷贝所有值字段,包括用户自定义的深层拷贝
    }

    #region TD
    [Serializable]
    public class TDBaseData : IBase, ICloneable
    {
        protected virtual string CustomInvalidID => "0";

        #region prop
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        protected BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        protected BaseAudioMgr AudioMgr => BaseGlobal.AudioMgr;
        public BaseUnit SelfBaseUnit { get; protected set; }
        protected object[] AddedObjs { get; private set; }
        public BaseUnit Owner { get; protected set; }
        #endregion

        #region config
        public int Index { get; set; }
        public long ID { get; set; }
        public string TDID { get; set; } = Const.STR_Inv;
        public string Group { get; set; } = Const.STR_Inv;
        //和所有人都是敌人的单位(盗贼,野怪等)
        public bool IsWild { get; set; } = false;
        //系统单位
        public bool IsSystem { get; set; } = false;
        public CloneType CloneType { get; set; } = CloneType.Memberwise;
        public string Icon { get; set; } = "";
        public string Desc { get; set; } = "";
        public string Cont { get; set; } = "";
        public string Name { get; set; } = "";
        public string Prefab { get; set; } = "";
        public string SFX { get; set; } = "";
        public string Template { get; set; } = "";
        public string Notes { get; set; } = "";
        public List<UnlockData> Unlock { get; set; } = new List<UnlockData>();
        #endregion

        #region Custom
        public bool IsLive { get; set; } = true;
        public string CustomName { get; set; } = "";
        public float CustomScore => throw new NotImplementedException();
        #endregion

        #region life
        /// <summary>
        /// 被添加的时候触发:手动调用
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="obj"></param>
        public virtual void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            IsLive = true;
            AddedObjs = obj;
            SetSelfBaseUnit(selfMono as BaseUnit);
        }
        /// <summary>
        /// 被移除的时候,手动调用
        /// </summary>
        public virtual void OnBeRemoved()
        {
            IsLive = false;
        }
        /// <summary>
        /// 新生成
        /// </summary>
        public virtual void OnNewSpawn() { }
        /// <summary>
        /// update:手动调用
        /// </summary>
        public virtual void OnUpdate() { }
        /// <summary>
        /// 帧同步:手动调用
        /// </summary>
        /// <param name="gameFramesPerSecond"></param>
        public virtual void GameFrameTurn(int gameFramesPerSecond) { }
        /// <summary>
        /// 更新:手动调用
        /// </summary>
        public virtual void GameLogicTurn() { }
        /// <summary>
        /// 被添加到数据表里
        /// </summary>
        public virtual void OnBeAddedToData() { }
        public virtual void OnLuaParseStart() { }
        public virtual void OnLuaParseEnd() { }
        public virtual void OnExcelParseStart() { }
        public virtual void OnExcelParseEnd() { }
        public virtual void OnAllLoadEnd1() { }
        public virtual void OnAllLoadEnd2() { }
        public virtual void DoDeath()
        {
            IsLive = false;
        }
        #endregion

        #region get
        // 安全获得输入对象
        protected TType GetAddedObjData<TType>(int index) where TType : class
        {
            if (AddedObjs == null || AddedObjs.Length <= index)
                return default;
            return (AddedObjs[index] as TType);
        }
        // 返回翻译后的名字
        public virtual string GetName()
        {
            if (!CustomName.IsInv())
                return CustomName;
            if (!Name.IsInv())
                return BaseLanguageMgr.Get(Name);
            string nameTDID = Const.Prefix_Name + TDID;
            if (BaseLanguageMgr.IsContain(nameTDID))
                return BaseLanguageMgr.Get(nameTDID);
            return BaseLanguageMgr.Get(TDID);
        }
        // 获取自动提示
        public virtual string GetDesc(params object[] ps)
        {
            if (!string.IsNullOrEmpty(Desc))
                return BaseLanguageMgr.Get(Desc, ps);
            string descTDID = Const.Prefix_Desc + TDID;
            if (BaseLanguageMgr.IsContain(descTDID))
                return BaseLanguageMgr.Get(descTDID, ps);
            return BaseLanguageMgr.Get(Const.STR_Desc_NoDesc);
        }
        // 获取icon
        public virtual Sprite GetIcon()
        {
            if (Icon == CustomInvalidID)
                return null;
            if (!Icon.IsInv()) 
                return GRMgr.Icon.Get(Icon,false);
            else if (!TDID.IsInv() && GRMgr.Icon.IsHave(TDID)) 
                return GRMgr.Icon.Get(TDID,false);
            return null;
        }
        // 获得禁用的图标,有可能没有
        public virtual Sprite GetDisIcon()
        {
            if (Icon == CustomInvalidID)
                return null;
            if (!Icon.IsInv())
                return GRMgr.Icon.Get(Icon + Const.Suffix_Disable,false);
            return GRMgr.Icon.Get(TDID + Const.Suffix_Disable,false);
        }
        public Sprite GetSelIcon()
        {
            if (Icon == CustomInvalidID)
                return null;
            if (!Icon.IsInv())
                return GRMgr.Icon.Get(Icon + Const.Suffix_Sel,false);
            return GRMgr.Icon.Get(TDID + Const.Suffix_Sel,false);
        }
        // prefab
        public virtual GameObject GetPrefab()
        {
            if (Prefab == CustomInvalidID)
                return null;
            if (!Prefab.IsInv())
                return GRMgr.Prefab.Get(Prefab);
            return GRMgr.Prefab.Get(TDID);
        }
        // 获得animator
        public virtual RuntimeAnimatorController GetAnimator()
        {
            return GRMgr.Animator.Get(TDID);
        }
        //获得SFX
        public virtual AudioClip GetSFX()
        {
            if (SFX == CustomInvalidID)
                return null;
            if (!SFX.IsInv())
                return GRMgr.Audio.Get(SFX);
            return null;
        }
        public string GetStr(string key, params object[] ps)
        {
            return BaseLanguageMgr.Get(key, ps);
        }
        #endregion

        #region set
        public void SetCustomName(string name)
        {
            CustomName = name;
        }
        public void SetOwner(BaseUnit owner)
        {
            Owner = owner;
        }
        public void SetSelfBaseUnit(BaseUnit unit)
        {
            SelfBaseUnit = unit;
        }
        #endregion

        #region is
        public bool IsPlayer()
        {
            if (SelfBaseUnit == null) return false;
            return SelfBaseUnit.IsPlayer();
        }
        #endregion

        #region copy
        protected virtual void DeepClone(object sourceObj)
        {
            throw new NotImplementedException("此函数必须被实现");
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public virtual TClass Copy<TClass>() where TClass : TDBaseData, new()
        {
            TClass tempBuff = null;
            {
                //浅层拷贝,拷贝所有值字段
                if (CloneType == CloneType.Memberwise)
                    tempBuff = Clone() as TClass;
                //拷贝所有值字段,包括用户自定义的深层拷贝
                else if (CloneType == CloneType.Deep)
                {
                    tempBuff = Clone() as TClass;
                    tempBuff.DeepClone(this);
                }
            }
            return tempBuff;
        }
        #endregion
    }
    #endregion

    #region Config
    // 会根据类名自动生成Lua方发, e.g. TDNationData 会截头去尾 变成:AddNation
    public class TDBaseConfig<T> : HashManager<string, T>, ITDLuaMgr 
        where T : TDBaseData, new()
    {
        #region val
        public Type DataType { get; private set; }
        public virtual string TableMapperInfo => "";
        public Excel.TableMapper TableMapper { get; protected set; }
        public Dictionary<string, TDBaseData> BaseDatas { get; private set; } = new Dictionary<string, TDBaseData>();
        public List<string> Keys { get; private set; } = new List<string>();
        public List<T> Values { get; private set; } = new List<T>();
        public List<object> ObjValues { get; private set; } = new List<object>();
        public Dictionary<string, List<T>> Groups { get; private set; } = new Dictionary<string, List<T>>();
        /// <summary>
        /// 默认lua表格数据
        /// </summary>
        protected string LuaTableKey = Const.STR_Inv;
        protected DynValue MetaTable = null;
        protected string AddMethonName;
        protected string AlterMethonName;
        /// <summary>
        /// 默认的命名控件
        /// </summary>
        protected string NameSpace = Const.STR_Inv;
        protected DynValue baseTable;
        protected T TempClassData;
        public T Default { get; private set; }
        #endregion

        #region init
        public TDBaseConfig()
        {
            Init();
        }
        public TDBaseConfig(string keyName)
        {
            Init(keyName);
        }

        protected virtual void Init(string keyName = null)
        {
            if (keyName == null) LuaTableKey = typeof(T).Name;
            else LuaTableKey = keyName;
            //去除头部TD前缀和尾部Data后缀
            {
                LuaTableKey = LuaTableKey.TrimStart("TD");
                LuaTableKey = LuaTableKey.TrimStart("Base");
                LuaTableKey = LuaTableKey.TrimEnd("Data");
            }
            NameSpace = typeof(T).Namespace.ToString();
            AddMethonName = string.Format("Add{0}", LuaTableKey);
            AlterMethonName = string.Format("Alter{0}", LuaTableKey);
            DataType = typeof(T);
            if (TableMapperInfo != "")
            {
                TableMapper = new Excel.TableMapper(typeof(T)).Map(TableMapperInfo);
            }
            BaseLuaMgr.AddGlobalAction(AddMethonName, AddFromLua);
            BaseLuaMgr.AddGlobalAction(AlterMethonName, AlterFronLua);
            BaseLuaMgr.AddTDLuaMgr(LuaTableKey, typeof(T), this);
        }
        #endregion

        #region set
        public override void Add(string id, T ent)
        {
            if (Data.ContainsKey(id))
            {
                CLog.Error("重复的Key:" + id);
                return;
            }
            ent.TDID = id;
            Data.Add(id, ent);
            BaseDatas.Add(id, ent);
            Keys.Add(ent.TDID);
            ObjValues.Add(ent);
            Values.Add(ent);
            if (!ent.Group.IsInv())
            {
                if (!Groups.ContainsKey(ent.Group))
                    Groups.Add(ent.Group,new List<T>());
                var groupData = Groups[ent.Group];
                groupData.Add(ent);
            }
        }
        public void Alter(string id, T ent)
        {
            if (!Data.ContainsKey(id))
            {
                CLog.Error("没有这个Key:" + id);
                return;
            }
            Data[id] = ent;
            BaseDatas[id] = ent;
        }
        public override void Remove(string id)
        {
            base.Remove(id);
            BaseDatas.Remove(id);
            Keys.Remove(id);
        }
        public T Rand()
        {
            return Get(Keys.Rand());
        }
        public string RandKey()
        {
            return Keys.Rand();
        }
        DynValue CopyPairs(DynValue target, DynValue source)
        {
            foreach (var item in source.Table.Pairs)
            {
                target.Table.Set(item.Key, item.Value);
            }
            return target;
        }
        #endregion

        #region Add data
        public DynValue AddByDefaultFromLua(DynValue luaValue)
        {
            if (MetaTable == null) return null;
            if (luaValue == null) return null;
            Closure funcCopyPairs = GetDynVal("CopyPairs").Function;
            Closure funcGetNewTable = GetDynVal("GetNewTable").Function;
            DynValue ret = funcGetNewTable.Call();
            if (MetaTable.IsNotNil())
                ret = CopyPairs(ret, MetaTable);
            if (luaValue.IsNotNil())
                ret = CopyPairs(ret, luaValue);

            return ret;
        }
        public void AddFromLua(DynValue luaValue)
        {
            if (luaValue.Table == null)
            {
                CLog.Error("错误!Add(DynValue table),必须是一个Table");
                return;
            }
            if (MetaTable == null) MetaTable = GetDynVal(LuaTableKey);

            if (MetaTable != null)
                baseTable = AddByDefaultFromLua(luaValue);
            else baseTable = luaValue;

            //获得Lua类模板
            Type classType = typeof(T);
            TempClassData = (T)LuaReader.Convert(baseTable, classType);
            if (TempClassData == null) return;
            string key = TempClassData.TDID;
            if (Data.ContainsKey(key))
            {
                CLog.Error(baseTable.ToString() + "Add:已经存在这个key:" + key);
                return;
            }
            TempClassData.TDID = key;
            AddFromObj(TempClassData);
        }
        //修改/重载/适用于DLC/玩家扩展
        public void AlterFronLua(DynValue luaValue)
        {
            if (luaValue.Table == null)
            {
                CLog.Error("错误!Add(DynValue table),必须是一个Table");
                return;
            }
            string key = luaValue.Table.Get("TDID").ToString();
            if (!Data.ContainsKey(key))
            {
                CLog.Error(baseTable.ToString() + "Alter:不存在key:" + key);
                return;
            }
            else
            {
                var cObject = Get(key);
                foreach (TablePair propertyPair in luaValue.Table.Pairs)
                    LuaReader.SetValue(cObject, propertyPair.Key.String, propertyPair.Value);
            }
        }
        public void AddAlterRangeFromObj(IEnumerable<object> data)
        {
            if (data == null)
                return;
            foreach (var obj in data)
            {
                var item = obj as T;
                if (!Contains(item.TDID))
                    AddFromObj(item);
                else
                    AlterFromObj(item);
            }
        }
        public void AddFromObj(T ent)
        {
            ent.OnBeAddedToData();
            Add(ent.TDID, ent);
        }
        public void AlterFromObj(T ent)
        {
            Alter(ent.TDID, ent);
        }
        #endregion

        #region get
        protected Table GetTable(string name)
        {
            DynValue temp = BaseLuaMgr.Lua.Globals.Get(name);
            if (temp == null)
                return null;
            return temp.Table;
        }
        protected DynValue GetDynVal(string name)
        {
            DynValue temp = BaseLuaMgr.Lua.Globals.Get(name);
            return temp;
        }
        protected string GetStrByBaseTable(string name)
        {
            DynValue temp = baseTable.Table.RawGet(name);
            if (temp == null)
                return null;
            return temp.String;
        }
        public T Get(int index)
        {
            if (index >= Values.Count)
                return null;
            return Values[index];
        }
        public TData Get<TData>(string key) where TData : TDBaseData
        {
            return Get(key) as TData;
        }
        public List<T> GetGroup(string group)
        {
            if (Groups.ContainsKey(group))
                return Groups[group];
            return new List<T>();
        }
        public IList GetRawGroup(string group)
        {
            if (Groups.ContainsKey(group))
                return Groups[group];
            return new List<T>();
        }
        protected string GetStr(string key, params object[] objs) => BaseLanguageMgr.Get(key, objs);
        #endregion

        #region Callback
        public virtual void OnLuaParseStart()
        {
            foreach (var item in Data)
            {
                item.Value.OnLuaParseStart();
            }
        }
        public virtual void OnLuaParseEnd()
        {
            int index = 0;
            if (MetaTable == null) MetaTable = GetDynVal(LuaTableKey);
            Default = (T)LuaReader.Convert(MetaTable, typeof(T));
            foreach (var item in Data)
            {
                item.Value.OnLuaParseEnd();
                item.Value.Index = index;
                index++;
            }
        }

        public virtual void OnExcelParseStart()
        {
            foreach (var item in Data)
            {
                item.Value.OnExcelParseStart();
            }
        }
        public virtual void OnExcelParseEnd()
        {
            foreach (var item in Data)
            {
                item.Value.OnExcelParseEnd();
            }
        }
        public virtual void OnAllLoadEnd1()
        {
            foreach (var item in Data)
            {
                item.Value.OnAllLoadEnd1();
            }
        }
        public virtual void OnAllLoadEnd2()
        {
            foreach (var item in Data)
            {
                item.Value.OnAllLoadEnd2();
            }
        }
        #endregion
    }
    public class TDBaseConfigStatic<T, TConfig> : TDBaseConfig<T>
        where T : TDBaseData, new()
        where TConfig : TDBaseConfig<T>
    {
        public static TConfig Ins { get; private set; }
        public TDBaseConfigStatic()
        {
            Ins = this as TConfig;
        }

        #region pub get
        public new static T Get(int index) => Ins.Get(index);
        public new static TData Get<TData>(string key) where TData : TDBaseData => Ins.Get<TData>(key);
        public new static T Get(string key) => Ins.Get(key);
        public new static List<T> GetGroup(string group) => Ins.GetGroup(group);
        public new static IList GetRawGroup(string group) => Ins.GetRawGroup(group);
        #endregion
    }
    #endregion
}