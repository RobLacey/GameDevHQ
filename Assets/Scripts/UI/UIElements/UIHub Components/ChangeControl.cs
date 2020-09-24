using System;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
///
public class ChangeControl
{
    public ChangeControl(InputScheme inputScheme, bool startInGame)
    {
        _controlMethod = inputScheme.ControlType;
        _inputScheme = inputScheme;
        _startInGame = startInGame;
        OnEnable();
    }

    //Variables
    private readonly ControlMethod _controlMethod;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private readonly bool _startInGame;
    private bool _usingMouse;
    private bool _usingKeysOrCtrl;
    private bool _noPopUps = true;
    private INode _lastHighlighted;
    private UIBranch _activeBranch;
    private bool _gameIsPaused;
    private readonly InputScheme _inputScheme;
 
    //Events
    public static event Action<bool> DoAllowKeys;
    public static event Func<UIBranch> ReturnNextPopUp; 

    //Properties
    private void SaveHighlighted(INode newNode) => _lastHighlighted = newNode;

    private void SaveActiveBranch(UIBranch newNode) => _activeBranch = newNode;

    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;
    private void SaveGameIsPaused(bool isPaused) => _gameIsPaused = isPaused;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToOnStart(StartGame);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToGameIsPaused(SaveGameIsPaused);
        _uiControlsEvents.SubscribeOnChangeControls(ChangeControlType);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
    }

    private void StartGame()
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
            _usingKeysOrCtrl = true;
            SetAllowKeys();
        }
    }

    private void ChangeControlType()
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
        _usingKeysOrCtrl = false;
        SetAllowKeys();
    }

    private void ActivateKeysOrControl()
    {
        Cursor.visible = false;
        if (_usingKeysOrCtrl) return;
        _usingMouse = false;
        _usingKeysOrCtrl = true;
        SetAllowKeys();
        SetNextHighlightedForKeys();
        UIHub.SetEventSystem(_lastHighlighted.ReturnNode.gameObject);
    }

    private void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.MouseOnly) return;
        DoAllowKeys?.Invoke(_usingKeysOrCtrl);
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
        if (!_noPopUps && !_gameIsPaused) return ReturnNextPopUp?.Invoke();
        if (nextBranch.LastSelected.HasChildBranch is null) return nextBranch;
        
        while (nextBranch.LastSelected.HasChildBranch.CanvasIsEnabled)
        {
            nextBranch = nextBranch.LastSelected.HasChildBranch;
        }

        return nextBranch;
    }
}
