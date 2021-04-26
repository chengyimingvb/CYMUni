using UnityEngine;

/// <summary>
/// ISRTS Camera
/// Created by SPINACH.
/// 
/// © 2013 - 2016 SPINACH All rights reserved.
/// </summary>
namespace CYM.Cam
{
    public class RTSCamera2D : MonoBehaviour
    {

        public float scrollValue;
        public bool unlockWhenMove;
        [SerializeField]
        public Range zoomRange;

        [SerializeField]
        public float movementLerpSpeed;

        #region bound
        public Rect bound;
        public Rect maxBound;
        float bound_yMax { get; set; }
        float bound_xMax { get; set; }
        float bound_yMin { get; set; }
        float bound_xMin { get; set; }
        AnimationCurve Curve_bound_yMax;
        AnimationCurve Curve_bound_xMax;
        AnimationCurve Curve_bound_yMin;
        AnimationCurve Curve_bound_xMin;
        #endregion

        public Transform followingTarget;

        public float desktopScrollSpeed;
        public float desktopMoveSpeed;
        public float desktopMoveDragSpeed = 10.0f;

        public float touchMoveSpeed;
        public float touchScrollSpeed;

        public string horizontalKeyboardAxis;
        public string verticalKeyboardAxis;

        public Vector2 objectPos;

        private Transform selfT;
        private Camera selfC;

        public bool keyBoardControl;
        public bool mouseControl;
        public bool touchControl;
        public bool allowFollow;

        static private RTSCamera2D self;
        private int mouseDragButton = 0;

        public bool IsDragOut { get; private set; }
        public bool IsLastClickUI { get; private set; }

        #region runtime_control_switch
        public void KeyboardControl(bool enable)
        {
            keyBoardControl = enable;
        }

        public void MouseControl(bool enable)
        {

            if (Application.isMobilePlatform) return;

            mouseControl = enable;
        }

        public void TouchControl(bool enable)
        {
            touchControl = enable;
        }
        #endregion

        #region static_methods
        static public RTSCamera2D GetInstantiated() { return self; }

        public void JumpTo(Transform target)
        {
            self.objectPos.x = target.position.x;
            self.objectPos.y = target.position.y;
        }

        public void FollowForMain(Transform target)
        {
            if (self.allowFollow)
            {
                self.followingTarget = target;
            }
        }

        static public void CancelFollowForMain()
        {
            self.followingTarget = null;
        }

        static public Transform GetFollowingTarget() { return self.followingTarget; }
        #endregion

        public void Follow(Transform target)
        {
            if (self.allowFollow)
            {
                self.followingTarget = target;
            }
        }

        public void CancelFollow()
        {
            self.followingTarget = null;
        }

        public Vector3 CalculateCurrentObjectPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Adjust to the attitude base on current setting.
        /// Editor use this method to generate preview.
        /// </summary>
        public void Adjust2AttitudeBaseOnCurrentSetting()
        {
            objectPos = CalculateCurrentObjectPosition();
            scrollValue = Mathf.Clamp01(scrollValue);

            GetComponent<Camera>().orthographicSize = zoomRange.Min + (scrollValue * zoomRange.Length);
        }

        void Awake()
        {
            self = this;
            selfT = transform;
            selfC = GetComponent<Camera>();

            Curve_bound_yMax = new AnimationCurve(new Keyframe(0, bound.yMax), new Keyframe(1, maxBound.yMax));
            Curve_bound_xMax = new AnimationCurve(new Keyframe(0, bound.xMax), new Keyframe(1, maxBound.xMax));
            Curve_bound_yMin = new AnimationCurve(new Keyframe(0, bound.yMin), new Keyframe(1, maxBound.yMin));
            Curve_bound_xMin = new AnimationCurve(new Keyframe(0, bound.xMin), new Keyframe(1, maxBound.xMin));
        }

        public void Start()
        {
            objectPos = CalculateCurrentObjectPosition();
            scrollValue = Mathf.Clamp01(scrollValue);

            KeyboardControl(keyBoardControl);
            MouseControl(mouseControl);
            TouchControl(touchControl);

        }

