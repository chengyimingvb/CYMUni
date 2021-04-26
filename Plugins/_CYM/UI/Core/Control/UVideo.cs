//------------------------------------------------------------------------------
// BaseVideo.cs
// Copyright 2020 2020/3/15 
// Created by CYM on 2020/3/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Video;

namespace CYM.UI
{
    public class UVideoData : UPresenterData
    {
        public Func<VideoClip> Video;
        public string VideoStr;
    }
    [AddComponentMenu("UI/Control/UVideo")]
    [HideMonoScript]
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class UVideo : UPresenter<UVideoData>
    {
        #region Inspector
        [SerializeField, FoldoutGroup("Inspector")]
        VideoPlayer VideoPlayer;
        [SerializeField, FoldoutGroup("Inspector")]
        UnityEngine.UI.RawImage Image;
        #endregion

        public bool IsPrepared => VideoPlayer.isPrepared;
        public bool IsPreparing { get; private set; } = false;

        protected override void Awake()
        {
            VideoPlayer = GO.GetComponent<VideoPlayer>();
            Image = GO.GetComponent<UnityEngine.UI.RawImage>();
            base.Awake();
            VideoPlayer.source = VideoSource.VideoClip;
            VideoPlayer.playOnAwake = false;
            VideoPlayer.prepareCompleted += Prepared;
        }

        public override void Refresh()
        {
            base.Refresh();
            if (Data.Video != null)
                VideoPlayer.clip = Data.Video.Invoke();
            else if (!Data.VideoStr.IsInv())
                VideoPlayer.clip = Data.VideoStr.GetVideoClip();
        }
        public override void OnShow(bool isShow)
        {
            base.OnShow(isShow);
            if (isShow)
            {
                Play();
            }
        }
        protected override void OnClose()
        {
            base.OnClose();
            Stop();
        }

        public void Play(VideoClip clip = null)
        {
            if (IsPlaying()) return;
            if (clip != null) VideoPlayer.clip = clip;
            if (VideoPlayer.clip == null) return;
            if (!VideoPlayer.isPrepared)
            {
                IsPreparing = true;
                VideoPlayer.Prepare();
            }
            else VideoPlayer.Play();

        }
        public void Stop()
        {
            VideoPlayer.Stop();
        }

        public bool IsPlaying()
        {
            return VideoPlayer.isPlaying || IsPreparing;
        }

        void Prepared(VideoPlayer vPlayer)
        {
            IsPreparing = false;
            Image.texture = VideoPlayer.texture;
            vPlayer.Play();
        }

    }
}