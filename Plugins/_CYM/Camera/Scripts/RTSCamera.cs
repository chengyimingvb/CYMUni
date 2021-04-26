using System.Collections.Generic;
using UnityEngine;
public enum MouseButton
{
    Left=0, 
    Right=1, 
    Middle=2
}
namespace CYM.Cam
{

    public class RTSCamera : MonoBehaviour
    {
        #region Output param
        public BoolState ControlDisabled { get; private set; } = new BoolState();
        public float DesktopMoveDragSpeed { get; set; } = 10.0f;
        public float DesktopMoveSpeed { get; set; } = 300;
        public float DesktopKeyMoveSpeed { get; set; } = 2;
        public float DesktopScrollSpeed { get; set; } = 1;
        public float DesktopRotateSpeed { get; set; } = 0;

        public float TouchMoveDragSpeed { get; set; }
        public float TouchMoveSpeed { get; set; }
        public float TouchScrollSpeed { get; set; }
        public float TouchRotateSpeed { get; set; }
        public float ScrollValue { get; set; }
        #endregion

        #region Param
        public AnimationCurve scrollXAngle = new AnimationCurve();
        public AnimationCurve scrollHigh = new AnimationCurve();
        public float MaxHight=20;
        public float MinHight=5;
        public float MaxAngle=45;
        public float MinAngle=45;
        public CamScrollAnimType scrollAnimationType;
        public bool groundHighTest;
        public float groundHighTestValMax = 125; //高于这个高度后进行插值
        public LayerMask groundMask;
        public Rect bound;
        public bool unlockWhenMove;
        public float movementLerpSpeed;
        public float rotationLerpSpeed;

        public float desktopScrollSpeed;
        public float desktopMoveSpeed;
        public float desktopMoveDragSpeed;
        public float desktopRotateSpeed;
        public float deskScreenEdgeWidth = 1;

        public float touchScrollSpeed;
        public float touchMoveSpeed;
        public float touchMoveDragSpeed;
        public float touchRotateSpeed;
        public float touchScreenEdgeWidth = 1;

        public int mouseDragButton;
        public int mouseRotateButton;

        public bool rotateControl;
        public bool screenEdgeMoveControl;
        public bool dragControl;
        public bool scrollControl;

        public bool mouseRotateControl;
        public bool mouseScreenEdgeMoveControl;
        public bool mouseDragControl;
        public bool mouseScrollControl;
        public bool desktopDisSEMWhenDrag = true;

        public bool touchDragControl;
        public bool touchScrollControl;
        public bool touchRotateControl;
        public bool touchScreenEdgeMoveControl;
        public bool allowFollow = true;
        #endregion

        #region prop
        Vector3 objectPos;
        Transform followingTarget;
        Transform fixedPoint;
        Transform trans;
        float targetCurrentGroundHigh = 0;
        float wantYAngle;
        float wantXAngle;
        bool isLastClickUI = false;
        bool isDragOut = false;
        float rightMouseButtonTime = 0;
        static private RTSCamera self;
        #endregion

        #region runtime_control_switch
        public void ScreenEdgeMoveControl(bool enable)
        {
            screenEdgeMoveControl = enable;
        }

        public void DragControl(bool enable)
        {
            dragControl = enable;
        }
        public void RotateControl(bool enable)
        {
            rotateControl = enable;
        }
        public void ScrollControl(bool enable)
        {
            scrollControl = enable;
        }
        #endregion

        #region static_methods
        //public void LockFixedPointForMain(Transform pos)
        //{
        //    if (self.allowFollow)
        //    {
        //        self.followingTarget = null;
        //        self.fixedPoint = pos;

        //        //Set the wantPos to make camera more smooth when leave fixed point.
        //        self.objectPos.x = pos.position.x;
        //        self.objectPos.z = pos.position.z;
        //    }
        //}

        //public void UnlockFixedPointForMain()
        //{
        //    self.fixedPoint = null;
        //}

        public void JumpToTarget(Transform target)
        {
            self.objectPos.x = target.position.x;
            self.objectPos.z = target.position.z;
        }

        //public void FollowForMain(Transform target)
        //{
        //    if (self.allowFollow)
        //    {
        //        self.fixedPoint = null;
        //        self.followingTarget = target;
        //    }
        //}

        //public void CancelFollowForMain()
        //{
        //    self.followingTarget = null;
        //}