        private void Update()
        {
            if (mouseControl)
            {
                Scroll(Input.GetAxis("Mouse ScrollWheel") * -desktopScrollSpeed);

                if (!IsMouseButton())
                {
                    if (IsDragOut && !IsInScreenEdge())
                        IsDragOut = false;

                    float speedFaction = 1.0f;
                    if (IsDragOut)
                        speedFaction = 0.05f;

                    if (Input.mousePosition.y >= Screen.height - 1f) { Move(Vector2.up, desktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.y <= 0) { Move(-Vector2.up, desktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.x <= 0) { Move(-Vector2.right, desktopMoveSpeed * Time.deltaTime * speedFaction); }
                    if (Input.mousePosition.x >= Screen.width - 1f) { Move(Vector2.right, desktopMoveSpeed * Time.deltaTime * speedFaction); }
                }

                if (Input.GetMouseButtonDown(mouseDragButton))
                    IsLastClickUI = BaseInputMgr.IsStayInUIWithoutHUD;//BaseInputMgr.CheckOverUI(true);
                if (Input.GetMouseButtonUp(mouseDragButton))
                    IsLastClickUI = false;

                if (IsLastClickUI)
                    return;
                if (Input.GetMouseButton(mouseDragButton))
                {
                    float mouseX = Input.GetAxis("Mouse X") / Screen.width * 10000f;
                    float mouseY = Input.GetAxis("Mouse Y") / Screen.height * 10000f;
                    Move(-Vector2.right * mouseX, desktopMoveDragSpeed * Time.deltaTime);
                    Move(-Vector2.up * mouseY, desktopMoveDragSpeed * Time.deltaTime);

                    if (IsInScreenEdge())
                        IsDragOut = true;
                    else
                        IsDragOut = false;
                }

            }

            UpdateTransform();
            UpdateBound();
        }

        public void Move(Vector2 dir, float offset)
        {
            dir *= offset;
            if (unlockWhenMove && dir != Vector2.zero)
            {
                followingTarget = null;
            }
            objectPos += dir;
            UpdatePosClamp();
        }

        public void Scroll(float value)
        {
            scrollValue += value;
            scrollValue = Mathf.Clamp01(scrollValue);
            UpdatePosClamp();
        }

        void UpdateTransform()
        {
            Vector3 cameraPos;

            if (followingTarget)
            {
                objectPos.x = followingTarget.position.x;
                objectPos.y = followingTarget.position.y;
            }

            float wantedSize = zoomRange.Min + (scrollValue * zoomRange.Length);
            selfC.orthographicSize = Mathf.Lerp(selfC.orthographicSize, wantedSize, movementLerpSpeed * Time.deltaTime);

            cameraPos = objectPos;
            cameraPos.z = selfC.transform.position.z;
            selfT.position = Vector3.Lerp(selfT.position, cameraPos, movementLerpSpeed * Time.deltaTime);
        }
        void UpdateBound()
        {
            bound_xMax = Curve_bound_xMax.Evaluate(scrollValue);
            bound_yMax = Curve_bound_yMax.Evaluate(scrollValue);
            bound_xMin = Curve_bound_xMin.Evaluate(scrollValue);
            bound_yMin = Curve_bound_yMin.Evaluate(scrollValue);
        }
        void UpdatePosClamp()
        {
            objectPos.x = Mathf.Clamp(objectPos.x, bound_xMin, bound_xMax);
            objectPos.y = Mathf.Clamp(objectPos.y, bound_yMin, bound_yMax);
        }
        #region is
        bool IsMouseButton()
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                return true;
            return false;
        }
        bool IsInScreenEdge()
        {
            if (Input.mousePosition.y >= Screen.height - 1f) return true;
            if (Input.mousePosition.y <= 1) return true;
            if (Input.mousePosition.x <= 1) return true;
            if (Input.mousePosition.x >= Screen.width - 1f) return true;
            return false;
        }
        #endregion

        void OnDrawGizmos()
        {
            Gizmos.DrawLine(new Vector3(bound.xMin, bound.yMin, 0), new Vector3(bound.xMin, bound.yMax, 0));
            Gizmos.DrawLine(new Vector3(bound.xMin, bound.yMax, 0), new Vector3(bound.xMax, bound.yMax, 0));
            Gizmos.DrawLine(new Vector3(bound.xMax, bound.yMax, 0), new Vector3(bound.xMax, bound.yMin, 0));
            Gizmos.DrawLine(new Vector3(bound.xMax, bound.yMin, 0), new Vector3(bound.xMin, bound.yMin, 0));
        }
    }
}