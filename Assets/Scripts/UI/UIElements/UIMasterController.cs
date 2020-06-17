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
    [SerializeField] [ReorderableList] [Label("Hotkeys (CAN'T have Independents as Hotkeys)")] List<HotKeys> _hotKeySettings;

    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }
    public static event Action<bool> AllowKeys;

    //Variables
    int _groupIndex = 0;
    UINode _lastHighlighted;
    UINode _lastRootNode;
    Vector3 _mousePos = Vector3.zero;
    bool _usingMouse = false;
    bool _usingKeysOrCtrl = false;
    bool _canStart = false;

    //Properties
    bool InMenu { get; set; } = true;
    public UIBranch ActiveBranch { get; set; }
    public EscapeKey GlobalEscape { get { return _GlobalEscapeKeyFunction;} }
    public UINode LastSelected { get; set; }
    public bool OnHomeScreen { get; set; } = false;
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
        _returnToGameControl.Invoke(InMenu);
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
        _lastHighlighted = _homeBranches[0].DefaultStartPosition;
        LastSelected = _homeBranches[0].DefaultStartPosition;
        ActiveBranch = _homeBranches[0];
        _mousePos = Input.mousePosition;
        UIHomeGroup.homeGroup = _homeBranches;
        UIHomeGroup.uIMaster = this;
        UICancel.homeGroup = _homeBranches;
        UICancel.myMaster = this;
        HotKeyProcess.UIMaster = this;
        OnHomeScreen = true;
        IntroAnimations();

        if (_inGameMenuSystem && _startGameWhere == StartInMenu.InGameControl)
        {
            _canStart = true;
            GameToMenuSwitching();
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
            GameToMenuSwitching();
        }

        foreach (var item in _hotKeySettings)
        {
            if(item.CheckHotkeys())
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
                    ActiveBranch.FromHotkey = false;
                    LastSelected.SetNotSelected_NoEffects();
                    CancelOrBack(EscapeKey.BackToHome);
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
        foreach (var item in _homeBranches)
        {
            if (item != _homeBranches[0])
            {
                item.DontSetAsActive = true;
            }
            item.MoveToNextLevel();
        }
    }

    private void CancelOrBack(EscapeKey escapeKey)
    {
        if (ActiveBranch.FromHotkey)
        {
            ActiveBranch.FromHotkey = false;
            LastSelected.SetNotSelected_NoEffects();
            escapeKey = EscapeKey.BackToHome;
        }

        UICancel.CancelOrBackButton(escapeKey);
    }

    public void SetLastSelected(UINode node)
    {
        if (LastSelected != null && _lastRootNode != null)
        {
            if (node != LastSelected)
            {
                if (OnHomeScreen)
                {
                    UINode temp = DeactiveLastSelected(node);
                    _lastRootNode.SetNotHighlighted();
                    _lastRootNode = temp;
                }
                else
                {
                    if (LastSelected.ChildBranch != null)
                    {
                        if (LastSelected.ChildBranch.MyBranchType == BranchType.Internal)
                        {
                            LastSelected.SetNotHighlighted();
                            LastSelected.PressedActions();
                        }
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
        _lastHighlighted.SetNotHighlighted();
        _lastHighlighted = newNode;
        ActiveBranch = _lastHighlighted.MyBranch;
        if (OnHomeScreen)
        {
            UIHomeGroup.SetHomeGroupIndex(_lastHighlighted.MyBranch, ref _groupIndex);
        }
        EventSystem.current.SetSelectedGameObject(_lastHighlighted.gameObject);
    }

    private void SwitchHomeGroups()
    {
        _lastHighlighted._audio.Play(UIEventTypes.Selected, _lastHighlighted._enabledFunctions);
        UIHomeGroup.SwitchHomeGroups(ref _groupIndex);
    }

    private void ActivateMouse()
    {
        _mousePos = Input.mousePosition;

        if (_usingMouse == false)
        {
            _usingMouse = true;
            _usingKeysOrCtrl = false;
            _lastHighlighted.SetNotHighlighted();
            AllowKeys?.Invoke(false);
        }
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

    public void ResetHierachy()
    {
        UINode thisNode = LastSelected;

        while (thisNode.IsSelected == true)
        {
            thisNode.SetNotSelected_NoEffects();
            thisNode = thisNode.MyBranch.MyParentBranch.LastSelected;
        }
    }

}
