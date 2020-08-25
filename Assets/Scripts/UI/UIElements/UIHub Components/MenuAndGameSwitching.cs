using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class MenuAndGameSwitching
{
    [Header("In-Game Menu Settings")]
    [SerializeField] InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [Label("Switch To/From Game Menus")] [InputAxis] string _switchToMenusButton;
    [SerializeField] InGameOrInMenu _returnToGameControl;

    //Variables
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private UIPopUpEvents _uiPopUpEvents;
    private bool _onHomeScreen = true;
    private bool _noPopUps = true;
    private bool _wasInGame;

    //Events
    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }
    public static event Action<bool> IsInTheMenu; // Subscribe To track if in game

    //Properties
    public bool ActiveInGameSystem => _inGameMenuSystem == InGameSystem.On;
    public bool StartInGame => _startGameWhere == StartInMenu.InGameControl && ActiveInGameSystem;
    public bool InTheMenu { get; private set; } = true;
    public void TurnOffGameSwitchSystem() => _inGameMenuSystem = InGameSystem.Off;
    private bool HasSwitchControls() => _switchToMenusButton != string.Empty;
    private void SaveOnHomeScreen (bool onHomeScreen) => _onHomeScreen = onHomeScreen;

    private void SaveNoPopUps(bool noActivePopUps)
    {
        if(!ActiveInGameSystem) return;
        _noPopUps = noActivePopUps;
        if (!InTheMenu && !noActivePopUps) _wasInGame = true;
         PopUpEventHandler();
    }
    
    //Enums
    private enum InGameSystem { On, Off }

    public void OnAwake()
    {
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        _uiPopUpEvents = new UIPopUpEvents();
        OnEnable();
    }
    
    public void OnEnable()
    {
        _uiDataEvents.SubscribeToOnStart(StartUp);
        _uiDataEvents.SubscribeToGameIsPaused(WhenTheGameIsPaused);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
        _uiControlsEvents.SubscribeFromHotKey(HotKeyActivated);
        _uiControlsEvents.SubscribeMenuGameSwitching(CheckForActivation);
    }
    
    private void HotKeyActivated()
    {
        if (!ActiveInGameSystem || !_onHomeScreen || InTheMenu) return;
        SwitchBetweenGameAndMenu();
    }

    private bool CheckForActivation()
    {
        if (!HasSwitchControls()) return false;
        if(!Input.GetButtonDown(_switchToMenusButton)) return false;
        if (!ActiveInGameSystem || !_onHomeScreen || !_noPopUps) return false;
        SwitchBetweenGameAndMenu();
        return true;
    }
    
    private void PopUpEventHandler()
    {
        if (!_noPopUps && !InTheMenu)
        {
            SwitchBetweenGameAndMenu();
        }
        
        if (_noPopUps && _wasInGame)
        {
            _wasInGame = false;
            SwitchBetweenGameAndMenu();
        }
    }

    private void StartUp()
    {
        InTheMenu = true;
        if (ActiveInGameSystem && _startGameWhere == StartInMenu.InGameControl)
        {
            SwitchBetweenGameAndMenu();
        }
        else
        {
            BroadcastState();
        }
    }

    public void SwitchBetweenGameAndMenu()
    {
        if (InTheMenu)
        {
            SwitchToGame();
        }
        else
        {
            SwitchToMenu();
        }
    }


    private void SwitchToGame()
    {
        InTheMenu = false;
        BroadcastState();
    }

    private void SwitchToMenu()
    {
        InTheMenu = true;
        BroadcastState();
    }

    private void BroadcastState()
    {
        _returnToGameControl.Invoke(InTheMenu);
        IsInTheMenu?.Invoke(InTheMenu);
    }

    private void WhenTheGameIsPaused(bool isPaused)
    {
        if (InTheMenu && isPaused) return;
        if(!_noPopUps) return;
        
        if (ActiveInGameSystem)
        {
            SwitchBetweenGameAndMenu();
        }
    }
}
