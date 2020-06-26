using System.Collections;
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
    [SerializeField] [ValidateInput("ProtectEscapekeySetting", "Can't set Global Settings to Global Settings")] 
    EscapeKey _globalCancelFunction;
    [SerializeField] [InputAxis] string _cancelButton = default;
    [SerializeField] [InputAxis] string _branchSwitchButton = default;
    [SerializeField] [Label("Pause / Option Button")] [InputAxis] string _pauseOptionButton = default;
    [SerializeField] [Label("Pause / Option Menu")] UIBranch _pauseOptionMenu = default;
    [SerializeField] [Label("No Cancel Action")] PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.Nothing;
    [SerializeField] [Label("Enable Controls After..")] float _atStartDelay = 0;
    [Header("In-Game Menu Settings")]
    [SerializeField] InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] [ShowIf("ActiveInGameSystem")] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [ShowIf("ActiveInGameSystem")] [Label("Switch To/From Game Menus")] [InputAxis] string _switchTOMenusButton;
    [SerializeField] [ShowIf("ActiveInGameSystem")] InGameOrInMenu _returnToGameControl = default;
    [SerializeField] [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")] List<UIBranch> _homeBranches;
    [SerializeField] [ReorderableList] [Label("Hotkeys (CAN'T have Independents as Hotkeys)")] List<HotKeys> _hotKeySettings;

    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }
    public static event Action<bool> AllowKeys;
    public static event Action<bool> IsPaused; // Subscrib To to trigger pause operations

    //Variables
    int _groupIndex = 0;
    UINode _lastRootNode = default;
    Vector3 _mousePos = Vector3.zero;
    bool _usingMouse = false;
    bool _usingKeysOrCtrl = false;
    bool _canStart = false;
    UIAudioManager _UIAudio = default;
    enum InGameSystem { On, Off }
    enum PauseOptionsOnEscape { EnterPauseOrEscape, Nothing }

    //Properties
    bool InMenu { get; set; } = true;
    public UIBranch ActiveBranch { get; set; }
    public EscapeKey GlobalEscape { get { return _globalCancelFunction; } }
    public UINode LastSelected { get; private set; }
    public UINode LastHighlighted { get; private set; }
    public bool OnHomeScreen { get; set; } = false;
    public bool GameIsPaused { get; private set; }
    public int GroupIndex { get { return _groupIndex; } set { _groupIndex = value; } }
    public List<UIBranch> ActivePopUps_Resolve { get; } = new List<UIBranch>();// Change to Methods
    public List<UIBranch> ActivePopUps_NonResolve { get; } = new List<UIBranch>(); // chnage To Methods
    public int PopIndex { get; set; }
    public UINode LastHomePosition { get { return _homeBranches[_groupIndex].LastHighlighted; } }
    public UINode LastNodeBeforePopUp { get; set; }
    public bool ActiveInGameSystem { get { return _inGameMenuSystem == InGameSystem.On; } }


    //Editor Scripts
    #region Editor Scripts

    private bool ProtectEscapekeySetting(EscapeKey escapeKey)
    {
        if (_globalCancelFunction == EscapeKey.GlobalSetting)
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

        if (ActiveInGameSystem && _startGameWhere == StartInMenu.InGameControl)
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

        if (Input.GetButtonDown(_pauseOptionButton))
        {
            PauseOptionMenu();
        }

        if (Input.GetButtonDown(_switchTOMenusButton) & ActiveInGameSystem)
        {
            GameToMenuSwitching();
        }

        if (ActivePopUps_Resolve.Count == 0)
        {
            foreach (HotKeys item in _hotKeySettings)
            {
                if (item.CheckHotkeys())
                {
                    if (ActiveInGameSystem)
                    {
                        GameToMenuSwitching();
                    }
                    return;
                }
            }
        }

        if (InMenu)
        {
            HandleInMenu();
        }
    }

    public void PauseOptionMenu()
    {
        if (GameIsPaused)
        {
            GameIsPaused = false;
            if (_pauseOptionMenu != null) 
                _pauseOptionMenu.IsPauseMenu.RestoreLastPosition();
        }
        else
        {
            GameIsPaused = true;
            if (_pauseOptionMenu != null) 
                _pauseOptionMenu.IsPauseMenu.StartPopUp();
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
                else if (GameIsPaused || ActiveBranch.IsAPopUpBranch())
                {
                    UICancel.CancelOrBackButton(EscapeKey.BackOneLevel);
                }
                else if (!UICancel.CanCancel())
                {
                    if (_pauseOptionsOnEscape == PauseOptionsOnEscape.EnterPauseOrEscape)
                    {
                        PauseOptionMenu();
                    }
                }
                else
                {
                    UICancel.Cancel();
                }
            }
            else if (CanSwitchBranches())
            {
                SwitchingGroups();
            }
            else
            {
                ActivateKeysOrControl();
            }
       }
    }

    private void SwitchingGroups()
    {
        if (ActivePopUps_NonResolve.Count > 0)
        {
            HandleActivePopUps();
        }
        else if (OnHomeScreen && _homeBranches.Count > 1)
        {
            SwitchHomeGroups();
        }
        else if (ActiveBranch.GroupListCount > 1)
        {
            ActiveBranch.SwitchBranchGroup();
        }
    }

    public void HandleActivePopUps()
    {
        int groupLength = ActivePopUps_NonResolve.Count;
        SetLastHighlighted(ActivePopUps_NonResolve[PopIndex].LastHighlighted);
        ActivePopUps_NonResolve[PopIndex].LastHighlighted.InitailNodeAsActive();
        PopIndex = PopIndex.Iterate(groupLength);
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

    public void SetLastSelected(UINode NewNode) //TODO Review as internal might be simplify
    {
        if (OnHomeScreen)
        {
            WhenOnHomeScreen(NewNode);
        }
        else
        {
            WhenNotOnHome(NewNode);
        }

        LastSelected = NewNode;
    }

    private void WhenNotOnHome(UINode newNode)
    {
        if (LastSelected != newNode)
        {
            if (LastSelected.ChildBranch != null)
            {
                if (LastSelected.ChildBranch.MyBranchType == BranchType.Internal)
                {
                    if (LastSelected.IsSelected == true) LastSelected.Deactivate();
                }
            }
        }
    }

    private void WhenOnHomeScreen(UINode node)
    {
        if (LastSelected != null && _lastRootNode != null)
        {
            UINode temp = DeactiveLastSelected(node);
            _lastRootNode = temp;
        }
        else
        {
            _lastRootNode = node;
        }
    }

    private UINode DeactiveLastSelected(UINode node)
    {
        if (node.MyBranch.IsAPopUpBranch()) return LastSelected; //***Review

        UINode temp = node;

        while (temp.MyBranch != temp.MyBranch.MyParentBranch)
        {
            temp = temp.MyBranch.MyParentBranch.LastSelected;
        }

        if (temp != _lastRootNode && _lastRootNode.IsSelected == true)
        {
            _lastRootNode.Deactivate();
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
                _groupIndex = UIHomeGroup.SetHomeGroupIndex(LastHighlighted.MyBranch);
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
                AllowKeys?.Invoke(true);

                if (ActivePopUps_Resolve.Count > 0 && OnHomeScreen)
                {
                    LastHighlighted.SetAsHighlighted();
                }
                else if (ActivePopUps_NonResolve.Count > 0 && OnHomeScreen)
                {
                    HandleActivePopUps();
                }
                else
                {
                    LastHighlighted.SetAsHighlighted();
                }
            }
        }
    }

    private void GameToMenuSwitching() //TODO Review in light of popUps, pause should work and returns to any popups first
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

    private bool CanSwitchBranches()
    {
        if (_branchSwitchButton != string.Empty)
        {
            if (Input.GetButtonDown(_branchSwitchButton) && ActivePopUps_Resolve.Count == 0)
            {
                return true;
            }
        }
        return false;
    }
}
