using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class UIInput : MonoBehaviour, IEventUser, IPausePressed, ISwitchGroupPressed, 
                       ICancelPressed, IChangeControlsPressed, IMenuGameSwitchingPressed, IServiceUser
{
    [SerializeField] 
    private InputScheme _inputScheme;
    [SerializeField] 
    private InGameOrInMenu _returnToGameControl;
    [SerializeField] 
    [ReorderableList] private List<HotKeys> _hotKeySettings;

    //Variables
    private bool _canStart, _inMenu, _gameIsPaused;
    private bool _noActivePopUps = true;
    private bool _onHomeScreen = true;
    private UINode _lastHomeScreenNode;
    private UIBranch _activeBranch;
    private MenuAndGameSwitching _menuToGameSwitching;
    private ChangeControl _changeControl;
    private IHistoryTrack _historyTrack;

    //Events
    private static CustomEvent<IMenuGameSwitchingPressed> OnMenuAndGameSwitch { get; } 
        = new CustomEvent<IMenuGameSwitchingPressed>();
    private static CustomEvent<IPausePressed> OnPausePressed { get; } = new CustomEvent<IPausePressed>();
    private static CustomEvent<ICancelPressed> OnCancelPressed { get; } = new CustomEvent<ICancelPressed>();
    private static CustomEvent<IChangeControlsPressed> OnChangeControlPressed { get; } 
        = new CustomEvent<IChangeControlsPressed>();
    private static CustomEvent<ISwitchGroupPressed> OnSwitchGroupPressed { get; } 
        = new CustomEvent<ISwitchGroupPressed>();
    
    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }

    //Properties
    private bool MouseOnly()
    {
        if(_inputScheme.ControlType == ControlMethod.MouseOnly) _inputScheme.TurnOffInGameMenuSystem();
         return _inputScheme.ControlType == ControlMethod.MouseOnly;
    }

    public bool StartInGame()
    {
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
        {
            return _inputScheme.WhereToStartGame == StartInMenu.InGameControl;
        }
        return false;
    }
    private void SaveInMenu(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        _returnToGameControl?.Invoke(_inMenu);
    }
    private void SaveNoActivePopUps(INoPopUps args) => _noActivePopUps = args.NoActivePopUps;
    private void SaveOnStart(IOnStart onStart) => _canStart = true;
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void SaveOnHomeScreen (IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    public SwitchType SwitchType { get; set; }
    public InputScheme ReturnScheme => _inputScheme;
    public EscapeKey EscapeKeySettings => _activeBranch.EscapeKeySetting;
    private bool NothingSelectedAction => _inputScheme.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;

    
    //Main
    private void Awake()
    {
        _inputScheme.OnAwake();
        _changeControl = new ChangeControl(_inputScheme, StartInGame());
        _menuToGameSwitching = new MenuAndGameSwitching();
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
            _menuToGameSwitching.StartWhere = _inputScheme.WhereToStartGame;
        SetUpHotKeys();
        ObserveEvents();
        SubscribeToService();
    }
    
    public void SubscribeToService() => _historyTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);


    private void SetUpHotKeys()
    {
        if (_hotKeySettings.Count == 0) return;
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake(_inputScheme);
        }
    }
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IGameIsPaused>(SaveGameIsPaused, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.Subscribe<IOnStart>(SaveOnStart, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomeScreen, this);
        EventLocator.Subscribe<IInMenu>(SaveInMenu, this);
        EventLocator.Subscribe<INoPopUps>(SaveNoActivePopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IGameIsPaused>(SaveGameIsPaused);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.Unsubscribe<IOnStart>(SaveOnStart);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EventLocator.Unsubscribe<IInMenu>(SaveInMenu);
        EventLocator.Unsubscribe<INoPopUps>(SaveNoActivePopUps);
    }
    
    private void OnDisable()
    {
        RemoveFromEvents();
        _changeControl.RemoveFromEvents();
        _menuToGameSwitching.RemoveFromEvents();
        foreach (HotKeys hotKeys in _hotKeySettings)
        {
            hotKeys.RemoveFromEvents();
        }
    }

    private void Update()
    {
        if (!_canStart) return;
        
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
        
        if (CanSwitchBranches() && SwitchGroupProcess()) return;

        OnChangeControlPressed?.RaiseEvent(this);
    }

    private bool CanPauseGame() => _inputScheme.PressPause();

    private void PausedPressedActions()
    {
        OnPausePressed?.RaiseEvent(this);
    }

    private bool CanSwitchBetweenInGameAndMenu()
    {
        if (!_inputScheme.PressedMenuToGameSwitch()) return false;
        OnMenuAndGameSwitch?.RaiseEvent(this);
        return true;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (_gameIsPaused || !_noActivePopUps) return false;
        if (!_hotKeySettings.Any(hotKey => hotKey.CheckHotKeys())) return false;
        if(!_inMenu)
            OnMenuAndGameSwitch?.RaiseEvent(this);
        return true;
    }

    private bool CanDoCancel() => _inputScheme.PressedCancel();

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
    
    private void CancelPressed() => OnCancelPressed?.RaiseEvent(this);
    
    private bool CanEnterPauseWithNothingSelected() =>
        (_noActivePopUps && !_gameIsPaused && _historyTrack.NoHistory) && NothingSelectedAction;
    
    private bool CanSwitchBranches() => _noActivePopUps && !MouseOnly();

    private bool SwitchGroupProcess()
    {
        if (_inputScheme.PressedPositiveSwitch())
        {
            SwitchType = SwitchType.Positive;
            OnSwitchGroupPressed?.RaiseEvent(this);
            return true;
        }

        if (_inputScheme.PressedNegativeSwitch())
        {
            SwitchType = SwitchType.Negative;
            OnSwitchGroupPressed?.RaiseEvent(this);
            return true;
        }
        return false;
    }

}
