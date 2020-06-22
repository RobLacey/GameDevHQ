﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]

public class UIHub : MonoBehaviour
{
    [Header("Main Settings")]
    [SerializeField] [ValidateInput("ProtectEscapekeySetting")] EscapeKey _GlobalEscapeKeyFunction;
    [SerializeField] [InputAxis] string _cancelButton;
    [SerializeField] [InputAxis] string _branchSwitchButton;
    [SerializeField] [InputAxis] string _pauseButton;
    [SerializeField] UIBranch _pauseMenu;
    [SerializeField] [Label("Enable Controls After..")] float _atStartDelay = 0;
    [Header("In-Game Menu Settings")]
    [SerializeField] bool _inGameMenuSystem = false;
    [SerializeField] [ShowIf("_inGameMenuSystem")] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [ShowIf("_inGameMenuSystem")] [Label("Switch To/From Game Menus")] [InputAxis] string _switchTOMenusButton;
    [SerializeField] [ShowIf("_inGameMenuSystem")] InGameOrInMenu _returnToGameControl;
    [SerializeField] [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")] List<UIBranch> _homeBranches;
    [SerializeField] [ReorderableList] [Label("Hotkeys (CAN'T have Independents as Hotkeys)")] List<HotKeys> _hotKeySettings;

    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }
    public static event Action<bool> AllowKeys;
    public static event Action<bool> IsPaused;

    //Variables
    int _groupIndex = 0;
    UINode _lastRootNode;
    Vector3 _mousePos = Vector3.zero;
    bool _usingMouse = false;
    bool _usingKeysOrCtrl = false;
    bool _canStart = false;
    UIAudioManager _UIAudio;
    public List<UIBranch> ActivePopUps { get; set; } = new List<UIBranch>();

    //Properties
    bool InMenu { get; set; } = true;
    public UIBranch ActiveBranch { get; set; }
    public EscapeKey GlobalEscape { get { return _GlobalEscapeKeyFunction; } }
    public UINode LastSelected { get; set; }
    public UINode LastHighlighted { get; set; }
    public bool OnHomeScreen { get; set; } = false;
    public bool GameIsPaused { get; private set; }
    public int GroupIndex { get { return _groupIndex; } }

    [Serializable]
    public class GroupList
    {
        public UIBranch _groupStartLevel;
    }

    //Editor Scripts
    #region Editor Scripts

    private bool ProtectEscapekeySetting(EscapeKey escapeKey)
    {
        if (_GlobalEscapeKeyFunction == EscapeKey.GlobalSetting)
        {
            Debug.Log("Escape KeyError");
        }
        return escapeKey != EscapeKey.GlobalSetting;
    }

    [Button("Add a New Home Branch Folder")]
    private void MakeFolder()
    {
        var newTree = new GameObject();
        newTree.transform.parent = transform;
        newTree.name = "New Tree Folder";
        newTree.AddComponent<RectTransform>();
        var newBranch = new GameObject();
        newBranch.transform.parent = newTree.transform;
        newBranch.name = "New Branch";
        newBranch.AddComponent<UIBranch>();
        var newNode = new GameObject();
        newNode.transform.parent = newBranch.transform;
        newNode.name = "New Node";
        newNode.AddComponent<UINode>();

    }

    #endregion

    private void Awake()
    {
        _returnToGameControl.Invoke(InMenu);
        _UIAudio = new UIAudioManager(GetComponent<AudioSource>());
    }

    private void OnEnable()
    {
        UINode.DoCancel += CancelOrBack;
    }

    private void OnDisable()
    {
        UINode.DoCancel -= CancelOrBack;
    }

    private void Start()
    {
        LastHighlighted = _homeBranches[0].DefaultStartPosition;
        LastSelected = _homeBranches[0].DefaultStartPosition;
        ActiveBranch = _homeBranches[0];
        _mousePos = Input.mousePosition;
        UICancel._homeGroup = _homeBranches;
        UICancel._myUIHub = this;
        HotKeyProcess._myUIHub = this;
        OnHomeScreen = true;
        IntroAnimations();
        SetUpHomeGroup();

        if (_inGameMenuSystem && _startGameWhere == StartInMenu.InGameControl)
        {
            _canStart = true;
            GameToMenuSwitching();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
            StartCoroutine(StartDelay());
        }
    }

    private void SetUpHomeGroup()
    {
        UIHomeGroup._homeGroup = _homeBranches;
        UIHomeGroup._myUIHub = this;
        UIHomeGroup._allBranches = FindObjectsOfType<UIBranch>();

        foreach (UIBranch item in FindObjectsOfType<UIBranch>())
        {
            if (item.MyBranchType == BranchType.PopUp)
            {
                UIHomeGroup._popUps.Add(item);
            }
        }
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(_atStartDelay);
        _canStart = true;
        ActivateKeysOrControl();
    }


    private void Update() //Review how keys work as hotkeys and pause shouldn't activate keyboard. Maybe hard setting rather than auto
    {
        if (!_canStart) return;

        if (Input.GetButtonDown(_pauseButton))
        {
            PauseMenu();
        }

        if (Input.GetButtonDown(_switchTOMenusButton) & _inGameMenuSystem)
        {
            GameToMenuSwitching();
        }

        foreach (HotKeys item in _hotKeySettings)
        {
            if (item.CheckHotkeys())
            {
                if (_inGameMenuSystem)
                {
                    GameToMenuSwitching();
                }
                return;
            }
        }

        if (InMenu)
        {
            HandleInMenu();
        }
    }

