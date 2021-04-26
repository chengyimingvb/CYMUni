//------------------------------------------------------------------------------
// BaseWeatherMgr.cs
// Copyright 2019 2019/2/25 
// Created by CYM on 2019/2/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public enum SeasonType
    {
        Spring,
        Summer,
        Fall,
        Winter,
    }
    public enum SubSeasonType
    {
        Normal,//正常
        Deep, //嚴冬.盛夏
    }

    public struct SeasonData
    {
        /// <summary>
        /// 太阳关照强度
        /// </summary>
        public float SunIntensity { get; set; }
        /// <summary>
        /// 积雪百分比
        /// </summary>
        public float AccumulatedSnow { get; set; }
        /// <summary>
        /// 风力
        /// </summary>
        public float WindzonePower { get; set; }

        public SubSeasonType Type { get; set; }

        public string TDID { get; set; }

    }

    public class BaseSeasonMgr : BaseGFlowMgr
    {
        readonly Dictionary<SeasonType, List<SeasonData>> Data = new Dictionary<SeasonType, List<SeasonData>>();
        readonly float WindPowerAdt = 0.8f;

        #region Callback
        /// <summary>
        /// 季节变化
        /// </summary>
        public event Callback<SeasonType, SubSeasonType> Callback_OnSeasonChanged;
        #endregion

        #region prop
        BaseDateTimeMgr DateTimeMgr => BaseGlobal.DateTimeMgr;
        private TweenerCore<float, float, FloatOptions> sunTween;
        private TweenerCore<float, float, FloatOptions> snowTween;
        WindZone Wind => WindZoneObj.Obj;
        public SeasonData CurData { get; private set; } = new SeasonData();
        public SeasonType SeasonType { get; private set; } = SeasonType.Spring;
        #endregion

        #region life
        protected virtual bool IsShowSnow => true;
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            Data.Add(SeasonType.Spring, new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.85f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.2f,
                    Type = SubSeasonType.Normal,
                    TDID="Season_春季",
                }
            });

            Data.Add(SeasonType.Summer, new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.9f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.25f,
                    Type = SubSeasonType.Normal,
                    TDID="Season_夏季",
                },

                new SeasonData
                {
                    SunIntensity = 0.9f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.28f,
                    Type = SubSeasonType.Deep,
                    TDID="Season_炎夏",
                }
            });

            Data.Add(SeasonType.Fall, new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.75f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.25f,
                    Type = SubSeasonType.Normal,
                    TDID="Season_秋季",
                }
            });

            Data.Add(SeasonType.Winter, new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.7f,
                    AccumulatedSnow = 0.2f,
                    WindzonePower = 0.29f,
                    Type = SubSeasonType.Normal,
                    TDID="Season_冬季",
                },

                new SeasonData
                {
                    SunIntensity = 0.7f,
                    AccumulatedSnow = 0.3f,
                    WindzonePower = 0.3f,
                    Type = SubSeasonType.Deep,
                    TDID="Season_寒冬",
                }
            });
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
        }
        public override void OnGameStart1()
        {
            base.OnGameStart1();
            RefreshByMonth(true);
        }
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
        }
        #endregion

        #region get
        public string GetSeasonID()
        {
            return CurData.TDID;
        }

        public Sprite GetIcon()
        {
            if (CurData.TDID == null)
                return null;
            return GetSeasonID().GetIcon();
        }
        public string GetName()
        {
            if (CurData.TDID == null)
                return null;
            return GetSeasonID().GetName();
        }
        public string GetDesc()
        {
            if (CurData.TDID == null)
                return null;
            return GetSeasonID().GetDesc();
        }
        #endregion

        #region set
        public void RefreshByMonth(bool force = false)
        {
            if (DateTimeMgr == null)
                return;
            var month = DateTimeMgr.CurDateTime.Month;
            if (month == 3 || month == 4 || month == 5)
            {
                Change(SeasonType.Spring, force);
            }
            else if (month == 6 || month == 7 || month == 8)
            {
                Change(SeasonType.Summer, force);
            }
            else if (month == 9 || month == 10 || month == 11)
            {
                Change(SeasonType.Fall, force);
            }
            else if (month == 12 || month == 1 || month == 2)
            {
                Change(SeasonType.Winter, force);
            }
            else
            {
                Change(SeasonType.Spring, force);
            }
        }
        public void Change(SeasonType type, bool isForce = false)
        {
            if (!isForce)
            {
                if (type == SeasonType)
                    return;

            }
            SeasonType = type;
            CurData = RandUtil.RandArray(Data[type]);
            if (sunTween != null) DOTween.Kill(sunTween);
            if (snowTween != null) DOTween.Kill(snowTween);
            sunTween = DOTween.To(() => SunObj.Obj.intensity, x => SunObj.Obj.intensity = x, CurData.SunIntensity, 1.0f);
            ChangeWindPower(CurData.WindzonePower);
            if (IsShowSnow)
            {
                snowTween = DOTween.To(() => ActiveTerrainMat.GetFloat("_SnowAmount"), x => ActiveTerrainMat.SetFloat("_SnowAmount", x), CurData.AccumulatedSnow, 1.0f);
            }
            Callback_OnSeasonChanged?.Invoke(type, CurData.Type);
            OnSeasonChanged(type, CurData.Type);
        }
        public void Next()
        {
            int val = (int)SeasonType + 1;
            if (val > (int)SeasonType.Winter)
            {
                val = 0;
            }
            Change((SeasonType)val);
        }
        public void ChangeWindPower(float power)
        {
            if (Wind) Wind.windMain = power * (1 + WindPowerAdt);
        }
        #endregion

        #region Callback
        protected virtual void OnSeasonChanged(SeasonType type, SubSeasonType subType)
        {

        }
        #endregion
    }
}