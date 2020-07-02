using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Linq;
using JetBrains.Annotations;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]

public class UIHub : MonoBehaviour, IHubData
{
    [Header("Main Settings")]
    [SerializeField] [ValidateInput("ProtectEscapeKeySetting", "Can't set Global Settings to Global Settings")] 
    EscapeKey _globalCancelFunction = EscapeKey.BackOneLevel;
    [SerializeField] private ControlMethod _mainControlType = ControlMethod.Mouse;
    [SerializeField] [InputAxis] string _cancelButton;
    [SerializeField] [HideIf("MouseOnly")] [InputAxis] string _branchSwitchButton;
    [SerializeField] [Label("Pause / Option Button")] [InputAxis] string _pauseOptionButton;
    [SerializeField] [Label("Pause / Option Menu")] UIBranch _pauseOptionMenu;
    [SerializeField] [Label("No Cancel Action")] PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.Nothing;
    [SerializeField] [Label("Enable Controls After..")] float _atStartDelay;
    [Header("In-Game Menu Settings")]
    [SerializeField] [HideIf("MouseOnly")]  InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] [ShowIf("ActiveInGameSystem")] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [ShowIf("ActiveInGameSystem")] [Label("Switch To/From Game Menus")] [InputAxis] string _switchToMenusButton;
    [SerializeField] [ShowIf("ActiveInGameSystem")] InGameOrInMenu _returnToGameControl;
    [SerializeField] [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")] List<UIBranch> _homeBranches;
    [SerializeField] [ReorderableList] [Label("Hot keys (CAN'T have Independents as Hot keys)")] List<HotKeys> _hotKeySettings;

    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }
    
    //Events
    public static event Action<bool> IsPaused; // Subscribe To to trigger pause operations

    //Variables
    private UINode _lastHomeScreenNode;
    private bool _canStart;
    private bool _inMenu = true;
    private UIAudioManager _uiAudio; //Used to hold instance of class
    private UICancel _myUiCancel;
    private bool _isPauseOptionMenuNotNull;
    private bool _canBranchSwitch;
    private IChangeControl _changeControl;

    private enum InGameSystem { On, Off }
    
    //Properties
    public UIBranch ActiveBranch { get; set; }
    public EscapeKey GlobalEscape => _globalCancelFunction;
    public UINode LastSelected { get; private set; }
    public UINode LastHighlighted { get; private set; }
    public bool OnHomeScreen { get; set; }
    public bool GameIsPaused { get; set; }
    public List<UIBranch> HomeGroupBranches => _homeBranches;
    public UIBranch[] AllBranches { get; private set; }
    public int GroupIndex { get; set; } = 0;
    public List<UIBranch> ActivePopUps_Resolve { get; } = new List<UIBranch>();
    public List<UIBranch> ActivePopUps_NonResolve { get; } = new List<UIBranch>();
    public int PopIndex { get; set; }
    public UINode LastNodeBeforePopUp { get; set; }
    private bool ActiveInGameSystem => _inGameMenuSystem == InGameSystem.On;
    public UIHomeGroup UIHomeGroup { get; private set; }
    private bool NoActivePopUps => ActivePopUps_Resolve.Count == 0
                                   & ActivePopUps_NonResolve.Count == 0;
    public PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    private bool MouseOnly => _mainControlType == ControlMethod.Mouse;

    //Editor Scripts
    #region Editor Scripts

    // ReSharper disable once UnusedMember.Local
    private bool ProtectEscapeKeySetting(EscapeKey escapeKey)
    {
        if (_globalCancelFunction == EscapeKey.GlobalSetting)
        {
            Debug.Log("Escape KeyError");
        }
        return escapeKey != EscapeKey.GlobalSetting;
    }
    
    [Button("Add a New Home Branch Folder")]
    // ReSharper disable once UnusedMember.Local
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
        _isPauseOptionMenuNotNull = _pauseOptionMenu != null;
        AllBranches = FindObjectsOfType<UIBranch>();
        _returnToGameControl.Invoke(_inMenu);
        _changeControl = new ChangeControl(this, _cancelButton, _branchSwitchButton, _mainControlType);
        _uiAudio = new UIAudioManager(GetComponent<AudioSource>());
        _myUiCancel = new UICancel(this);
        UIHomeGroup = new UIHomeGroup(this);
        foreach (UIBranch branch in AllBranches)
        {
            branch.OnAwake(this, this, UIHomeGroup);
        }
    }

    private void OnEnable()
    {
        UINode.DoCancel += _myUiCancel.CancelOrBack;
    }

    private void OnDisable()
    {
        UINode.DoCancel -= _myUiCancel.CancelOrBack;
    }

    private void Start()
    {
        LastHighlighted = _homeBranches[0].DefaultStartPosition;
        LastSelected = _homeBranches[0].DefaultStartPosition;
        ActiveBranch = _homeBranches[0];
        _canBranchSwitch = _branchSwitchButton != string.Empty;

        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake(this, UIHomeGroup);
        }
        OnHomeScreen = true;
        IntroAnimations();

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

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(_atStartDelay);
        _canStart = true;
        var temp = FindObjectsOfType<MonoBehaviour>().OfType<IAllowKeys>().ToArray();
        _changeControl.StartGame(temp);
    }

    private void Update() // Review how keys work as hot keys and pause shouldn't activate keyboard. Maybe hard setting rather than auto
    {
        if (!_canStart) return;

        if (Input.GetButtonDown(_pauseOptionButton))
        {
            if (_isPauseOptionMenuNotNull) PauseOptionMenu();
        }

        if (NoActivePopUps && ActiveInGameSystem && Input.GetButtonDown(_switchToMenusButton))
        {
            GameToMenuSwitching();
        }

        if (NoActivePopUps && _hotKeySettings.Count > 0)
        {
            foreach (var item in _hotKeySettings)
            {
                if (!item.CheckHotKeys()) continue;
                if (ActiveInGameSystem) GameToMenuSwitching();
                return;
            }
        }

        if (_inMenu)
        {
            HandleInMenu();
        }
    }
    
    private void HandleInMenu()
    {
        _changeControl.ChangeControlType();

        if (Input.anyKeyDown)
        {
            if (Input.GetButtonDown(_cancelButton))
            {
                _myUiCancel.CancelPressed();
            }
            else if (CanSwitchBranches() && !MouseOnly)
            {
                SwitchingGroups();
            }
        }
    }

    public void PauseOptionMenu()
    {
        _pauseOptionMenu.IsPauseMenu.PauseMenu();
        IsPaused?.Invoke(GameIsPaused);
    }

    private void GameToMenuSwitching() //TODO Review in light of popUps, pause should work and returns to any popups first
    {
        if (MouseOnly) return;
        
        if (_inMenu)
        {
            _inMenu = false;
            LastHighlighted.SetNotHighlighted();
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            _inMenu = true;
            LastHighlighted.SetAsHighlighted();
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
        }
        _returnToGameControl.Invoke(_inMenu);
    }


    private bool CanSwitchBranches()
    {
        return _canBranchSwitch && Input.GetButtonDown(_branchSwitchButton) && ActivePopUps_Resolve.Count == 0;
    }

    private void SwitchingGroups()
    {
        if (ActivePopUps_NonResolve.Count > 0)
        {
            HandleActivePopUps();
        }
        else if (OnHomeScreen && _homeBranches.Count > 1)
        {
            LastHighlighted.IAudio.Play(UIEventTypes.Selected);
            UIHomeGroup.SwitchHomeGroups();
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
        ActivePopUps_NonResolve[PopIndex].LastHighlighted.SetNodeAsActive();
        PopIndex = PopIndex.Iterate(groupLength);
    }
    
    public void SetLastSelected(UINode newNode)
    {
        if (_lastHomeScreenNode == null) _lastHomeScreenNode = newNode;
        if (LastSelected == newNode) return;

        if (OnHomeScreen)
        {
            WhenOnHomeScreen(newNode);
        }
        else
        {
            WhenNotOnHomeScreen();
        }
        LastSelected = newNode;
    }

    private void WhenNotOnHomeScreen()
    {
        if (LastSelected.ChildBranch.MyBranchType == BranchType.Internal)
        {
            LastSelected.Deactivate();
        }
    }

    private void WhenOnHomeScreen([NotNull] UINode newNode)
    {
        if (newNode.MyBranch.IsAPopUpBranch() || newNode.MyBranch.IsPause()) return;

        while (newNode.MyBranch != newNode.MyBranch.MyParentBranch)
        {
            newNode = newNode.MyBranch.MyParentBranch.LastSelected;
        }

        if (newNode != _lastHomeScreenNode && _lastHomeScreenNode.IsSelected)
        {
            _lastHomeScreenNode.Deactivate();
        }
        _lastHomeScreenNode = newNode;
    }

    public void SetLastHighlighted(UINode newNode)
    {
        if (newNode == LastHighlighted) return;
        if (!newNode.MyBranch.IsAPopUpBranch())         //Todo Check Pause might need it
        {
            LastNodeBeforePopUp = newNode;
        }
        LastHighlighted.SetNotHighlighted();
        LastHighlighted = newNode;
        ActiveBranch = LastHighlighted.MyBranch;
        if (OnHomeScreen) UIHomeGroup.SetHomeGroupIndex(LastHighlighted.MyBranch);
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }
}
