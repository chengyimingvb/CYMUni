using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    [AddComponentMenu("UI/Control/UScrollRect")]
    [HideMonoScript]
    public class UScrollRect : UPresenter<UPresenterData>
    {
        #region Inspector
        ScrollRect ScrollRect;
        [SerializeField]
        RectTransform Content;
        [SerializeField]
        Scrollbar Scrollbar;
        [SerializeField]
        ScrollRect.ScrollbarVisibility ScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        [SerializeField]
        bool IsResetPosition = true;
        #endregion

        #region prop
        ContentSizeFitter ContentSizeFitter;
        RectTransform ScrollRectTrans;
        #endregion

        public override bool IsAutoInit => true;
        protected override void Start()
        {
            base.Start();
            ScrollRect = GO.SafeAddComponet<ScrollRect>();
            ScrollRectTrans = ScrollRect.transform as RectTransform;
            ScrollRect.content = Content;
            if (ScrollRect != null)
            {
                if (ScrollRect.vertical)
                    ScrollRect.verticalScrollbar = Scrollbar;
                else if (ScrollRect.horizontal)
                    ScrollRect.horizontalScrollbar = Scrollbar;

                ScrollRect.horizontalScrollbarVisibility = ScrollbarVisibility;
                ScrollRect.verticalScrollbarVisibility = ScrollbarVisibility;
                ScrollRect.movementType = ScrollRect.MovementType.Clamped;
                ScrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
                ScrollRect.inertia = true;
                ScrollRect.decelerationRate = 0.2f;
            }
            if (Scrollbar != null)
            {
                Scrollbar.size = 0.0f;
                Scrollbar.onValueChanged.AddListener(_ScrollBar_OnValueChanged);
            }

            ContentSizeFitter = Content.GetComponentInChildren<ContentSizeFitter>();
            if (ContentSizeFitter != null)
            {
                if (LayoutElement == null)// LayoutElement = Content.gameObject.AddComponent<LayoutElement>();
                    EnsureLayoutElement();
                if (IsVertical) LayoutElement.minHeight = ScrollRectTrans.sizeDelta.y + 100;
                else if (IsHorizontal) LayoutElement.minWidth = ScrollRectTrans.sizeDelta.x + 100;
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Scrollbar) Scrollbar.size = 0.0f;
            if (IsResetPosition)
                ResetScrollBar(1.0f);
        }
        public override void OnViewShow(bool b)
        {
            base.OnViewShow(b);
            if (b)
            {
                if (Scrollbar) Scrollbar.size = 0.0f;
                ResetScrollBar(1.0f);
            }
        }

        private void Update()
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                if (BaseInputMgr.IsScrollWheel) ScrollRect.velocity = Vector2.zero;
                Scrollbar.size = 0.0f;
            }
        }

        public void ResetScrollBar(float val)
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                Scrollbar.value = val;
            }
        }

        private void _ScrollRect_OnValueChanged(Vector2 arg0)
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                Scrollbar.size = 0.0f;
            }
        }

        private void _ScrollBar_OnValueChanged(float arg0)
        {
            if (Scrollbar != null && ScrollRect != null)
            {
                Scrollbar.size = 0.0f;
            }
        }

        #region is
        public bool IsVertical
        {
            get
            {
                if (Scrollbar == null) return false;
                return Scrollbar.direction == Scrollbar.Direction.BottomToTop || Scrollbar.direction == Scrollbar.Direction.TopToBottom;
            }
        }
        public bool IsHorizontal
        {
            get
            {
                if (Scrollbar == null) return false;
                return Scrollbar.direction == Scrollbar.Direction.LeftToRight || Scrollbar.direction == Scrollbar.Direction.RightToLeft;
            }
        }
        #endregion
    }

}