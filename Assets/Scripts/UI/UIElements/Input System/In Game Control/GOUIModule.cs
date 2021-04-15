using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public interface ICursorHandler
{
    void VirtualCursorEnter();
    void VirtualCursorExit();
    bool AlwaysOnIsActive { get; }
}

public interface IGOUIModule
{
    IBranch TargetBranch { get; }
    void StartInGameUi();
    void ExitInGameUi();
    bool AlwaysOnIsActive { get; }
}

namespace UIElements
{
    public class GOUIModule : MonoBehaviour, IEventUser, ICursorHandler, IEventDispatcher, 
                              IGOUIModule, ISetUpUIGOBranch, IStartGOUIBranch, ICloseInGameNode,
                              IOffscreen
    {
        [SerializeField]
        private StartGOUI _startHow = StartGOUI.OnPointerEnter;
        
        [SerializeField] 
        private UIBranch _myGOUIBranch;

        [SerializeField] 
        private Renderer _myRenderer;
        [SerializeField] 
        [Range(0, 20)] [Label(FrequencyName)] [Tooltip(FrequencyTooltip)]
        private int _checkFrequency = 10;
        [SerializeField]
        [Space(10f, order = 1)] [InfoBox(InfoBox, order = 2)]
        private Transform _useOffsetPosition = null;
        
        [SerializeField] 
        private IsActive _useOffScreenMarker = IsActive.No;
        
        [SerializeField] 
        [HideIf(NotOffScreenMarker)] [Space(10f)]
        private OffScreenMarker _offScreenMarker;
        

        [SerializeField] 
        [Space(20f)] [Foldout("Events")]
        private UnityEvent<bool> _activeGOUI;

        //Variables
        private bool _active;
        private bool _onHomeScreen = true;
        private bool _gameIsPaused;
        private bool _canStart;
        private INode _lastHighlighted;
        private bool _forceStopExit;
        private Coroutine _coroutine;
        private readonly WaitFrameCustom _waitFrame = new WaitFrameCustom();
        private bool _forceStop;

        private INode _lastSelected;
        private IBranch _activeBranch;

        //Editor
        private const string FrequencyName = "Check Visible Frequency";
        private const string FrequencyTooltip = "How often the system checks and sets the position of the GOUI. " +
                                                "Increase to improve performance but decreases smoothness. " +
                                                "Effects both GOUI and Off Screen Marker";
        
        //Enums
        private enum StartGOUI
        {
            AlwaysOn, OnPointerEnter
        }
        
        //Events
        private Action<ISetUpUIGOBranch> SetUpUIGOBranch { get; set; }
        private Action<IStartGOUIBranch> StartBranch { get; set; }
        private Action<ICloseInGameNode> CloseBranch { get; set; }
        private Action<IOffscreen> GOUIOffScreen { get; set; }
        
        //Editor
        private const string InfoBox = "If left blank the centre of object will be used";
        private const string NotOffScreenMarker = nameof(OffScreen);

        //Properties & Set / Getters
        public IBranch TargetBranch => _myGOUIBranch;
        public bool IsOffscreen { get; private set; }
        private bool OffScreen => _useOffScreenMarker == IsActive.No; 
        public bool CanUseOffScreen => _useOffScreenMarker == IsActive.Yes; 
        public bool AlwaysOnIsActive => _startHow == StartGOUI.AlwaysOn /*&& !_forceStop*/;
        public Transform GOUITransform => _useOffsetPosition;
        public GOUIModule ReturnGOUIModule => this;
        private bool ChildOn()
        {
            if (_myGOUIBranch.LastSelected.HasChildBranch.IsNull())
                return false;
            return _myGOUIBranch.LastSelected.HasChildBranch.CanvasIsEnabled;
        }
        private bool CanNotDoAction => !_onHomeScreen || _gameIsPaused || !_canStart;
        private void CanStart(IOnStart args)
        {
            _canStart = true;
            _coroutine = StaticCoroutine.StartCoroutine(CheckVisibility());
        }
        private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
        private void SaveIsGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
        
        //Main
        private void Awake()
        {
            if (_useOffsetPosition == null)
            {
                _useOffsetPosition = transform;
            }

            FetchEvents();
            _lastHighlighted = _myGOUIBranch.DefaultStartOnThisNode;
            _offScreenMarker.SetParent(this);
            _offScreenMarker.OnAwake();
        }

        public void FetchEvents()
        {
            SetUpUIGOBranch = EVent.Do.Fetch<ISetUpUIGOBranch>();
            StartBranch = EVent.Do.Fetch<IStartGOUIBranch>();
            CloseBranch = EVent.Do.Fetch<ICloseInGameNode>();
            GOUIOffScreen = EVent.Do.Fetch<IOffscreen>();
        }
        
        private void OnEnable() => ObserveEvents();
        
        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IClearAll>(ClearUI);
            EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            EVent.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
            EVent.Do.Subscribe<IOnStart>(CanStart);
            EVent.Do.Subscribe<ISwitchGroupPressed>(SwitchControlPressed);
            EVent.Do.Subscribe<IHighlightedNode>(SaveLastHighlighted);
            EVent.Do.Subscribe<ISelectedNode>(SaveLastSelected);
            EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
            EVent.Do.Subscribe<ICloseGOUIModule>(CloseAsOtherBranchSelected);
            _offScreenMarker.OnEnable();
        }

