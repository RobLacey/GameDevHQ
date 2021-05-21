using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UIInputEvents
{
    [Serializable]
    public class SwitchGameOrMenu : UnityEvent<InMenuOrGame> { }

    [Serializable] public class GameIsPaused : UnityEvent<bool> { }

    [InfoBox(InputEventsMessage, order = 1)]
    [SerializeField] 
    private SwitchGameOrMenu _switchBetweenGameAndMenu;
    [SerializeField] 
    private GameIsPaused _gameIsPaused;
    
    //Editor
    private const String InputEventsMessage = "Used to message other parts of your game. \n" +
                                              "For example, when the game is paused";

    //Main
    public void SwitchBetweenGameAndMenuPressed(InMenuOrGame args) => _switchBetweenGameAndMenu?.Invoke(args);
    public void GamePausedStatus(bool isPaused) => _gameIsPaused?.Invoke(isPaused);
}