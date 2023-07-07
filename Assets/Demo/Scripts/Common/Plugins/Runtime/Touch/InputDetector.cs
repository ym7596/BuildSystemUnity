using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityUtility;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UnityFunction
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputDetector : SingletonBehaviour<InputDetector>
    {
        public LayerMask IgnoreLayer
        {
            get { return _ignoreLayer; }
            set
            {
                if (_ignoreLayer != value)
                    SetIgnoreLayerMask(value);
            }
        }

        [SerializeField] private LayerMask _ignoreLayer;

        public Vector2 MoveValue { get; private set; } = Vector2.zero;

        #region Primary
        public TouchPhase PrimaryPhase { get; private set; } = TouchPhase.None;
        public Vector2 PrimaryPosition { get; private set; } = Vector2.zero;
        public Vector2 PrimaryDelta { get; private set; } = Vector2.zero;
        public Vector2 PrimaryScroll { get; private set; } = Vector2.zero;
        public bool IsPrimaryPressed { get; private set; } = false;
        public bool IsUILayerDetected { get; private set; } = false;

        private Vector2 _previousPosition = Vector2.zero;
        private Coroutine _stationaryCo;
        private Coroutine _endedCo;
        private WaitForEndOfFrame _endedWait = new WaitForEndOfFrame();

        public static event Action<TouchPhase> OnPrimaryPhaseListener;
        public static event Action<TouchPhase> OnValidPrimaryPhaseListener;
        #endregion

        #region Normal
        public PressedState PressedState
        {
            get
            {
                var pressedCount = _touchDetector.ValidTouchCount;

                if (pressedCount == 0)
                    return IsPrimaryPressed ? PressedState.Single : PressedState.None;
                else
                    return _touchDetector.PressedState;
            }
        }

        public static Action<int, TouchPhase> OnPhaseListener;
        #endregion

        private TouchDetector _touchDetector;

        private Coroutine _enableInputCo = null;
        [SerializeField] private PlayerInput _playerInput;
        private List<InputAction> _actionList = new List<InputAction>();
        
        private void Reset()
        {
            _playerInput = gameObject.GetComponent<PlayerInput>();
        }

		private void OnApplicationPause(bool pause)
		{
            OnPause(pause);
        }

		protected override void OnDestroy()
		{
            if (_stationaryCo != null)
                _stationaryCo.Stop(this);

            _stationaryCo = null;

            base.OnDestroy();
		}

		protected override void Awake()
        {
            base.Awake();

            if (_playerInput == null)
			{
             
                return;
			}

            _touchDetector = new LegacyTouchDetector();
			_touchDetector.OnTouchPhaseListener += (index, phase) =>
			{
				
				OnPhaseListener?.Invoke(index, phase);
			};

            SetIgnoreLayerMask(_ignoreLayer);

            var tapAction = _playerInput.actions[nameof(Tap)];
            tapAction.started += Tap;
            tapAction.performed += Tap;
            tapAction.canceled += Tap;
            _actionList.Add(tapAction);

            var positionAction = _playerInput.actions[nameof(Position)];
            positionAction.started += Position;
            positionAction.performed += Position;
            positionAction.canceled += Position;
            _actionList.Add(positionAction);

            var dragAction = _playerInput.actions[nameof(Drag)];
            dragAction.started += Drag;
            dragAction.performed += Drag;
            dragAction.canceled += Drag;
            _actionList.Add(dragAction);

            var scrollAction = _playerInput.actions[nameof(Scroll)];
            scrollAction.started += Scroll;
            scrollAction.performed += Scroll;
            scrollAction.canceled += Scroll;
            _actionList.Add(scrollAction);

            var moveAction = _playerInput.actions[nameof(Move)];
            moveAction.started += Move;
            moveAction.performed += Move;
            moveAction.canceled += Move;
            _actionList.Add(moveAction);

            EnableAction(true);
        }

        protected virtual void Update()
        {
            if (_touchDetector == null)
                return;

            _touchDetector.Update();
        }

        #region Common
        private void OnPause(bool isPause)
		{
            if (_playerInput != null)
                return;

            if (isPause == true)
            {
                Clear();
                EnableAction(false);
            }
            else
            {
                if (_enableInputCo != null)
                    _enableInputCo.Stop(this);

                _enableInputCo = EnablePlayerInput().CStart(this);
            }
        }

        private IEnumerator EnablePlayerInput()
		{
            if (_playerInput != null)
                _playerInput.enabled = false;

            yield return new WaitForEndOfFrame();

            if (_playerInput != null)
                _playerInput.enabled = true;

            EnableAction(true);

            _enableInputCo = null;
        }

        public void Clear()
		{
            PrimaryPhase = TouchPhase.None;
            PrimaryPosition = Vector2.zero;
            PrimaryDelta = Vector2.zero;
            PrimaryScroll = Vector2.zero;
            IsPrimaryPressed  = false;
            IsUILayerDetected  = false;

            if (_touchDetector != null)
                _touchDetector.Clear();
        }

        private void SetIgnoreLayerMask(LayerMask layerMask)
        {
            _ignoreLayer = layerMask;

            _touchDetector.SetLayerMask(layerMask);
        }

        private TouchPhase ConvertTouchPhase(InputActionPhase actionPhase)
		{
            var phase = TouchPhase.None;

            switch (actionPhase)
            {
                case InputActionPhase.Started:
                    {
                        phase = TouchPhase.Began;
                    }
                    break;
                case InputActionPhase.Performed:
					{
                        phase = TouchPhase.Moved;
                    }
                    break;
                case InputActionPhase.Waiting:
					{
                        phase = TouchPhase.Stationary;
					}
                    break;
                case InputActionPhase.Canceled:
                    {
                        phase = TouchPhase.Ended;
                    }
                    break;
            }

            return phase;
        }
        #endregion

        #region Move
        private void Move(InputAction.CallbackContext context)
		{
            MoveValue = context.ReadValue<Vector2>();

            //UnityLog.Log($"MoveValue : {MoveValue}", DebugLogLevel.Row);
        }
		#endregion

		#region Tap
		private void Tap(InputAction.CallbackContext context)
		{
            IsPrimaryPressed = context.control.IsPressed();
            PrimaryPhase = ConvertTouchPhase(context.phase);

           

            switch (PrimaryPhase)
			{
                case TouchPhase.Began:
					{
                        IsUILayerDetected = PrimaryPosition.IsPointerOverUIObject(_ignoreLayer);

                        if (IsUILayerDetected == true)
                            OnValidPrimaryPhaseListener?.Invoke(PrimaryPhase);

                        if(this != null)
						{
                            _stationaryCo = StationaryCheckCo().CStart(this);
                            _endedCo?.Stop(this);
                        }
                        //UnityLog.Log($"IsUILayerDetected : {PrimaryPosition}, {IsUILayerDetected}", DebugLogLevel.Row);
                    }
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
					{
                        if (IsUILayerDetected == true)
                            OnValidPrimaryPhaseListener?.Invoke(PrimaryPhase);

                        if (this != null)
						{
                            _stationaryCo?.Stop(this);
                            _stationaryCo = null;
                            _endedCo?.Stop(this);
                            _endedCo = EndedCo().CStart(this);
                        }
                    }
                    break;
			}

            OnPrimaryPhaseListener?.Invoke(PrimaryPhase);
            //UnityLog.Log($"Tap orgPhase : {context.phase}, touchPhase : {PrimaryPhase}, Pressed{IsPrimaryPressed}", DebugLogLevel.Row);
        }

        private IEnumerator StationaryCheckCo()
        {
            while(true)
			{
                if(IsPrimaryPressed == true)
                    PrimaryPhase = _previousPosition == PrimaryPosition ? TouchPhase.Stationary : TouchPhase.Moved;

                _previousPosition = PrimaryPosition;

                yield return null;
			}
        }

        private IEnumerator EndedCo()
		{
            yield return _endedWait;

            IsUILayerDetected = false;
            PrimaryPhase = TouchPhase.None;
		}

        #endregion

        #region Drag
        private void Drag(InputAction.CallbackContext context)
		{            
            PrimaryDelta = context.ReadValue<Vector2>();
            //UnityLog.Log($"PrimaryDelta : {PrimaryDelta}");
		}
		#endregion

		#region Position
        private void Position(InputAction.CallbackContext context)
		{
            PrimaryPosition = context.ReadValue<Vector2>();
        }
        #endregion

        #region Scroll
        public void Scroll(InputAction.CallbackContext context)
		{
            PrimaryScroll = context.ReadValue<Vector2>();

            //UnityLog.Log($"Scroll : {PrimaryScroll}");
		}
		#endregion

		#region Normal
		public TouchPhase Phase(int index, bool isLayerCheck = false)
		{
            var touches = isLayerCheck == false ? _touchDetector.Touches : _touchDetector.ValidTouches;

            if (touches == null || touches.Count == 0 || touches.Count < index)
                return TouchPhase.None;
            else
                return touches[index].phase;
        }

        public bool Pressed(int index, bool isLayerCheck = false)
		{
            var touches = isLayerCheck == false ? _touchDetector.Touches : _touchDetector.ValidTouches;

            if (touches == null || touches.Count == 0 || touches.Count < index)
                return false;
            else
                return touches[index].isInProgress;
        }

		#region Positioin
		public Vector2 Position(int index, bool isLayerCheck = false)
		{
            var touches = isLayerCheck == false ? _touchDetector.Touches : _touchDetector.ValidTouches;

            if (touches == null || touches.Count == 0 || touches.Count < index)
                return Vector2.zero;
            else
                return touches[index].position;
        }
        #endregion

        #region Delta
        public Vector2 Delta(int index, bool isLayerCheck = false)
		{
            var touches = isLayerCheck == false ? _touchDetector.Touches : _touchDetector.ValidTouches;

      

            if (touches == null || touches.Count == 0 || touches.Count < index)
                return Vector2.zero;
            else
                return touches[index].delta;
        }
		#endregion

		#region MultipleDelta
		public float MultipleDelta(bool isLayerCheck = false)
        {
            var touches = _touchDetector.Touches;

            if (isLayerCheck == true)
                touches = _touchDetector.ValidTouches;

            if (touches.Count == 0)
                return PrimaryScroll.y;
            else if(touches.Count > 1)
			{
                Vector2 position01 = touches[0].position;
                Vector2 delta01 = touches[0].delta;

                Vector2 position02 = touches[1].position;
                Vector2 delta02 = touches[1].delta;

                Vector2 positionDifference01 = position01 - delta01;
                Vector2 positionDifference02 = position02 - delta02;

                float differecnceMagnitude = (positionDifference01 - positionDifference02).magnitude;
                float positionMagnitude = (position01 - position02).magnitude;

                var multipleDelta = differecnceMagnitude - positionMagnitude;

                return multipleDelta;
            }

            return 0;
        }
		#endregion

		#region TouchValue
		public TouchValue GetTouch(int index, bool isLayerCheck = false)
        {
            if (_touchDetector.TouchCount == 0)
                return null;

            List<TouchValue> touches = isLayerCheck == true ? _touchDetector.ValidTouches : _touchDetector.Touches;

            if (touches.Count <= index)
                return null;
            
            return touches[index];
        }
		#endregion

		#endregion

		#region Enable & Disable
        private void EnableAction(bool isEnabled)
		{
            foreach (var action in _actionList)
			{
                if (isEnabled == true)
                    action.Enable();
                else
                    action.Disable();
            }   
        }
		#endregion
	}
}
