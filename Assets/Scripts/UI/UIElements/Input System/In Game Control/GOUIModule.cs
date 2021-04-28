﻿using System;
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
    void SwitchEnter();
    void SwitchExit();
    bool AlwaysOnIsActive { get; }
    bool PointerOver { get; }
    bool NodeIsSelected { get; }
}

namespace UIElements
{
    public class GOUIModule : MonoBehaviour, IEventUser, ICursorHandler, IEventDispatcher, 
                              IGOUIModule, ISetUpUIGOBranch, IStartGOUIBranch, ICloseGOUIBranch, IEServUser
    {
        [SerializeField]
        private StartGOUI _startHow = StartGOUI.OnPointerEnter;
        
        [SerializeField] 
        private UIBranch _myGOUIBranch;

        [SerializeField]
        [Tooltip(InfoBox)]
        private Transform _useOffsetPosition = null;
        
        [SerializeField]
        [Space(10f)]
        private CheckVisibility _checkVisibility = default;
        
        [SerializeField] 
        private OffscreenMarkerData _offScreenMarker;

        [SerializeField] 
        [Space(20f)] [Foldout("Events")]
        private UnityEvent<bool> _activeGOUI;

        //Variables
        private bool _active, _gameIsPaused, _canStart;
        private bool _onHomeScreen = true;
        private readonly CheckIfUnderUI _checkIfUnderUI = new CheckIfUnderUI();
        private IHub _iHub;

        //Enums
        private enum StartGOUI { AlwaysOn, OnPointerEnter }
        
        //Events
        private Action<ISetUpUIGOBranch> SetUpUIGOBranch { get; set; }
        private Action<IStartGOUIBranch> StartBranch { get; set; }
        private Action<ICloseGOUIBranch> CloseAndResetBranch { get; set; }
        
        //Editor
        private const string InfoBox = "If left blank the centre of object will be used";

        //Properties & Set / Getters
        public IBranch TargetBranch => _myGOUIBranch;
        public OffscreenMarkerData OffScreenMarkerData => _offScreenMarker;
        public bool GOUIIsActive => _active; 
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
        
        //Main
        private void Awake()
        {
            if (_useOffsetPosition == null)
                _useOffsetPosition = transform;

            FetchEvents();
            _checkVisibility.SetUp(this);
            _checkVisibility.OnAwake();
        }

        public void FetchEvents()
        {
            SetUpUIGOBranch = EVent.Do.Fetch<ISetUpUIGOBranch>();
            StartBranch = EVent.Do.Fetch<IStartGOUIBranch>();
            CloseAndResetBranch = EVent.Do.Fetch<ICloseGOUIBranch>();
        }
        
        private void OnEnable()
        {
            UseEServLocator();
            ObserveEvents();
            _checkVisibility.OnEnable();

            if(_canStart)
            {
                StartUpAlwaysOnBranch();
                SetUpUIGOBranch.Invoke(this);
            }        
        }
        
        public void UseEServLocator()
        {
            _iHub = EServ.Locator.Get<IHub>(this);
        }

        public void ObserveEvents()
        {
            if(_canStart) return;
            EVent.Do.Subscribe<IClearAll>(ClearUI);
            EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            EVent.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
            EVent.Do.Subscribe<IOnStart>(CanStart);
            EVent.Do.Subscribe<IHighlightedNode>(ClearNodeWhenLeftOnWhenControlsChange);
            EVent.Do.Subscribe<ISelectedNode>(ChildIsOpen);
            EVent.Do.Subscribe<ICloseGOUIModule>(CloseAsOtherBranchSelectedOrCancelled);
            EVent.Do.Subscribe<IAllowKeys>(AllowKeys);
        }
        
        private void OnDisable()
        {
            _checkVisibility.OnDisable();
            
            CloseAndResetBranch?.Invoke(this);
            _active = false;
            NodeIsSelected = false;
            PointerOver = false;
            _activeGOUI.Invoke(_active);
        }

        private void Start()
        {
            _checkVisibility.OnStart();
            SetUpUIGOBranch.Invoke(this);

            if(_iHub.CanStart && !_canStart)
            {
                Debug.Log("Delayed");
                _checkVisibility.CanStart();
                _canStart = true;
                StartUpAlwaysOnBranch();
            }
        }
        
        private void StartUpAlwaysOnBranch()
        {
            if (AlwaysOnIsActive && _canStart)
            {
                PointerOver = true;
                _myGOUIBranch.DontSetBranchAsActive();
                _myGOUIBranch.MoveToThisBranch();
                PointerOver = false;
            }
        }

        private void AllowKeys(IAllowKeys args)
        {
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
        
        private void CloseAsOtherBranchSelectedOrCancelled(ICloseGOUIModule args)
        {
            if (args.TargetBranch.NotEqualTo(_myGOUIBranch) || !_active) return;
            
            NodeIsSelected = false;
            if(PointerOver) return;
            ExitInGameUi();
        }

        private void OnMouseEnter()
        {
            if (_checkIfUnderUI.UnderUI()) return;

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

        public void VirtualCursorEnter() => EnterGOUI();

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
        
        private void ClearUI(IClearAll args = null)
        {
            if(!_active || CanNotDoAction) return;
            ExitInGameUi();
        }

        private void StartInGameUi()
        {
            if(CanNotDoAction || _active) return;
            
            _active = true;
            StartBranch?.Invoke(this);
            _myGOUIBranch.MoveToThisBranch();
            _activeGOUI.Invoke(_active);
        }

        private void ExitInGameUi()
        {
            if (CanNotDoAction || !_active) return;
            _myGOUIBranch.StartBranchExitProcess(OutTweenType.Cancel);
            _active = false;
            _checkVisibility.StopOffScreenMarker();
            _activeGOUI.Invoke(_active);
        }

    }
}