    public void PauseMenu()
    {
        if (GameIsPaused)
        {
            _pauseMenu.IsPauseMenu.RestoreLastPosition();
            GameIsPaused = false;
            Time.timeScale = 1;
        }
        else
        {
            _pauseMenu.IsPauseMenu.StartPopUp();
            GameIsPaused = true;
            Time.timeScale = 0;
        }
        IsPaused?.Invoke(GameIsPaused);
    }

    private void HandleInMenu()
    {
        if (Input.mousePosition != _mousePos)
        {
            ActivateMouse();
        }

        if (Input.anyKeyDown)
        {
            if (Input.GetButtonDown(_cancelButton))
            {
                if (ActiveBranch.FromHotkey)
                {
                    CancelOrBack(EscapeKey.BackToHome);
                }
                else if(GameIsPaused || ActivePopUps.Count > 0)
                {
                    UICancel.CancelOrBackButton(EscapeKey.BackOneLevel);
                }
                else
                {
                    UICancel.Cancel();
                }
            }
            else if (Input.GetButtonDown(_branchSwitchButton))
            {
                if (OnHomeScreen)
                {
                    SwitchHomeGroups();
                }
                else
                {
                    ActiveBranch.SwitchBranchGroup();
                }
            }
            else
            {
                ActivateKeysOrControl();
            }
        }
    }

    private void IntroAnimations()
    {
        foreach (UIBranch homeBranch in _homeBranches)
        {
            if (homeBranch != _homeBranches[0])
            {
                homeBranch.DontSetAsActive = true;
            }
            homeBranch.MoveToNextLevel();
        }
    }

    private void CancelOrBack(EscapeKey escapeKey)
    {
        if (ActiveBranch.FromHotkey)
        {
            ActiveBranch.FromHotkey = false;
        }

        UICancel.CancelOrBackButton(escapeKey);
    }

    public void SetLastSelected(UINode node)
    {
        if (LastSelected != null && _lastRootNode != null)
        {
            if (OnHomeScreen)
            {
                UINode temp = DeactiveLastSelected(node);
                _lastRootNode.SetNotHighlighted();
                _lastRootNode = temp;
            }
            else
            {
                if (LastSelected.ChildBranch != null && node.MyBranch.MyBranchType != BranchType.PauseMenu)
                {
                    if (LastSelected.ChildBranch.MyBranchType == BranchType.Internal)
                    {
                        LastSelected.SetNotHighlighted();
                        if (LastSelected.IsSelected == true) LastSelected.PressedActions();
                    }
                }
            }
        }
        else
        {
            _lastRootNode = node;
        }
        LastSelected = node;
    }

    private UINode DeactiveLastSelected(UINode node)
    {
        if (node.MyBranch.MyBranchType == BranchType.PopUp) return LastSelected;

        UINode temp = node;

        while (temp.MyBranch != temp.MyBranch.MyParentBranch)
        {
            temp = temp.MyBranch.MyParentBranch.LastSelected;
        }

        if (temp != _lastRootNode && _lastRootNode.IsSelected == true)
        {
            _lastRootNode.PressedActions();
        }
        return temp;
    }

    public void SetLastHighlighted(UINode newNode)
    {
        if (newNode != LastHighlighted)
        {
            LastHighlighted.SetNotHighlighted();
            LastHighlighted = newNode;
            ActiveBranch = LastHighlighted.MyBranch;
            if (OnHomeScreen)
            {
                UIHomeGroup.SetHomeGroupIndex(LastHighlighted.MyBranch, ref _groupIndex);
            }
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
        }
    }

    private void SwitchHomeGroups()
    {
        LastHighlighted.IAudio.Play(UIEventTypes.Selected);
        UIHomeGroup.SwitchHomeGroups(ref _groupIndex);
    }

    private void ActivateMouse()
    {
        _mousePos = Input.mousePosition;

        if (_usingMouse == false)
        {
            _usingMouse = true;
            _usingKeysOrCtrl = false;
            LastHighlighted.SetNotHighlighted();
            AllowKeys?.Invoke(false);
        }
    }

    private void ActivateKeysOrControl()
    {
        if (!_canStart) return;

        if (!Input.GetMouseButton(0) & !Input.GetMouseButton(1))
        {
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);

            if (_usingKeysOrCtrl == false)
            {
                _usingKeysOrCtrl = true;
                _usingMouse = false;
                LastHighlighted.SetAsHighlighted();
                AllowKeys?.Invoke(true);
            }
        }
    }

    private void GameToMenuSwitching()
    {
        if (!_usingMouse)
        {
            if (InMenu)
            {
                InMenu = false;
                LastHighlighted.SetNotHighlighted();
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                InMenu = true;
                LastHighlighted.SetAsHighlighted();
                EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
            }

            _returnToGameControl.Invoke(InMenu);
        }
    }

    public void PrintEnter()
    {
        Debug.Log("This is Enter test");
    }
    public void PrintExit()
    {
        Debug.Log("This is Exit test");
    }
    public void PrintClick()
    {
        Debug.Log("This is Cllick test");
    }
    public void PrintToggle()
    {
        Debug.Log("This is Toggle test");
    }
}