        public Transform GetFollowingTarget() { return self.followingTarget; }
        public Transform GetFixedPoint() { return self.fixedPoint; }
        #endregion

        public void SetMinMaxHeight(float min, float max)
        {
            scrollHigh = AnimationCurve.Linear(0, min, 1, max);
        }
        public void SetGroundTest(bool b)
        {
            groundHighTest = b;
        }

        public void LockFixedPoint(Transform pos)
        {
            if (self.allowFollow)
            {
                self.followingTarget = null;
                self.fixedPoint = pos;
            }
        }

        public void UnlockFixedPoint()
        {
            self.fixedPoint = null;
        }

        public void Follow(Transform target)
        {
            if (self.allowFollow)
            {
                self.fixedPoint = null;
                self.followingTarget = target;
            }
        }

        public void CancelFollow()
        {
            self.followingTarget = null;
        }

        public Vector3 CalculateCurrentObjectPosition()
        {

            float dist = objectPos.y * Mathf.Tan((90f - wantXAngle) * Mathf.Deg2Rad);

            Vector3 objectPosDir = -(transform.rotation * (-Vector3.forward * dist));
            return transform.position + objectPosDir;
        }

        void Awake()
        {
            self = this;
            trans = transform;
        }

        public void Start()
        {
            scrollHigh = new AnimationCurve(new Keyframe(0,MinHight),new Keyframe(1,MaxHight));
            scrollXAngle = new AnimationCurve(new Keyframe(0,MinAngle),new Keyframe(1,MaxAngle));

            objectPos = CalculateCurrentObjectPosition();
            ScrollValue = Mathf.Clamp01(ScrollValue);
            objectPos.y = scrollHigh.Evaluate(ScrollValue);
            wantXAngle = scrollXAngle.Evaluate(ScrollValue);

            Vector3 rot = trans.eulerAngles;
            rot.x = MathUtil.WrapAngle(rot.x);
            rot.y = MathUtil.WrapAngle(rot.y);
            wantYAngle = rot.y;
            rot.x = scrollXAngle.Evaluate(ScrollValue);
            wantXAngle = rot.x;
            trans.eulerAngles = rot;
        }

