//------------------------------------------------------------------------------
// GlobalUITextMgr.cs
// Copyright 2020 2020/7/19 
// Created by CYM on 2020/7/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public sealed class GlobalUITextMgr : MonoBehaviour 
    {
        #region font text
        static Dictionary<Text, FontType> Texts = new Dictionary<Text, FontType>();
        public static void AddText(Text text, FontType fontType)
        {
            var newFont = UIConfig.Ins.GetFont(fontType);
            if (newFont == null)
                return;
            if (!Texts.ContainsKey(text)) Texts.Add(text, fontType);
            else Texts[text] = fontType;
            if (text.font != newFont)
                text.font = newFont;
        }
        public static void RemoveText(Text text)
        {
            Texts.Remove(text);
        }
        public static void RefreshFont()
        {
            foreach (var item in Texts)
            {
                if (item.Key == null) continue;
                var newFont = UIConfig.Ins.GetFont(item.Value);
                if (newFont == null)
                    return;
                item.Key.font = newFont;
            }
        }
        #endregion
    }
}