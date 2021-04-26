using CYM.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//**********************************************
// Class Name	: CYMBaseScreenController
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public partial class UIUtil:BaseUIUtil
    {

        #region UIFormat
        // 条件
        public static string Condition(bool isTrue, string str)
        {
            if (isTrue)
                return Const.STR_Indent + "<Color=" + Const.COL_Green + ">" + str + "</Color>";
            return Const.STR_Indent + "<Color=" + Const.COL_Red + ">" + str + "</Color>";
        }
        // 标题内容
        public static string TitleContent(string title, string content) => string.Format("{0}\n------------------------\n{1}", title, content);
        // 短日期
        public static string DateTimeShort(DateTime date) => date.ToString("M月d日 HH:mm");
        // 日期
        public static string TimeSpan(TimeSpan span)
        {
            int totalHours = span.Days * 24 + span.Hours;
            if (totalHours > 0)
            {
                return string.Format("{1}{0}{2}{0}{3}{0}{4}", BaseLanguageMgr.Space, totalHours, BaseLanguageMgr.Get("Unit_小时"), span.Minutes, BaseLanguageMgr.Get("Unit_分钟"));
            }
            else
            {
                return string.Format("{1}{0}{2}", BaseLanguageMgr.Space, span.Minutes, BaseLanguageMgr.Get("Unit_分钟"));
            }
        }
        // buff后缀
        public static string BuffSuffix(string str) => string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_Buff"));
        // Attr name 后缀
        public static string AttrTypeNameSuffix(string str, Enum type) => string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, type.GetName());
        // 天后缀
        public static string DaySuffix(string str) => string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_天"));
        // 天后缀
        public static string MonthSuffix(string str) => string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_月"));
        // 天后缀
        public static string YearSuffix(string str) => string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_年"));
        //分数
        public static string Fraction(float val, float denominator, bool isInt = false)
        {
            if (isInt) return Mathf.RoundToInt(val).ToString() + "/" + Mathf.RoundToInt(denominator).ToString();
            else return UIUtil.OneD(val) + "/" + UIUtil.OneD(denominator);
        }
        public static string FractionCol(float val, float denominator, bool isInt = false, bool onlyRed = false)
        {
            string ret = "";
            if (isInt)
                ret = ((int)val).ToString() + "/" + ((int)denominator).ToString();
            else
                ret = UIUtil.OneD(val) + "/" + UIUtil.OneD(denominator);

            if (val > denominator) return Red(ret);
            if (!onlyRed)
            {
                if (val == denominator) return Yellow(ret);
                return Green(ret);
            }
            else
            {
                return ret;
            }
        }
        #endregion

        #region other
        public static string GetStr(string key, params object[] ps) => BaseLanguageMgr.Get(key, ps);
        public static string GetPath(GameObject go) => go.transform.parent == null ? "/" + go.name : GetPath(go.transform.parent.gameObject) + "/" + go.name;
        // Finds the component in the game object's parents.
        public static T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null)
                return null;

            var comp = go.GetComponent<T>();

            if (comp != null)
                return comp;

            Transform t = go.transform.parent;

            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }

            return comp;
        }
        public static IList GetTestScrollData(int count=30)
        {
            List<object> test = new List<object>();
            Util.For(count, (i) => test.Add(new object()));
            return test;
        }
        #endregion    
    }

}