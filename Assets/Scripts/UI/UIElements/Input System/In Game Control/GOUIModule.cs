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

namespace UIElements
{

    public class GOUIModule : MonoBehaviour, IEventUser, ICursorHandler, IEventDispatcher, 
                              IGOUIModule, ISetUpUIGOBranch, IStartBranch, ICloseBranch, IClearAll
    {
        [SerializeField] 
        private UIBranch _branch;
        
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
        private bool _onHomeScreen = true;
        private bool _gameIsPaused;
        private bool _canStart;

        //Events
        private Action<ISetUpUIGOBranch> SetUpUIGOBranch { get; set; }
        private Action<IStartBranch> StartBranch { get; set; }
        private Action<ICloseBranch> CloseBranch { get; set; }
        
        //Editor
        private const string InfoBox = "If left blank the centre of object will be used";

        //Properties & Set / Getters
        public IBranch TargetBranch => _branch;
        public IsActive AlwaysOn => _alwaysOnUI;
        public Transform UIGOTransform => _uiPosition;
        public GOUIModule ReturnGOUIModule => this;
        private bool CanNotDoAction => !_onHomeScreen || _gameIsPaused || !_canStart;
        private void CanStart(IOnStart args) => _canStart = true;
        private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
        private void SaveIsGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;

        //Main
        private void Awake()
        {
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
            Debug.Log("Just got to get the select work with the virual cursor then test everything");
            EVent.Do.Subscribe<IClearAll>(ClearUI);
            EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
            EVent.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
            EVent.Do.Subscribe<IOnStart>(CanStart);
        }

        //TODO expland so can select many too
        /// <summary>
        /// For use by External scripts and Events to trigger GOUI
        /// </summary>
        public void ActivateGOUI() => StartInGameUi();
        public void DactivateGOUI() => ExitInGameUi();

        private void OnMouseEnter() => EnterGOUI();
        
        private void OnMouseExit() => ExitGOUI();

        private void OnMouseDown() => SelectedGOUI();

        public void VirtualCursorEnter() => EnterGOUI();

        public void VirtualCursorExit() => ExitGOUI();

        public void VirtualCursorDown() => SelectedGOUI();


        private void EnterGOUI()
        {
            if (CanNotDoAction) return;

            StartInGameUi();
        }

        private void ExitGOUI()
        {
            if (CanNotDoAction) return;

            ExitInGameUi();
        }
        
        private void SelectedGOUI()
        {
            Debug.Log($"{_branch} : {_active}");
            
            if (CanNotDoAction) return;
            
            if (_active)
            {
                ExitInGameUi();
            }
            else
            {
                StartInGameUi();
            }
        }
        
        private void ClearUI(IClearAll args = null)
        {
            if(!_active) return;
            ExitInGameUi();
        }

        public void StartInGameUi()
        {
            _active = true;
            _branch.MoveToThisBranch();
            StartBranch?.Invoke(this);
            _activeGOUI.Invoke(_active);
        }

        public void ExitInGameUi()
        {
            _active = false;
            _branch.StartBranchExitProcess(OutTweenType.Cancel);
            CloseBranch?.Invoke(this);
            _activeGOUI.Invoke(_active);
        }
    }
}
