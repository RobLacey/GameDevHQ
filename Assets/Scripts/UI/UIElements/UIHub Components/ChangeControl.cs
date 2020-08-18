using System;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
///
public class ChangeControl
{
    public ChangeControl(ControlMethod controlMethod, bool startInGame)
    {
        _controlMethod = controlMethod;
        _startInGame = startInGame;
        OnEnable();
    }

    //Variables
    private Vector3 _mousePos = Vector3.zero;
    private readonly ControlMethod _controlMethod;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private readonly bool _startInGame;
    private bool _usingMouse;
    private bool _usingKeysOrCtrl;
    private bool _noPopUps = true;
    private UINode _lastHighlighted;
    private UIBranch _activeBranch;

    //Events
    public static event Action<bool> DoAllowKeys;
    public static event Func<UIBranch> ReturnNextPopUp; 

    //Properties
    private void SaveHighlighted(UINode newNode) => _lastHighlighted = newNode;

    private void SaveActiveBranch(UIBranch newNode) => _activeBranch = newNode;

    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToOnStart(StartGame);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiControlsEvents.SubscribeOnChangeControls(ChangeControlType);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
    }

    private void StartGame()
    {
        _mousePos = Input.mousePosition;
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
        if (CanSwitchToMouseControl())
        {
            ActivateMouse();
        }
        else if(CanSwitchToKeysOrController())
        {
            if (MouseButtonsClicked()) return;
            ActivateKeysOrControl();
        }
    }

    private bool CanSwitchToMouseControl() 
        => _mousePos != Input.mousePosition && _controlMethod != ControlMethod.KeysOrControllerOnly;

    private bool CanSwitchToKeysOrController() 
        => Input.anyKeyDown &&_controlMethod != ControlMethod.MouseOnly;

    private static bool MouseButtonsClicked()
        => !(!Input.GetMouseButton(0) & !Input.GetMouseButton(1));

    private void ActivateMouse()
    {
        _mousePos = Input.mousePosition;
        Cursor.visible = true;
        if (_usingMouse) return;
        _usingMouse = true;
        _usingKeysOrCtrl = false;
        SetAllowKeys();
        _lastHighlighted.SetNotHighlighted();
    }

    private void ActivateKeysOrControl()
    {
        Cursor.visible = false;
        if (_usingKeysOrCtrl) return;
        _usingMouse = false;
        _usingKeysOrCtrl = true;
        SetAllowKeys();
        SetNextHighlightedForKeys();
        UIHub.SetEventSystem(_lastHighlighted.gameObject);
    }

    private void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.MouseOnly) return;
        DoAllowKeys?.Invoke(_usingKeysOrCtrl);
    }

    private void SetNextHighlightedForKeys()
    {
        var nextBranch = _activeBranch;
        nextBranch = FindLastActiveBranchesEndNode(nextBranch);
        nextBranch.MoveToBranchWithoutTween();
        // nextBranch.LastHighlighted.ThisNodeIsHighLighted();
        // nextBranch.set
        // nextBranch.LastHighlighted.SetAsHighlighted();
    }

    private UIBranch FindLastActiveBranchesEndNode(UIBranch nextBranch)
    {
        if (!_noPopUps) return ReturnNextPopUp?.Invoke();
        if (nextBranch.LastSelected.HasChildBranch == null) return nextBranch;
        
        while (nextBranch.LastSelected.HasChildBranch.CanvasIsEnabled)
        {
            nextBranch = nextBranch.LastSelected.HasChildBranch;
        }

        return nextBranch;
    }
}