        private void Update()
        {
            UpdateMouseControl();
            UpdateTouchControl();
            UpdateTransform();
        }
        void UpdateTouchControl()
        {
            if (!Application.isMobilePlatform) return;
            if (Application.isEditor) return;
            Vector2 dV = BaseInputMgr.TouchDragValue;

            if (dragControl && touchDragControl && dV != Vector2.zero)
            {
                Quaternion fixedDir = Quaternion.Euler(new Vector3(0, trans.eulerAngles.y, trans.eulerAngles.z));
                Vector3 forwardDir = fixedDir * Vector3.forward;
                _Move(dV.y * forwardDir, TouchMoveDragSpeed * Time.deltaTime);
                _Move(dV.x * trans.right, TouchMoveDragSpeed * Time.deltaTime);
            }

            if (scrollControl && touchScrollControl && BaseInputMgr.TouchScaleValue != 0) 
                Scroll(BaseInputMgr.TouchScaleValue * touchScrollSpeed);

            if (rotateControl && touchRotateControl && BaseInputMgr.TouchRotateValue != 0) 
                Rotate(BaseInputMgr.TouchRotateValue * touchRotateSpeed);

            if (screenEdgeMoveControl && touchScreenEdgeMoveControl && BaseInputMgr.TouchPosition != Vector3.zero)
            {
                float speedFaction = 1.0f;
                if (BaseInputMgr.TouchPosition.y >= Screen.height - touchScreenEdgeWidth) { _Move(trans.forward, TouchMoveSpeed * Time.deltaTime * speedFaction); }
                if (BaseInputMgr.TouchPosition.y <= touchScreenEdgeWidth) { _Move(-trans.forward, TouchMoveSpeed * Time.deltaTime * speedFaction);  }
                if (BaseInputMgr.TouchPosition.x <= touchScreenEdgeWidth) { _Move(-trans.right, TouchMoveSpeed * Time.deltaTime * speedFaction); }
                if (BaseInputMgr.TouchPosition.x >= Screen.width - touchScreenEdgeWidth) { _Move(trans.right, TouchMoveSpeed * Time.deltaTime * speedFaction);  }
            }
        }
        void UpdateMouseControl()
        {
            if (Application.isMobilePlatform) return;
            if (Input.GetMouseButton((int)MouseButton.Right))
            {
                rightMouseButtonTime += Time.deltaTime;
            }
            if (Input.GetMouseButtonUp((int)MouseButton.Right))
            {
                rightMouseButtonTime = 0;
            }

            if (screenEdgeMoveControl && mouseScreenEdgeMoveControl)
            {
                if (IsMouseButton() && desktopDisSEMWhenDrag)
                    return;
                if (isDragOut && !IsInScreenEdge())
                    isDragOut = false;

                float speedFaction = 1.0f;
                if (isDragOut)
                    speedFaction = 0.05f;

                if (Input.mousePosition.y >= Screen.height - deskScreenEdgeWidth) { _Move(trans.forward, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                if (Input.mousePosition.y <= deskScreenEdgeWidth) { _Move(-trans.forward, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                if (Input.mousePosition.x <= deskScreenEdgeWidth) { _Move(-trans.right, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
                if (Input.mousePosition.x >= Screen.width - deskScreenEdgeWidth) { _Move(trans.right, DesktopMoveSpeed * Time.deltaTime * speedFaction); }
            }

            if (scrollControl && mouseScrollControl)
            {
                Scroll(Input.GetAxis("Mouse ScrollWheel") * -DesktopScrollSpeed);
            }

            if (Input.GetMouseButtonDown(mouseDragButton))
                isLastClickUI = BaseInputMgr.IsStayInUIWithoutHUD;//BaseInputMgr.CheckOverUI(true);
            if (Input.GetMouseButtonUp(mouseDragButton))
                isLastClickUI = false;

            if (rotateControl && mouseRotateControl)
            {
                if (isLastClickUI)
                    return;

                if (Input.GetMouseButton(mouseRotateButton) && rightMouseButtonTime > 0.2f)
                {
                    Rotate(Input.GetAxis("Mouse X") * desktopRotateSpeed);
                }
            }
            if (dragControl && mouseDragControl)
            {
                if (isLastClickUI)
                    return;

                if (Input.GetMouseButton(mouseDragButton))
                {
                    float mouseX = Input.GetAxis("Mouse X") / Screen.width * 10000f;
                    float mouseY = Input.GetAxis("Mouse Y") / Screen.height * 10000f;
                    _Move(-trans.right, DesktopMoveDragSpeed * mouseX * Time.deltaTime);
                    _Move(-trans.forward, DesktopMoveDragSpeed * mouseY * Time.deltaTime);

                    if (IsInScreenEdge()) isDragOut = true;
                    else isDragOut = false;
                }
            }
        }

        public void Move(Vector3 dir)
        {
            _Move(dir, DesktopMoveSpeed * Time.deltaTime);
        }

        void _Move(Vector3 dir, float speed)
        {
            if (ControlDisabled.IsIn()) return;

            dir.y = 0;
            dir.Normalize();
            dir *= speed;
            if (unlockWhenMove && dir != Vector3.zero)
            {
                followingTarget = null;
                fixedPoint = null;
            }
            objectPos += dir;

            objectPos.x = Mathf.Clamp(objectPos.x, bound.xMin, bound.xMax);
            objectPos.z = Mathf.Clamp(objectPos.z, bound.yMin, bound.yMax);
        }

        public void SetBound(List<float> data)
        {
            if (data.Count < 4)
            {
                CLog.Error("data 数据不足4");
                return;
            }
            float x = data[0];
            float y = data[1];
            float width = data[2];
            float height = data[3];
            bound = new Rect(x, y,width - x,height - y);
        }

        public void Rotate(float dir)
        {
            if (ControlDisabled.IsIn()) return;

            wantYAngle += dir;
            MathUtil.WrapAngle(wantYAngle);
        }

        public void Scroll(float value)
        {
            if (ControlDisabled.IsIn()) return;

            ScrollValue += value;
            ScrollValue = Mathf.Clamp01(ScrollValue);
            objectPos.y = scrollHigh.Evaluate(ScrollValue);
            wantXAngle = scrollXAngle.Evaluate(ScrollValue);
        }
        public void SetScroll(float value)
        {
            if (ControlDisabled.IsIn()) return;

            ScrollValue = value;
            ScrollValue = Mathf.Clamp01(ScrollValue);
            objectPos.y = scrollHigh.Evaluate(ScrollValue);
            wantXAngle = scrollXAngle.Evaluate(ScrollValue);
        }

        void UpdateTransform()
        {
            Vector3 cameraPosDir = Vector3.zero;
            Vector3 cameraPos = Vector3.zero;

            if (!fixedPoint)
            {
                float currentGroundHigh = 0;

                //Set wanted position to target's position if we are following something.
                if (followingTarget)
                {
                    objectPos.x = followingTarget.position.x;
                    objectPos.z = followingTarget.position.z;
                }

                //Calculate vertical distance to ground to avoid intercepting ground.
                RaycastHit hit;
                Vector3 emitPos = objectPos;
                emitPos.y += 99999f;
                if (groundHighTest && Physics.Raycast(emitPos, -Vector3.up, out hit, Mathf.Infinity, groundMask))
                {
                    currentGroundHigh = hit.point.y;
                }

                emitPos = trans.position;
                emitPos.y += 99999f;
                if (groundHighTest && Physics.Raycast(emitPos, -Vector3.up, out hit, Mathf.Infinity, groundMask))
                {
                    currentGroundHigh = Mathf.Max(currentGroundHigh, hit.point.y);
                }

                if (followingTarget)
                {
                    currentGroundHigh = followingTarget.position.y;
                }

                //Lerp actual rotation to wanted value.
                Quaternion targetRot = Quaternion.Euler(wantXAngle, wantYAngle, 0f);
                trans.rotation = Quaternion.Lerp(trans.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);

                //Calculate a world position refers to the center of screen.
                float dist = objectPos.y * Mathf.Tan((90f - wantXAngle) * Mathf.Deg2Rad);

                //Use this vector to move camera back and rotate.
                Quaternion targetYRot = Quaternion.Euler(0f, wantYAngle, 0f);
                cameraPosDir = targetYRot * (Vector3.forward * dist);

                //Calculate the actual world position to prepare to move our camera object.
                cameraPos = objectPos - cameraPosDir;

                if (trans.position.y > groundHighTestValMax)
                {
                    targetCurrentGroundHigh = Mathf.Lerp(targetCurrentGroundHigh, currentGroundHigh, 0.5f * Time.smoothDeltaTime);
                }
                else
                {
                    targetCurrentGroundHigh = currentGroundHigh;
                }
                cameraPos.y = (objectPos.y + targetCurrentGroundHigh);
                //Lerp to wanted position.
                trans.position = Vector3.Lerp(trans.position, cameraPos, movementLerpSpeed * Time.deltaTime);
            }
            else
            {
                //If we are positioning to a fixed point, we simply move to it.
                trans.rotation = Quaternion.Lerp(trans.rotation, fixedPoint.rotation, rotationLerpSpeed * Time.deltaTime);
                trans.position = Vector3.Lerp(trans.position, fixedPoint.position, movementLerpSpeed * Time.deltaTime);

                //We also keep objectPos to fixedPoint to make a stable feeling while leave fixed point mode.
                objectPos.x = fixedPoint.position.x;
                objectPos.z = fixedPoint.position.z;
            }
            if (scrollHigh != null)
            {
                MaxHight = scrollHigh.Evaluate(1.0f);
                MinHight = scrollHigh.Evaluate(0.0f);
            }
            if (scrollXAngle != null)
            {
                MaxAngle = scrollXAngle.Evaluate(1.0f);
                MinAngle = scrollXAngle.Evaluate(0.0f);
            }
        }

        void OnDrawGizmosSelected()
        {
            //Draw debug lines.
            Vector3 mp = transform.position;
            Gizmos.DrawLine(new Vector3(bound.xMin, mp.y, bound.yMin), new Vector3(bound.xMin, mp.y, bound.yMax));
            Gizmos.DrawLine(new Vector3(bound.xMin, mp.y, bound.yMax), new Vector3(bound.xMax, mp.y, bound.yMax));
            Gizmos.DrawLine(new Vector3(bound.xMax, mp.y, bound.yMax), new Vector3(bound.xMax, mp.y, bound.yMin));
            Gizmos.DrawLine(new Vector3(bound.xMax, mp.y, bound.yMin), new Vector3(bound.xMin, mp.y, bound.yMin));
        }

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
    }
}

public enum CamScrollAnimType
{
    Simple, 
    Advanced
}