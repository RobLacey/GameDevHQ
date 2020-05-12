using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using NaughtyAttributes;
using UnityEngine.UI;

public class UITrunk : MonoBehaviour
{
    [Header("Main Settings")] 
    [Label("UI Starting Branch")] [Required("MUST have a starting branch")]
    [SerializeField] UIBranch _uiTopLevel;
    [SerializeField] EscapeKey _GlobalEscapeKeyFunction;
    [SerializeField] [InputAxis] string _cancelButton;
    [SerializeField] [InputAxis] string _changeMenuGroupButton;
    [Header("In-Game Menu Settings")]
    [SerializeField] bool _inGameMenuSystem = false;
    [SerializeField] [ShowIf("_inGameMenuSystem")] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [ShowIf("_inGameMenuSystem")] [Label("Switch To/From Game Menus")] [InputAxis] string _switchTOMenusButton;
    [SerializeField] [ShowIf("_inGameMenuSystem")] InGameOrInMenu _returnToGameControl;
    [Header("UI Groups")]
    [Label("List of UI Groups")] [Tooltip("Add a group if keyboard/Controller group switching is needed")] 
    [SerializeField] GroupList[] _groupList;

    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }

    //Variables
    int _groupIndex = 0;
    UIBranch[] _allUIBranches;
    UILeaf _uiElementLastSelected;
    Vector3 _mousePos = Vector3.zero;
    bool _usingMouse = false;

    public static bool InMenu { get; set; } = true; //***May not need to be static or even public

    public UIGroupID ActiveGroup { get; set; }
    enum EscapeKey { BackOneLevel, BackToRootLevel }
    enum StartInMenu { InMenu, InGameControl }

    [Serializable]
    public class GroupList
    {
        public UIGroupID _groupIDNumber;
        public UIBranch _groupStartLevel;
    }

    private void Awake()
    {
        _allUIBranches = FindObjectsOfType<UIBranch>();
        foreach (var item in _groupList)
        {
            item._groupStartLevel.MyUIGroup = item._groupIDNumber;
        }

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
        UILeaf.Canceller += OnCancel;
    }

    private void OnDisable()
    {
        UILeaf.Canceller -= OnCancel;
    }

    private void Start()
    {
        _uiElementLastSelected = _uiTopLevel.DefaultStartPosition;
        _mousePos = Input.mousePosition;

        if (InMenu)
        {
            EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
            ActivateKeysOrControl();
            StartMainMenusAnimation();
        }
    }

    private void Update()
    {
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
                    OnCancel();
                }
                else if (Input.GetButtonDown(_changeMenuGroupButton))
                {
                    SwitchControlGroups();
                }
                else
                {
                    ActivateKeysOrControl();
                }
            }
        }
    }

    private void StartMainMenusAnimation()
    {
        _uiTopLevel.MoveToNextLevel(_uiTopLevel);

        foreach (var item in _groupList)
        {
            if (item._groupStartLevel != _uiTopLevel)
            {
                item._groupStartLevel.IsCancelling = true;
                item._groupStartLevel.ActivateEffects();
            }
        }
    }

    private void ActivateMouse()
    {
        _mousePos = Input.mousePosition;

        if (_usingMouse == false)
        {
            _uiElementLastSelected.SetNotHighlighted();
            foreach (var item in _allUIBranches)
            {
                item.AllowKeys = false;
                _usingMouse = true;
            }
        }
    }

    private void SwitchControlGroups()
    {
        _uiElementLastSelected._audio.Play(UIEventTypes.Selected);
        _uiElementLastSelected.SetNotHighlighted();

        _groupIndex++;
        if (_groupIndex > _groupList.Length - 1)
        {
            _groupIndex = 0;
        }
        _groupList[_groupIndex]._groupStartLevel.DontTweenNow = true;
        _groupList[_groupIndex]._groupStartLevel.MoveToNextLevel();
    }

    private void ActivateKeysOrControl()
    {
        if (!Input.GetMouseButton(0) & !Input.GetMouseButton(1))
        {
            _uiElementLastSelected.SetAsHighlighted();
            EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);

            foreach (var item in _allUIBranches)
            {
                item.AllowKeys = true;
                _usingMouse = false;
            }
        }
    }

    public void SetLastUIObject(UILeaf uiObject, UIGroupID uIGroupID) 
    {
        if (uIGroupID != ActiveGroup)
        {
            RootCancelProcess();
            ActiveGroup = uIGroupID;

            for (int i = 0; i < _groupList.Length; i++) // sets groupindex to new selected group - Mouse
            {
                if(_groupList[i]._groupIDNumber == ActiveGroup)
                {
                    _groupIndex = i;
                    break;
                }
            }
        }
        _uiElementLastSelected = uiObject;
        EventSystem.current.SetSelectedGameObject(_uiElementLastSelected.gameObject);
    }

    private void OnCancel(UILeaf uILeaf = null)
    {

        //*******Cancel system needs overhaul**********
        //*******Make cancel one level key and root cancel key
        //*******need Hierachy cancel to work properly including when clicking on cancel button in level
        Debug.Log("Cancelling");
        Debug.Log(_uiElementLastSelected);
        _uiElementLastSelected.GetComponentInParent<UIBranch>().IsCancelling = true;

        if (_uiElementLastSelected.GetComponentInParent<UIBranch>().KillAllOtherUI)
        {
            RestoreFromFullScreen();
        }

        if (_uiElementLastSelected.EscapeKeyFunction == UILeaf.EscapeKey.BackOneLevel)
        {
            if (_uiElementLastSelected.MyParentController)
            {
                _uiElementLastSelected.OnCancel();
            }
        }
        else if (_uiElementLastSelected.EscapeKeyFunction == UILeaf.EscapeKey.BackToRootLevel)
        {
            UILeaf temp = RootCancelProcess();
            temp.GetComponentInParent<UIBranch>().MoveBackALevel();
            temp._audio.Play(UIEventTypes.Cancelled);
        }

        else if (_uiElementLastSelected.EscapeKeyFunction == UILeaf.EscapeKey.GlobalSetting)
        {
            UseGlobalEscapeSettings();
        }
    }

    private void UseGlobalEscapeSettings()
    {
        if (_GlobalEscapeKeyFunction == EscapeKey.BackOneLevel)
        {
            if (_uiElementLastSelected.MyParentController)
            {
                _uiElementLastSelected.OnCancel();
            }
        }

        if (_GlobalEscapeKeyFunction == EscapeKey.BackToRootLevel)
        {
            UILeaf temp = RootCancelProcess();
            temp.GetComponentInParent<UIBranch>().MoveBackALevel();
            temp._audio.Play(UIEventTypes.Cancelled);
        }
    }

    private UILeaf RootCancelProcess()
    {
        foreach (var item in _groupList)
        {
            if (item._groupIDNumber == ActiveGroup)
            {
                item._groupStartLevel.LastSelected.RootCancel();
                return item._groupStartLevel.LastSelected;
            }
        }
        return null;
    }

    public void ToFullScreen(UIBranch currentBranch)
    {
        foreach (var item in _allUIBranches)
        {
            if (currentBranch != item)
            {
                item.MyCanvas.enabled = false;
            }
        }
    }

    private void RestoreFromFullScreen()
    {
        foreach (var item in _groupList)
        {
            if (item._groupStartLevel == _uiElementLastSelected)
            {
                item._groupStartLevel.MoveBackALevel();
            }
            else
            {
                item._groupStartLevel.MyCanvas.enabled = true;
            }
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
            item.InteractiveAndVisability.blocksRaycasts = InMenu;
        }
    }
}
