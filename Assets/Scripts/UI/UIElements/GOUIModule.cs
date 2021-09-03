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
}

public interface IRuntimeData
{
    IBranch TargetBranch { get; }
}

namespace UIElements
{
    [RequireComponent(typeof(RuntimeSettingExample))]
    public class GOUIModule : MonoBehaviour, IEZEventUser, ICursorHandler, IEZEventDispatcher, 
                              IGOUIModule, IStartGOUIBranch, ICloseBranch, IServiceUser, IRuntimeData
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
        private bool _active, _GOUIBranchSetUp;
        private readonly CheckIfUnderUI _checkIfUnderUI = new CheckIfUnderUI();
        private IDataHub _myDataHub;
        private IBranch _myGOUIBranch;

        //Enums
        private enum StartGOUI { AlwaysOn, OnPointerEnter }
        
        //Events
        private Action<IStartGOUIBranch> StartBranch { get; set; }
        
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
        private bool CanNotDoAction => !OnHomeScreen || GameIsPaused || !_myDataHub.SceneStarted;
        private void CanStart(IOnStart args) => StartUpAlwaysOnBranch();
        private void SceneIsChanging(ISceneIsChanging args) => SceneChanging = true;

        private bool GameIsPaused => _myDataHub.GamePaused;
        private bool OnHomeScreen => _myDataHub.OnHomeScreen;
        private bool SceneChanging { get; set; }
        private bool AllowKeys => _myDataHub.AllowKeys;
        private string BranchesName => $"{name} : InGameObj";

        //Main
        private void Awake()
        {
            if (DisableIfNotInUse()) return;
            
            if (_useOffsetPosition == null)
                _useOffsetPosition = transform;
            
            _myGOUIBranch = new RuntimeCreateBranch().CreateGOUI(_myGOUIPrefab).NewName(BranchesName);
        }

        public void RenameGouiBranch() => _myGOUIBranch.NewName(BranchesName);

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
            _checkVisibility.OnEnable();
            _myGOUIBranch.OnEnable();
            LateStartUp();
        }

        public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

        public void FetchEvents() => StartBranch = GOUIEvents.Do.Fetch<IStartGOUIBranch>();

        public void ObserveEvents()
        {
            HistoryEvents.Do.Subscribe<ISceneIsChanging>(SceneIsChanging);
            HistoryEvents.Do.Subscribe<IOnStart>(CanStart);
            HistoryEvents.Do.Subscribe<IHighlightedNode>(ClearNodeWhenLeftOnWhenControlsChange);
            HistoryEvents.Do.Subscribe<ISelectedNode>(ChildIsOpen);
            InputEvents.Do.Subscribe<IAllowKeys>(SetAllowKeys);
            GOUIEvents.Do.Subscribe<ICloseThisGOUIModule>(CloseAsOtherBranchSelectedOrCancelled);
        }

        public void UnObserveEvents()
        {
            HistoryEvents.Do.Unsubscribe<ISceneIsChanging>(SceneIsChanging);
            HistoryEvents.Do.Unsubscribe<IOnStart>(CanStart);
            HistoryEvents.Do.Unsubscribe<IHighlightedNode>(ClearNodeWhenLeftOnWhenControlsChange);
            HistoryEvents.Do.Unsubscribe<ISelectedNode>(ChildIsOpen);
            InputEvents.Do.Unsubscribe<IAllowKeys>(SetAllowKeys);
            GOUIEvents.Do.Unsubscribe<ICloseThisGOUIModule>(CloseAsOtherBranchSelectedOrCancelled);
        }

        private void LateStartUp()
        {
            if(_myDataHub.IsNull()) return;
            
            if (_myDataHub.SceneStarted)
            {
                CheckGOUIBranchIsSetUp();
                StartUpAlwaysOnBranch();
            }        
        }

        private void OnDisable()
        {
            if(SceneChanging) return;
            _checkVisibility.OnDisable();
            _myGOUIBranch.OnDisable();
            UnObserveEvents();
            _active = false;
            NodeIsSelected = false;
            PointerOver = false;
            _activateGOUI?.Invoke(false);
        }

        private void OnDestroy()
        {
            UnObserveEvents();
            _checkVisibility.OnDestroy();
            _myGOUIBranch.OnDestroy();
            if(SceneChanging) return;
            Destroy(_myGOUIBranch.ThisBranchesGameObject);
        }

        private void Start()
        {
            CheckGOUIBranchIsSetUp();
            _checkVisibility.SetUpOnStart(this);
        }

        private void CheckGOUIBranchIsSetUp()
        {
            if (_GOUIBranchSetUp) return;
            _myGOUIBranch.SetUpGOUIBranch(this);
            _GOUIBranchSetUp = true;
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

        private void SetAllowKeys(IAllowKeys args)
        {
            if(!AllowKeys && _active)
                ExitGOUI();
            
            if (AllowKeys && _active)
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
            if(!AllowKeys)
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
            if(!AllowKeys)
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