using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
public class ChangeControl : IMono
{
    private readonly IPopUpControls _popUpControls;
    private Vector3 _mousePos = Vector3.zero;
    private readonly ControlMethod _controlMethod;
    private bool _gameStarted;
    
    public ChangeControl(ControlMethod controlMethod, IPopUpControls popUpControls)
    {
        _popUpControls = popUpControls;
        _controlMethod = controlMethod;
        OnEnable();
    }

    //Delegates
    public static event Action<bool> DoAllowKeys; 

    //Properties
    private bool UsingMouse { get; set; }
    private bool UsingKeysOrCtrl { get; set; }
    private UINode LastHighlighted { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;

    public void OnEnable()
    {
        UINode.DoHighlighted += SaveHighlighted;
    }
    
    public void OnDisable()
    {
        UINode.DoHighlighted -= SaveHighlighted;
    }

    public void StartGame()
    {
        _mousePos = Input.mousePosition;
        
        if (MousePreferredControlMethod())
        {
            ActivateMouse();
        }
        else
        {
            ActivateKeysOrControl();
        }
    }

    private bool MousePreferredControlMethod() 
        => _controlMethod == ControlMethod.Mouse || _controlMethod == ControlMethod.BothStartAsMouse;

    public void ChangeControlType()
    {
        if (CanSwitchToMouseControl())
        {
            ActivateMouse();
        }
        else if(CanSwitchToKeysOrController())
        {
            if (!(!Input.GetMouseButton(0) & !Input.GetMouseButton(1))) return;
            ActivateKeysOrControl();
        }
    }

    private bool CanSwitchToKeysOrController() 
        => Input.anyKeyDown &&_controlMethod != ControlMethod.Mouse;

    private bool CanSwitchToMouseControl() 
        => _mousePos != Input.mousePosition && _controlMethod != ControlMethod.KeysOrController;

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
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }

    private void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.Mouse) return;
        DoAllowKeys?.Invoke(UsingKeysOrCtrl);
    }

    private void SetNextHighlightedForKeys()
    {
        if (!_popUpControls.NoActivePopUps)
        {
            _popUpControls.ActiveNextPopUp();
        }
        else
        {
            LastHighlighted.SetAsHighlighted();
        }
    }
}
