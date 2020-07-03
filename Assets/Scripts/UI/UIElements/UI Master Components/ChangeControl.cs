using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeControl: IChangeControl
{
    private readonly IHubData _hubData;
    private Vector3 _mousePos = Vector3.zero;
    private readonly string _cancel;
    private readonly string _switch;
    private readonly ControlMethod _controlMethod;
    private bool _gameStarted;

    //Properties
    public bool UsingMouse { get; private set; }
    public bool UsingKeysOrCtrl { get; set; }
    public IAllowKeys[] AllowKeyClasses { get; set; }

    //Internal Class
    public ChangeControl(IHubData newHubDataData, ControlMethod controlMethod, string cancelButton, string switchButton)
    {
        _hubData = newHubDataData;
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
        if(CheckIfAllowedInput()) return;
        if (_mousePos != Input.mousePosition && _controlMethod != ControlMethod.KeysOrController)
        {
            _mousePos = Input.mousePosition;
            if (UsingMouse) return;
            ActivateMouse();
        }
        else if(Input.anyKeyDown && /*!UsingKeysOrCtrl && */_controlMethod != ControlMethod.Mouse)
        {
            if (!(!Input.GetMouseButton(0) & !Input.GetMouseButton(1))) return;
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

    public void ActivateKeysOrControl()
    {
        if (!_hubData.CanStart) return;
        if (!UsingKeysOrCtrl)
        {
            UsingKeysOrCtrl = true;
            UsingMouse = false;
            SetAllowKeys();
            SetHighlightedForKeys();
        }
        EventSystem.current.SetSelectedGameObject(_hubData.LastHighlighted.gameObject);
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

    private bool CheckIfAllowedInput()
    {
        return Input.GetButtonDown(_cancel) || Input.GetButtonDown(_switch);
    }

    public void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.Mouse) return;
        
        foreach (var item in AllowKeyClasses)
        {
            item.AllowKeys = UsingKeysOrCtrl;
        }
    }
}