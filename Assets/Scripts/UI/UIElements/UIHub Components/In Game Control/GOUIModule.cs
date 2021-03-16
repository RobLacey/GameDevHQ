using System;
using NaughtyAttributes;
using UnityEngine;
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
    void SwitchActive(bool canStart);
}

public enum InGameUiTurnOn
{
    OnClick, OnEnter
}
public enum InGameUiTurnOff
{
    OnClick, OnExit, Both, ScriptCall
}


namespace UIElements
{

    public class GOUIModule : MonoBehaviour, IEventUser, ICursorHandler, /*IActiveInGameObject,*/ IEventDispatcher, 
                              IGOUIModule, ISetUpUIGOBranch, IStartBranch, ICloseBranch
    {
        [SerializeField] private UIBranch _branch;
        [SerializeField] private RectTransform _mainCanvasRect;

        [SerializeField] 
        [DisableIf(AppIsRunning)] 
        private InGameUiTurnOn _turnOnWhen;
        [SerializeField] 
        [DisableIf(AppIsRunning)] 
        private InGameUiTurnOff _turnOffWhen;
        [SerializeField] 
        [Space(10f, order = 1)] [InfoBox(GOUIModule.InfoBox, order = 2)] 
        private Transform _uiPosition;

        [SerializeField] private bool _canStartChildBranch;

        //Variables
        private bool _active;
        private bool _allowKeys;
        private GOUIController _controller;
      //  private IGOUIInput _GOUIInput;
        private bool _onHomeScreen = true;
        private bool _gameIsPaused;
        private bool _canStart;
        private bool _switchActive;

        //Editor
        private const string AppIsRunning = nameof(IsRunning);
        private bool IsRunning() => Application.isPlaying;


        //Events
        // private Action<IActiveInGameObject> SetAsActiveGameObject { get; set; }
        private Action<ISetUpUIGOBranch> SetUpUIGOBranch { get; set; }
        private Action<IStartBranch> StartBranch { get; set; }
        private Action<ICloseBranch> CloseBranch { get; set; }


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
        public void SwitchActive(bool canSwitch) => _switchActive = canSwitch;


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
            _controller = FindObjectOfType<GOUIController>();
            //_GOUIInput = new GOUIInput(this);
            
            if (_uiPosition == null)
            {
                _uiPosition = transform;
            }
            FetchEvents();
        }

        public void FetchEvents()
        {
            // SetAsActiveGameObject = EVent.Do.Fetch<IActiveInGameObject>();
            SetUpUIGOBranch = EVent.Do.Fetch<ISetUpUIGOBranch>();
            StartBranch = EVent.Do.Fetch<IStartBranch>();
            CloseBranch = EVent.Do.Fetch<ICloseBranch>();
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
           // EVent.Do.Subscribe<IActiveInGameObject>(SetActivateUIGO);
        }


        /*
        private void SetActivateUIGO(IActiveInGameObject args)
        {
            Debug.Log(args.MyGOUIModule);
            
            if(args.MyGOUIModule.IsNull() || args.MyGOUIModule != this)
            {
                if(!_active) return;
                ExitInGameUi();
            }
        }
        */

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
            if(DontAcceptMouseInput) return;
            //_GOUIInput.EnterGO(_active);
           StartInGameUi();
        }

        private void OnMouseExit()
        {
            if(DontAcceptMouseInput) return;
            // _GOUIInput.ExitGO(_active);
            if(_branch.GetStayOn() == IsActive.Yes && _branch.LastSelected.HasChildBranch.CanvasIsEnabled) return;
            ExitInGameUi();
        }
        
        private void OnMouseDown()
        {
            // if(DontAcceptMouseInput) return;
            // _GOUIInput.ClickOnGO(_active);
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
            Debug.Log("Clear All : UIGO");
            if(!_active ) return;
            ExitInGameUi();
            //_GOUIInput.ClearGOUIExceptHighlighted();
        }

        public void StartInGameUi()
        {
            if(_active) return;
            _controller.SetIndex(this);
            _active = true;
            //SetAsActiveGameObject?.Invoke(this);
            StartBranch?.Invoke(this);
            
            Debug.Log("Here : look at how index works with different triggers. " +
                      "Add OnEnter/OnExit then work on VC and Switcher :(");
            
            
            if(_canStartChildBranch && _switchActive)
            {
                UINode temp = (UINode) TargetBranch.DefaultStartOnThisNode;
                temp.OnPointerDown(new PointerEventData(EventSystem.current));
            }
        }

        public void ExitInGameUi()
        {
            if(!_active) return;
            CloseBranch?.Invoke(this);
            _active = false;
            //SetAsActiveGameObject?.Invoke(null);
        }
    }
}
