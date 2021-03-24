using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface ICursorHandler
{
    void CursorEnter();
    void CursorExit();
    void CursorDown();
    void NotInGame();
}


public interface IGOUIModule
{
    IBranch TargetBranch  { get; }
    InGameUiTurnOn TurnOnWhen { get; }
    InGameUiTurnOff TurnOffWhen { get; }
    void StartInGameUi();
    void ExitInGameUi();
    void StartChild(bool startChild);
    void HighlightBranch();
    void UnHighlightBranch();
    void CheckForSetLayerMask(LayerMask layerMask);
}

public interface IGOUIObject
{
    void ActivateObject(bool active);
}

public enum InGameUiTurnOn
{
    OnClick, OnEnter
}
public enum InGameUiTurnOff
{
    Standard, AlsoOnPointerOnExit
}


namespace UIElements
{

    public class GOUIModule : MonoBehaviour, IEventUser, ICursorHandler, IEventDispatcher, 
                              IGOUIModule, ISetUpUIGOBranch, IStartBranch, ICloseBranch, IClearAll
    {
        [SerializeField] private UIBranch _branch;
        [SerializeField] private RectTransform _mainCanvasRect;

        [SerializeField] 
        [DisableIf(AppIsRunning)] 
        private InGameUiTurnOn _turnOnWhen;
        [SerializeField] 
        [DisableIf(AppIsRunning)] 
        private InGameUiTurnOff _turnOffWhen;
        [SerializeField] private IsActive _startChildOnSwitchPressed = IsActive.Yes;
        [SerializeField] 
        [Space(10f, order = 1)] [InfoBox(GOUIModule.InfoBox, order = 2)] 
        private Transform _uiPosition;

        [SerializeField] [Space(20f)] private UnityEvent<bool> _activeGOUI;

        //Variables
        private bool _active;
        private bool _allowKeys;
        private GOUIController _controller;
        private bool _onHomeScreen = true;
        private bool _gameIsPaused;
        private bool _canStart;
        private bool _startChild;
        private UINode _inGameUINode;

        //Editor
        private const string AppIsRunning = nameof(IsRunning);
        private bool IsRunning() => Application.isPlaying;

        //Events
        private Action<ISetUpUIGOBranch> SetUpUIGOBranch { get; set; }
        private Action<IStartBranch> StartBranch { get; set; }
        private Action<ICloseBranch> CloseBranch { get; set; }
        private Action<IClearAll> ClearAll { get; set; }


        //Editor
        private const string InfoBox = "If left blank the centre of object will be used";

        //Properties & Set / Getters
        public IBranch TargetBranch { get; private set; }
        public OutTweenType OutTweenType { get; } = OutTweenType.Cancel;
        public Action EndOfExitAction { get; } = null;
        private bool DontAcceptMouseInput => _allowKeys || !_onHomeScreen || _gameIsPaused || !_canStart;
        public InGameUiTurnOn TurnOnWhen => _turnOnWhen;
        public InGameUiTurnOff TurnOffWhen => _turnOffWhen;
        private void SetAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
        private void CanStart(IOnStart args) => _canStart = true;
        public Transform UIGOTransform => _uiPosition;
       public GOUIModule UIGOModule => this;
        public RectTransform MainCanvas => _mainCanvasRect;
        private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
        private void SaveIsGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
        public void StartChild(bool startChild) => _startChild = startChild;


        public void CheckForSetLayerMask(LayerMask layerMask)
        {
            var temp = LayerMask.LayerToName(gameObject.layer);
            var thisLayerMask = LayerMask.GetMask(temp);
            if ((thisLayerMask & layerMask) == 0)
            {
                throw new Exception
                    ($"Set Layer on {gameObject.name} to _layerToHit found in UI GameObject Controller");
            }
        }

        //Main
        private void Awake()
        {
            TargetBranch = _branch;
            _inGameUINode = (UINode) TargetBranch.DefaultStartOnThisNode;
            _controller = FindObjectOfType<GOUIController>();
            
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
            ClearAll = EVent.Do.Fetch<IClearAll>();
            
        }
        
        private void OnEnable() => ObserveEvents();

        private void Start()
        {
            SetUpUIGOBranch.Invoke(this);
        }

        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IClearAll>(ClearUI);
            EVent.Do.Subscribe<IAllowKeys>(SetAllowKeys);
            EVent.Do.Subscribe<IClearScreen>(CancelUIForFullScreen);
            EVent.Do.Subscribe<IInMenu>(CancelWhenInMenu);
            EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            EVent.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
            EVent.Do.Subscribe<IOnStart>(CanStart);
        }

