using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
///
public class ChangeControl : IEventUser, IAllowKeys
{
    public ChangeControl(InputScheme inputScheme, bool startInGame)
    {
        _controlMethod = inputScheme.ControlType;
        _inputScheme = inputScheme;
        _startInGame = startInGame;
        ObserveEvents();
    }

    //Variables
    private readonly ControlMethod _controlMethod;
    private readonly bool _startInGame;
    private bool _usingMouse;
    private bool _noPopUps = true;
    private INode _lastHighlighted;
    private UIBranch _activeBranch;
    private bool _gameIsPaused;
    private readonly InputScheme _inputScheme;

    //Events
    private static CustomEvent<IReturnNextPopUp, UIBranch> ReturnNextPopUp { get; } 
        = new CustomEvent<IReturnNextPopUp, UIBranch>();

    //Properties
    private void SaveHighlighted(IHighlightedNode args) => _lastHighlighted = args.Highlighted;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    public bool CanAllowKeys { get; private set; }

    //Events
    private static CustomEvent<IAllowKeys> AllowKeysEvent { get; } = new CustomEvent<IAllowKeys>();
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IChangeControlsPressed>(ChangeControlType, this);
        EventLocator.Subscribe<IGameIsPaused>(SaveGameIsPaused, this);
        EventLocator.Subscribe<IHighlightedNode>(SaveHighlighted, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<INoPopUps, bool>(SaveNoPopUps, this);
        EventLocator.Subscribe<IOnStart>(StartGame, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IChangeControlsPressed>(ChangeControlType);
        EventLocator.Unsubscribe<IGameIsPaused>(SaveGameIsPaused);
        EventLocator.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<INoPopUps, bool>(SaveNoPopUps);
        EventLocator.Unsubscribe<IOnStart>(StartGame);
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
        UIHub.SetEventSystem(_lastHighlighted.ReturnNode.gameObject);
    }

    private void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.MouseOnly) return;
        AllowKeysEvent?.RaiseEvent(this);
   }

    //Must track active branch in case user closes branches while PopUps are open
    private void SetNextHighlightedForKeys() 
    {
        var nextBranch = _activeBranch;
        nextBranch = FindActiveBranchesEndNode(nextBranch);
        nextBranch.MoveToBranchWithoutTween();
    }

    private UIBranch FindActiveBranchesEndNode(UIBranch nextBranch)
    {
        if (!_noPopUps && !_gameIsPaused) return ReturnNextPopUp?.RaiseEvent();
        if (nextBranch.LastSelected.HasChildBranch is null) return nextBranch;
        
        while (nextBranch.LastSelected.HasChildBranch.CanvasIsEnabled)
        {
            nextBranch = nextBranch.LastSelected.HasChildBranch;
        }

        return nextBranch;
    }
}
