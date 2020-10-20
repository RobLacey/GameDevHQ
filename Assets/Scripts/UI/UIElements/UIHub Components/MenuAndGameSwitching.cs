
public class MenuAndGameSwitching : IEventUser
{
    public MenuAndGameSwitching() => OnAwake();

    //Variables
    private bool _noPopUps = true;
    private bool _wasInGame;

    //Events
    private static CustomEvent<IInMenu, bool> IsInMenu { get; } = new CustomEvent<IInMenu, bool>();

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
        ObserveEvents();
    }
    
    public void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<IMenuGameSwitchingPressed>(CheckForActivation, this);
        EventLocator.SubscribeToEvent<IGameIsPaused, bool>(WhenTheGameIsPaused, this);
        EventLocator.SubscribeToEvent<IOnStart>(StartUp, this);
        EventLocator.SubscribeToEvent<INoPopUps, bool>(SaveNoPopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IMenuGameSwitchingPressed>(CheckForActivation);
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(WhenTheGameIsPaused);
        EventLocator.UnsubscribeFromEvent<IOnStart>(StartUp);
        EventLocator.UnsubscribeFromEvent<INoPopUps, bool>(SaveNoPopUps);
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

    private void SwitchBetweenGameAndMenu()
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

    private void BroadcastState() => IsInMenu?.RaiseEvent(InTheMenu);

    private void WhenTheGameIsPaused(bool isPaused)
    {
        if (isPaused && !InTheMenu)
        {
            SwitchBetweenGameAndMenu();
            _wasInGame = true;
            return;
        }

        if (!isPaused && _wasInGame)
        {
            SwitchBetweenGameAndMenu();
            _wasInGame = false;
        }
    }
}
