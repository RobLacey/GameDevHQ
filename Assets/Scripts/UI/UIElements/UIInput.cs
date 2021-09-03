using System;
using System.Collections.Generic;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;

public interface IInput : IParameters
{
    bool StartInGame();
}

public interface IVirtualCursorSettings : IInput
{
    Transform GetParentTransform { get; }
}

public class UIInput : MonoBehaviour, IEZEventUser, IPausePressed, ICancelPressed, IChangeControlsPressed, 
                       IMenuGameSwitchingPressed, IServiceUser, IEZEventDispatcher, IVirtualCursorSettings,
                       IIsAService
{
    [SerializeField] [Space(10f)]
    [ValidateInput(CheckForScheme, InfoBox)]
    private InputScheme _inputScheme  = default;
    
    [SerializeField] [Foldout("Hot Keys")]    
    [ReorderableList] private List<HotKeys> _hotKeySettings = new List<HotKeys>();
    
    [Header(Settings, order = 2)][HorizontalLine(1f, EColor.Blue, order = 3)] 
    
    [SerializeField] 
    private UIInputEvents _uiInputEvents  = default;

    //Variables
    private bool _inMenu;
    private IDataHub _myDataHub;
    private UINode _lastHomeScreenNode;
    private bool _nothingSelected;
    
    //Editor
    private const string Settings = "Other Settings ";
    private bool HasScheme(InputScheme scheme) => scheme != null;
    private const string InfoBox = "Must Assign an Input Scheme";
    private const string CheckForScheme = nameof(HasScheme);

    //Events
    private IReturnFromEditor ReturnControlFromEditor { get; set; }
    private Action<IPausePressed> OnPausedPressed { get; set; }
    private Action<IMenuGameSwitchingPressed> OnMenuAndGameSwitch { get; set; }
    private Action<ICancelPressed> OnCancelPressed { get; set; }
    private Action<IChangeControlsPressed> OnChangeControlPressed { get; set; }
    
    //Properties and Getters / Setters
    private bool GameIsPaused => _myDataHub.GamePaused;
    private bool AllowKeys => _myDataHub.AllowKeys;
    private bool NoActivePopUps => _myDataHub.NoPopups;
    private bool OnHomeScreen => _myDataHub.OnHomeScreen;
    private IBranch ActiveBranch => _myDataHub.ActiveBranch;
    private bool CanStart => _myDataHub.SceneStarted;

    public bool StartInGame()
    {
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
        {
            return _inputScheme.WhereToStartGame == InMenuOrGame.InGameControl;
        }
        return false;
    }
    
    private void SaveInMenu(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if(_inMenu)
        {
            _uiInputEvents.SwitchBetweenGameAndMenuPressed(InMenuOrGame.InMenu);
        }
        else
        {
            _uiInputEvents.SwitchBetweenGameAndMenuPressed(InMenuOrGame.InGameControl);
        }
    }

    public Transform GetParentTransform => transform;
    private void SaveGameIsPaused(IGameIsPaused args) => _uiInputEvents.GamePausedStatus(GameIsPaused);
    public EscapeKey EscapeKeySettings => ActiveBranch.EscapeKeyType;
    private bool NothingSelectedAction => _inputScheme.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    private IMenuAndGameSwitching MenuToGameSwitching { get; set; }
    private IChangeControl ChangeControl { get; set; }
    private IHistoryTrack HistoryTracker { get; set; }
    private ISwitchGroup SwitchGroups { get; set; }
    private IVirtualCursor VirtualCursor { get; set; }


    //Main
    private void Awake()
    {
        _inputScheme.Awake();
        ChangeControl = EZInject.Class.WithParams<IChangeControl>(this);
        MenuToGameSwitching = EZInject.Class.NoParams<IMenuAndGameSwitching>();
        SwitchGroups = EZInject.Class.NoParams<ISwitchGroup>();
        ReturnControlFromEditor = EZInject.Class.NoParams<IReturnFromEditor>();
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor = EZInject.Class.WithParams<IVirtualCursor>(this);
        AddService();
    }

    public void AddService() => EZService.Locator.AddNew<IInput>(this);

    public void OnRemoveService() { }

    private void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        SetUpHotKeys();
        ChangeControl.OnEnable();
        MenuToGameSwitching.OnEnable();
        SwitchGroups.OnEnable();
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor.OnEnable();
        ObserveEvents();
    }

    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        HistoryTracker = EZService.Locator.Get<IHistoryTrack>(this);
    }

    private void SetUpHotKeys()
    {
        if (_hotKeySettings.Count == 0) return;
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnEnable();
        }
    }
    
    public void FetchEvents()
    {
        OnPausedPressed = InputEvents.Do.Fetch<IPausePressed>();
        OnMenuAndGameSwitch = InputEvents.Do.Fetch<IMenuGameSwitchingPressed>();
        OnCancelPressed = InputEvents.Do.Fetch<ICancelPressed>();
        OnChangeControlPressed = InputEvents.Do.Fetch<IChangeControlsPressed>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IGameIsPaused>(SaveGameIsPaused);
        HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);
    }

    public void UnObserveEvents() { }

    private void Start()   
    {
        ChangeControl.OnStart();
        SwitchGroups.OnStart();
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor.OnStart();
        MenuToGameSwitching.OnStart();
    }

    private void Update()
    {
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor.PreStartMovement();
        
        if (!CanStart) return;

        if(ReturnControlFromEditor.CanReturn(_inMenu, ActiveBranch)) return;
        
        if (CanPauseGame())
        {
            PausedPressedActions();
            return;
        }
        
        if (CanSwitchBetweenInGameAndMenu() && OnHomeScreen) return;
        
        if (CheckIfHotKeyAllowed()) return;
        
        if (_inMenu) InMenuControls();
    }

    private void InMenuControls()
    {
        if (CanDoCancel())
        {
            WhenCancelPressed();
            return;
        }

        if(SwitchGroups.CanSwitchBranches())
        {
            if (SwitchGroups.GOUISwitchProcess() || 
                SwitchGroups.SwitchGroupProcess() || 
                SwitchGroups.BranchGroupSwitchProcess()) return;
        }
        
        if(_inputScheme.CanUseVirtualCursor)
        {
            if (MoveVirtualCursor()) return;
        }
        
        if(MultiSelectPressed) return;
        DoChangeControlPressed();
    }

    private bool MoveVirtualCursor()
    {
        if (!VirtualCursor.CanMoveVirtualCursor()) return false;
        
        VirtualCursor.Update();
        return true;
    }

    private void DoChangeControlPressed() => OnChangeControlPressed?.Invoke(this);

    private bool MultiSelectPressed => _inputScheme.MultiSelectPressed() && !AllowKeys;

    private bool CanPauseGame() => _inputScheme.PressPause() && !MultiSelectPressed;

    private void PausedPressedActions() => OnPausedPressed?.Invoke(this);

    private bool CanSwitchBetweenInGameAndMenu()
    {
        if (!_inputScheme.PressedMenuToGameSwitch()) return false;
        OnMenuAndGameSwitch?.Invoke(this);
        return true;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (GameIsPaused || !NoActivePopUps || MultiSelectPressed) return false;
        if (!HasMatchingHotKey()) return false;
        if(!_inMenu)
            OnMenuAndGameSwitch?.Invoke(this);
        return true;
    }

    private bool HasMatchingHotKey()
    {
        foreach (var hotKeySetting in _hotKeySettings)
        {
            if (hotKeySetting.CheckHotKeys()) return true;
        }
        return false;
    }

    private bool CanDoCancel() => _inputScheme.PressedCancel() && !MultiSelectPressed;

    private void WhenCancelPressed()
    {
         if (CanEnterPauseWithNothingSelected() || CanUnpauseGame())
         {
            PausedPressedActions();
         }
         else
         {
             CancelPressed();
         }
    }

    private bool CanUnpauseGame() => GameIsPaused && ActiveBranch.IsPauseMenuBranch();
    
    private void CancelPressed() => OnCancelPressed?.Invoke(this);

    private bool CanEnterPauseWithNothingSelected() =>
        (NoActivePopUps && !GameIsPaused && HistoryTracker.NoHistory)
        && NothingSelectedAction;

}
