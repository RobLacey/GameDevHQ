using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using NaughtyAttributes;

public class UIMasterController : MonoBehaviour
{
    [Header("Main Settings")] 
    [Label("UI Starting Branch")] [Required("MUST have a starting branch")]
    [SerializeField] UIBranch _uiTopLevel;
    [SerializeField] [ValidateInput("ProtectEscapekeySetting")] EscapeKey _GlobalEscapeKeyFunction;
    [SerializeField] [InputAxis] string _cancelButton;
    [SerializeField] [InputAxis] string _changeMenuGroupButton;
    [SerializeField] float _atStartDelay = 0;
    [Header("In-Game Menu Settings")]
    [SerializeField] bool _inGameMenuSystem = false;
    [SerializeField] [ShowIf("_inGameMenuSystem")] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [ShowIf("_inGameMenuSystem")] [Label("Switch To/From Game Menus")] [InputAxis] string _switchTOMenusButton;
    [SerializeField] [ShowIf("_inGameMenuSystem")] InGameOrInMenu _returnToGameControl;
    [Header("Root UI Groups")]
    [Label("List of Root UI Groups")] [Tooltip("Add a group if keyboard/Controller group switching is needed")] 
    [SerializeField] GroupList[] _groupList;

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

    #endregion    
    
    [Serializable]
    public class GroupList
    {
        public UIGroupID _groupIDNumber;
        public UIBranch _groupStartLevel;
    }

    private void Awake()
    {
        _allUIBranches = FindObjectsOfType<UIBranch>();

        if (_inGameMenuSystem)
        {
            if (_startGameWhere == StartInMenu.InGameControl)
            {
                InMenu = false;
            }
        }
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
        _uiElementLastSelected = _uiTopLevel.DefaultStartPosition;
        _activeBranch = _uiTopLevel;
        _mousePos = Input.mousePosition;
        _onHomeScreen = true;

        if (InMenu)
        {
            EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
            StartCoroutine(StartDelay());
            IntroAnimations();
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
                else if (Input.GetButtonDown(_changeMenuGroupButton))
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
        foreach (var item in _groupList)
        {
            if (item._groupStartLevel != _uiTopLevel)
            {
                item._groupStartLevel.DontSetAsActive = true;
                item._groupStartLevel.ActivateINTweens();
            }
            else
            {
                _uiTopLevel.MoveToNextLevel(_uiTopLevel);
            }
        }
    }

    private void ActivateMouse()
    {
        if (!_canStart) return;

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
        CleaAndSetRootGroup(uiObject);
        _uiElementLastSelected = uiObject;
        _activeBranch = _uiElementLastSelected.MyBranchController;
        EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
    }

    private void CleaAndSetRootGroup(UINode uiObject)
    {
        int tempIndexStore = _groupIndex;
        if (SetRootGroup(uiObject.MyBranchController))
        {
            _groupList[tempIndexStore]._groupStartLevel.LastSelected.DisableLevel();
            _groupList[tempIndexStore]._groupStartLevel.LastSelected.SetNotHighlighted();
        }
    }

    private void SwitchRootGroups()
    {
        _uiElementLastSelected._audio.Play(UIEventTypes.Selected, _uiElementLastSelected._functionToUse);
        _uiElementLastSelected.SetNotHighlighted();

        _groupList[_groupIndex]._groupStartLevel.LastSelected.DisableLevel();
        _groupIndex++;
        if (_groupIndex > _groupList.Length - 1)
        {
            _groupIndex = 0;
        }
        _groupList[_groupIndex]._groupStartLevel.DontAnimateOnChange = true;
        _groupList[_groupIndex]._groupStartLevel.MoveToNextLevel();
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
        _uiElementLastSelected.MyBranchController.StartOutTweens();
        _uiElementLastSelected._audio.Play(UIEventTypes.Cancelled, _uiElementLastSelected._functionToUse);
        RestoreFromFullScreen();
        _groupList[_groupIndex]._groupStartLevel.LastSelected.DisableLevel();
        _groupList[_groupIndex]._groupStartLevel.MoveBackALevel();
    }

    private void BackOneLevel()
    {
        if (!_onHomeScreen && IsItPartOfRootMenu(_uiElementLastSelected.MyParentController))
        {
            RestoreFromFullScreen();
        }

        if (_uiElementLastSelected.MyParentController)
        {
            _uiElementLastSelected.OnCancel();
        }
    }

    public void ToFullScreen(UIBranch currentBranch)
    {
        if (!_onHomeScreen) return;

        foreach (var item in _allUIBranches)
        {
            if (currentBranch != item)
            {
                item.MyCanvas.enabled = false;
                _onHomeScreen = false;
            }
        }
    }

    private void RestoreFromFullScreen()
    {
        foreach (var item in _groupList)
        {
            _onHomeScreen = true;
            item._groupStartLevel.RestoreFromFullscreen();
        }
    }

    private void ProcessGameToMenuSwitching()
    {
        InMenu = !InMenu;
        _uiElementLastSelected.SetNotHighlighted();
        EventSystem.current.SetSelectedGameObject(null);
        _returnToGameControl.Invoke(InMenu);

        foreach (var item in _allUIBranches)
        {
            item._myCanvasGroup.blocksRaycasts = InMenu;
        }
    }

    private bool IsItPartOfRootMenu(UIBranch uIBranch)
    {
        foreach (var item in _groupList)
        {
            if (item._groupStartLevel == uIBranch)
            {
                return true;
            }
        }
        return false;
    }

    private bool SetRootGroup(UIBranch uIBranch)
    {
        for (int i = 0; i < _groupList.Length; i++)
        {
            if (_groupList[i]._groupStartLevel == uIBranch)
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
