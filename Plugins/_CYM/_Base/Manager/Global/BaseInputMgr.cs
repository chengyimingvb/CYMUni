//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using CYM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace CYM
{
    public enum InputBntType
    {
        Normal,
        Up,
        Down,
        DoublePressHold,
        DoublePressDown,
        DoublePressUp,
    }
    public enum InputAxisType
    {
        Normal,
        Raw,
        DoubleClick,
    }

    public class BaseInputMgr : BaseGFlowMgr
    {
        #region const
        const string StrMouseScrollWheel = "Mouse ScrollWheel";
        #endregion

        #region Callback
        /// <summary>
        /// 选择单位的时候触发
        /// 1:选择的单位
        /// 2:是否重复选择
        /// </summary>
        public event Callback<BaseUnit, bool> Callback_OnSelectedUnit;
        public event Callback<Vector3, int> Callback_OnTouchDown;
        public event Callback<Vector3, int> Callback_OnTouchUp;
        public event Callback<BaseUnit> Callback_OnMouseEnterUnit;
        public event Callback<BaseUnit> Callback_OnMouseExitUnit;
        public event Callback<BaseUnit> Callback_OnRightClick;
        public event Callback<BaseUnit> Callback_OnLeftClick;
        public event Callback Callback_OnInputMapChanged;
        public event Callback Callback_OnAnyKeyDown;
        #endregion

        #region mgr
        protected IBaseScreenMgr ScreenMgr => BaseGlobal.ScreenMgr;
        protected BaseUnit BasePlayer => BaseGlobal.ScreenMgr?.BaseLocalPlayer;
        protected static BaseInputMgr Ins => BaseGlobal.InputMgr;
        BuildConfig BuildConfig => BuildConfig.Ins;
        #endregion

        #region map
        public static InputActionMap GamePlayMap { get; private set; }
        public static InputActionMap MenuMap { get; private set; }
        protected static InputActionAsset InputAsset { get; private set; }
        #endregion

        #region last
        public static GameObject LastHitUI
        {
            get
            {
                if (LastHitUIResults.Count > 0)
                    return LastHitUIResults[0].gameObject;
                return null;
            }
        }
        public static List<RaycastResult> LastRaycastUIObj { get; private set; } = new List<RaycastResult>();
        public static List<RaycastResult> LastHitUIResults { get; private set; } = new List<RaycastResult>();
        protected static int LeftSelectFilterIndex { get; set; } = 0;
        protected static Collider LastHitCollider { get; set; }
        protected static Collider LastHitUpCollider { get; set; }
        protected static Vector3 LastMouseDownPos { get; private set; }
        protected static Vector3 LastMousePos { get; private set; }
        protected static Vector3 LastTouchDownPos { get; private set; }
        protected static RaycastHit LastHit => lastHit;
        static RaycastHit lastHit;
        #endregion

        #region state
        protected static BoolState IsFullScreenState { get; set; } = new BoolState();
        protected static BoolState IsDisablePlayerInputState { get; set; } = new BoolState();
        protected static BoolState IsDisableUnitSelectState { get; set; } = new BoolState();
        #endregion

        #region static obj
        public static UHUDBar HoverHUDBar { get; private set; } = null;
        public static BaseUnit PreLastMouseOverUnit { get; private set; } = null;
        public static BaseUnit LastMouseOverUnit { get; private set; } = null;
        public static BaseUnit HoverUnit { get; private set; } = null;
        public static BaseUnit SelectedUnit { get; private set; } = null;
        #endregion

        #region timer
        // 选择单位后的一个记时,可以防止重复操作
        static Timer SelectUnitTimer = new Timer(0.1f);
        static Timer MouseEnterUnitTimer = new Timer(0.02f);
        #endregion

        #region private
        static float TouchDPI = 1;
        static float DragDPI = 1;
        static float TouchLastAngle;
        static float TouchLastDist = 0;
        static float LongPressTimeFlag = 0;
        #endregion

        #region pub
        public static float LongPressTime
        {
            get
            {
                if (LongPressTimeFlag <= 0)
                    return 0;
                return Time.time - LongPressTimeFlag;
            }
        }
        public static float PreLongPressTime { get; private set; }
        #endregion

        #region pub prop
        public static bool IsIMUIShow { get; private set; } = false;
        public static bool IsDevConsoleShow { get; private set; } = false;
        public static bool IsFeedbackShow { get; private set; } = false;
        //是否全屏
        public static bool IsFullScreen => IsFullScreenState.IsIn();
        public static bool IsEnablePlayerInput => !IsDisablePlayerInputState.IsIn();
        public static bool IsStayHUDBar { get; private set; } = false;
        public static bool IsStayInUI { get; private set; } = false;
        public static bool IsStayInUIWithoutHUD => IsStayInUI && !IsStayHUDBar;
        public static bool IsStayInTerrain { get; private set; } = false;
        public static bool IsStayInUnit { get; private set; } = false;
        public static bool IsScrollWheel { get; private set; }
        public static bool IsMousePress { get; private set; }
        public static bool IsBlockSelectUnit { get; private set; }
        public static bool IsInGuideMask { get; private set; } = false;
        #endregion

        #region Is
        //传入的Unit是否和最近悬浮的Unit是一样的
        public static bool IsOverSameLastUnit(BaseUnit unit) => LastMouseOverUnit == unit;
        // 鼠标按下的位置是否和弹起的时候处于同一个位置
        public static bool IsSameMousePt(float val = 0f)
        {
            if (val > 0) return MathUtil.Approximately(LastMouseDownPos, Input.mousePosition, val);
            else return MathUtil.Approximately(LastMouseDownPos, Input.mousePosition);
        }
        public static bool IsSameTouchPt(float val = 0f)
        {
            if (val > 0) return MathUtil.Approximately(LastTouchDownPos, ScreenPos, val);
            else return MathUtil.Approximately(LastTouchDownPos, ScreenPos);
        }
        public static bool IsSamePt(float val = 0f)
        {
            if (val > 0) return MathUtil.Approximately(LastScreenDownPos, ScreenPos, val);
            else return MathUtil.Approximately(LastScreenDownPos, ScreenPos);
        }
        public static bool IsHaveLastHitUI()
        {
            if (LastHitUIResults.Count > 0) return true;
            return false;
        }
        public static bool IsHaveLastHitCollider() => LastHitCollider != null;
        public static bool IsSameLastHitCollider()
        {
            if (LastHitCollider == null || LastHitUpCollider == null)
                return false;
            return LastHitCollider == LastHitUpCollider;
        }
        //选择单位之后的时间
        public static bool IsInSelectUnitTime() => !SelectUnitTimer.IsOver();
        public static bool IsOverSamePreLastUnit() => LastMouseOverUnit == PreLastMouseOverUnit;
        public static bool IsMouseEnterUnitTimerOver() => MouseEnterUnitTimer.CheckOver();
        public static bool IsLongPressTime(float duration)
        {
            if (LongPressTime >= duration)
                return true;
            if (PreLongPressTime >= duration)
                return true;
            return false;
        }
        #endregion

        #region is can
        public static bool IsCanSelectUnit()
        {
            if (!IsCanInput()) return false;
            return !IsDisableUnitSelectState.IsIn();
        }
        public static bool IsCanInput()
        {
            if (BaseGlobal.Ins == null) return false;
            if (IsFullScreen) return false;
            if (IsDevConsoleShow) return false;
            if (IsFeedbackShow) return false;
            return true;
        }
        public static bool IsCanMenuInput()
        {
            if (BaseGlobal.Ins == null) return false;
            if (BaseGlobal.BattleMgr == null) return false;
            if (BaseGlobal.BattleMgr.IsInBattle) return false;
            if (!IsCanInput()) return false;
            return true;
        }
        public static bool IsCanGamePlayInput()
        {
            if (BaseGlobal.Ins == null) return false;
            if (BaseGlobal.IsPause) return false;
            if (BaseGlobal.BattleMgr == null) return false;
            if (!BaseGlobal.BattleMgr.IsInBattle) return false;
            if (!IsCanInput()) return false;
            if (IsDisablePlayerInputState.IsIn()) return false;
            if (IsDevConsoleShow) return false;
            return true;
        }
        #endregion

        #region unit
        protected virtual LayerMask SelectUnitLayerMask => (LayerMask)Const.Layer_Default;
        protected virtual bool IsCanSelectUnit(BaseUnit unit)
        {
            if (!IsCanSelectUnit()) return false;
            //如果这个单位死亡后,则不可被选择
            if (!unit.IsLive) return false;
            return true;
        }
        public static void SelectUnit(BaseUnit unit)
        {
            if (IsBlockSelectUnit) return;
            //选择一个单位后无法再次选择
            if (IsInSelectUnitTime())
                return;
            if (unit)
            {
                //检测这个是否可以被选择
                if (!Ins.IsCanSelectUnit(unit))
                    return;
            }

            //检测是否重复选择
            bool isRepeat = false;
            if (SelectedUnit == unit)
            {
                isRepeat = true;
            }
            else
            {
                SelectedUnit?.OnUnBeSelected();
            }

            if (unit)
            {
                unit?.OnBeSelected(isRepeat);
                SelectedUnit = unit;
            }
            else
            {
                SelectedUnit = null;
            }
            SelectUnitTimer.Restart();
            Ins?.OnSelectedUnit(SelectedUnit, isRepeat);
        }
        public static void UnSelectUnit()
        {
            SelectUnit(null);
        }
        public static void BlockSelectUnit(bool b)
        {
            SelectUnit(null);
            IsBlockSelectUnit = b;
        }
        #endregion

        #region set
        public static void SetHoverHUDBar(UHUDBar bar)
        {
            if (bar != null)
            {
                IsStayHUDBar = true;
                HoverHUDBar = bar;
            }
            else
            {
                IsStayHUDBar = false;
            }
        }
        #endregion

        #region set state
        public static void PushPlayerInputState(bool b) => IsDisablePlayerInputState.Push(!b);
        public static void PushFullScreenState(bool b) => IsFullScreenState.Push(b);
        public static void PushUnitSelect(bool b) => IsDisableUnitSelectState.Push(!b);
        public static void ResetFullScreenState()=> IsFullScreenState.Reset();
        public static void ResetPlayerInputState() => IsDisablePlayerInputState.Reset();
        public static void ResetUnitSelectState() => IsDisableUnitSelectState.Reset();
        #endregion

        #region life
        protected override string ResourcePrefab => "BaseStandInput";
        protected virtual bool NeedUpdateRaycast3D => true;
        protected virtual bool NeedUpdateHitUI => true;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            TouchDPI = Mathf.Max(Screen.dpi, BuildConfig.TouchDPI);
            DragDPI = Mathf.Max(Screen.dpi, BuildConfig.DragDPI);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            BaseUnit.Callback_OnRealDeathG += OnRealDeathG;
            BaseUnit.Callback_OnDeathG += OnDeathG;
            BaseUIMgr.Callback_OnControlClick += OnControlClick;

        }
        public override void OnDisable()
        {
            BaseUnit.Callback_OnDeathG -= OnDeathG;
            BaseUnit.Callback_OnRealDeathG -= OnRealDeathG;
            BaseUIMgr.Callback_OnControlClick -= OnControlClick;
            base.OnDisable();
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            InputAsset = Resources.Load<InputActionAsset>("InputConfig");
            if (InputAsset == null)
            {
                CLog.Error("没有配置:InputConfig");
            }
            else
            {
                GamePlayMap = TryGetActionMap("GamePlay");
                MenuMap = TryGetActionMap("Menu");
            }
        }
        public override void OnStart()
        {
            base.OnStart();
            Load();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateMapEnable(GamePlayMap, IsCanGamePlayInput);
            UpdateMapEnable(MenuMap, IsCanMenuInput);
            UpdateMouseControl();
            UpdateKey();
            UpdateTouch();
            UpdateRaycast3D();
            UpdateGUI();
            UpdateHitUI();
            UpdateFlag();
        }
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
            ResetPlayerInputState();
            ResetUnitSelectState();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
        }
        #endregion

        #region update
        void UpdateKey()
        {
            if (Application.isMobilePlatform)
                return;
            if (IsAnyKeyDown())
            {
                Callback_OnAnyKeyDown?.Invoke();
            }
        }
        void UpdateGUI()
        {
            if (BaseGuideView.Default != null)
            {
                IsInGuideMask = BaseGuideView.Default.IsInMask;
            }
            IsFeedbackShow = IMUIFeedback.IsShow;
            IsDevConsoleShow = BaseConsoleMgr.IsShow;
            IsIMUIShow = IMUIBase.CheckUI();
            bool pre = IsStayInUI;
            IsStayInUI = CheckOverUI();
            if (IsStayInUI != pre)
            {
                if (IsStayInUI)
                    OnEnterUI();
                else
                    OnExitUI();
            }
        }
        void UpdateHitUI()
        {
            if (!NeedUpdateHitUI)
                return;
            if (IsStayInUI && GetDown())
            {
                LastHitUIResults.Clear();
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = ScreenPos;
                EventSystem.current.RaycastAll(eventDataCurrentPosition, LastHitUIResults);
            }
        }
        void UpdateRaycast3D()
        {
            if (!NeedUpdateRaycast3D)
                return;
            if (!BattleMgr.IsInBattle) 
                return;
            if (IsStayInUI)
            {
                IsStayInTerrain = false;
                return;
            }
            if (IsStayInUnit)
            {
                IsStayInTerrain = false;
                return;
            }
            if (BaseGlobal.MainCamera == null) return;
            if (BaseGlobal.MainCamera.orthographic) return;
            if (Physics.Raycast(BaseGlobal.MainCamera.ScreenPointToRay(Input.mousePosition), out lastHit, int.MaxValue, (LayerMask)Const.Layer_Terrain, QueryTriggerInteraction.Collide))
            {
                bool pre = IsStayInTerrain;
                IsStayInTerrain = true;
                IsStayInUI = false;
                IsStayInUnit = false;
                if (pre != IsStayInTerrain)
                {
                    if (IsStayInTerrain)
                        OnEnterTerrain(LastHit.point);
                    else
                        OnExitTerrain();
                }
            }
            else
            {
                IsStayInTerrain = false;
            }
        }
        void UpdateMouseControl()
        {
            if (Application.isMobilePlatform)
                return;
            IsScrollWheel = Input.GetAxis(StrMouseScrollWheel) != 0;
            IsMousePress = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
            if (!IsCanGamePlayInput())
                return;
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    OnMouseDown(Input.mousePosition, i);
                    Callback_OnTouchDown?.Invoke(Input.mousePosition, i);
                }
                else if (Input.GetMouseButtonUp(i))
                {
                    OnMouseUp(Input.mousePosition, i);
                    Callback_OnTouchUp?.Invoke(Input.mousePosition, i);
                }
            }

            if (LastMousePos != Input.mousePosition)
            {
                OnMouseMove(Input.mousePosition, 0);
            }
            LastMousePos = Input.mousePosition;
        }
        void UpdateTouch()
        {
            if (!Application.isMobilePlatform)
                return;
            CalculateTouchDrag();
            CalculateTouchScale();
            CalculateTouchRotate();
            CalculateTouchPosition();
            void CalculateTouchPosition()
            {
                if (Input.touchCount > 0)
                {
                    var touch = Input.touches[0];
                    if (touch.phase == UnityEngine.TouchPhase.Began)
                        LastTouchDownPos = TouchPosition;
                }
            }
            void CalculateTouchDrag()
            {
                if (Input.touchCount != 1 || Input.touches[0].phase == UnityEngine.TouchPhase.Ended)
                {
                    if (PreLongPressTime < 0.2f ||
                        LongPressTime<0.2f)
                    {
                        LerpToZero();
                    }
                    else
                    {
                        TouchDragValue = Vector2.zero;
                    }
                    return;
                }

                if (Input.touches[0].phase != UnityEngine.TouchPhase.Moved)
                {
                    TouchDragValue = Vector2.zero;
                    return;
                }

                if (Input.touchCount > 0)
                {
                    float threold=0.005f;
                    Vector2 dragVal = DeltaMovementForTouch(0);
                    if (Mathf.Abs(dragVal.x) > threold || Mathf.Abs(dragVal.y) > threold)
                        TouchDragValue = -dragVal;
                    else
                        TouchDragValue = Vector2.zero;
                }

                Vector2 DeltaMovementForTouch(int fingerID)
                {
                    Touch touch = Input.touches[fingerID];
                    return touch.deltaPosition / DragDPI;
                }
                void LerpToZero()
                {
                    TouchDragValue = Vector3.Lerp(TouchDragValue, Vector2.zero,Time.deltaTime*15);
                }
            }
            void CalculateTouchScale()
            {
                if (Input.touchCount != 2)
                {
                    TouchScaleValue = 0f;
                    TouchLastDist = 0;
                    return;
                }

                if (Input.touches[0].phase != UnityEngine.TouchPhase.Moved && Input.touches[1].phase != UnityEngine.TouchPhase.Moved)
                {
                    TouchScaleValue = 0;
                    if (Input.touches[0].phase == UnityEngine.TouchPhase.Ended && Input.touches[1].phase == UnityEngine.TouchPhase.Ended) TouchLastDist = 0;

                    return;
                }

                float curDist = DistanceForTouch(0, 1);
                if (TouchLastDist == 0) TouchLastDist = curDist;
                TouchScaleValue = (curDist - TouchLastDist) * -0.01f;
                TouchLastDist = curDist;

                float DistanceForTouch(int fingerA, int fingerB)
                {
                    return (Input.touches[0].position - Input.touches[1].position).magnitude / TouchDPI;
                }
            }
            void CalculateTouchRotate()
            {
                if (Input.touchCount != 2)
                {
                    TouchRotateValue = 0;
                    TouchLastAngle = 0;
                    return;
                }
                Vector2 v2 = (Input.touches[1].position - Input.touches[0].position) / TouchDPI;
                float curAngle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
                if (TouchLastAngle == 0) TouchLastAngle = curAngle;

                TouchRotateValue = curAngle - TouchLastAngle;
                TouchLastAngle = curAngle;
            }
        }
        void UpdateFlag()
        {
            if (GetUp())
            {
                PreLongPressTime = LongPressTimeFlag;
                LongPressTimeFlag = 0;
            }
            if (GetDown())
            {
                PreLongPressTime = 0;
                LongPressTimeFlag = Time.time;
            }
        }
        #endregion

        #region get
        protected InputActionMap TryGetActionMap(string id) => InputAsset?.FindActionMap(id);
        protected InputAction GetGamePlayAction(string id) => GamePlayMap?.FindAction(id);
        protected InputAction GetMenuAction(string id) => MenuMap?.FindAction(id);
        #endregion

        #region set
        void UpdateMapEnable(InputActionMap map, Func<bool> DoIsEnable)
        {
            if (map != null && DoIsEnable != null)
            {
                bool temp = DoIsEnable();
                if (map.enabled != temp)
                {
                    if (temp)
                        map.Enable();
                    else
                        map.Disable();
                }
            }
        }
        protected InputAction RegisterGameplay(string id, Action<CallbackContext> perform, Action<CallbackContext> start = null, Action<CallbackContext> cancel = null)
        {
            var item = GetGamePlayAction(id);
            if (item == null)
                return null;
            item.performed += perform;
            if (start != null) item.started += start;
            if (cancel != null) item.canceled += cancel;
            return item;
        }
        protected InputAction RegisterMenu(string id, Action<CallbackContext> perform, Action<CallbackContext> start = null, Action<CallbackContext> cancel = null)
        {
            var item = GetMenuAction(id);
            if (item == null)
                return null;
            item.performed += perform;
            if (start != null) item.started += start;
            if (cancel != null) item.canceled += cancel;
            return item;
        }
        public void Save()
        {
            FileUtil.SaveJson(Const.Path_Shortcuts, InputAsset.ToJson());
        }
        public void Load()
        {
            string data = FileUtil.LoadFile(Const.Path_Shortcuts);
            if (data == null)
                return;
            InputAsset.LoadFromJson(data);
        }
        #endregion

        #region mouse click
        public virtual void LeftClick(BaseUnit arg1, bool isForce = false)
        {
            if (!isForce &&
                !IsStayInUI &&
                !IsHaveLastHitCollider() &&
                IsSameMousePt(0))
                SelectUnit(null);
            else if (arg1 != null && !IsStayInUIWithoutHUD)
                SelectUnit(arg1);
            Callback_OnLeftClick?.Invoke(arg1);
        }
        public virtual void RightClick(BaseUnit arg1, bool isForce = false)
        {
            Callback_OnRightClick?.Invoke(arg1);
        }
        #endregion

        #region pub callback
        public virtual bool OnEnterUnit(BaseUnit arg1)
        {
            MouseEnterUnitTimer.Restart();
            PreLastMouseOverUnit = LastMouseOverUnit;
            IsStayInUnit = true;
            LastMouseOverUnit = arg1;
            HoverUnit = arg1;
            Callback_OnMouseEnterUnit?.Invoke(arg1);
            return true;
        }
        public virtual bool OnExitUnit(BaseUnit arg1)
        {
            //退出后射线检测Unit
            RaycastHit hit;
            if (IsStayInUI &&
                Util.MouseRayCast(out hit, SelectUnitLayerMask))
            {
                BaseUnit unit = hit.collider.GetComponent<BaseUnit>();
                OnEnterUnit(unit);
                return false;
            }
            else
            {
                IsStayInUnit = false;
                HoverUnit = null;
                Callback_OnMouseExitUnit?.Invoke(arg1);
                return true;
            }
        }
        #endregion

        #region Callback
        protected virtual void OnControlClick(UControl arg1, PointerEventData arg2)
        {
            SelectUnitTimer.Restart();
        }
        protected virtual void OnMouseMove(Vector3 mousePosition, int i) { }
        protected virtual void OnMouseUp(Vector3 mousePosition, int i)
        {
            if (IsStayInUI)
            {
                BaseUIMgr.CloseRecordControl();
                SelectUnitTimer.Restart();
            }

            RaycastHit hit;
            Util.MouseRayCast(out hit, SelectUnitLayerMask);
            {
                LastHitUpCollider = hit.collider;
                if (i == 1) { }
                else if (i == 0)
                {
                    BaseUnit final = null;
                    if (IsSameLastHitCollider())
                    {
                        var cols = Physics.OverlapSphere(LastHitUpCollider.transform.position, 3.0f, SelectUnitLayerMask);
                        List<BaseUnit> unitsList = new List<BaseUnit>();
                        foreach (var item in cols)
                            unitsList.Add(item.GetComponent<BaseUnit>());

                        //过滤
                        unitsList = unitsList.FindAll((x) => OnMouseUpFilter(x)).ToList();
                        if (unitsList.Count <= 0)
                        {
                            final = null;
                        }
                        else
                        {
                            if (LeftSelectFilterIndex < unitsList.Count) { }
                            else { LeftSelectFilterIndex = 0; }

                            final = unitsList[LeftSelectFilterIndex];
                            LeftSelectFilterIndex++;
                        }
                    }
                    LeftClick(final);
                }
            }
        }
        protected virtual bool OnMouseUpFilter(BaseUnit unit)
        {
            if (BasePlayer == null)
                return false;
            return BasePlayer.IsSelf(unit);
        }
        protected virtual void OnMouseDown(Vector3 mousePosition, int i)
        {
            LastMouseDownPos = Input.mousePosition;
            RaycastHit hit;
            Util.MouseRayCast(out hit, SelectUnitLayerMask);
            {
                if (IsStayInUI)
                    return;
                LastHitCollider = hit.collider;
                if (i == 1)//右键
                {
                    BaseUnit tempUnit = null;
                    if (LastHitCollider != null)
                        tempUnit = LastHitCollider.GetComponent<BaseUnit>();
                    RightClick(tempUnit);
                }
                else if (i == 0) { }
            }
        }
        protected virtual void OnEnterTerrain(Vector3 point) { }
        protected virtual void OnExitTerrain() { }
        protected virtual void OnEnterUI() { }
        protected virtual void OnExitUI() { }
        protected virtual void OnSelectedUnit(BaseUnit arg1, bool repeat) => Callback_OnSelectedUnit?.Invoke(arg1, repeat);
        protected virtual void OnRealDeathG(BaseUnit arg1) { }
        protected virtual void OnDeathG(BaseUnit arg1)
        {
            if (SelectedUnit == arg1)
            {
                SelectUnit(null);
            }
        }
        #endregion

        #region check util
        // 检测鼠标是否悬在在ui上
        // isIgnoreHUD 如果是true 如果 检测到了hud 则 返回false 表示鼠标不悬浮在UI上
        private static bool CheckOverUI(bool isIgnoreHUD = false)
        {
            if (IsFullScreen) return true;
            if (EventSystem.current == null) return false;

            if (isIgnoreHUD)
            {
                bool isOver;
                if (Application.isMobilePlatform)
                {
                    if (Input.touchCount <= 0) return false;
                    isOver = EventSystem.current.IsPointerOverGameObject(FingerId);
                }
                else isOver = EventSystem.current.IsPointerOverGameObject();

                if (isOver)
                {
                    if (IsStayHUDBar) return false;
                    return true;
                }
                return false;
            }
            else
            {
                if (Application.isMobilePlatform)
                {
                    if (Input.touchCount <= 0) return false;
                    return EventSystem.current.IsPointerOverGameObject(FingerId);
                }
                else return EventSystem.current.IsPointerOverGameObject();
            }
        }
        // 检测鼠标是否悬浮在HUD上
        private static bool CheckOverHUD()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = ScreenPos;
            LastRaycastUIObj.Clear();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, LastRaycastUIObj);
            if (LastRaycastUIObj.Count == 0)
                return false;
            if (LastRaycastUIObj[0].gameObject.CompareTag(Const.Tag_IgnorGUIBlock))
                return false;
            return false;
        }
        #endregion

        #region Old Input
        public static bool IsAnyKey() => Input.anyKey;
        public static bool IsAnyKeyDown() => Input.anyKeyDown;
        public static bool GetMouseDown(int index, bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            return Input.GetMouseButtonDown(index);
        }
        public static bool GetMouseUp(int index, bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            return Input.GetMouseButtonUp(index);
        }
        public static bool GetMouse(int index, bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            return Input.GetMouseButton(index);
        }
        public static bool GetMouseClick(int index, bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            return IsSameMousePt() && Input.GetMouseButtonUp(index);
        }
        public static bool GetMousePress(int index, float duration = 0.5f, bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            if (Input.GetMouseButton(index) && IsSameMousePt())
            {
                if (LongPressTime > duration)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool GetKeyDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);
        public static bool GetKeyUp(KeyCode keyCode) => Input.GetKeyUp(keyCode);
        public static bool GetKey(KeyCode keyCode) => Input.GetKey(keyCode);
        #endregion

        #region Old Touch
        static public int FingerId => Input.GetTouch(0).fingerId;
        static public Vector2 TouchDragValue { get; private set; }
        static public float TouchScaleValue { get; private set; }
        static public float TouchRotateValue { get; private set; }
        static public Vector3 TouchPosition
        {
            get
            {
                if (Input.touchCount > 0) 
                    return Input.touches[0].position;
                return Vector2.zero;
            }
        }
        static public bool GetTouchStationary(bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            if (Input.touchCount != 1) return false;
            else if (Input.touches[0].phase == UnityEngine.TouchPhase.Stationary) return true;
            return false;
        }
        static public bool GetTouchDown(bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            if (Input.touchCount != 1) return false;
            else if (Input.touches[0].phase == UnityEngine.TouchPhase.Began) return true;
            return false;
        }
        static public bool GetTouchUp(bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            if (Input.touchCount != 1) return false;
            else if (Input.touches[0].phase == UnityEngine.TouchPhase.Ended) return true;
            return false;
        }
        public static bool GetTouchPress(float duration=0.5f, bool checkUI = true)
        {
            if ((IsStayInUI || IsIMUIShow) && checkUI) return false;
            if (GetTouchStationary() && IsSameTouchPt())
            {
                if (LongPressTime > duration)
                    return true;
            }
            return false;
        }
        public static bool GetTouchCount(int count)
        {
            if (count <= 0)
                return false;
            return Input.touchCount >= count;
        }
        #endregion

        #region Mouse & Touch
        public static Vector2 ScreenPos
        {
            get
            {
                if (Application.isMobilePlatform)
                    return TouchPosition;
                else return Input.mousePosition;
            }
        }
        public static Vector2 LastScreenDownPos
        {
            get
            {
                if (Application.isMobilePlatform)
                    return LastTouchDownPos;
                else return LastMouseDownPos;
            }
        }
        public static Vector3 GetHitTerrinPoint()
        {
            Util.RayCast(out RaycastHit hit, ScreenPos, (LayerMask)Const.Layer_Terrain);
            return hit.point;
        }
        public static bool GetStationary(bool checkUI = true) => GetMouse(0, checkUI) || GetTouchStationary(checkUI);
        public static bool GetDown(bool checkUI = true) => GetMouseDown(0, checkUI) || GetTouchDown(checkUI);
        public static bool GetUp(bool checkUI = true) => GetMouseUp(0, checkUI) || GetTouchUp(checkUI);
        public static bool GetPress(float duration=0.5f, bool checkUI = true) => GetMousePress(0,duration, checkUI) || GetTouchPress(duration, checkUI);
        public static bool GetClick(bool checkUI = true) => IsSamePt(0) && GetUp(checkUI) && !IsLongPressTime(0.2f);
        #endregion
    }

}