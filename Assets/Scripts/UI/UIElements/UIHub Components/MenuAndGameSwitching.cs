using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[Serializable]
public class MenuAndGameSwitching : IMono
{
    [Header("In-Game Menu Settings")]
    [SerializeField] InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [Label("Switch To/From Game Menus")] [InputAxis] string _switchToMenusButton;
    [SerializeField] InGameOrInMenu _returnToGameControl;

    //Variables
    private UIData _uiData;
    
    //Events
    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }
    public static event Action<bool> IsInTheMenu; // Subscribe To track if in game

    //Properties
    public bool ActiveInGameSystem => _inGameMenuSystem == InGameSystem.On;
    public bool StartInGame => _startGameWhere == StartInMenu.InGameControl;
    public bool CanStartInGame => ActiveInGameSystem && StartInGame;
    public bool InMenu { get; private set; } = true;
    public string SwitchControls => _switchToMenusButton;
    private UINode LastHighlighted { get; set; }
    private void SaveLastHighlighted(UINode newNode) => LastHighlighted = newNode;

    private void GameIsPaused(bool isPaused)
    {
        if (ActiveInGameSystem)
        {
            SwitchBetweenGameAndMenu();
        }
    }

    //Enums
    private enum InGameSystem { On, Off }

    public void OnAwake()
    {
        _uiData = new UIData();
        OnEnable();
    }
    
    public void OnEnable()
    {
        UIHub.OnStart += StartUp;
        _uiData.NewHighLightedNode = SaveLastHighlighted;
        _uiData.IsGamePaused = GameIsPaused;
    }

    public void OnDisable()
    {
        UIHub.OnStart -= StartUp;
    }

    private void StartUp()
    {
        InMenu = true;
        if (ActiveInGameSystem && StartInGame)
        {
            SwitchBetweenGameAndMenu();
        }
        else
        {
            _returnToGameControl.Invoke(InMenu);
            IsInTheMenu?.Invoke(InMenu);
        }
    }


    public void TurnOffGameSwitchSystem() => _inGameMenuSystem = InGameSystem.Off;
    public bool HasSwitchControls() => _switchToMenusButton != string.Empty;

    /// <summary>
    /// Create a method that switches control when the below events are triggered
    /// </summary>
    private void EventTriggersSwitch()
    {
        //New PopUp
        //HotKey - Remove from UI hub
    }
    
    public void SwitchBetweenGameAndMenu()
    {
        if (!ActiveInGameSystem) return;
    
        if (InMenu)
        {
            SwitchToGame();
        }
        else
        {
            SwitchToMenu();
        }
        _returnToGameControl.Invoke(InMenu);
        IsInTheMenu?.Invoke(InMenu);
    }
    
    private void SwitchToGame()
    {
        InMenu = false;
        LastHighlighted.SetNotHighlighted();
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    private void SwitchToMenu()
    {
        InMenu = true;
        LastHighlighted.SetAsHighlighted();
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }
}
