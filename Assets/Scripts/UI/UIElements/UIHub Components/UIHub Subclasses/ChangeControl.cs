using System;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>

public interface IChangeControl : IEZEventUser, IMonoEnable, IMonoStart { }

public class ChangeControl : IChangeControl, IAllowKeys, IEZEventDispatcher, IVCSetUpOnStart, 
                             IVcChangeControlSetUp, IServiceUser
{
    public ChangeControl(IInput input) => _startInGame = input.StartInGame();

    //Variables
    private ControlMethod _controlMethod;
    private readonly bool _startInGame;
    private InputScheme _inputScheme;
    private bool _fromSwitchPress;
    private IHistoryTrack _myHistoryTracker;
    private IDataHub _myDataHub;

    //Properties
    public bool CanAllowKeys { get; private set; }
    private bool UsingVirtualCursor => _inputScheme.CanUseVirtualCursor;
    public bool ShowCursorOnStart { get; private set; }
    private bool SceneStarted => _myDataHub.SceneStarted;

    //Events
    private Action<IAllowKeys> AllowKeys { get; set; }
    private Action<IVCSetUpOnStart> VcStartSetUp { get; set; }
    private Action<IVcChangeControlSetUp> SetVcUsage { get; set; }

    //Getters / Setters

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
    }
    
    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myHistoryTracker = EZService.Locator.Get<IHistoryTrack>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void FetchEvents()
    {
        AllowKeys = InputEvents.Do.Fetch<IAllowKeys>();
        VcStartSetUp = InputEvents.Do.Fetch<IVCSetUpOnStart>();
        SetVcUsage = InputEvents.Do.Fetch<IVcChangeControlSetUp>();
    }

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<IChangeControlsPressed>(ChangeControlType);
        InputEvents.Do.Subscribe<IChangeControlsSwitchPressed>(ChangeControlSwitch);
        HistoryEvents.Do.Subscribe<IOnStart>(StartGame);
    }

    public void UnObserveEvents() { }

    //Main
    public void OnStart()
    {
        _controlMethod = _inputScheme.ControlType;
        OnLevelSetUp();
    }

    private void OnLevelSetUp()
    {
        ShowCursorOnStart = (_inputScheme.ControlType == ControlMethod.MouseOnly
                           || _inputScheme.ControlType == ControlMethod.AllowBothStartWithMouse) 
                            || !_inputScheme.HideMouseCursor;

        ShowMouseCursorOnStart();
        VcStartSetUp?.Invoke(this);
    }

    private void ShowMouseCursorOnStart()
    {
        if (UsingVirtualCursor)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = ShowCursorOnStart;
        }
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
        
        SetUpVcCorrectly();
    }

    private bool MousePreferredControlMethod() 
        => _controlMethod == ControlMethod.MouseOnly || _controlMethod == ControlMethod.AllowBothStartWithMouse;

    private void SetUpMouse()
    {
        if (!_startInGame)
        {
            CanAllowKeys = true;
            ActivateMouseOrVirtualCursor();
        }
        else
        {
            CanAllowKeys = false;
            SetAllowKeys();
        }
    }

    private void SetUpKeysOrCtrl()
    {
        if (!_startInGame)
        {
            CanAllowKeys = false;
            ActivateKeysOrControl();
        }
        else
        {
            CanAllowKeys = true;
            SetAllowKeys();
        }
    }

    private void ChangeControlSwitch(IChangeControlsSwitchPressed args)
    {
        _fromSwitchPress = true;
        ChangeControlType(args);
    }

    private void ChangeControlType(IChangeControlsPressed args)
    {
        if (_inputScheme.CanSwitchToMouseOrVC(CanAllowKeys))
        {
            ActivateMouseOrVirtualCursor();
        }
        else if(_inputScheme.CanSwitchToKeysOrController(CanAllowKeys))
        {
            if (_inputScheme.AnyMouseClicked) return;
            ActivateKeysOrControl();
        }
        
        SetUpVcCorrectly();
    }
    
    private void SetUpVcCorrectly()
    {
        if(!UsingVirtualCursor) return;
        SetVcUsage?.Invoke(this);
    }

    private void ActivateMouseOrVirtualCursor()
    {
        ShowMouseCursor();
        
        if (!CanAllowKeys) return;
        CanAllowKeys = false;
        _fromSwitchPress = false;
        SetAllowKeys();
        if(!SceneStarted) return;
        _myDataHub.Highlighted.ThisNodeNotHighLighted();
    }

    private void ShowMouseCursor() => Cursor.visible = !UsingVirtualCursor;

    private void ActivateKeysOrControl()
    {
        if (_inputScheme.HideMouseCursor)
        {
            Cursor.visible = false;
        }
        
        if (CanAllowKeys) return;
        CanAllowKeys = true;
        SetAllowKeys();
        if(!SceneStarted) return;
        SetNextHighlightedForKeys();
    }

    private void SetAllowKeys()
    {
        _myDataHub.SetAllowKeys(CanAllowKeys);
        AllowKeys?.Invoke(this);
    }

    private void SetNextHighlightedForKeys()
    {
        if(!_fromSwitchPress)
        {
           _myHistoryTracker.MoveToLastBranchInHistory();
        }
        _fromSwitchPress = false;
    }
}
