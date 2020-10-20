using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class UIInput : MonoBehaviour, IEventUser
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

    //Events
    private static CustomEvent<IMenuGameSwitchingPressed> OnMenuAndGameSwitch { get; } 
        = new CustomEvent<IMenuGameSwitchingPressed>();
    private static CustomEvent<IPausePressed> OnPausePressed { get; } = new CustomEvent<IPausePressed>();
    private static CustomEvent<ICancelPressed> OnCancelPressed { get; } = new CustomEvent<ICancelPressed>();
    private static CustomEvent<IChangeControlsPressed> OnChangeControlPressed { get; } 
        = new CustomEvent<IChangeControlsPressed>();
    private static CustomEvent<ISwitchGroupPressed, SwitchType> OnSwitchGroupPressed { get; } 
        = new CustomEvent<ISwitchGroupPressed, SwitchType>();
    
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
    private void SaveInMenu(bool isInMenu)
    {
        _returnToGameControl?.Invoke(isInMenu);
        _inMenu = isInMenu;
    }
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SaveOnStart() => _canStart = true;
    private void SaveGameIsPaused(bool gameIsPaused) => _gameIsPaused = gameIsPaused;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private void SaveOnHomeScreen (bool onHomeScreen) => _onHomeScreen = onHomeScreen;

    public InputScheme ReturnScheme => _inputScheme;
    
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
    }
    
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
        EventLocator.SubscribeToEvent<IGameIsPaused, bool>(SaveGameIsPaused, this);
        EventLocator.SubscribeToEvent<IActiveBranch, UIBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<IOnStart>(SaveOnStart, this);
        EventLocator.SubscribeToEvent<IOnHomeScreen, bool>(SaveOnHomeScreen, this);
        EventLocator.SubscribeToEvent<IInMenu, bool>(SaveInMenu, this);
        EventLocator.SubscribeToEvent<INoPopUps, bool>(SaveNoActivePopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(SaveGameIsPaused);
        EventLocator.UnsubscribeFromEvent<IActiveBranch, UIBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<IOnStart>(SaveOnStart);
        EventLocator.UnsubscribeFromEvent<IOnHomeScreen, bool>(SaveOnHomeScreen);
        EventLocator.UnsubscribeFromEvent<IInMenu, bool>(SaveInMenu);
        EventLocator.UnsubscribeFromEvent<INoPopUps, bool>(SaveNoActivePopUps);
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

        OnChangeControlPressed?.RaiseEvent();
    }

    private bool CanPauseGame() => _inputScheme.PressPause();

    private void PausedPressedActions()
    {
        OnPausePressed?.RaiseEvent();
    }

    private bool CanSwitchBetweenInGameAndMenu()
    {
        if (!_inputScheme.PressedMenuToGameSwitch()) return false;
        OnMenuAndGameSwitch?.RaiseEvent();
        return true;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (!_hotKeySettings.Any(hotKey => hotKey.CheckHotKeys())) return false;
        if(!_inMenu)
            OnMenuAndGameSwitch?.RaiseEvent();
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
             OnCancelPressed?.RaiseEvent();
         }
    }

    private bool CanUnpauseGame() => _gameIsPaused && _activeBranch.IsPauseMenuBranch();

    private bool CanEnterPauseWithNothingSelected() =>
        (_noActivePopUps && _activeBranch == _activeBranch.MyParentBranch)
        && _inputScheme.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;

    private bool CanSwitchBranches() => _noActivePopUps && !MouseOnly();

    private bool SwitchGroupProcess()
    {
        if (_inputScheme.PressedPositiveSwitch())
        {
            OnSwitchGroupPressed?.RaiseEvent(SwitchType.Positive);
            return true;
        }

        if (_inputScheme.PressedNegativeSwitch())
        {
            OnSwitchGroupPressed?.RaiseEvent(SwitchType.Negative);
            return true;
        }
        return false;
    }
}
