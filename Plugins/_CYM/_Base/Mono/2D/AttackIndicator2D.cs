//------------------------------------------------------------------------------
// AttackIndicator2D.cs
// Copyright 2020 2020/2/20 
// Created by CYM on 2020/2/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public class AttackIndicator2D : BaseMono
    {
        [SerializeField]
        protected SpriteRenderer Sprite;
        [SerializeField]
        protected Sprite[] AnimSprites;
        //[SerializeField]
        //protected float AnimTime = 0.05f;

        bool IsShow = true;
        float curDetail = 0;
        int AnimIndex = 0;
        //Timer AnimTimer = new Timer(0.05f);

        public override void Awake()
        {
            base.Awake();
            //AnimTimer = new Timer(AnimTime);
        }
        public void Look(BaseMono self, BaseMono mono)
        {
            if (mono != null && self != null && Trans != null)
                Trans.LookAt(mono.Trans, Vector3.forward);
        }
        public void Show(bool b)
        {
            if (IsShow == b) return;
            IsShow = b;
            gameObject.SetActive(b);
        }
        public void RefreshSort(int sort)
        {
            Sprite.sortingOrder = sort;
        }
        public void RefreshIcon(Sprite sprite)
        {
            Sprite.sprite = sprite;
        }
        public void UpdateAnim()
        {
            if (!IsShow) return;
            if (AnimSprites == null || AnimSprites.Length <= 1)
                return;
            RefreshIcon(AnimSprites[AnimIndex]);
            AnimIndex++;
            if (AnimIndex >= AnimSprites.Length)
                AnimIndex = 0;
        }
    }
}