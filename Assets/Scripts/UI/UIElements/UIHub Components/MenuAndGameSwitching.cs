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
    private bool _noResolvePopUps = true;
    private bool _noNonResolvePoUps = true;
    
    //Events
    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }
    public static event Action<bool> IsInTheMenu; // Subscribe To track if in game

    //Properties
    public bool ActiveInGameSystem => _inGameMenuSystem == InGameSystem.On;
    public bool StartInGame => _startGameWhere == StartInMenu.InGameControl;
    public bool CanStartInGame => ActiveInGameSystem && StartInGame;
    public bool InTheMenu { get; private set; } = true;
    public string SwitchControls => _switchToMenusButton;
    private UINode LastHighlighted { get; set; }
    private void SaveLastHighlighted(UINode newNode) => LastHighlighted = newNode;
    private void SaveOnHomeScreen (bool onHomeScreen) => _onHomeScreen = onHomeScreen;
    private bool NoPopUps => _noResolvePopUps && _noNonResolvePoUps;
    private void SaveNoResolvePopUps(bool newPopUp)
    {
        _noResolvePopUps = newPopUp;
        PopUpEventHandler();
    }
    private void SaveNoOptionalPopUps(bool newPopUp)
    {
        _noNonResolvePoUps = newPopUp;
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
        _uiDataEvents.SubscribeToHighlightedNode(SaveLastHighlighted);
        _uiControlsEvents.SubscribeToGameIsPaused(WhenTheGameIsPaused);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SaveNoResolvePopUps);
        _uiPopUpEvents.SubscribeNoOptionalPopUps(SaveNoOptionalPopUps);
        _uiControlsEvents.SubscribeFromHotKey(SwitchBetweenGameAndMenu);
    }
    
    private void PopUpEventHandler()
    {
        if (!NoPopUps && !InTheMenu)
            SwitchBetweenGameAndMenu();
        
        if (NoPopUps)
            SwitchBetweenGameAndMenu();
    }

    private void StartUp()
    {
        InTheMenu = true;
        if (ActiveInGameSystem && StartInGame)
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
        if (!ActiveInGameSystem || !_onHomeScreen) return;
    
        if (InTheMenu)
        {
            SwitchToGame();
        }
        else
        {
            SwitchToMenu();
        }
        BroadcastState();
    }


    private void SwitchToGame()
    {
        InTheMenu = false;
        LastHighlighted.SetNotHighlighted();
        UIHub.SetEventSystem(null);
    }

    private void SwitchToMenu()
    {
        InTheMenu = true;
        LastHighlighted.SetAsHighlighted();
        UIHub.SetEventSystem(LastHighlighted.gameObject);
    }

    private void BroadcastState()
    {
        _returnToGameControl.Invoke(InTheMenu);
        IsInTheMenu?.Invoke(InTheMenu);
    }

    private void WhenTheGameIsPaused(bool isPaused)
    {
        if (InTheMenu && isPaused) return;
        if(!NoPopUps) return;
        
        if (ActiveInGameSystem)
        {
            SwitchBetweenGameAndMenu();
        }
    }
    
    public void TurnOffGameSwitchSystem() => _inGameMenuSystem = InGameSystem.Off;
    
    public bool HasSwitchControls() => _switchToMenusButton != string.Empty;

}
