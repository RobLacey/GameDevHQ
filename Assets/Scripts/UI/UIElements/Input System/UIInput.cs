using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

public interface IInput : IParameters
{
    InputScheme ReturnScheme { get; }
    bool StartInGame();
}

public interface IVirtualCursorSettings : IInput
{
    Transform GetParentTransform { get; }
}

public interface ISwitchGroupSettings
{
    InputScheme ReturnScheme { get; }
    IGOUIModule[] GetPlayerObjects();
    void DoChangeControlPressed();
}

public class UIInput : MonoBehaviour, IEventUser, IPausePressed,  
                       ICancelPressed, IChangeControlsPressed, IMenuGameSwitchingPressed, 
                       IEServUser, IEventDispatcher, IClearAll, IVirtualCursorSettings,
                       ISwitchGroupSettings
{
    [SerializeField] [Space(10f)]
    [ValidateInput(CheckForScheme, InfoBox)]
    private InputScheme _inputScheme  = default;
    
    [SerializeField] [Foldout("Hot Keys")]    
    [ReorderableList] private List<HotKeys> _hotKeySettings = new List<HotKeys>();
    
    [Header(Settings, order = 2)][HorizontalLine(1f, EColor.Blue, order = 3)] 
    
    [SerializeField] 
    private InputEvents _inputEvents  = default;

    //Variables
    private bool _canStart, _inMenu, _gameIsPaused, _allowKeys, _nothingSelected;
    private bool _noActivePopUps = true;
    private bool _onHomeScreen = true;
    private UINode _lastHomeScreenNode;
    private IBranch _activeBranch;
    
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
    private Action<IClearAll> OnClearAll { get; set; }

    
    //Properties and Getters / Setters
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
            _inputEvents.SwitchBetweenGameAndMenuPressed(InMenuOrGame.InMenu);
        }
        else
        {
            _inputEvents.SwitchBetweenGameAndMenuPressed(InMenuOrGame.InGameControl);
        }
    }

    public Transform GetParentTransform => transform;
    public IGOUIModule[] GetPlayerObjects() => FindObjectsOfType<GOUIModule>().ToArray<IGOUIModule>();
    private void SaveNoActivePopUps(INoPopUps args) => _noActivePopUps = args.NoActivePopUps;
    private void SaveOnStart(IOnStart onStart) => _canStart = true;
    private void SaveGameIsPaused(IGameIsPaused args)
    {
        _gameIsPaused = args.GameIsPaused;
        _inputEvents.GamePausedStatus(_gameIsPaused);
    }
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void SaveOnHomeScreen (IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveAllowKeys (IAllowKeys args) => _allowKeys = args.CanAllowKeys;
    public InputScheme ReturnScheme => _inputScheme;
    public EscapeKey EscapeKeySettings => _activeBranch.EscapeKeyType;
    private bool NothingSelectedAction => _inputScheme.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    private IMenuAndGameSwitching MenuToGameSwitching { get; set; }
    private IChangeControl ChangeControl { get; set; }
    private IHistoryTrack HistoryTracker { get; set; }
    private ISwitchGroup SwitchGroups { get; set; }
    private IVirtualCursor VirtualCursor { get; set; }


    //Main
    private void Awake()
    {
        _inputScheme.OnAwake();
        ChangeControl = EJect.Class.WithParams<IChangeControl>(this);
        MenuToGameSwitching = EJect.Class.WithParams<IMenuAndGameSwitching>(this);
        VirtualCursor = EJect.Class.WithParams<IVirtualCursor>(this);
        SwitchGroups = EJect.Class.WithParams<ISwitchGroup>(this);
        ReturnControlFromEditor = EJect.Class.WithParams<IReturnFromEditor>(this);
        EServ.Locator.AddNew((IInput)this);
        UseEServLocator();
        _inputScheme.SetCursor();
    }

    private void OnEnable()
    {
        FetchEvents();
        SetUpHotKeys();
        ChangeControl.OnEnable();
        MenuToGameSwitching.OnEnable();
        SwitchGroups.OnEnable();
        VirtualCursor.OnEnable();
        ObserveEvents();
    }

    public void UseEServLocator() => HistoryTracker = EServ.Locator.Get<IHistoryTrack>(this);

    private void SetUpHotKeys()
    {
        if (_hotKeySettings.Count == 0) return;
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake(_inputScheme);
            hotKey.OnEnable();
        }
    }
    
    public void FetchEvents()
    {
        OnPausedPressed = EVent.Do.Fetch<IPausePressed>();
        OnMenuAndGameSwitch = EVent.Do.Fetch<IMenuGameSwitchingPressed>();
        OnCancelPressed = EVent.Do.Fetch<ICancelPressed>();
        OnChangeControlPressed = EVent.Do.Fetch<IChangeControlsPressed>();
        OnClearAll = EVent.Do.Fetch<IClearAll>();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IGameIsPaused>(SaveGameIsPaused);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<IOnStart>(SaveOnStart);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Subscribe<IInMenu>(SaveInMenu);
        EVent.Do.Subscribe<INoPopUps>(SaveNoActivePopUps);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
    }

    private void Start()   
    {
        ChangeControl.OnStart();
        SwitchGroups.OnStart();
    }

    private void Update()
    {
        VirtualCursor.PreStartMovement();
        
        if (!_canStart) return;

        if(ReturnControlFromEditor.CanReturn(_inMenu, _activeBranch)) return;
        
        if (CancelWhenClickedOff.CanCancel(_inputScheme, _onHomeScreen, _allowKeys, VirtualCursor))
        {
            if (_activeBranch.CloseIfClickedOff == IsActive.Yes)
            {
                OnClearAll?.Invoke(this);
            }
            return;
        }

        if (CanPauseGame())
        {
            PausedPressedActions();
            return;
        }
        
        if (CanSwitchBetweenInGameAndMenu() && _onHomeScreen) return;
        
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
            if (SwitchGroups.GOUISwitchProcess() || SwitchGroups.SwitchGroupProcess()) return;
        }

        if(VirtualCursor.CanMoveVirtualCursor())
        {
            VirtualCursor.Update();
            return;
        }

        if(MultiSelectPressed) return;
        DoChangeControlPressed();
    }

    public void DoChangeControlPressed() => OnChangeControlPressed?.Invoke(this);

    private bool MultiSelectPressed => _inputScheme.MultiSelectPressed() && !_allowKeys;

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
        if (_gameIsPaused || !_noActivePopUps || MultiSelectPressed) return false;
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

    private bool CanUnpauseGame() => _gameIsPaused && _activeBranch.IsPauseMenuBranch();
    
    private void CancelPressed() => OnCancelPressed?.Invoke(this);

    private bool CanEnterPauseWithNothingSelected() =>
        (_noActivePopUps && !_gameIsPaused && HistoryTracker.NoHistory)
        && NothingSelectedAction;
}
