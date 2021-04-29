using System;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>

public interface IChangeControl : IEventUser
{
    void OnEnable();
    void OnStart();
}

public class ChangeControl : IChangeControl, IAllowKeys, IEventDispatcher, IVCSetUpOnStart, IVcChangeControlSetUp, IActivateBranchOnControlsChange
{
    public ChangeControl(IInput input)
    {
        _inputScheme = input.ReturnScheme;
        _controlMethod = _inputScheme.ControlType;
        _startInGame = input.StartInGame();
    }

    //Variables
    private readonly ControlMethod _controlMethod;
    private readonly bool _startInGame;
    private bool _sceneStarted;
    private readonly InputScheme _inputScheme;
    private bool _switchJustPressed;
    private IBranch _activeBranch;

    //Properties
    public bool CanAllowKeys { get; private set; }
    private bool UsingVirtualCursor => _inputScheme.CanUseVirtualCursor;
    public bool ShowCursorOnStart { get; private set; }
    public IBranch ActiveBranch => _activeBranch;

    //Events
    private Action<IAllowKeys> AllowKeys { get; set; }
    private Action<IVCSetUpOnStart> VCStartSetUp { get; set; }
    private Action<IVcChangeControlSetUp> SetVCUsage { get; set; }
    private Action<IActivateBranchOnControlsChange> ActivateBranchOnControlsChanged { get; set; }

    //Getters / Setters
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;

    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }
    
    public void FetchEvents()
    {
        AllowKeys = EVent.Do.Fetch<IAllowKeys>();
        VCStartSetUp = EVent.Do.Fetch<IVCSetUpOnStart>();
        SetVCUsage = EVent.Do.Fetch<IVcChangeControlSetUp>();
        ActivateBranchOnControlsChanged = EVent.Do.Fetch<IActivateBranchOnControlsChange>();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IChangeControlsPressed>(ChangeControlType);
        EVent.Do.Subscribe<IOnStart>(StartGame);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
    }

    //Main
    public void OnStart() => OnLevelSetUp();

    private void OnLevelSetUp()
    {
        ShowCursorOnStart = (_inputScheme.ControlType == ControlMethod.MouseOnly
                           || _inputScheme.ControlType == ControlMethod.AllowBothStartWithMouse) 
                            || !_inputScheme.HideMouseCursor;

        if (UsingVirtualCursor)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = ShowCursorOnStart;
        }
        VCStartSetUp?.Invoke(this);
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
        
        SetUpVCCorrectly();
        _sceneStarted = true;
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
        
        SetUpVCCorrectly();
    }
    
    private void SetUpVCCorrectly()
    {
        if(!UsingVirtualCursor) return;
        SetVCUsage?.Invoke(this);
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
        
        if (!CanAllowKeys) return;
        CanAllowKeys = false;
        _switchJustPressed = false;
        SetAllowKeys();
    }

    private void ActivateKeysOrControl()
    {
        if (_inputScheme.HideMouseCursor)
        {
            Cursor.visible = false;
        }
        
        if (CanAllowKeys) return;
        CanAllowKeys = true;
        SetAllowKeys();
        if(!_sceneStarted) return;
        SetNextHighlightedForKeys();
    }

    private void SetAllowKeys() => AllowKeys?.Invoke(this);

    private void SetNextHighlightedForKeys() => ActivateBranchOnControlsChanged?.Invoke(this);
}
