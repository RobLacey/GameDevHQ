
public class MenuAndGameSwitching : IEventUser, IInMenu, IServiceUser
{
    public MenuAndGameSwitching() => OnAwake();

    //Variables
    private bool _noPopUps = true;
    private bool _wasInGame;
    private IHistoryTrack _historyTracker;

    //Events
    private static CustomEvent<IInMenu> IsInMenu { get; } = new CustomEvent<IInMenu>();

    //Properties
    public bool InTheMenu { get; set; } = true;
    public StartInMenu StartWhere { get; set; }

    private void SaveNoPopUps(INoPopUps args)
    {
        _noPopUps = args.NoActivePopUps;
        if (!InTheMenu && !_noPopUps) _wasInGame = true;
         PopUpEventHandler();
    }

    private void OnAwake()
    {
        ObserveEvents();
        SubscribeToService();
    }
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IMenuGameSwitchingPressed>(CheckForActivation, this);
        EventLocator.Subscribe<IGameIsPaused>(WhenTheGameIsPaused, this);
        EventLocator.Subscribe<IOnStart>(StartUp, this);
        EventLocator.Subscribe<INoPopUps>(SaveNoPopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IMenuGameSwitchingPressed>(CheckForActivation);
        EventLocator.Unsubscribe<IGameIsPaused>(WhenTheGameIsPaused);
        EventLocator.Unsubscribe<IOnStart>(StartUp);
        EventLocator.Unsubscribe<INoPopUps>(SaveNoPopUps);
    }
    
    public void SubscribeToService() => _historyTracker =  ServiceLocator.GetNewService<IHistoryTrack>(this);

    private void CheckForActivation(IMenuGameSwitchingPressed arg)
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

    private void StartUp(IOnStart onStart)
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
        _historyTracker.BackToHome();
        BroadcastState();
    }

    private void SwitchToMenu()
    {
        InTheMenu = true;
        BroadcastState();
    }

    private void BroadcastState() => IsInMenu?.RaiseEvent(this);

    private void WhenTheGameIsPaused(IGameIsPaused args)
    {
        if (args.GameIsPaused && !InTheMenu)
        {
            SwitchBetweenGameAndMenu();
            _wasInGame = true;
            return;
        }

        if (!args.GameIsPaused && _wasInGame)
        {
            SwitchBetweenGameAndMenu();
            _wasInGame = false;
        }
    }
}
