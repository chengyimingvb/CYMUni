//------------------------------------------------------------------------------
// BaseWeatherMgr.cs
// Copyright 2019 2019/9/4 
// Created by CYM on 2019/9/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public enum WorldWeatherType
    {
        Autumn,
        Mist,
        Ranning,
        Snowing,
        StormDust,
        Tornado,
        GoldRay,
    }
    public class WorldWeatherData
    {
        public WorldWeatherData(int cd, BasePerform perform)
        {
            CD = new CD(cd);
            Perform = perform;
        }
        public CD CD;
        public BasePerform Perform;
    }
    public class BaseWorldWeatherMgr : BaseGFlowMgr
    {
        #region prop
        protected virtual int StartCount { get; private set; } = 30;
        protected virtual int TotalCount { get; private set; } = 40;
        protected virtual int CellCount { get; private set; } = 30;
        protected virtual int Squar { get; private set; } = 1024;
        int RealCount => CellCount - 1;
        Vector2[,] PosIndex;
        protected Dictionary<WorldWeatherType, List<string>> ConfigData { get; private set; } = new Dictionary<WorldWeatherType, List<string>>();
        protected Dictionary<string, WorldWeatherData> Data { get; private set; } = new Dictionary<string, WorldWeatherData>();
        Timer UpdateTimer = new Timer(1.0f);
        Timer SpawnTimer = new Timer(10.0f);
        Type WeatherType = typeof(WorldWeatherType);
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            Data.Clear();
            ConfigData.Clear();
            int step = Squar / CellCount;
            PosIndex = new Vector2[RealCount, RealCount];
            for (int i = 0; i < RealCount; ++i)
            {
                for (int j = 0; j < RealCount; ++j)
                {
                    PosIndex[i, j] = new Vector2((i + 1) * step, (j + 1) * step);
                }
            }
            OnAddWeather();
        }
        protected virtual void OnAddWeather() { }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (BattleMgr.IsGameStartOver)
            {
                if (UpdateTimer.CheckOver())
                {
                    List<string> clearData = new List<string>();
                    foreach (var item in Data)
                    {
                        item.Value.CD.Update();
                        if (item.Value.CD.IsOver())
                        {
                            clearData.Add(item.Key);
                        }
                    }
                    foreach (var item in clearData)
                    {
                        Despwn(item);
                    }
                }
                if (SpawnTimer.CheckOver())
                {
                    RandWeather(0.5f);
                }
            }
        }
        #endregion

        #region set
        public void Spawn(WorldWeatherType type, Vector2 index)
        {
            if (Data.ContainsKey(index.ToString())) return;
            if (index.x >= RealCount || index.y >= RealCount) return;
            if (!ConfigData.ContainsKey(type)) return;
            Vector2 pos = PosIndex[(int)index.x, (int)index.y];
            Vector3 realPos = new Vector3(pos.x, BaseSceneObject.GetAbsHeight(pos.x, pos.y), pos.y);
            realPos.x += RandUtil.RandFloat(-50, 50);
            realPos.z += RandUtil.RandFloat(-50, 50);
            var perform = PerfomMgr.Spawn(ConfigData[type].Rand(), realPos, Quaternion.identity);
            Data.Add(index.ToString(), new WorldWeatherData(RandUtil.RandInt(50, 400), perform));
        }
        public void Despwn(string key, bool isRemove = true)
        {
            if (!Data.ContainsKey(key)) return;
            PerfomMgr.Despawn(Data[key].Perform);
            if (isRemove)
                Data.Remove(key);
        }
        void RandWeather(float prop = 0.2f)
        {
            if (Data.Count <= TotalCount)
            {
                if (RandUtil.Rand(prop))
                {
                    Spawn(WeatherType.Rand<WorldWeatherType>(), new Vector2(RandUtil.RandInt(0, RealCount), RandUtil.RandInt(0, RealCount)));
                }
            }
        }
        #endregion

        #region Callback
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            for (int i = 0; i < StartCount; ++i)
            {
                RandWeather(1.0f);
            }
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            foreach (var item in Data)
            {
                Despwn(item.Key, false);
            }
            Data.Clear();
        }
        #endregion
    }
}