using System;

public class MenuAndGameSwitching : IEventUser
{
    public MenuAndGameSwitching() => OnAwake();

    //Variables
    private UIDataEvents _uiDataEvents;
    private UIPopUpEvents _uiPopUpEvents;
    private bool _noPopUps = true;
    private bool _wasInGame;

    //Events
    public static event Action<bool> IsInTheMenu; // Subscribe To track if in game

    //Properties
    private bool InTheMenu { get; set; } = true;
    public StartInMenu StartWhere { get; set; }

    private void SaveNoPopUps(bool noActivePopUps)
    {
        _noPopUps = noActivePopUps;
        if (!InTheMenu && !noActivePopUps) _wasInGame = true;
         PopUpEventHandler();
    }

    private void OnAwake()
    {
        _uiDataEvents = new UIDataEvents();
        _uiPopUpEvents = new UIPopUpEvents();
        OnEnable();
        ObserveEvents();
    }
    
    public void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<IMenuGameSwitchingPressed>(CheckForActivation, this);
        EventLocator.SubscribeToEvent<IGameIsPaused, bool>(WhenTheGameIsPaused, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IMenuGameSwitchingPressed>(CheckForActivation);
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(WhenTheGameIsPaused);
    }


    private void OnEnable()
    {
        _uiDataEvents.SubscribeToOnStart(StartUp);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
    }
    
    private void CheckForActivation()
    {
        if (!_noPopUps) return;
        SwitchBetweenGameAndMenu();
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
        if (StartWhere == StartInMenu.InGameControl)
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

    private void BroadcastState() => IsInTheMenu?.Invoke(InTheMenu);

    private void WhenTheGameIsPaused(bool isPaused)
    {
        if (InTheMenu && isPaused) return;
        if(!_noPopUps || InTheMenu) return;
        SwitchBetweenGameAndMenu();
    }
}
