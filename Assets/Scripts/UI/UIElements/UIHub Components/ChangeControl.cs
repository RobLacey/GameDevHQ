using System;
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
    private readonly IPopUpControls _popUpControls;
    private Vector3 _mousePos = Vector3.zero;
    private readonly ControlMethod _controlMethod;
    private readonly UIData _uiData;
    private bool _gameIsPaused;

    public ChangeControl(ControlMethod controlMethod, IPopUpControls popUpControls)
    {
        _popUpControls = popUpControls;
        _controlMethod = controlMethod;
        _uiData = new UIData();
        _uiData.SubscribeToHighlightedNode(SaveHighlighted);
        _uiData.SubscribeToGameIsPaused(SaveGameIsPaused);
    }
    
    //Delegates
    public static event Action<bool> DoAllowKeys;

    //Properties
    private bool UsingMouse { get; set; }
    private bool UsingKeysOrCtrl { get; set; }
    private UINode LastHighlighted { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    private void SaveGameIsPaused(bool isPaused) => _gameIsPaused = isPaused;

    public void OnDisable()
    {
        _uiData.OnDisable();
    }

    public void StartGame(bool startingInGame)
    {
        _mousePos = Input.mousePosition;
        if (MousePreferredControlMethod())
        {
            SetUpMouse(startingInGame);
        }
        else
        {
            SetUpKeysOrCtrl(startingInGame);
        }
    }

    private bool MousePreferredControlMethod() 
        => _controlMethod == ControlMethod.Mouse || _controlMethod == ControlMethod.BothStartAsMouse;

    private void SetUpMouse(bool startingInGame)
    {
        if (!startingInGame)
        {
            ActivateMouse();
        }
        else
        {
            UsingMouse = true;
            SetAllowKeys();
        }
    }

    private void SetUpKeysOrCtrl(bool startingInGame)
    {
        if (!startingInGame)
        {
            ActivateKeysOrControl();
        }
        else
        {
            UsingKeysOrCtrl = true;
            SetAllowKeys();
        }
    }

    public void ChangeControlType()
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
        => _mousePos != Input.mousePosition && _controlMethod != ControlMethod.KeysOrController;

    private bool CanSwitchToKeysOrController() 
        => Input.anyKeyDown &&_controlMethod != ControlMethod.Mouse;

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
        if (_controlMethod == ControlMethod.Mouse) return;
        DoAllowKeys?.Invoke(UsingKeysOrCtrl);
    }

    private void SetNextHighlightedForKeys()
    {
        if (_popUpControls.NoActivePopUps || _gameIsPaused)
        {
            LastHighlighted.ThisNodeIsHighLighted();
            LastHighlighted.SetAsHighlighted();
        }
        else
        {
            _popUpControls.ActivateCurrentPopUp();
        }
    }
}
