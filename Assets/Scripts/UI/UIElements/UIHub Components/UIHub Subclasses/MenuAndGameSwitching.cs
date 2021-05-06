
using System;
using UIElements;

public interface IMenuAndGameSwitching : IEventUser, IMonoEnable, IMonoStart { }

public class MenuAndGameSwitching : IMenuAndGameSwitching, IInMenu, IEventDispatcher, IEServUser
{
    //Variables
    private bool _noPopUps = true;
    private bool _wasInGame;
    private InputScheme _inputScheme;

    //Events
    private Action<IInMenu> IsInMenu { get; set; }

    //Properties
    public bool InTheMenu { get; set; } = true;
    private InMenuOrGame StartWhere { get; set; }

    private void SaveNoPopUps(INoPopUps args)
    {
        _noPopUps = args.NoActivePopUps;
        if (!InTheMenu && !_noPopUps) _wasInGame = true;
         PopUpEventHandler();
    }

    public void OnEnable()
    {
        UseEServLocator();
        FetchEvents();
        ObserveEvents();
    }
    
    public void UseEServLocator() => _inputScheme = EServ.Locator.Get<InputScheme>(this);

    public void FetchEvents() => IsInMenu = EVent.Do.Fetch<IInMenu>();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IMenuGameSwitchingPressed>(CheckForActivation);
        EVent.Do.Subscribe<IGameIsPaused>(WhenTheGameIsPaused);
        EVent.Do.Subscribe<IOnStart>(StartUp);
        EVent.Do.Subscribe<INoPopUps>(SaveNoPopUps);
    }
    
    public void OnStart()
    {
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
            StartWhere = _inputScheme.WhereToStartGame;
    }

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
        if (StartWhere == InMenuOrGame.InGameControl)
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

    private void BroadcastState() => IsInMenu?.Invoke(this);

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