        private void SaveActiveBranch(IActiveBranch args)
        {
            _activeBranch = args.ActiveBranch;
        }

        private void SaveLastSelected(ISelectedNode args)
        {
            _lastSelected = args.UINode;
        }

        private void OnDisable()
        {
            _offScreenMarker.OnDisable();
            StaticCoroutine.StopCoroutines(_coroutine);
        }

        private void Start()
        {
            _offScreenMarker.OnStart();
            SetUpUIGOBranch.Invoke(this);
        }
        
        private void CloseAsOtherBranchSelected(ICloseGOUIModule args)
        {
            
            //TODO Do I Need this?
            if(args.TargetBranch.IsEqualTo(_myGOUIBranch) && _active)
            {
                Debug.Log("Here");
                _active = false;
                // if (_myGOUIBranch.LastSelected.HasChildBranch == _activeBranch)
                // {
                //     Debug.Log("Same child");
                //     //_myGOUIBranch.DoNotTween();
                // }
                // else
                // {
                    _myGOUIBranch.StartBranchExitProcess(OutTweenType.Cancel);
             //   }
                _activeGOUI.Invoke(_active);
            }        
        }

        private void SaveLastHighlighted(IHighlightedNode args)
        {
            if(args.Highlighted.MyBranch.IsInGameBranch())
                _lastHighlighted = args.Highlighted;
            
            if(!_myGOUIBranch.CanvasIsEnabled) return;
            if (_lastHighlighted.MyBranch.IsInGameBranch() && _lastHighlighted.MyBranch.NotEqualTo(_myGOUIBranch))
            {
                ExitInGameUi();
            }
        }

        private void SwitchControlPressed(ISwitchGroupPressed args)
        {
            if (_myGOUIBranch.CanvasIsEnabled)
                ExitInGameUi();
        }

        //TODO expand so can select many too
        
        /// <summary>
        /// For use by External scripts and Events to trigger GOUI
        /// </summary>
        public void ActivateGOUI() => EnterGOUI();
        public void DactivateGOUI() => ExitGOUI();
        public void ForceDactivateGOUI() => ExitInGameUi();

        private void OnMouseEnter() => EnterGOUI();
        
        private void OnMouseExit() => ExitGOUI();

        public void VirtualCursorEnter() => EnterGOUI();

        public void VirtualCursorExit() => ExitGOUI();

        private void EnterGOUI()
        {
            StartInGameUi();
        }

        private void ExitGOUI()
        {
            if(ChildOn()) return;
            ExitInGameUi();
        }
        
        private void ClearUI(IClearAll args = null)
        {
            if(!_active || CanNotDoAction) return;
            ExitInGameUi();
        }
        
        public void StartInGameUi()
        {
            if(CanNotDoAction || _active) return;
            _active = true;
            StartBranch?.Invoke(this);
            _myGOUIBranch.MoveToThisBranch();
            _activeGOUI.Invoke(_active);
        }

        public void ExitInGameUi()
        {
            if (CanNotDoAction || !_active) return;
            _active = false;
            CloseBranch?.Invoke(this);
            _myGOUIBranch.StartBranchExitProcess(OutTweenType.Cancel);
            _offScreenMarker.StopOffScreenMarker();
            _activeGOUI.Invoke(_active);
        }

       
        private IEnumerator CheckVisibility()
        {
            while (true)
            {
                if (_myRenderer.isVisible)
                {
                    if(IsOffscreen)
                    {
                        IsOffscreen = false;
                        DoTurnOn();
                    }                
                }
                else
                {
                    if(!IsOffscreen)
                    {
                        IsOffscreen = true;
                        DoTurnOff();
                    }                
                }

                yield return _waitFrame.SetFrameTarget(_checkFrequency);
            }

            void DoTurnOn() { ForceOn(); }
            
            void DoTurnOff() { ForceStop(); }
        }

        private void ForceStop()
        {
            GOUIOffScreen?.Invoke(this);

            _myGOUIBranch.MyCanvas.enabled = false;
            _offScreenMarker.StartOffScreenMarker();
        }

        private void ForceOn()
        {
            GOUIOffScreen?.Invoke(this);

            if(_active || AlwaysOnIsActive)
                _myGOUIBranch.MyCanvas.enabled = true;
            _offScreenMarker.StopOffScreenMarker();
        }
    }
}
