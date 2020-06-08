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
    [SerializeField] [ReorderableList] List<HotKeys> _hotKeySettings;

    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }

    //Variables
    int _groupIndex = 0;
    UIBranch[] _allUIBranches;
    List<UIBranch> _newHomeBranches = new List<UIBranch>();
    UINode _lastHighlighted;
    UINode _lastSelected;
    UINode _lastRootNode;
    Vector3 _mousePos = Vector3.zero;
    bool _usingMouse = false;
    bool _usingKeysOrCtrl = false;
    bool _onHomeScreen = false;
    bool _canStart = false;

    //Properties
    bool InMenu { get; set; } = true;
    public UIBranch ActiveBranch { get; set; }
    public UINode LastSelected { get { return _lastSelected; } }

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
        foreach (var item in _allUIBranches)
        {
            if (item.MyBranchType == BranchType.HomeScreenUI)
            {
                item.MyCanvas.enabled = true;
            }
            else
            {
                item.MyCanvas.enabled = false;
            }
        }

        _lastHighlighted = _homeBranches[0].DefaultStartPosition;
        //_lastSelected = _homeBranches[0].DefaultStartPosition;
        ActiveBranch = _homeBranches[0];
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
            EventSystem.current.SetSelectedGameObject(_lastHighlighted.gameObject);
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

        foreach (var item in _hotKeySettings)
        {
            if(item.CheckHotkeys())
            {
                if (_inGameMenuSystem)
                {
                    ProcessGameToMenuSwitching();
                }
                return;
            }
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
                    if (_lastSelected._navigation._childBranch != null)
                    {
                       OnCancel(_lastSelected._navigation._childBranch.EscapeKeySetting);
                    }
                }
                else if (Input.GetButtonDown(_branchSwitchButton))
                {
                    if (_onHomeScreen)
                    {
                        SwitchRootGroups();
                    }
                    else
                    {
                        ActiveBranch.SwitchGroup();
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
            _lastHighlighted.SetNotHighlighted();

            foreach (var item in _allUIBranches)
            {
                item.AllowKeys = false;
            }
        }
    }

    public void SetLastSelected(UINode node)
    {
        if (_lastSelected != null)
        {
            if (node != _lastSelected)
            {
                if (_onHomeScreen)
                {
                    UINode temp = TurnOffLastSelected(node);
                    _lastRootNode.SetNotHighlighted();
                    _lastRootNode = temp;
                }
                else
                {
                    if (_lastSelected._navigation._childBranch != null)
                    {
                        //if (_lastSelected._navigation._childBranch.ScreenType == ScreenType.FullScreen_Internal)
                        if (_lastSelected._navigation._childBranch.MyBranchType == BranchType.Internal)
                        {
                            _lastSelected._navigation._childBranch.StartOutTweens(false);
                            _lastSelected.SetNotSelected_NoEffects();
                        }
                    }
                }
            }
        }
        else
        {
            _lastRootNode = node;
        }
        _lastSelected = node;
    }

    private UINode TurnOffLastSelected(UINode node)
    {
        UINode temp = node;
        while (temp.MyBranchController != temp.MyBranchController.MyParentController)
        {
            temp = temp.MyBranchController.MyParentController.LastSelected;
        }

        if (temp != _lastRootNode && _lastRootNode.IsSelected == true)
        {
            _lastRootNode.OnPointerDown();
        }

        return temp;
    }

    public void SetLastHighlighted(UINode newNode)
    {
        _lastHighlighted.SetNotHighlighted();
        _lastHighlighted = newNode;
        ActiveBranch = _lastHighlighted.MyBranchController;
        SetRootGroup(newNode.MyBranchController);
        EventSystem.current.SetSelectedGameObject(_lastHighlighted.gameObject);
    }

    private void SwitchRootGroups()
    {
        _lastHighlighted._audio.Play(UIEventTypes.Selected, _lastHighlighted._functionToUse);
        _lastHighlighted.SetNotHighlighted();

        _groupIndex++;
        if (_groupIndex > _homeBranches.Count - 1)
        {
            _groupIndex = 0;
        }
        if (_lastSelected != null)
        {
            TurnOffLastSelected(_homeBranches[_groupIndex].LastSelected);
            _lastSelected.Deactivate();
            _lastSelected.SetNotHighlighted();
        }
        _homeBranches[_groupIndex].TweenOnChange = false;
        _homeBranches[_groupIndex].MoveToNextLevel();
    }

    private void ActivateKeysOrControl()
    {
        if (!_canStart) return;

        if (!Input.GetMouseButton(0) & !Input.GetMouseButton(1))
        {
            EventSystem.current.SetSelectedGameObject(_lastHighlighted.gameObject);

            if (_usingKeysOrCtrl == false)
            {
                _usingKeysOrCtrl = true;
                _usingMouse = false;
                _lastHighlighted.SetAsHighlighted();

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
            BackToHomeScreen();
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
            BackToHomeScreen();
        }
    }
    private void BackToHomeScreen()
    {
        _lastSelected._navigation._childBranch.StartOutTweens(false, ()=> EndOfBackToHome());
        _lastHighlighted._audio.Play(UIEventTypes.Cancelled, _lastHighlighted._functionToUse);
    }

    private void EndOfBackToHome()
    {
        _homeBranches[_groupIndex].LastHighlighted.SetNotHighlighted();
        _homeBranches[_groupIndex].LastSelected.Deactivate();
        _homeBranches[_groupIndex].MoveToNextLevel();
    }

    private void BackOneLevel()
    {
        if (_lastSelected._navigation._childBranch.MoveToNext == MoveNext.AtTweenEnd)
        {
            _lastSelected._navigation._childBranch.StartOutTweens(false, () => GoingBackProcess());
        }
        else
        {
            _lastSelected._navigation._childBranch.StartOutTweens(false);
            GoingBackProcess();
        }
    }

    private void GoingBackProcess()
    {
        if (_lastSelected.IsSelected != false)
        {
            _lastSelected.OnPointerDown();
            if (_lastSelected.MyBranchController.DontTurnOff || 
                _lastSelected._navigation._childBranch.MyBranchType == BranchType.Internal)
            {
                _lastSelected.MyBranchController.TweenOnChange = false;
            }
            _lastSelected.MyBranchController.MoveToNextLevel();
            _lastSelected = _lastSelected.MyBranchController.MyParentController.LastSelected;
        }
    }

    public void ClearHomeScreen(UIBranch ignoreBranch)
    {
        if (!_onHomeScreen) return;
        _onHomeScreen = false;

        foreach (var branch in _homeBranches)
        {
            if (branch != ignoreBranch)
            {
                branch.LastSelected.Deactivate();
                branch.MyCanvas.enabled = false;
            }
            branch.LastHighlighted.SetNotHighlighted();
        }
    }

    public void RestoreHomeScreen()
    {
        if (!_onHomeScreen)
        {
            foreach (var item in _homeBranches)
            {
                _onHomeScreen = true;
                item.ResetHomeScreen(_lastSelected.MyBranchController);
            }
        }
    }

    private void ProcessGameToMenuSwitching()
    {
        if (!_usingMouse)
        {
            if (InMenu)
            {
                InMenu = false;
                _lastHighlighted.SetNotHighlighted();
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                InMenu = true;
                _lastHighlighted.SetAsHighlighted();
                EventSystem.current.SetSelectedGameObject(_lastHighlighted.gameObject);
            }

            _returnToGameControl.Invoke(InMenu);
        }    
    }

    private void SetRootGroup(UIBranch uIBranch)
    {
        for (int i = 0; i < _homeBranches.Count; i++)
        {
            if (_homeBranches[i] == uIBranch)
            {
                if (i != _groupIndex)
                {
                    _groupIndex = i;
                }
            }
        }
    }
}
