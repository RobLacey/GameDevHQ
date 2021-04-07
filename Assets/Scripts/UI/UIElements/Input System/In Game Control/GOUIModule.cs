using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public interface ICursorHandler
{
    void VirtualCursorEnter();
    void VirtualCursorExit();
    void VirtualCursorDown();
    IsActive AlwaysOn { get; }
}


public interface IGOUIModule
{
    void StartInGameUi();
    void ExitInGameUi();
}

public enum InGameUiTurnOn
{
    OnClick, OnEnter
}
public enum InGameUiTurnOff
{
    OnClick, OnExit
}


namespace UIElements
{

    public class GOUIModule : MonoBehaviour, IEventUser, ICursorHandler, IEventDispatcher, 
                              IGOUIModule, ISetUpUIGOBranch, IStartBranch, ICloseBranch, IClearAll
    {
        [SerializeField] 
        private UIBranch _branch;
        
        [SerializeField] 
        private RectTransform _mainCanvasRect;
        
        [SerializeField] 
        [DisableIf(EConditionOperator.Or, AppIsRunning, WhenAlwaysOnSet)] 
        private InGameUiTurnOn _turnOnWhen;
        
        [SerializeField] 
        [DisableIf(EConditionOperator.Or, AppIsRunning, WhenAlwaysOnSet)] 
        private InGameUiTurnOff _turnOffWhen;
        
        [SerializeField] 
        private IsActive _startChildWhenActivated = IsActive.Yes;
        
        [SerializeField] 
        private IsActive _alwaysOnUI = IsActive.No;

        [SerializeField] 
        [Space(10f, order = 1)] [InfoBox(GOUIModule.InfoBox, order = 2)] 
        private Transform _uiPosition;

        [SerializeField] 
        [Space(20f)] 
        private UnityEvent<bool> _activeGOUI;

        //Variables
        private bool _active;
        private bool _allowKeys;
        private bool _onHomeScreen = true;
        private bool _gameIsPaused;
        private bool _canStart;

        //Editor
        private const string AppIsRunning = nameof(IsRunning);
        private bool IsRunning() => Application.isPlaying;

        //Events
        private Action<ISetUpUIGOBranch> SetUpUIGOBranch { get; set; }
        private Action<IStartBranch> StartBranch { get; set; }
        private Action<ICloseBranch> CloseBranch { get; set; }


        //Editor
        private const string InfoBox = "If left blank the centre of object will be used";
        private bool AlwaysOnSet => _alwaysOnUI == IsActive.Yes;
        private const string WhenAlwaysOnSet = nameof(AlwaysOnSet);

        //Properties & Set / Getters
        public IBranch TargetBranch => _branch;
        public IsActive StartChildWhenActivated => _startChildWhenActivated;
        public IsActive AlwaysOn => _alwaysOnUI;
        public Transform UIGOTransform => _uiPosition;
        public GOUIModule ReturnGOUIModule => this;
        public RectTransform MainCanvas => _mainCanvasRect;
        private bool CanNotDoAction => _allowKeys || !_onHomeScreen || _gameIsPaused || !_canStart;
        private void SetAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
        private void CanStart(IOnStart args) => _canStart = true;
        private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
        private void SaveIsGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;

        //Main
        private void Awake()
        {
            Debug.Log("Upto : Check Through rest of In Game control to make sure works and tidy");
            
            if (_uiPosition == null)
            {
                _uiPosition = transform;
            }
            FetchEvents();
        }

        public void FetchEvents()
        {
            SetUpUIGOBranch = EVent.Do.Fetch<ISetUpUIGOBranch>();
            StartBranch = EVent.Do.Fetch<IStartBranch>();
            CloseBranch = EVent.Do.Fetch<ICloseBranch>();
        }
        
        private void OnEnable() => ObserveEvents();

        private void Start() => SetUpUIGOBranch.Invoke(this);

        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IClearAll>(ClearUI);
            EVent.Do.Subscribe<IAllowKeys>(SetAllowKeys);
            EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            EVent.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
            EVent.Do.Subscribe<IOnStart>(CanStart);
        }

        //TODO expland so can select many too
        /// <summary>
        /// For use by External scripts and Events to trigger GOUI
        /// </summary>
        public void ActivateGOUI() => StartInGameUi();

        private void OnMouseEnter() => EnterGOUI();
        
        private void OnMouseExit() => ExitGOUI();

        private void OnMouseDown() => SelectedGOUI();

        public void VirtualCursorEnter() => EnterGOUI();

        public void VirtualCursorExit() => ExitGOUI();

        public void VirtualCursorDown() => SelectedGOUI();


        private void EnterGOUI()
        {
            if (CanNotDoAction || _turnOnWhen != InGameUiTurnOn.OnEnter) return;

            StartInGameUi();
        }

        private void ExitGOUI()
        {
            if (CanNotDoAction || _turnOffWhen != InGameUiTurnOff.OnExit) return;
            if (StayOnIfChildIsActive()) return;

            ExitInGameUi();
        }
        
        private bool StayOnIfChildIsActive()
        {
            return HasAnActiveChildBranch();
        }

        private bool HasAnActiveChildBranch() => _branch.LastSelected.HasChildBranch.CanvasIsEnabled;

        private void SelectedGOUI()
        {
            if (CanNotDoAction) return;

            if (!_active)
            {
                if (_turnOnWhen != InGameUiTurnOn.OnClick) return;
                StartInGameUi();
                return;
            }

            if (_active && _turnOffWhen == InGameUiTurnOff.OnClick)
            {
                ExitInGameUi();
            }
        }
        
        private void ClearUI(IClearAll args = null)
        {
            if(!_active ) return;
            ExitInGameUi();
        }

        public void StartInGameUi()
        {
            _active = true;
             StartBranch?.Invoke(this);
            _activeGOUI.Invoke(_active);
        }

        public void ExitInGameUi()
        {
            _active = false;
            CloseBranch?.Invoke(this);
            _activeGOUI.Invoke(_active);
        }
    }
}
