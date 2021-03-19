using System;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>

public interface IChangeControl : IEventUser
{
    void OnEnable();
}

public class ChangeControl : IChangeControl, IAllowKeys, IEServUser, IEventDispatcher
{
    public ChangeControl(IInput input)
    {
        _inputScheme = input.ReturnScheme;
        _controlMethod = _inputScheme.ControlType;
        _startInGame = input.StartInGame();
        UseEServLocator();
    }

    //Variables
    private readonly ControlMethod _controlMethod;
    private readonly bool _startInGame;
    private bool _usingMouse, _sceneStarted;
    private readonly InputScheme _inputScheme;
    private IHistoryTrack _historyTracker;

    //Properties
    public bool CanAllowKeys { get; private set; }

    //Events
    private Action<IAllowKeys> AllowKeys { get; set; }

    public void UseEServLocator() => _historyTracker = EServ.Locator.Get<IHistoryTrack>(this);

    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }
    
    public void FetchEvents() => AllowKeys = EVent.Do.Fetch<IAllowKeys>();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IChangeControlsPressed>(ChangeControlType);
        EVent.Do.Subscribe<IOnStart>(StartGame);
    }

    private void StartGame(IOnStart onStart)
    {
        _inputScheme.SetMousePosition();
        if (MousePreferredControlMethod())
        {
            SetUpMouse();
        }
        else
        {
            SetUpKeysOrCtrl();
        }

        _sceneStarted = true;
    }

    private bool MousePreferredControlMethod() 
        => _controlMethod == ControlMethod.MouseOnly || _controlMethod == ControlMethod.AllowBothStartWithMouse;

    private void SetUpMouse()
    {
        if (!_startInGame)
        {
            ActivateMouse();
        }
        else
        {
            _usingMouse = true;
            SetAllowKeys();
        }
    }

    private void SetUpKeysOrCtrl()
    {
        if (!_startInGame)
        {
            ActivateKeysOrControl();
        }
        else
        {
            CanAllowKeys = true;
            SetAllowKeys();
        }
    }

    private void ChangeControlType(IChangeControlsPressed args)
    {
        if (_inputScheme.CanSwitchToMouse)
        {
            ActivateMouse();
        }
        else if(_inputScheme.CanSwitchToKeysOrController)
        {
            if (_inputScheme.MouseClicked) return;
            ActivateKeysOrControl();
        }
        else if(_inputScheme.PressedNegativeGOUISwitch() || _inputScheme.PressedPositiveGOUISwitch())
        {
            ActivateKeysOrControl();
        }
    }

    private void ActivateMouse()
    { 
        _inputScheme.SetMousePosition();
        Cursor.visible = true;
        if (_usingMouse) return;
        _usingMouse = true;
        CanAllowKeys = false;
        SetAllowKeys();
    }

    private void ActivateKeysOrControl()
    {
        Cursor.visible = false;
        if (CanAllowKeys) return;
        _usingMouse = false;
        CanAllowKeys = true;
        SetAllowKeys();
        if(!_sceneStarted) return;
        SetNextHighlightedForKeys();
    }

    private void SetAllowKeys() => AllowKeys?.Invoke(this);

    private void SetNextHighlightedForKeys() => _historyTracker.MoveToLastBranchInHistory();
}
