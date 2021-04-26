//------------------------------------------------------------------------------
// BaseSaveOrLoadView.cs
// Copyright 2019 2019/5/27 
// Created by CYM on 2019/5/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM
{
    public enum SaveOrLoad
    {
        Save,
        Load,
    }
    public class BaseSaveOrLoadView : UUIView
    {
        #region Inspector
        [FoldoutGroup("Inspector"), SerializeField]
        UDupplicate DPTabs;
        [FoldoutGroup("Inspector"), SerializeField]
        UScroll LocalScroll;
        [FoldoutGroup("Inspector"), SerializeField]
        UScroll RemoteScroll;
        [FoldoutGroup("Inspector"), SerializeField]
        UInputField InputField;
        [FoldoutGroup("Inspector"), SerializeField]
        UButton BntSaveOrLoad;
        #endregion

        #region prop
        IBaseDBMgr DBMgr => BaseGlobal.DBMgr;
        SaveOrLoad SaveOrLoad = SaveOrLoad.Load;
        IBaseArchiveFile CurAchieve;
        IBaseBattleMgr BattleMgr => BaseGlobal.BattleMgr;
        #endregion

        #region life
        protected override string TitleKey => "Title_存档";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            DPTabs.Init<UCheckBox, UCheckBoxData>(
                new UCheckBoxData { NameKey = "Text_本地存档", LinkControl = LocalScroll, OnClick = OnClickDPTabs },
                new UCheckBoxData { NameKey = "Text_远程存档", LinkControl = RemoteScroll, OnClick = OnClickDPTabs }
            );
            LocalScroll.Init(
                GetLocalData,
                OnRefreshAchieve
            );
            RemoteScroll.Init(
                GetRemoteData,
                OnRefreshAchieve
            );
            BntSaveOrLoad.Init(new UButtonData()
            {
                Name = () =>GetStr(SaveOrLoad == SaveOrLoad.Load ? "Bnt_加载" : "Bnt_保存"),
                OnClick = OnBntSaveOrLoad,
                IsInteractable = GetBntSaveOrLoadInteractable
            });
        }
        public override void Refresh()
        {
            base.Refresh();
            RefreshKeyElement();
        }
        public void Show(SaveOrLoad saveOrLoad)
        {
            SaveOrLoad = saveOrLoad;
            Show(true);
        }
        #endregion

        #region get
        protected virtual Sprite GetArchiveIcon(IBaseArchiveFile file)
        {
            return null;
        }
        protected virtual void OnArchiveItemRefresh(BaseSaveOrLoadItem item, IBaseArchiveFile file)
        {

        }
        #endregion

        #region utile
        private IList GetLocalData()
        {
            return GetData(true);
        }
        private IList GetRemoteData()
        {
            return GetData(false);
        }
        private IList GetData(bool isLocal)
        {
            List<object> ret = new List<object>();
            var item = DBMgr.GetBaseAchieveMgr(isLocal);
            var all = item.GetAllBaseArchives(true);
            foreach (var archive in all)
            {
                ret.Add(archive);
            }
            return ret;
        }
        void OnRefreshAchieve(object p, object d)
        {
            BaseSaveOrLoadItem item = p as BaseSaveOrLoadItem;
            IBaseArchiveFile itemData = d as IBaseArchiveFile;
            if (item.Text) item.Text.text = itemData.Name;
            if (item.Time) item.Time.text = itemData.SaveTime.ToShortDateString();
            if (item.Duration) item.Duration.text = itemData.PlayTime.ToString();
            if (item.BntClose) item.BntClose.Data.OnClick = OnClickDelete;
            item.Data.OnClick = OnSaveOrLoadItemClick;

            bool IsInData = BuildConfig.Ins.IsInData(itemData.Header.Version);
            if (!IsInData) item.Text.text = string.Format($"<color=red>{"!"}</color>{item.Text.text}");
            if (item.ArchiveIcon) item.ArchiveIcon.overrideSprite = GetArchiveIcon(itemData);

            OnArchiveItemRefresh(item, itemData);
        }
        IBaseArchiveFile GetArchiveFile(int index = Const.INT_Inv)
        {
            if (index.IsInv())
            {
                if (DPTabs.CurSelectIndex == 0)
                    return LocalScroll.GetData<IBaseArchiveFile>();
                else
                    return RemoteScroll.GetData<IBaseArchiveFile>();
            }
            else
            {
                if (DPTabs.CurSelectIndex == 0)
                    return LocalScroll.GetData<IBaseArchiveFile>(index);
                else
                    return RemoteScroll.GetData<IBaseArchiveFile>(index);
            }
        }
        void RefreshKeyElement()
        {
            DBMgr.UseRemoteArchives(DPTabs.CurSelectIndex == 0 ? false : true);
            CurAchieve = GetArchiveFile();
            if (SaveOrLoad == SaveOrLoad.Load)
            {
                InputField.EnableInput(false);
                if (CurAchieve != null)
                    InputField.InputText = CurAchieve.Name;
                else
                    InputField.InputText = "None";
            }
            else
            {
                InputField.EnableInput(true);
                if (CurAchieve != null)
                    InputField.InputText = CurAchieve.Name;
                else
                    InputField.InputText = DBMgr.GetDefaultSaveName();
            }
            //刷新按钮
            BntSaveOrLoad.Refresh();
        }
        #endregion

        #region Callback
        private void OnClickDPTabs(UControl arg1, PointerEventData arg2)
        {
            SetDirtyRefresh();
        }
        private void OnSaveOrLoadItemClick(UControl arg1, PointerEventData arg2)
        {
            SetDirtyRefresh();
        }

        private void OnBntSaveOrLoad(UControl arg1, PointerEventData arg2)
        {
            if (SaveOrLoad == SaveOrLoad.Save)
            {
                if (InputField.InputText.IsInv())
                    return;

                if (DBMgr.IsHaveSameArchives(InputField.InputText))
                {
                    BaseModalBoxView.Default?.ShowOKCancle("Msg_覆盖存档", "Msg_覆盖存档Desc",
                        () =>
                        {
                            DBMgr.SaveCurGameAs(InputField.InputText,false,false,true,false);
                            SetDirtyData();
                        },
                        null
                        );
                }
                else
                {
                    DBMgr.SaveCurGameAs(InputField.InputText, false, false, true, false);
                    SetDirtyData();
                }
            }
            else
            {
                BattleMgr.LoadGame(InputField.InputText);
                Close();
            }
        }
        private void OnClickDelete(UControl arg1, PointerEventData arg2)
        {
            BaseModalBoxView.Default?.ShowOKCancle("Msg_删除存档", "Msg_删除存档Desc",
                   () =>
                   {
                       DBMgr.DeleteArchives(GetArchiveFile(arg1.Index).Name);
                       SetDirtyData();
                   },
                   null
                   );
        }
        private bool GetBntSaveOrLoadInteractable(int arg)
        {
            if (SaveOrLoad == SaveOrLoad.Load)
            {
                if (CurAchieve == null)
                    return false;
                return true;
            }
            else
            {

            }

            return true;
        }
        #endregion
    }
}