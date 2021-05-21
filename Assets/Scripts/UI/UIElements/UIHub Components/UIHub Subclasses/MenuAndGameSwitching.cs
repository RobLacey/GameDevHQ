
using System;
using EZ.Events;
using EZ.Service;
using UIElements;

public interface IMenuAndGameSwitching : IEZEventUser, IMonoEnable, IMonoStart { }

public class MenuAndGameSwitching : IMenuAndGameSwitching, IInMenu, IEZEventDispatcher, IServiceUser
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
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
    }
    
    public void UseEZServiceLocator() => _inputScheme = EZService.Locator.Get<InputScheme>(this);

    public void FetchEvents() => IsInMenu = HistoryEvents.Do.Fetch<IInMenu>();

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<IMenuGameSwitchingPressed>(CheckForActivation);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(WhenTheGameIsPaused);
        HistoryEvents.Do.Subscribe<IOnStart>(StartUp);
        PopUpEvents.Do.Subscribe<INoPopUps>(SaveNoPopUps);
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
