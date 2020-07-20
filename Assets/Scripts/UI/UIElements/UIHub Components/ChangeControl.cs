using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>
public class ChangeControl : INodeData, IBranchData, IHUbData, IMono
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
    
    public bool GameIsPaused { get; private set; }

    //Internal Class
    public ChangeControl(UIHub newUiHub, ControlMethod controlMethod)
    {
        _uIHub = newUiHub;
        _controlMethod = controlMethod;
        OnEnable();
    }

    public void OnEnable()
    {
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
        UIHub.GamePaused += IsGamePaused;
    }
    
    public void OnDisable()
    {
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        UIHub.GamePaused -= IsGamePaused;
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
        LastHighlighted.SetNotHighlighted();
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
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }

    private void SetNextHighlightedForKeys()
    {
        if (GameIsPaused)
        {
            LastHighlighted.SetAsHighlighted();
        }
        else if (_uIHub.ActivePopUpsResolve.Count > 0)
        {
            _uIHub.ActiveNextPopUp(_uIHub.ActivePopUpsResolve);
        }
        else if (_uIHub.ActivePopUpsNonResolve.Count > 0)
        {
            _uIHub.ActiveNextPopUp(_uIHub.ActivePopUpsNonResolve);
        }
        else
        {
            ActiveBranch.TweenOnChange = false; //TODO Review after changes
            ActiveBranch.MoveToThisBranch();
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

    public void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;

    public void SaveSelected(UINode newNode) => LastSelected = newNode;

    public void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
    public void IsGamePaused(bool paused) => GameIsPaused = paused;
}