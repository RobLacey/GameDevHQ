using System;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>

public interface IChangeControl : IEventUser
{
    void OnEnable();
}

public class ChangeControl : IChangeControl, IAllowKeys, IEventDispatcher, IVCActive
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
    private bool _usingMouse, _sceneStarted;
    private readonly InputScheme _inputScheme;
    private IBranch _activeBranch;
    private INode _lastHighlighted;

    //Properties
    public bool CanAllowKeys { get; private set; }
    private bool UsingVirtualCursor => _inputScheme.CanUseVirtualCursor == VirtualControl.Yes;
    public bool VCActive => _usingMouse;

    //Events
    private Action<IAllowKeys> AllowKeys { get; set; }
    private Action<IVCActive> VCIsActive { get; set; }

    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void SaveHighlighted(IHighlightedNode args) => _lastHighlighted = args.Highlighted;

    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }
    
    public void FetchEvents()
    {
        AllowKeys = EVent.Do.Fetch<IAllowKeys>();
        VCIsActive = EVent.Do.Fetch<IVCActive>();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IChangeControlsPressed>(ChangeControlType);
        EVent.Do.Subscribe<IOnStart>(StartGame);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
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
        if (_inputScheme.CanSwitchToMouseOrVC(_usingMouse))
        {
            ActivateMouseOrVirtualCursor();
        }
        else if(_inputScheme.CanSwitchToKeysOrController(CanAllowKeys))
        {
            if (_inputScheme.AnyMouseClicked) return;
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
        if(UsingVirtualCursor) 
            SetUpVcActivity();
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

    private void SetNextHighlightedForKeys()
    {
        if(UsingVirtualCursor)
            _lastHighlighted.ClearNode();
        
        if (_activeBranch.IsHomeScreenBranch())
        {
            _lastHighlighted.MyBranch.DoNotTween();
            _lastHighlighted.MyBranch.MoveToThisBranch();
        }
        else
        {
            _activeBranch.DoNotTween();
            _activeBranch.MoveToThisBranch();
        }
    }
    
    private void SetUpVcActivity() => VCIsActive?.Invoke(this);
}
