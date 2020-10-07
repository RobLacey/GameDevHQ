﻿using System;
using UnityEngine;

public class MenuAndGameSwitching
{
    public MenuAndGameSwitching() => OnAwake();

    //Variables
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private UIPopUpEvents _uiPopUpEvents;
    // private bool _onHomeScreen = true;
    private bool _noPopUps = true;
    private bool _wasInGame;

    //Events
    public static event Action<bool> IsInTheMenu; // Subscribe To track if in game

    //Properties
    private bool InTheMenu { get; set; } = true;
    // private void SaveOnHomeScreen (bool onHomeScreen) => _onHomeScreen = onHomeScreen;
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
        _uiControlsEvents = new UIControlsEvents();
        _uiPopUpEvents = new UIPopUpEvents();
        OnEnable();
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToOnStart(StartUp);
        _uiDataEvents.SubscribeToGameIsPaused(WhenTheGameIsPaused);
        //_uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
        _uiControlsEvents.SubscribeMenuGameSwitching(CheckForActivation);
    }
    
    private bool CheckForActivation()
    {
        if (/*!_onHomeScreen || */!_noPopUps) return false;
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
