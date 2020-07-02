using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeControl: IChangeControl
{
    private readonly IHubData _hubData;
    public bool UsingMouse { get; private set; }
    private bool UsingKeysOrCtrl { get; set; }
    private Vector3 _mousePos = Vector3.zero;
    private readonly string _cancel;
    private readonly string _switch;
    private readonly ControlMethod _controlMethod;
    private IAllowKeys[] _allowKeyClasses;

    public ChangeControl(IHubData newHubDataData, string cancelButton, string switchButton, ControlMethod controlMethod)
    {
        _hubData = newHubDataData;
        _cancel = cancelButton;
        _switch = switchButton;
        _controlMethod = controlMethod;
    }
    
    public void StartGame(IAllowKeys[] allowKeys)
    {
        _allowKeyClasses = allowKeys;
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
        if(CheckInput()) return;
        if (_mousePos != Input.mousePosition && _controlMethod != ControlMethod.KeysOrController)
        {
            _mousePos = Input.mousePosition;
            if (UsingMouse) return;
            ActivateMouse();
        }
        else if(Input.anyKeyDown && !UsingKeysOrCtrl && _controlMethod != ControlMethod.Mouse)
        {
            ActivateKeysOrControl();
        }
    }
    
    private void ActivateMouse()
    {
        UsingMouse = true;
        UsingKeysOrCtrl = false;
        SetAllowKeys();
        _hubData.LastHighlighted.SetNotHighlighted();
    }

    private void ActivateKeysOrControl()
    {
        if (!(!Input.GetMouseButton(0) & !Input.GetMouseButton(1))) return;

        UsingKeysOrCtrl = true;
        UsingMouse = false;
        SetAllowKeys();
        EventSystem.current.SetSelectedGameObject(_hubData.LastHighlighted.gameObject);
        SetHighlightedForKeys();
    }
    
    private void SetHighlightedForKeys()
    {
        if (_hubData.GameIsPaused || _hubData.ActivePopUps_Resolve.Count > 0)
        {
            _hubData.LastHighlighted.SetAsHighlighted();
        }
        else if (_hubData.ActivePopUps_NonResolve.Count > 0)
        {
            _hubData.HandleActivePopUps();
        }
        else
        {
            _hubData.LastHighlighted.SetAsHighlighted();
        }
    }

    private bool CheckInput()
    {
        return Input.GetButtonDown(_cancel) && Input.GetButtonDown(_switch);
    }

    private void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.Mouse) return;
        
        foreach (var item in _allowKeyClasses)
        {
            item.AllowKeys = UsingKeysOrCtrl;
            Debug.Log(UsingKeysOrCtrl);
        }
    }
}