        public void OverFocus()
        {
            if(_active || _turnOnWhen == InGameUiTurnOn.OnClick) return;
            StartInGameUi();
        }

        public void UnFocus() 
        {
            if (!_active) return;
            ExitInGameUi();
        }

        public void SwitchGOUI_MouseOnly() => StartInGameUi();
        
        /// <summary>
        /// For use by External scripts and Events to trigger GOUI
        /// </summary>
        public void ActivateGOUI() => StartInGameUi();

        private void OnMouseEnter()
        {
            if(DontAcceptMouseInput || _turnOnWhen != InGameUiTurnOn.OnEnter) return;
            
            _startChild = true;
           StartInGameUi();
        }

        private void OnMouseExit()
        {
            if(DontAcceptMouseInput || _turnOffWhen != InGameUiTurnOff.AlsoOnPointerOnExit) return;
            if(StayOnIfChildIsActive()) return;
            
            ClearAll?.Invoke(this);
            ExitInGameUi();
        }

        private bool StayOnIfChildIsActive() 
            => _branch.GetStayOn() == IsActive.Yes && HasAnActiveChildBranch() 
                                                   && _turnOffWhen != InGameUiTurnOff.AlsoOnPointerOnExit;

        private bool HasAnActiveChildBranch() => _branch.LastSelected.HasChildBranch.CanvasIsEnabled;

        private void OnMouseDown()
        {
             if(DontAcceptMouseInput) return;
             
             if (!_active)
             {
                 if(_turnOnWhen != InGameUiTurnOn.OnClick) return;
                 StartInGameUi();
                 return;
             }

             if (_active && _turnOffWhen == InGameUiTurnOff.Standard)
             {
                 ExitInGameUi();
             }
        }

        public void CursorEnter(){} /*=> _GOUIInput.EnterGO(_active);*/

        public void CursorExit(){} /*=> _GOUIInput.ExitGO(_active);*/

        public void CursorDown()
        {
        }

        public void NotInGame()
        {
            //_GOUIInput.ClearGOUIExceptHighlighted();
        }

        private void CancelUIForFullScreen(IClearScreen args) => CancelUi();

        private void CancelWhenInMenu(IInMenu args)
        {
            // if (args.InTheMenu && _active)
            // {
            //     CancelUi();
            // }
        }

        public void CancelUi()
        {
            if(!_active) return;
            //_GOUIInput.ClearGOUI();
           // ExitInGameUi();
        }

        private void ClearUI(IClearAll args = null)
        {
           // Debug.Log("Clear All : UIGO");
            if(!_active ) return;
            ExitInGameUi();
        }

        public void StartInGameUi()
        {
            if(_active) return;
            _controller.SetIndex(this);
            _active = true;
            StartBranch?.Invoke(this);
            _activeGOUI.Invoke(_active);
            
            ActivateChildIfConditionAllows(_startChild);
        }

        private void ActivateChildIfConditionAllows(bool condition)
        {
            if (_startChildOnSwitchPressed == IsActive.No || !condition) return;
            
            _inGameUINode.OnPointerDown(new PointerEventData(EventSystem.current));
        }

        public void ExitInGameUi()
        {
            if(!_active) return;
            _active = false;
            _startChild = false;
            CloseBranch?.Invoke(this);
            _activeGOUI.Invoke(_active);
        }

        public void HighlightBranch()
        {
            _inGameUINode.SetNodeAsActive();
        }

        public void UnHighlightBranch()
        {
            ClearAll?.Invoke(this);
            _inGameUINode.ClearNode();
        }
    }
}
