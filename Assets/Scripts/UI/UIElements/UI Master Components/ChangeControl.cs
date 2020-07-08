using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeControl
{
    private readonly UIHub _uIHub;
    private Vector3 _mousePos = Vector3.zero;
    private readonly string _cancel;
    private readonly string _switch;
    private readonly ControlMethod _controlMethod;
    private bool _gameStarted;

    //Properties
    public bool UsingMouse { get; private set; }
    public bool UsingKeysOrCtrl { get; set; }
    public UIBranch[] AllowKeyClasses { get; set; }

    //Internal Class
    public ChangeControl(UIHub newUiHub, ControlMethod controlMethod, string cancelButton, string switchButton)
    {
        _uIHub = newUiHub;
        _cancel = cancelButton;
        _switch = switchButton;
        _controlMethod = controlMethod;
    }
    
    public void StartGame()
    {
        if (_controlMethod == ControlMethod.Mouse || _controlMethod == ControlMethod.BothStartAsMouse)
        {
            ActivateMouse();
        }
        else
        {
            _mousePos = Input.mousePosition;
            ActivateKeysOrControl();
        }
    }

    public void ChangeControlType()
    {
        //if(CheckIfAllowedInput()) return;
        if (_mousePos != Input.mousePosition && _controlMethod != ControlMethod.KeysOrController)
        {
            _mousePos = Input.mousePosition;
            if (UsingMouse) return;
            ActivateMouse();
        }
        else if(Input.anyKeyDown &&_controlMethod != ControlMethod.Mouse)
        {
            if (!(!Input.GetMouseButton(0) & !Input.GetMouseButton(1))) return;
            ActivateKeysOrControl();
        }
    }

    private void ActivateMouse()
    {
        Cursor.visible = true;
        UsingMouse = true;
        UsingKeysOrCtrl = false;
        SetAllowKeys();
        _uIHub.LastHighlighted.SetNotHighlighted();
    }

    public void ActivateKeysOrControl()
    {
        if (!_uIHub.CanStart) return;
        if (!UsingKeysOrCtrl)
        {
            Cursor.visible = false;
            UsingKeysOrCtrl = true;
            UsingMouse = false;
            SetAllowKeys();
            SetHighlightedForKeys();
        }
        EventSystem.current.SetSelectedGameObject(_uIHub.LastHighlighted.gameObject);
    }

    private void SetHighlightedForKeys()
    {
        if (_uIHub.GameIsPaused || _uIHub.ActivePopUpsResolve.Count > 0)
        {
            _uIHub.LastHighlighted.SetAsHighlighted();
        }
        else if (_uIHub.ActivePopUpsNonResolve.Count > 0)
        {
            _uIHub.HandleActivePopUps();
        }
        else
        {
            _uIHub.LastHighlighted.SetAsHighlighted();
        }
    }

    // private bool CheckIfAllowedInput()
    // {
    //     return Input.GetButtonDown(_cancel) || Input.GetButtonDown(_switch);
    // }

    public void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.Mouse) return;
        
        foreach (var item in AllowKeyClasses)
        {
            item.AllowKeys = UsingKeysOrCtrl;
        }
    }
}