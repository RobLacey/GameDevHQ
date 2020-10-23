using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
///
public class ChangeControl : IEventUser, IAllowKeys, IServiceUser
{
    public ChangeControl(InputScheme inputScheme, bool startInGame)
    {
        _controlMethod = inputScheme.ControlType;
        _inputScheme = inputScheme;
        _startInGame = startInGame;
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
    private static CustomEvent<IAllowKeys> AllowKeysEvent { get; } = new CustomEvent<IAllowKeys>();
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IChangeControlsPressed>(ChangeControlType, this);
        EventLocator.Subscribe<IHighlightedNode>(SaveHighlighted, this);
        EventLocator.Subscribe<IOnStart>(StartGame, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IChangeControlsPressed>(ChangeControlType);
        EventLocator.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        EventLocator.Unsubscribe<IOnStart>(StartGame);
    }
    
    public void SubscribeToService() => _historyTracker = ServiceLocator.GetNewService<IHistoryTrack>(this);


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

    private void SetNextHighlightedForKeys() => _historyTracker.MoveToLastBranchInHistory();
}
