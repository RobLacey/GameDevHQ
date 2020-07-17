using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
public class ChangeControl : INodeData, IBranchData
{
    private readonly UIHub _uIHub;
    private Vector3 _mousePos = Vector3.zero;
    private readonly ControlMethod _controlMethod;
    private bool _gameStarted;

    //Properties
    private bool UsingMouse { get; set; }
    public bool UsingKeysOrCtrl { get; set; }
    public UIBranch[] AllowKeyClasses { get; set; }
    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; private set; }
    public UIBranch ActiveBranch { get; private set; }

    //Internal Class
    public ChangeControl(UIHub newUiHub, ControlMethod controlMethod)
    {
        _uIHub = newUiHub;
        _controlMethod = controlMethod;
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
    }
    
    public void OnDisabled()
    {
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
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
        /*_uIHub.*/LastHighlighted.SetNotHighlighted();
    }

    private void ActivateKeysOrControl()
    {
        if (!UsingKeysOrCtrl)
        {
            Cursor.visible = false;
            UsingKeysOrCtrl = true;
            UsingMouse = false;
            SetAllowKeys();
            SetNextHighlightedForKeys();
        }
        EventSystem.current.SetSelectedGameObject(_uIHub.LastHighlighted.gameObject);
    }

    private void SetNextHighlightedForKeys()
    {
        if (_uIHub.GameIsPaused || _uIHub.ActivePopUpsResolve.Count > 0)
        {
            /*_uIHub.*/LastHighlighted.SetAsHighlighted();
        }
        else if (_uIHub.ActivePopUpsNonResolve.Count > 0)
        {
            _uIHub.ActiveNextPopUp();
        }
        else
        {
            /*_uIHub.*/ActiveBranch.TweenOnChange = false; //TODO Review after changes
            /*_uIHub.*/ ActiveBranch.MoveToThisBranch();
        }
    }

    public void SetAllowKeys()
    {
        if (_controlMethod == ControlMethod.Mouse) return;
        
        foreach (var item in AllowKeyClasses)
        {
            item.AllowKeys = UsingKeysOrCtrl;
        }
    }

    public void SaveHighlighted(UINode newNode)
    {
        LastHighlighted = newNode;
    }

    public void SaveSelected(UINode newNode)
    {
        LastSelected = newNode;
    }

    public void SaveActiveBranch(UIBranch newBranch)
    {
        ActiveBranch = newBranch;
    }
}