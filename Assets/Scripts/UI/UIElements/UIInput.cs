using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class UIInput : MonoBehaviour
{
    [SerializeField] private InputScheme _inputScheme;
    [SerializeField] private InGameOrInMenu _returnToGameControl;
    [SerializeField] 
    [ReorderableList] private List<HotKeys> _hotKeySettings;


    //Variables
    private bool _canStart, _inMenu, _gameIsPaused;
    private bool _noActivePopUps = true;
    private bool _onHomeScreen = true;
    private UINode _lastHomeScreenNode;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private UIBranch _activeBranch;
    private MenuAndGameSwitching _menuToGameSwitching;

    //Events
    public static event Action OnChangeControls, OnCancelPressed, OnPausedPressed;
    public static event Action<SwitchType> OnSwitchGroupsPressed;
    public static event Func<bool> OnGameToMenuSwitchPressed;
    
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
        var unused4 = new ChangeControl(_inputScheme, StartInGame());
        _menuToGameSwitching = new MenuAndGameSwitching();
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
            _menuToGameSwitching.StartWhere = _inputScheme.WhereToStartGame;
        SetUpHotKeys();
    }
    
    private void SetUpHotKeys()
    {
        if (_hotKeySettings.Count == 0) return;
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake(_inputScheme);
        }
    }


    private void OnEnable()
    {
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToOnStart(SaveOnStart);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToGameIsPaused(SaveGameIsPaused);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);

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

        OnChangeControls?.Invoke();
    }

    private bool CanPauseGame() => _inputScheme.PressPause();

    private static void PausedPressedActions() => OnPausedPressed?.Invoke();

    private bool CanSwitchBetweenInGameAndMenu()
    {
        if (!_inputScheme.PressedMenuToGameSwitch()) return false;
        OnGameToMenuSwitchPressed?.Invoke(); //Might need to move
        return true;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (!_hotKeySettings.Any(hotKey => hotKey.CheckHotKeys())) return false;
        if(!_inMenu)
            OnGameToMenuSwitchPressed?.Invoke();
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
            OnCancelPressed?.Invoke();
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
            OnSwitchGroupsPressed?.Invoke(SwitchType.Positive);
            return true;
        }

        if (_inputScheme.PressedNegativeSwitch())
        {
            OnSwitchGroupsPressed?.Invoke(SwitchType.Negative);
            return true;
        }
        return false;
    }
}
