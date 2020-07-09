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

public class UIHub : MonoBehaviour
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] private ControlMethod _mainControlType = ControlMethod.Mouse;
    [SerializeField] [Label("Enable Controls After..")] float _atStartDelay;
    [Header("Cancel Settings")]
    [SerializeField] [InputAxis] string _cancelButton;
    [SerializeField] [ValidateInput("ProtectEscapeKeySetting", "Can't set Global Settings to Global Settings")]
    EscapeKey _globalCancelFunction = EscapeKey.BackOneLevel;
    [SerializeField] [Label("Nothing to Cancel")] 
    PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    [Header("Pause Settings")]
    [SerializeField] [Label("Pause / Option Button")] [InputAxis] string _pauseOptionButton;
    [SerializeField] [Label("Pause / Option Menu")] UIBranch _pauseOptionMenu;
    [Header("Home Branch Switch Settings")]
    [SerializeField] [HideIf("MouseOnly")] [InputAxis] string _posSwitchButton;
    [SerializeField] [HideIf("MouseOnly")] [InputAxis] string _negSwitchButton;
    [SerializeField] [HideIf("MouseOnly")] [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")] 
    List<UIBranch> _homeBranches;
    [SerializeField] [ReorderableList] [Label("Hot Keys (CAN'T have PopUps as Hot keys)")] 
    List<HotKeys> _hotKeySettings;
    [Header("In-Game Menu Settings")] 
    [SerializeField] [HideIf("MouseOnly")] InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] [ShowIf("ActiveInGameSystem")] StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] [ShowIf("ActiveInGameSystem")] [Label("Switch To/From Game Menus")] 
    [InputAxis] string _switchToMenusButton;
    [SerializeField] [ShowIf("ActiveInGameSystem")] InGameOrInMenu _returnToGameControl;
    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }

    //Events
    public static event Action<bool> IsPaused; // Subscribe To to trigger pause operations

    //Variables
    private UINode _lastHomeScreenNode;
    private UIAudioManager _uiAudio; //Used to hold instance of class
    private UICancel _myUiCancel;
    private bool _hasPauseAxis;
    private bool _hasPosSwitchAxis;
    private bool _hasNegSwitchAxis;
    private ChangeControl _changeControl;
    private bool _hasCancelAxis;
    private bool _hasToMenuAxis;
    bool activatedHotKey;

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
    public List<UIBranch> ActivePopUpsResolve { get; } = new List<UIBranch>();
    public List<UIBranch> ActivePopUpsNonResolve { get; } = new List<UIBranch>();
    public int PopIndex { get; set; }
    public UINode LastNodeBeforePopUp { get; private set; }
    private bool ActiveInGameSystem => _inGameMenuSystem == InGameSystem.On;
    private UIHomeGroup UiHomeGroup { get; set; }
    public bool NoActivePopUps => ActivePopUpsResolve.Count == 0
                                   & ActivePopUpsNonResolve.Count == 0;
    public PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    public bool InMenu { get; private set; } = true;
    public bool CanStart { get; private set; }
    private bool MouseOnly()
    {
        if(_mainControlType == ControlMethod.Mouse) _inGameMenuSystem = InGameSystem.Off;
        return _mainControlType == ControlMethod.Mouse;
    }

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
        CheckForControls();
        AllBranches = FindObjectsOfType<UIBranch>();
        CreateSubClasses();

        foreach (UIBranch branch in AllBranches)
        {
            branch.OnAwake(this, UiHomeGroup);
        }
    }

    private void CreateSubClasses()
    {
        _changeControl = new ChangeControl(this, _mainControlType);
        _changeControl.AllowKeyClasses = FindObjectsOfType<UIBranch>();
        _uiAudio = new UIAudioManager(GetComponent<AudioSource>());
        _myUiCancel = new UICancel(this);
        UiHomeGroup = new UIHomeGroup(this);
    }

    private void CheckForControls()
    {
        _hasPauseAxis = _pauseOptionButton != string.Empty;
        _hasPosSwitchAxis = _posSwitchButton != string.Empty;
        _hasNegSwitchAxis = _negSwitchButton != string.Empty;
        _hasCancelAxis = _cancelButton != string.Empty;
        _hasToMenuAxis = _switchToMenusButton != string.Empty;
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
        LastNodeBeforePopUp = LastHighlighted;
        ActiveBranch = _homeBranches[0];
        
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake(this, UiHomeGroup);
        }
        OnHomeScreen = true;
        _returnToGameControl.Invoke(InMenu);

        if (ActiveInGameSystem && _startGameWhere == StartInMenu.InGameControl)
        {
            InMenu = true;
            GameToMenuSwitching();
            _changeControl.StartGame();
            IntroAnimations(true);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
            IntroAnimations(false);
        }
        StartCoroutine(StartDelay());
    }

    private void IntroAnimations(bool blockAll)
    {
        foreach (UIBranch homeBranch in _homeBranches)
        {
            homeBranch.MyCanvasGroup.blocksRaycasts = false;
            if (blockAll)
                _homeBranches[0].DontSetAsActive = true;
            
            if ( homeBranch != _homeBranches[0])
            {
                homeBranch.DontSetAsActive = true;
            }
            homeBranch.MoveToNextLevel();
        }
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(_atStartDelay);
        CanStart = true;
        if (!ActiveInGameSystem || _startGameWhere != StartInMenu.InGameControl)
            _changeControl.StartGame();
        
        if (_mainControlType == ControlMethod.KeysOrController)
        {
            foreach (var branch in AllBranches)
            {
                branch.MyCanvasGroup.blocksRaycasts = false;
            }
            _changeControl.UsingKeysOrCtrl = true;
            _changeControl.SetAllowKeys();
        }
        else
        {
            foreach (var homeBranch in _homeBranches)
            {
                homeBranch.MyCanvasGroup.blocksRaycasts = true;
            }
        }
    }

    private void Update() 
    {
        if (!CanStart) return;
        if (_hasPauseAxis)
        {
            if (Input.GetButtonDown(_pauseOptionButton))
            {
                PauseOptionMenu();
                return;
            }
        }

        if (_hasToMenuAxis)
        {
            if (Input.GetButtonDown(_switchToMenusButton) && NoActivePopUps)
            {
                GameToMenuSwitching();
                return;
            }
        }

        if (HotKeys())
        {
            return;
        }

        if (InMenu)
        {
            HandleInMenu();
        }
    }

    private bool HotKeys()
    {
        //Checks
        if (_hotKeySettings.Count <= 0) return false;
        if (ActivePopUpsResolve.Count > 0 || GameIsPaused) return false;
        if (_changeControl.UsingKeysOrCtrl && !NoActivePopUps) return false;

        activatedHotKey = _hotKeySettings.Any(hotKeys => hotKeys.CheckHotKeys());
        if (activatedHotKey)
        {
            if (!InMenu) GameToMenuSwitching();
        }
        return activatedHotKey;
    }

    public void PauseOptionMenu()
    {
        _pauseOptionMenu.PauseMenuClass.PauseMenu();
        IsPaused?.Invoke(GameIsPaused);
    }
    
    private void HandleInMenu()
    {
        if (_hasCancelAxis)
        {
            if (Input.GetButtonDown(_cancelButton))
            {
                _myUiCancel.CancelPressed();
                return;
            }
        }

        if (CanSwitchBranches())
        {
            if (_hasPosSwitchAxis)
            {
                if (Input.GetButtonDown(_posSwitchButton))
                {
                    SwitchingGroups(SwitchType.Positive);
                    return;
                }
            }

            if (_hasNegSwitchAxis)
            {
                if (Input.GetButtonDown(_negSwitchButton))
                {
                    SwitchingGroups(SwitchType.Negative);
                    return;
                }
            }
            _changeControl.ChangeControlType();
        }
    }

    private bool CanSwitchBranches()
    {
        return ActivePopUpsResolve.Count == 0 && !MouseOnly();
    }

    public void GameToMenuSwitching() //TODO Review in light of popUps, pause should work and returns to any popups first
    {
        if (MouseOnly()) return;
        if(!ActiveInGameSystem ) return;
        
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

    private void SwitchingGroups(SwitchType switchType)
    {
        if (ActivePopUpsNonResolve.Count > 0)
        {
            HandleActivePopUps();
        }
        else if (OnHomeScreen && _homeBranches.Count > 1)
        {
            LastHighlighted.Audio.Play(UIEventTypes.Selected);
            UiHomeGroup.SwitchHomeGroups(switchType);
        }
        else if (ActiveBranch.GroupListCount > 1)
        {
            ActiveBranch.SwitchBranchGroup(switchType);
        }
    }

    public void HandleActivePopUps()
    {
        int groupLength = ActivePopUpsNonResolve.Count;
        SetLastHighlighted(ActivePopUpsNonResolve[PopIndex].LastHighlighted);
        ActivePopUpsNonResolve[PopIndex].LastHighlighted.SetNodeAsActive();
        PopIndex = PopIndex.PositiveIterate(groupLength);
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
        if(!LastSelected.ChildBranch) return; //Stops Tween Error when no child
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
        if (!newNode.MyBranch.IsAPopUpBranch() && !GameIsPaused)         //Todo Check Pause might need it
        {
            LastNodeBeforePopUp = newNode;
        }
        LastHighlighted.SetNotHighlighted();
        LastHighlighted = newNode;
        ActiveBranch = LastHighlighted.MyBranch;
        if (OnHomeScreen) 
            UiHomeGroup.SetHomeGroupIndex(LastHighlighted.MyBranch);
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }
}
