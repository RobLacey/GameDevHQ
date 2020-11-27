using System;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>

public interface IChangeControl : IEventUser { }

public class ChangeControl : IChangeControl, IAllowKeys, IServiceUser
{
    public ChangeControl(IInput input)
    {
        _inputScheme = input.ReturnScheme;
        _controlMethod = _inputScheme.ControlType;
        _startInGame = input.StartInGame();
        SubscribeToService();
        ObserveEvents();
    }

    //Variables
    private readonly ControlMethod _controlMethod;
    private readonly bool _startInGame;
    private bool _usingMouse;
    private INode _lastHighlighted;
    private readonly InputScheme _inputScheme;
    private IHistoryTrack _historyTracker;

    //Properties
    private void SaveHighlighted(IHighlightedNode args) => _lastHighlighted = args.Highlighted;
    public bool CanAllowKeys { get; private set; }

    //Events
    private Action<IAllowKeys> AllowKeys { get; } = EVent.Do.FetchEVent<IAllowKeys>();
    
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IChangeControlsPressed>(ChangeControlType);
        EVent.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Subscribe<IOnStart>(StartGame);
    }

    public void RemoveFromEvents()
    {
        EVent.Do.Unsubscribe<IChangeControlsPressed>(ChangeControlType);
        EVent.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Unsubscribe<IOnStart>(StartGame);
    }
    
    public void SubscribeToService() => _historyTracker = ServiceLocator.Get<IHistoryTrack>(this);


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
        SetNextHighlightedForKeys();
        UIHub.SetEventSystem(_lastHighlighted.ReturnGameObject);
    }

    private void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.MouseOnly) return;
        AllowKeys?.Invoke(this);
   }

    private void SetNextHighlightedForKeys() => _historyTracker.MoveToLastBranchInHistory();
}
