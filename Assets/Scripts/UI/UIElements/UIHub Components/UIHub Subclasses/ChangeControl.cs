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
    public bool UsingVirtualCursor => _inputScheme.CanUseVirtualCursor == VirtualControl.Yes;


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
            ActivateMouseOrVirtualCursor();
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
        if (_inputScheme.CanSwitchToMouseOrVC && !_usingMouse)
        {
            Debug.Log("Mouse");
            ActivateMouseOrVirtualCursor();
        }
        else if(_inputScheme.CanSwitchToKeysOrController && _usingMouse)
        {
            Debug.Log("Keys");

            if (_inputScheme.AnyMouseClicked) return;
            ActivateKeysOrControl();
        }
        else if(_inputScheme.PressedNegativeGOUISwitch() || _inputScheme.PressedPositiveGOUISwitch())
        {
            ActivateKeysOrControl();
        }
    }

    private void ActivateMouseOrVirtualCursor()
    {
        if (UsingVirtualCursor)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
        
        if (_usingMouse) return;
        _usingMouse = true;
        CanAllowKeys = false;
        SetAllowKeys();
    }
    
    private void ActivateKeysOrControl()
    {
        if (_inputScheme.HideMouseCursor)
        {
            Cursor.visible = false;
        }
        
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
