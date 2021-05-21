using System;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public interface ICursorHandler
{
    void VirtualCursorEnter();
    void VirtualCursorExit();
}

public interface IGOUIModule
{
    bool GOUIModuleCanBeUsed { get; }
    void SwitchEnter();
    void SwitchExit();
    bool AlwaysOnIsActive { get; }
    bool PointerOver { get; }
    bool NodeIsSelected { get; }
    Transform GOUITransform { get; }
    IBranch TargetBranch { get; }
}

namespace UIElements
{
    [RequireComponent(typeof(RunTimeGetter))]
    public class GOUIModule : MonoBehaviour, IEZEventUser, ICursorHandler, IEZEventDispatcher, 
                              IGOUIModule, IStartGOUIBranch, ICloseGOUIBranch, IServiceUser
    {
        [SerializeField]
        private StartGOUI _startHow = StartGOUI.OnPointerEnter;
        
        [SerializeField] 
        private UIBranch _myGOUIPrefab;

        [SerializeField]
        [Tooltip(InfoBox)]
        private Transform _useOffsetPosition = null;
        
        [SerializeField]
        [Space(10f)]
        private CheckVisibility _checkVisibility = default;
        
        [SerializeField] 
        private OffscreenMarkerData _offScreenMarker;

        [SerializeField] [Foldout("Events")]
        private UnityEvent<bool> _activateGOUI;

        //Variables
        private bool _active, _gameIsPaused, _canStart;
        private bool _onHomeScreen = true;
        private readonly CheckIfUnderUI _checkIfUnderUI = new CheckIfUnderUI();
        private IDataHub _myDataHub;
        private bool _allowKeys;
        private IBranch _myGOUIBranch;
        private bool _isQuiting;
        private bool _sceneChanging;

        //Enums
        private enum StartGOUI { AlwaysOn, OnPointerEnter }
        
        //Events
        private Action<IStartGOUIBranch> StartBranch { get; set; }
        private Action<ICloseGOUIBranch> CloseAndResetBranch { get; set; }
        
        //Editor
        private const string InfoBox = "If left blank the centre of object will be used";

        //Properties & Set / Getters
        public IBranch TargetBranch => _myGOUIBranch;
        public OffscreenMarkerData OffScreenMarkerData => _offScreenMarker;
        public bool GOUIIsActive => _active;
        public bool GOUIModuleCanBeUsed => enabled; 
        public bool AlwaysOnIsActive => _startHow == StartGOUI.AlwaysOn;
        public Transform GOUITransform => _useOffsetPosition;
        public GOUIModule ReturnGOUIModule => this;
        public bool NodeIsSelected { get; private set; }
        public bool PointerOver { get; private set; }
        private bool CanNotDoAction => !_onHomeScreen || _gameIsPaused || !_canStart;
        private void CanStart(IOnStart args)
        {
            _canStart = true;
            StartUpAlwaysOnBranch();
        }
        private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
        private void SaveIsGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
        private void SceneIsChanging(ISceneIsChanging args) => _sceneChanging = true;

        
        //Main
        private void Awake()
        {
            if (DisableIfNotInUse()) return;
            
            if (_useOffsetPosition == null)
                _useOffsetPosition = transform;
            
            _myGOUIBranch = GetComponent<IRunTimeGetter>().CreateBranch(_myGOUIPrefab);
            _myGOUIBranch.SetUpGOUIBranch(this);
        }

        private bool DisableIfNotInUse()
        {
            if (_myGOUIPrefab.IsNull())
            {
                enabled = false;
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            FetchEvents();
            ObserveEvents();
            LateStartUp();
            _checkVisibility.OnEnable();
            _myGOUIBranch.OnEnable();
        }

        public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

        public void FetchEvents()
        {
            StartBranch = GOUIEvents.Do.Fetch<IStartGOUIBranch>();
            CloseAndResetBranch = GOUIEvents.Do.Fetch<ICloseGOUIBranch>();
        }

        public void ObserveEvents()
        {
            HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            HistoryEvents.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
            HistoryEvents.Do.Subscribe<IOnStart>(CanStart);
            HistoryEvents.Do.Subscribe<IHighlightedNode>(ClearNodeWhenLeftOnWhenControlsChange);
            HistoryEvents.Do.Subscribe<ISelectedNode>(ChildIsOpen);
            HistoryEvents.Do.Subscribe<ISceneIsChanging>(SceneIsChanging);
            GOUIEvents.Do.Subscribe<ICloseThisGOUIModule>(CloseAsOtherBranchSelectedOrCancelled);
            InputEvents.Do.Subscribe<IAllowKeys>(AllowKeys);
        }

        private void UnObserveEvents()
        {
            HistoryEvents.Do.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
            HistoryEvents.Do.Unsubscribe<IGameIsPaused>(SaveIsGamePaused);
            HistoryEvents.Do.Unsubscribe<IOnStart>(CanStart);
            HistoryEvents.Do.Unsubscribe<IHighlightedNode>(ClearNodeWhenLeftOnWhenControlsChange);
            HistoryEvents.Do.Unsubscribe<ISelectedNode>(ChildIsOpen);
            HistoryEvents.Do.Unsubscribe<ISceneIsChanging>(SceneIsChanging);
            GOUIEvents.Do.Unsubscribe<ICloseThisGOUIModule>(CloseAsOtherBranchSelectedOrCancelled);
            InputEvents.Do.Unsubscribe<IAllowKeys>(AllowKeys);
        }

        private void LateStartUp()
        {
            if(_myDataHub.IsNull()) return;
            
            if (_myDataHub.SceneAlreadyStarted)
            {
                _canStart = true;
                StartUpAlwaysOnBranch();
            }        
        }

        private void OnDisable()
        {
            if(_sceneChanging) return;
            CloseAndResetBranch?.Invoke(this);
            _checkVisibility.OnDisable();
            _myGOUIBranch.OnDisable();
            UnObserveEvents();
            _active = false;
            NodeIsSelected = false;
            PointerOver = false;
            _activateGOUI?.Invoke(false);
            
            //TODO Add Event to clear history, fullscreen data, switch, interactUI and multiselect lists,

        }

        private void OnDestroy()
        {
            UnObserveEvents();
            _checkVisibility.OnDestroy();
            _myGOUIBranch.OnDestroy();
        }

        private void Start()
        {
            //TODO Fix this to match name wise
            //TODO see why events are not calling to change canvas or adding to multi
            //TODO Add ability to change initial InGameUi from Setter (have variables just needs methods)
            //ToDo ** Don't do above, remove whats there
            
            Debug.Log("I am UpTo here");
            _checkVisibility.SetUpOnStart(this);
        }

        private void StartUpAlwaysOnBranch()
        {
            _myGOUIBranch.ThisBranchesGameObject.SetActive(true);

            StartBranch?.Invoke(this);
            if (AlwaysOnIsActive)
            {
                PointerOver = true;
                _myGOUIBranch.DontSetBranchAsActive();
                _myGOUIBranch.MoveToThisBranch();
                PointerOver = false;
            }
        }

        private void AllowKeys(IAllowKeys args)
        {
            _allowKeys = args.CanAllowKeys;
            
            if(PointerOver)
                ExitGOUI();
            
            if (args.CanAllowKeys && _active)
                PointerOver = true;
        }

        private void ChildIsOpen(ISelectedNode args)
        {
            if (args.UINode == _myGOUIBranch.LastSelected)
                NodeIsSelected = !NodeIsSelected;
        }

        private void ClearNodeWhenLeftOnWhenControlsChange(IHighlightedNode args)
        {
            if(NodeIsSelected || !_active) return;
            
            if (args.Highlighted.MyBranch.NotEqualTo(_myGOUIBranch))
            {
                ExitGOUI();
            }
        }
        
        private void CloseAsOtherBranchSelectedOrCancelled(ICloseThisGOUIModule args)
        {
            if (args.TargetBranch.NotEqualTo(_myGOUIBranch) || !_active) return;
            
            NodeIsSelected = false;
            if(PointerOver) return;
            ExitInGameUi();
        }


        private void OnMouseEnter()
        {
            if (_checkIfUnderUI.UnderUI()) return;
            if(!_allowKeys)
                _myGOUIBranch.DontSetBranchAsActive();
            EnterGOUI();
        }

        private void OnMouseOver()
        {
            if(_checkIfUnderUI.MouseNotUnderUI())
            {
                EnterGOUI();
            }
        }

        /// <summary>
        /// For use by External scripts and Events to trigger GOUI
        /// </summary>
        public void ActivateGOUI() => StartInGameUi();
        public void DactivateGOUI() => ExitInGameUi();

        private void OnMouseExit() => ExitGOUI();

        public void VirtualCursorEnter()
        {
            if(!_allowKeys)
                _myGOUIBranch.DontSetBranchAsActive();
            EnterGOUI();
        }

        public void VirtualCursorExit() => ExitGOUI();
        public void SwitchEnter() => EnterGOUI();

        public void SwitchExit() => ExitGOUI();

        private void EnterGOUI()
        {
            PointerOver = true;
            StartInGameUi();
        }

        private void ExitGOUI()
        {
            PointerOver = false;
            if(NodeIsSelected) return;
            ExitInGameUi();
        }
        
        private void StartInGameUi()
        {
            if(CanNotDoAction || _active) return;
            
            _active = true;
            StartBranch?.Invoke(this);
            _myGOUIBranch.MoveToThisBranch();
            _activateGOUI?.Invoke(_active);
        }

        private void ExitInGameUi()
        {
            if (CanNotDoAction || !_active) return;
            _myGOUIBranch.StartBranchExitProcess(OutTweenType.Cancel);
            _active = false;
            _checkVisibility.StopOffScreenMarker();
            _activateGOUI?.Invoke((_active));
        }
    }
}