//**********************************************
// Class Name	: BaseTooltipView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Invoke;
namespace CYM.UI
{
    public enum Corner : int
    {
        BottomLeft = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3,
    }
    public enum Anchoring
    {
        None,
        Corners,
        LeftOrRight,
        TopOrBottom
    }

    public enum Anchor
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight,
        Left,
        Right,
        Top,
        Bottom
    }
    public class BaseTooltipView : BaseStaticUIView<BaseTooltipView>
    {
        // 当这个标志为true的时候,自动关闭,用于UI控件的OnExit触发
        static bool IsDirtyCloseTip { get; set; } = false;
        public static BaseTooltipView CurShow { get; protected set; }

        #region presenter
        [FoldoutGroup("Inspector"), SerializeField]
        protected UText Text;
        [FoldoutGroup("Data"), SerializeField, Tooltip("是否自动计算ContentSize")]
        protected bool AutoContentSize = true;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 TopOffset = Vector2.zero;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 BottomOffset = Vector2.zero * 20f;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 AnchoredOffset = Vector2.zero;
        [FoldoutGroup("Data"), SerializeField]
        Anchoring Anchoring = Anchoring.Corners;
        [FoldoutGroup("Data"), SerializeField]
        Graphic AnchorGraphic;
        [FoldoutGroup("Data"), SerializeField]
        Vector2 AnchorGraphicOffset = Vector2.zero;
        Vector2? InputMousePos;
        Vector3? InputWorldPos;
        #endregion

        #region prop
        Anchor CurrentAnchor = Anchor.BottomLeft;
        Corner CurrentCorner = Corner.TopLeft;
        string InputDesc = "No Set";
        RectTransform AnchorToTarget;
        ContentSizeFitter SizeFitter;
        float? Width = 0;
        private IJob closeInvoke;
        #endregion

        #region life
        //public override void Awake()
        //{
        //    base.Awake();
        //}
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            SizeFitter = GetComponent<ContentSizeFitter>();
            if (Default == null)
                Default = this;
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!IsCompleteClose)
            {
                this.UpdatePositionAndPivot();
                //有鼠标点击的时候自动关闭
                if (Input.GetMouseButtonDown(0) ||
                    Input.GetMouseButtonDown(1) ||
                    Input.GetMouseButtonDown(2) ||
                    IsDirtyCloseTip)
                {
                    Close();
                }
            }
        }
        #endregion

        #region set
        public override void Show(bool b = true, bool useGroup = true, bool force = false)
        {
            if (b)
            {
                //所有的Tooltip均为互斥
                if (CurShow != null && CurShow != this)
                    CurShow.Show(false, true, true);
                CurShow = this;
                RectTrans.SetAsLastSibling();
                closeInvoke?.Kill();
                base.Show(b, useGroup, force);
            }
            else
            {
                closeInvoke?.Kill();
                if (force)
                {
                    base.Show(b, useGroup, force);
                }
                else
                {
                    closeInvoke = Util.Invoke(() => base.Show(b, useGroup, force), 0.01f);
                }
            }
        }
        public void Show(string key, params object[] ps) => Show(key, null, null, null, null, ps);
        public void ShowFix(string key, params object[] ps) => Show(key, null, null, null, 300, ps);
        public void Show(string key, Vector2 inputPos, params object[] ps) => Show(key, null, inputPos, null, null, ps);
        public void Show(string key, Vector3 worldPos, params object[] ps) => Show(key, null, null, worldPos, null, ps);
        public void ShowStr(string str, Vector3 worldPos) => Show(str, null, null, worldPos, null);
        public void ShowStr(string str) => Show(str, null, null, null, null);
        public void Show(string key, RectTransform AnchorToTarget, Vector2? inputMousePos, Vector3? inputWorldPos, float? width = null, params object[] ps) => Show(GetStr(key, ps), AnchorToTarget, inputMousePos, inputWorldPos, width);
        public void Show(string str, RectTransform AnchorToTarget, Vector2? inputPos, Vector3? inputWorldPos, float? width = null)
        {
            if (str.IsInv()) return;
            InputMousePos = inputPos;
            this.AnchorToTarget = AnchorToTarget;
            InputWorldPos = inputWorldPos;
            Text.NameText = InputDesc = str;
            IsDirtyCloseTip = false;
            Show(true, true, false);
            RefreshLayout(width);
        }
        void RefreshLayout(float? width)
        {
            if (SizeFitter && AutoContentSize)
            {
                Text.NameText = InputDesc;
                LayoutRebuilder.ForceRebuildLayoutImmediate(RectTrans);
                SizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                SizeFitter.SetLayoutHorizontal();

                if (width != null)
                {
                    RectTrans.sizeDelta = new Vector2(Width.Value, RectTrans.sizeDelta.y);
                    Width = width;
                }
                else
                {
                    float max = 400;
                    float min = 100;
                    if (RectTrans.sizeDelta.x > max)
                    {
                        SizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        RectTrans.sizeDelta = new Vector2(max, RectTrans.sizeDelta.y);
                        Width = max;
                    }
                    else if (RectTrans.sizeDelta.x < min)
                    {
                        SizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        RectTrans.sizeDelta = new Vector2(min, RectTrans.sizeDelta.y);
                        Width = min;
                    }
                    else
                    {
                        SizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        RectTrans.sizeDelta = new Vector2(RectTrans.sizeDelta.x, RectTrans.sizeDelta.y);
                    }
                }
                SizeFitter.SetLayoutHorizontal();
                SizeFitter.SetLayoutVertical();
            }
        }
        #endregion

        #region get
        private Vector2 GetShowPos()
        {
            if (InputMousePos != null)
                return InputMousePos.Value;
            else if (InputWorldPos != null)
                return BaseGlobal.MainCamera.WorldToScreenPoint(InputWorldPos.Value);
            return Input.mousePosition;

        }
        #endregion

        #region utile
        /// <summary>
        /// Gets the camera responsible for the tooltip.
        /// </summary>
        /// <value>The camera.</value>
        public Camera UICamera
        {
            get
            {
                if (this.RootView.Canvas == null)
                    return null;

                if (this.RootView.Canvas.renderMode == RenderMode.ScreenSpaceOverlay || (this.RootView.Canvas.renderMode == RenderMode.ScreenSpaceCamera && this.RootView.Canvas.worldCamera == null))
                {
                    return null;
                }

                return (!(this.RootView.Canvas.worldCamera != null)) ? BaseGlobal.MainCamera : this.RootView.Canvas.worldCamera;
            }
        }

        /// <summary>
        /// Updates the tooltip position.
        /// </summary>
        protected virtual void UpdatePositionAndPivot()
        {
            // Update the tooltip pivot
            this.UpdatePivot();

            // Update the tooltip position to the mosue position
            // If the tooltip is not anchored to a target
            if (this.AnchorToTarget == null)
            {
                // Convert the offset based on the pivot
                Vector2 pivotBasedOffset = Vector2.zero;

                if (CurrentCorner == Corner.BottomLeft ||
                    CurrentCorner == Corner.BottomRight)
                {
                    pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.TopOffset.x * -1f) : this.TopOffset.x),
                                                           ((this.RectTrans.pivot.y == 1f) ? (this.TopOffset.y * -1f) : this.TopOffset.y));
                }
                else if (CurrentCorner == Corner.TopLeft ||
                    CurrentCorner == Corner.TopLeft)
                {

                    pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.BottomOffset.x * -1f) : this.BottomOffset.x),
                                       ((this.RectTrans.pivot.y == 1f) ? (this.BottomOffset.y * -1f) : this.BottomOffset.y));
                }

                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RootView.Canvas.transform as RectTransform, GetShowPos(), this.UICamera, out localPoint))
                {
                    this.RectTrans.anchoredPosition = pivotBasedOffset + localPoint;
                }
            }

            // Check if we are anchored to a target
            else
            {
                if (this.Anchoring == Anchoring.Corners)
                {
                    // Set the anchor position to the opposite of the tooltip's pivot
                    Vector3[] targetWorldCorners = new Vector3[4];
                    this.AnchorToTarget.GetWorldCorners(targetWorldCorners);

                    // Convert the tooltip pivot to corner
                    Corner pivotCorner = VectorPivotToCorner(this.RectTrans.pivot);

                    // Get the opposite corner of the pivot corner
                    Corner oppositeCorner = GetOppositeCorner(pivotCorner);

                    // Convert the offset based on the pivot
                    Vector2 pivotBasedOffset = Vector2.zero;
                    pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.AnchoredOffset.x * -1f) : this.AnchoredOffset.x),
                                                           ((this.RectTrans.pivot.y == 1f) ? (this.AnchoredOffset.y * -1f) : this.AnchoredOffset.y));

                    // Get the anchoring point
                    Vector2 anchorPoint = this.RootView.Canvas.transform.InverseTransformPoint(targetWorldCorners[(int)oppositeCorner]);

                    // Apply anchored position
                    this.RectTrans.anchoredPosition = pivotBasedOffset + anchorPoint;
                }
                else if (this.Anchoring == Anchoring.LeftOrRight || this.Anchoring == Anchoring.TopOrBottom)
                {
                    Vector3[] targetWorldCorners = new Vector3[4];
                    AnchorToTarget.GetWorldCorners(targetWorldCorners);

                    Vector2 topleft = this.RootView.Canvas.transform.InverseTransformPoint(targetWorldCorners[1]);

                    if (this.Anchoring == Anchoring.LeftOrRight)
                    {
                        Vector2 pivotBasedOffset = new Vector2(((this.RectTrans.pivot.x == 1f) ? (this.AnchoredOffset.x * -1f) : this.AnchoredOffset.x), this.AnchoredOffset.y);

                        if (this.RectTrans.pivot.x == 0f)
                        {
                            this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(this.AnchorToTarget.rect.width, (this.AnchorToTarget.rect.height / 2f) * -1f);
                        }
                        else
                        {
                            this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(0f, (this.AnchorToTarget.rect.height / 2f) * -1f);
                        }
                    }
                    else if (this.Anchoring == Anchoring.TopOrBottom)
                    {
                        Vector2 pivotBasedOffset = new Vector2(this.AnchoredOffset.x, ((this.RectTrans.pivot.y == 1f) ? (this.AnchoredOffset.y * -1f) : this.AnchoredOffset.y));

                        if (this.RectTrans.pivot.y == 0f)
                        {
                            this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(this.AnchorToTarget.rect.width / 2f, 0f);
                        }
                        else
                        {
                            this.RectTrans.anchoredPosition = topleft + pivotBasedOffset + new Vector2(this.AnchorToTarget.rect.width / 2f, this.AnchorToTarget.rect.height * -1f);
                        }
                    }
                }
            }

            // Fix position to nearest even number
            this.RectTrans.anchoredPosition = new Vector2(Mathf.Round(this.RectTrans.anchoredPosition.x), Mathf.Round(this.RectTrans.anchoredPosition.y));
            this.RectTrans.anchoredPosition = new Vector2(this.RectTrans.anchoredPosition.x + (this.RectTrans.anchoredPosition.x % 2f), this.RectTrans.anchoredPosition.y + (this.RectTrans.anchoredPosition.y % 2f));
        }

        /// <summary>
        /// Updates the pivot.
        /// </summary>
        protected void UpdatePivot()
        {
            //if (Anchoring == Anchoring.None)
            //    return;
            // Get the mouse position
            Vector3 targetPosition = GetShowPos();
            if (Anchoring == Anchoring.None)
            {
                Vector2 corner = new Vector2(0f, 1f);
                // Set the pivot
                this.SetPivot(VectorPivotToCorner(corner));
            }
            else if (this.Anchoring == Anchoring.Corners)
            {
                // Determine which corner of the screen is closest to the mouse position
                Vector2 corner = new Vector2(
                    ((targetPosition.x > (Screen.width / 2f)) ? 1f : 0f),
                    ((targetPosition.y > (Screen.height / 2f)) ? 1f : 0f)
                );

                // Set the pivot
                this.SetPivot(VectorPivotToCorner(corner));
            }
            else if (this.Anchoring == Anchoring.LeftOrRight)
            {
                // Determine the pivot
                Vector2 pivot = new Vector2(((targetPosition.x > (Screen.width / 2f)) ? 1f : 0f), 0.5f);

                // Set the pivot
                this.SetPivot(pivot);
            }
            else if (this.Anchoring == Anchoring.TopOrBottom)
            {
                // Determine the pivot
                Vector2 pivot = new Vector2(0.5f, ((targetPosition.y > (Screen.height / 2f)) ? 1f : 0f));

                // Set the pivot
                this.SetPivot(pivot);
            }
        }

        /// <summary>
        /// Sets the pivot.
        /// </summary>
        /// <param name="pivot">The pivot.</param>
        protected void SetPivot(Vector2 pivot)
        {
            // Update the pivot
            this.RectTrans.pivot = pivot;

            // Update the current anchor value
            this.CurrentAnchor = VectorPivotToAnchor(pivot);

            // Update the anchor graphic position to the new pivot point
            this.UpdateAnchorGraphicPosition();
        }

        /// <summary>
        /// Sets the pivot corner.
        /// </summary>
        /// <param name="point">Point.</param>
        protected void SetPivot(Corner point)
        {
            CurrentCorner = point;
            // Update the pivot
            switch (point)
            {
                case Corner.BottomLeft:
                    this.RectTrans.pivot = new Vector2(0f, 0f);
                    break;
                case Corner.BottomRight:
                    this.RectTrans.pivot = new Vector2(1f, 0f);
                    break;
                case Corner.TopLeft:
                    this.RectTrans.pivot = new Vector2(0f, 1f);
                    break;
                case Corner.TopRight:
                    this.RectTrans.pivot = new Vector2(1f, 1f);
                    break;
            }

            // Update the current anchor value
            this.CurrentAnchor = VectorPivotToAnchor(this.RectTrans.pivot);

            // Update the anchor graphic position to the new pivot point
            this.UpdateAnchorGraphicPosition();
        }

        protected void UpdateAnchorGraphicPosition()
        {
            if (this.AnchorGraphic == null)
                return;

            // Get the rect transform
            RectTransform rt = (this.AnchorGraphic.transform as RectTransform);
            if (this.Anchoring == Anchoring.None)
            {
                rt.pivot = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.anchorMin = new Vector2(0f, 1f);
                rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, (this.AnchorGraphicOffset.y * -1f) - rt.rect.height, rt.localPosition.z);
                rt.localScale = new Vector3(1f, -1f, rt.localScale.z);
            }
            else if (this.Anchoring == Anchoring.Corners)
            {
                // Pivot should always be bottom left
                rt.pivot = Vector2.zero;

                // Update it's anchor to the tooltip's pivot
                rt.anchorMax = this.RectTrans.pivot;
                rt.anchorMin = this.RectTrans.pivot;

                // Update it's local position to the defined offset
                rt.localPosition = new Vector3(((this.RectTrans.pivot.x == 1f) ? (this.AnchorGraphicOffset.x * -1f) : this.AnchorGraphicOffset.x),
                                               ((this.RectTrans.pivot.y == 1f) ? (this.AnchorGraphicOffset.y * -1f) : this.AnchorGraphicOffset.y),
                                               rt.localPosition.z);

                // Flip the anchor graphic based on the pivot
                rt.localScale = new Vector3(((this.RectTrans.pivot.x == 0f) ? 1f : -1f), ((this.RectTrans.pivot.y == 0f) ? 1f : -1f), rt.localScale.z);
            }
            else if (this.Anchoring == Anchoring.LeftOrRight || this.Anchoring == Anchoring.TopOrBottom)
            {
                switch (this.CurrentAnchor)
                {
                    case Anchor.Left:
                        rt.pivot = new Vector2(0f, 0.5f);
                        rt.anchorMax = new Vector2(0f, 0.5f);
                        rt.anchorMin = new Vector2(0f, 0.5f);
                        rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, this.AnchorGraphicOffset.y, rt.localPosition.z);
                        rt.localScale = new Vector3(1f, 1f, rt.localScale.z);
                        break;
                    case Anchor.Right:
                        rt.pivot = new Vector2(1f, 0.5f);
                        rt.anchorMax = new Vector2(1f, 0.5f);
                        rt.anchorMin = new Vector2(1f, 0.5f);
                        rt.localPosition = new Vector3((this.AnchorGraphicOffset.x * -1f) - rt.rect.width, this.AnchorGraphicOffset.y, rt.localPosition.z);
                        rt.localScale = new Vector3(-1f, 1f, rt.localScale.z);
                        break;
                    case Anchor.Bottom:
                        rt.pivot = new Vector2(0.5f, 0f);
                        rt.anchorMax = new Vector2(0.5f, 0f);
                        rt.anchorMin = new Vector2(0.5f, 0f);
                        rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, this.AnchorGraphicOffset.y, rt.localPosition.z);
                        rt.localScale = new Vector3(1f, 1f, rt.localScale.z);
                        break;
                    case Anchor.Top:
                        rt.pivot = new Vector2(0.5f, 1f);
                        rt.anchorMax = new Vector2(0.5f, 1f);
                        rt.anchorMin = new Vector2(0.5f, 1f);
                        rt.localPosition = new Vector3(this.AnchorGraphicOffset.x, (this.AnchorGraphicOffset.y * -1f) - rt.rect.height, rt.localPosition.z);
                        rt.localScale = new Vector3(1f, -1f, rt.localScale.z);
                        break;
                }
            }
        }
        #endregion

        #region Callback

        #endregion

        #region Static Methods
        /// <summary>
        /// Convert vector pivot to corner.
        /// </summary>
        /// <returns>The corner.</returns>
        /// <param name="pivot">Pivot.</param>
        Corner VectorPivotToCorner(Vector2 pivot)
        {
            // Pivot to that corner
            if (pivot.x == 0f && pivot.y == 0f)
            {
                return Corner.BottomLeft;
            }
            else if (pivot.x == 0f && pivot.y == 1f)
            {
                return Corner.TopLeft;
            }
            else if (pivot.x == 1f && pivot.y == 0f)
            {
                return Corner.BottomRight;
            }

            // 1f, 1f
            return Corner.TopRight;
        }

        /// <summary>
        /// Convert vector pivot to anchor.
        /// </summary>
        /// <returns>The anchor.</returns>
        /// <param name="pivot">Pivot.</param>
        public Anchor VectorPivotToAnchor(Vector2 pivot)
        {
            // Pivot to anchor
            if (pivot.x == 0f && pivot.y == 0f)
            {
                return Anchor.BottomLeft;
            }
            else if (pivot.x == 0f && pivot.y == 1f)
            {
                return Anchor.TopLeft;
            }
            else if (pivot.x == 1f && pivot.y == 0f)
            {
                return Anchor.BottomRight;
            }
            else if (pivot.x == 0.5f && pivot.y == 0f)
            {
                return Anchor.Bottom;
            }
            else if (pivot.x == 0.5f && pivot.y == 1f)
            {
                return Anchor.Top;
            }
            else if (pivot.x == 0f && pivot.y == 0.5f)
            {
                return Anchor.Left;
            }
            else if (pivot.x == 1f && pivot.y == 0.5f)
            {
                return Anchor.Right;
            }

            // 1f, 1f
            return Anchor.TopRight;
        }

        /// <summary>
        /// Gets the opposite corner.
        /// </summary>
        /// <returns>The opposite corner.</returns>
        /// <param name="corner">Corner.</param>
        public Corner GetOppositeCorner(Corner corner)
        {
            switch (corner)
            {
                case Corner.BottomLeft:
                    return Corner.TopRight;
                case Corner.BottomRight:
                    return Corner.TopLeft;
                case Corner.TopLeft:
                    return Corner.BottomRight;
                case Corner.TopRight:
                    return Corner.BottomLeft;
            }

            // Default
            return Corner.BottomLeft;
        }

        /// <summary>
        /// Gets the opposite anchor.
        /// </summary>
        /// <returns>The opposite anchor.</returns>
        /// <param name="anchor">Anchor.</param>
        public Anchor GetOppositeAnchor(Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.BottomLeft:
                    return Anchor.TopRight;
                case Anchor.BottomRight:
                    return Anchor.TopLeft;
                case Anchor.TopLeft:
                    return Anchor.BottomRight;
                case Anchor.TopRight:
                    return Anchor.BottomLeft;
                case Anchor.Top:
                    return Anchor.Bottom;
                case Anchor.Bottom:
                    return Anchor.Top;
                case Anchor.Left:
                    return Anchor.Right;
                case Anchor.Right:
                    return Anchor.Left;
            }

            // Default
            return Anchor.BottomLeft;
        }
        #endregion

        #region Inspector
        protected override bool Inspector_HideGroup()
        {
            return true;
        }
        protected override bool Inspector_HideIsExclusive()
        {
            return true;
        }
        #endregion
    }
}