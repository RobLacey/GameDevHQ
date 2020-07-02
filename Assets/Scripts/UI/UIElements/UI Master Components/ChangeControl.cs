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
    
    public ChangeControl(IHubData newHubDataData, string cancelButton, string switchButton)
    {
        _hubData = newHubDataData;
        _cancel = cancelButton;
        _switch = switchButton;
    }

    public void StartGame(ControlMethod controlMethod)
    {
        if (controlMethod == ControlMethod.Mouse || controlMethod == ControlMethod.BothStartAsMouse)
        {
            ActivateMouse();
        }
        else
        {
            ActivateKeysOrControl();
        }
    }

    public void ChangeControlType()
    {
        if(CheckInput()) return;
        if (_mousePos != Input.mousePosition)
        {
            _mousePos = Input.mousePosition;
            if (UsingMouse) return;
            ActivateMouse();
        }
        else if(Input.anyKeyDown && !UsingKeysOrCtrl)
        {
            ActivateKeysOrControl();
        }
    }
    
    private void ActivateMouse()
    {
        UsingMouse = true;
        UsingKeysOrCtrl = false;
        _hubData.LastHighlighted.SetNotHighlighted();
        _hubData.AllowKeyInvoker(false);
    }

    private void ActivateKeysOrControl()
    {
        if (UsingKeysOrCtrl) return;
        if (!(!Input.GetMouseButton(0) & !Input.GetMouseButton(1))) return;

        UsingKeysOrCtrl = true;
        UsingMouse = false;
        _hubData.AllowKeyInvoker(true);
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

}