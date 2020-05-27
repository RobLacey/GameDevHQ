﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using NaughtyAttributes;

public class UIMasterController : MonoBehaviour
{
    [Header("Main Settings")] 
    //[Label("UI Starting Branch")] [Required("MUST have a starting branch")]
    //[SerializeField] UIBranch _uiTopLevel;
    [SerializeField] [ValidateInput("ProtectEscapekeySetting")] EscapeKey _GlobalEscapeKeyFunction;
    [SerializeField] [InputAxis] string _cancelButton;
    [SerializeField] [InputAxis] string _branchSwitchButton;
    [SerializeField] [InputAxis] string _pauseButton;
    [SerializeField] [Label("Enable Controls After..")] float _atStartDelay = 0;
    [Header("In-Game Menu Settings")]
    [SerializeField] bool _inGameMenuSystem = false;
    [SerializeField] [ShowIf("_inGameMenuSystem")] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [ShowIf("_inGameMenuSystem")] [Label("Switch To/From Game Menus")] [InputAxis] string _switchTOMenusButton;
    [SerializeField] [ShowIf("_inGameMenuSystem")] InGameOrInMenu _returnToGameControl;
    [SerializeField] [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")] List<UIBranch> _homeBranches;

    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }

    //Variables
    int _groupIndex = 0;
    UIBranch[] _allUIBranches;
    UINode _uiElementLastSelected;
    UIBranch _activeBranch;
    Vector3 _mousePos = Vector3.zero;
    bool _usingMouse = false;
    bool _usingKeysOrCtrl = false;
    bool _onHomeScreen = false;
    bool _canStart = false;

    //Properties
    bool InMenu { get; set; } = true; 

    //Editor Scripts
    #region Editor Scripts

    private bool ProtectEscapekeySetting(EscapeKey escapeKey)
    {
        if (_GlobalEscapeKeyFunction == EscapeKey.GlobalSetting)
        {
            Debug.Log("Escape KeyError"); 
        }
        return escapeKey != EscapeKey.GlobalSetting; }

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

    [Serializable]
    public class GroupList
    {
        public UIBranch _groupStartLevel;
    }

    private void Awake()
    {
        _allUIBranches = FindObjectsOfType<UIBranch>();
        _returnToGameControl.Invoke(InMenu);
    }

    private void OnEnable()
    {
        UINode.Canceller += OnCancel;
    }

    private void OnDisable()
    {
        UINode.Canceller -= OnCancel;
    }

    private void Start()
    {
        _uiElementLastSelected = _homeBranches[0].DefaultStartPosition;
        _activeBranch = _homeBranches[0];
        _mousePos = Input.mousePosition;
        _onHomeScreen = true;
        IntroAnimations();

        if (_inGameMenuSystem && _startGameWhere == StartInMenu.InGameControl)
        {
            _canStart = true;
            ProcessGameToMenuSwitching();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
            StartCoroutine(StartDelay());
        }
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(_atStartDelay);
        _canStart = true;
        ActivateKeysOrControl();
    }


    private void Update()
    {
        if (!_canStart) return;

        if (Input.GetButtonDown(_switchTOMenusButton) & _inGameMenuSystem)
        {
            ProcessGameToMenuSwitching();
        }

        if (InMenu)
        {
            if (Input.mousePosition != _mousePos)
            {
                ActivateMouse();
            }

            if (Input.anyKeyDown)
            {
                if (Input.GetButtonDown(_cancelButton))
                {
                    OnCancel(_uiElementLastSelected.GetComponentInParent<UIBranch>().EscapeKeySetting);
                }
                else if (Input.GetButtonDown(_branchSwitchButton))
                {
                    if (_onHomeScreen)
                    {
                        SwitchRootGroups();
                    }
                    else
                    {
                        _activeBranch.SwitchGroup();
                    }
                }
                else
                {
                    ActivateKeysOrControl();
                }
            }
        }
    }

    private void IntroAnimations()
    {
        foreach (var item in _homeBranches)
        {
            if (item != _homeBranches[0])
            {
                item.DontSetAsActive = true;
                item.ActivateINTweens();
            }
            else
            {
                _homeBranches[0].MoveToNextLevel(_homeBranches[0]);
            }
        }
    }

    private void ActivateMouse()
    {
        _mousePos = Input.mousePosition;

        if (_usingMouse == false)
        {
            _usingMouse = true;
            _usingKeysOrCtrl = false;
            _uiElementLastSelected.SetNotHighlighted();

            foreach (var item in _allUIBranches)
            {
                item.AllowKeys = false;
            }
        }
    }

