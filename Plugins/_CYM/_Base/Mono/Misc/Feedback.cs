//------------------------------------------------------------------------------
// Feedback.cs
// Copyright 2021 2021/3/21 
// Created by CYM on 2021/3/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class Feedback : MonoBehaviour
    {
        static BuildConfig BuildConfig => BuildConfig.Ins;
        static private string SendTitle = "";
        static private string SendDesc = "";
        #region set
        public static void SendError(string desc, string contactInfo, bool isSendAchieve = false)
        {
            string realDesc = getDesc("错误信息", desc, contactInfo);
            //简体中文地区自动发送邮件
            if (BaseLanguageMgr.LanguageType == LanguageType.Chinese)
            {
                string filePath = null;
                if (isSendAchieve)
                {
                    if (BaseGlobal.BattleMgr.IsInBattle)
                    {
                        BaseGlobal.DBMgr.SaveTemp();
                        filePath = BaseGlobal.DBMgr.GetTempSavePath();
                    }
                }
                CMail.Send(IMUIErrorCatcher.GetTitle(), realDesc, filePath);
            }
        }
        public static void SendMail(string title, string desc, string contactInfo)
        {
            if (title.IsInv()) return;
            string realDesc = getDesc(title, desc, contactInfo);
            CMail.Send(title, realDesc, null);
        }
        #endregion

        #region get
        static string getDesc(string title, string desc, string contactInfo)
        {
            SendTitle = title;
            SendDesc = desc;
            string realDesc = string.Format(
                $"{MarkdownUtil.H2("Version")}:\n{BuildConfig.FullVersionName}\n" +
                $"{MarkdownUtil.H2("Contact")}:\n{contactInfo}\n" +
                $"{MarkdownUtil.H2("GMMode")}:\n{BaseGlobal.DiffMgr.IsSettedGMMod()}\n" +
                $"{MarkdownUtil.H2("Name")}:\n{BaseGlobal.PlatSDKMgr.GetName()}\n" +
                $"{MarkdownUtil.H2("Desc")}:\n{SendDesc}\n" +
                $"{MarkdownUtil.H2("Error")}:\n{IMUIErrorCatcher.GetErrorString()}\n" +
                $"{MarkdownUtil.H2("SystemInfo")}:\n{Util.AdvSystemInfo}"
                );
            return realDesc;
        }
        #endregion
    }
}