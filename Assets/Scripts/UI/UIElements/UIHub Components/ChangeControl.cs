using System;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
///
//ToDo make changing to keys pick up the last highlighted plus open branch not where the mouse was last.
// TODO Currently if a node is moused over that becomes the next node activated when switched to keys which might not be
// TODO in the open branch.
public class ChangeControl
{
    private Vector3 _mousePos = Vector3.zero;
    private readonly ControlMethod _controlMethod;
    private readonly UIDataEvents _uiDataEvents;
    private readonly UIControlsEvents _uiControlsEvents;
    private readonly bool _startInGame;

    public ChangeControl(ControlMethod controlMethod, bool startInGame)
    {
        _controlMethod = controlMethod;
        _startInGame = startInGame;
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents(); 
        OnEnable();
    }
    
    //Delegates
    public static event Action<bool> DoAllowKeys;

    //Properties
    private bool UsingMouse { get; set; }
    private bool UsingKeysOrCtrl { get; set; }
    private UINode LastHighlighted { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToOnStart(StartGame);
        _uiControlsEvents.SubscribeOnChangeControls(ChangeControlType);
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
            UsingMouse = true;
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
            UsingKeysOrCtrl = true;
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
        if (UsingMouse) return;
        UsingMouse = true;
        UsingKeysOrCtrl = false;
        SetAllowKeys();
        LastHighlighted.SetNotHighlighted();
    }

    private void ActivateKeysOrControl()
    {
        Cursor.visible = false;
        if (UsingKeysOrCtrl) return;
        UsingKeysOrCtrl = true;
        UsingMouse = false;
        SetAllowKeys();
        SetNextHighlightedForKeys();
        UIHub.SetEventSystem(LastHighlighted.gameObject);
    }

    private void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.MouseOnly) return;
        DoAllowKeys?.Invoke(UsingKeysOrCtrl);
    }

    private void SetNextHighlightedForKeys()
    {
        LastHighlighted.ThisNodeIsHighLighted();
        LastHighlighted.SetAsHighlighted();
    }
}