    public void SetLastUIObject(UINode uiObject)
    {
        ClearAndSetRootGroup(uiObject);
        _uiElementLastSelected = uiObject;
        _activeBranch = _uiElementLastSelected.MyBranchController;
        EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
    }

    public void ClearAndSetRootGroup(UINode uiObject)
    {
        int tempIndexStore = _groupIndex;
        if (SetRootGroup(uiObject.MyBranchController))
        {
            _homeBranches[tempIndexStore].LastSelected.DisableLevel();
            _homeBranches[tempIndexStore].LastSelected.SetNotHighlighted();
        }
    }

    private void SwitchRootGroups()
    {
        _uiElementLastSelected._audio.Play(UIEventTypes.Selected, _uiElementLastSelected._functionToUse);
        _uiElementLastSelected.SetNotHighlighted();

        _homeBranches[_groupIndex].LastSelected.DisableLevel();
        _groupIndex++;
        if (_groupIndex > _homeBranches.Count - 1)
        {
            _groupIndex = 0;
        }
        _homeBranches[_groupIndex].DontAnimateOnChange = true;
        _homeBranches[_groupIndex].MoveToNextLevel();
    }

    private void ActivateKeysOrControl()
    {
        if (!_canStart) return;

        if (!Input.GetMouseButton(0) & !Input.GetMouseButton(1))
        {
            EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);

            if (_usingKeysOrCtrl == false)
            {
                _usingKeysOrCtrl = true;
                _usingMouse = false;
                _uiElementLastSelected.SetAsHighlighted();

                foreach (var item in _allUIBranches)
                {
                    item.AllowKeys = true;
                }
            }
        }
    }

    private void OnCancel(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.BackOneLevel)
        {
            BackOneLevel();
        }
        else if (escapeKey == EscapeKey.BackToRootLevel)
        {
            BackToRoot();
        }

        else if (escapeKey == EscapeKey.GlobalSetting)
        {
            UseGlobalEscapeSettings();
        }
    }

    private void UseGlobalEscapeSettings()
    {
        if (_GlobalEscapeKeyFunction == EscapeKey.BackOneLevel)
        {
            BackOneLevel();       
        }

        if (_GlobalEscapeKeyFunction == EscapeKey.BackToRootLevel)
        {
            BackToRoot();
        }
    }
    private void BackToRoot()
    {
        _uiElementLastSelected.MyBranchController.StartOutTweens(false);
        _uiElementLastSelected._audio.Play(UIEventTypes.Cancelled, _uiElementLastSelected._functionToUse);
        RestoreHomeScreen();
        _homeBranches[_groupIndex].LastSelected.DisableLevel();
        _homeBranches[_groupIndex].MoveBackALevel();
    }

    private void BackOneLevel()
    {
        if (!_onHomeScreen && IsItPartOfRootMenu(_uiElementLastSelected.MyParentController))
        {
            RestoreHomeScreen();
        }

        if (_uiElementLastSelected.MyParentController)
        {
            _uiElementLastSelected.OnCancel();
        }
    }

    public void ClearHomeScreen()
    {
        if (!_onHomeScreen) return;

        foreach (var item in _homeBranches)
        {
            _onHomeScreen = false;
            item.MyCanvas.enabled = false;
            _homeBranches[_groupIndex].LastSelected.DisableLevel();
            _homeBranches[_groupIndex].LastSelected.SetNotHighlighted();
        }
    }

    public void RestoreHomeScreen()
    {
        foreach (var item in _homeBranches)
        {
            _onHomeScreen = true;
            item.RestoreFromFullscreen();
        }
    }

    private void ProcessGameToMenuSwitching()
    {
        if (!_usingMouse)
        {
            if (InMenu)
            {
                InMenu = false;
                _uiElementLastSelected.SetNotHighlighted();
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                InMenu = true;
                _uiElementLastSelected.SetAsHighlighted();
                EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
            }

            _returnToGameControl.Invoke(InMenu);
        }    
    }

    private bool IsItPartOfRootMenu(UIBranch uIBranch) //Check that I'm on the homescreen when mving back levels
    {
        foreach (var item in _homeBranches)
        {
            if (item == uIBranch)
            {
                return true;
            }
        }
        return false;
    }

    private bool SetRootGroup(UIBranch uIBranch)
    {
        for (int i = 0; i < _homeBranches.Count; i++)
        {
            if (_homeBranches[i] == uIBranch)
            {
                if (i != _groupIndex)
                {
                    _groupIndex = i;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }
}
