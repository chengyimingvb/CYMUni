//------------------------------------------------------------------------------
// BaseDateMgr.cs
// Copyright 2018 2018/11/10 
// Created by CYM on 2018/11/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace CYM
{
    public enum DateTimeAgeType
    {
        AD,
        BC,
    }
    public class BaseDateTimeMgr : BaseGFlowMgr
    {
        #region prop
        const string space = Const.UCD_NoBreakingSpace;
        readonly string DateStrFormat = "{0}"+ space + "{1}.{2}.{3}";
        readonly string DateStrFormat_Year = "{0}"+ space + "{1}";
        readonly string DateStrFormat_Month = "{0}"+ space + "{1}{3}"+ space + "{2}{4}";
        public DateTimeAgeType CurDateTimeAgeType { get; protected set; } = DateTimeAgeType.BC;
        public DateTimeAgeType StartDateTimeAgeType { get; protected set; } = DateTimeAgeType.BC;
        public DateTime CurDateTime { get; protected set; } = new DateTime(1, 3, 1);
        public DateTime StartDateTime { get; protected set; } = new DateTime(1, 1, 1);
        int PreMonth { get; set; }
        int PreYear { get; set; }
        int PreDay { get; set; }
        #endregion

        #region Callback
        public event Callback Callback_OnDayChanged;
        public event Callback Callback_OnMonthChanged;
        public event Callback Callback_OnYearChanged;
        #endregion

        #region set
        public void InitStartTime(DateTime startTime, DateTimeAgeType type)
        {
            StartDateTime = startTime;
            StartDateTimeAgeType = type;
        }
        public void InitTime(DateTime curTimeDate)
        {
            CurDateTime = curTimeDate;
        }
        public void AddDay(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddDays(val);
            CheckChange();
        }
        public void AddMonth(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddMonths(val);
            CheckChange();
        }
        public void AddYear(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddYears(val);
            CheckChange();
        }
        #endregion

        #region get
        string GetTimeAgeType()
        {
            string dateTypeStr = "AD";
            if (CurDateTimeAgeType == DateTimeAgeType.BC)
                dateTypeStr = "BC";
            return dateTypeStr;
        }
        public int GetYear()
        {
            int curYear = 0;
            if (CurDateTimeAgeType == DateTimeAgeType.BC)
                curYear = (StartDateTime.Year - CurDateTime.Year + 1);
            else if (CurDateTimeAgeType == DateTimeAgeType.AD)
            {
                if (StartDateTimeAgeType == DateTimeAgeType.AD)
                    curYear = (StartDateTime.Year + CurDateTime.Year - 1);
                else
                    curYear = (CurDateTime.Year - StartDateTime.Year - 1);
            }
            curYear = Mathf.Clamp(curYear, 0, int.MaxValue);
            return curYear;
        }
        public string GetCurDateStr()
        {

            return string.Format(DateStrFormat, GetTimeAgeType(), GetYear(), CurDateTime.Month, CurDateTime.Day);
        }
        public string GetCurYear()
        {
            return string.Format(DateStrFormat_Year, GetTimeAgeType(), GetYear());
        }
        public string GetCurYearMonth()
        {
            return string.Format(
                DateStrFormat_Month,
                GetTimeAgeType(),
                GetYear(),
                CurDateTime.Month,
                BaseLanguageMgr.Get("Unit_年"),
                BaseLanguageMgr.Get("Unit_月"));
        }
        #endregion

        #region utile
        void RecodePreDate()
        {
            PreMonth = CurDateTime.Month;
            PreYear = CurDateTime.Year;
            PreDay = CurDateTime.Day;
        }
        void CheckChange()
        {
            bool isDayChanged = CurDateTime.Day != PreDay;
            bool isMonthChanged = CurDateTime.Month != PreMonth;
            bool isYearChanged = CurDateTime.Year != PreYear;
            if (isDayChanged) Callback_OnDayChanged?.Invoke();
            if (isMonthChanged) Callback_OnMonthChanged?.Invoke();
            if (isYearChanged)
            {
                UpdateDateTimeType();
                Callback_OnYearChanged?.Invoke();
            }
        }
        void UpdateDateTimeType()
        {
            if (CurDateTimeAgeType == DateTimeAgeType.BC &&
                (StartDateTime.Year - CurDateTime.Year) <= 0)
            {
                CurDateTimeAgeType = DateTimeAgeType.AD;
            }
        }
        #endregion
    }
}