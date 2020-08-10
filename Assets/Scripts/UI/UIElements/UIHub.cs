using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

/// <summary>
/// UIHub is the core of the system and looks after shared functionality as well as tracking current UI state
/// </summary>

[RequireComponent(typeof(AudioSource))]

public partial class UIHub : MonoBehaviour
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
    [SerializeField] UIBranch _pauseMenu;
    [Header("Home Branch Switch Settings")]
    [SerializeField] [HideIf("MouseOnly")] [InputAxis] string _posSwitchButton;
    [SerializeField] [HideIf("MouseOnly")] [InputAxis] string _negSwitchButton;
    [SerializeField] [HideIf("MouseOnly")] [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")] 
    List<UIBranch> _homeBranches;
    [SerializeField] [ReorderableList] [Label("Hot Keys (CAN'T have PopUps as Hot keys)")] 
    List<HotKeys> _hotKeySettings;
    [SerializeField] private MenuAndGameSwitching _menuAndGameSwitching = new MenuAndGameSwitching();

    //Events
    //public static event Action OnEndOfUse;
    public static event Action OnStart;

    //Variables
    private UINode _lastHomeScreenNode;
    private UIAudioManager _uiAudio; 
    private UICancel _myUiCancel;
    private UIHomeGroup _uiHomeGroup;
    private PopUpController _popUpController;
    private ChangeControl _changeControl;
    private UIData _uiData;
    private bool _hasPauseAxis, _hasPosSwitchAxis, _hasNegSwitchAxis, _hasCancelAxis, _hasSwitchToMenuAxis;
    private bool _onHomeScreen;
    private bool _inMenu;

    //Properties
    private bool StartingInGame => _menuAndGameSwitching.ActiveInGameSystem 
                                      && _menuAndGameSwitching.StartInGame;
    private UIBranch[] AllBranches { get; set; }
    private bool MouseOnly()
    {
        if(_mainControlType == ControlMethod.Mouse) _menuAndGameSwitching.TurnOffGameSwitchSystem();
        return _mainControlType == ControlMethod.Mouse;
    }

    private void Awake()
    {
        AllBranches = FindObjectsOfType<UIBranch>();
        CheckForControls();
        CreateSubClasses();

        foreach (UIBranch branch in AllBranches)
        {
            branch.OnAwake(_uiHomeGroup);
        }
    }

    private void CreateSubClasses()
    {
        _popUpController = new PopUpController();
        _changeControl = new ChangeControl(_mainControlType, _popUpController);
        _uiAudio = new UIAudioManager(GetComponent<AudioSource>());
        _uiHomeGroup = new UIHomeGroup(_homeBranches.ToArray(), AllBranches);
        _myUiCancel = new UICancel( _globalCancelFunction, _popUpController);
        _uiData = new UIData();
        _menuAndGameSwitching.OnAwake();
        
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake();
        }
    }

    private void CheckForControls()
    {
        _hasPauseAxis = _pauseOptionButton != string.Empty;
        _hasPosSwitchAxis = _posSwitchButton != string.Empty;
        _hasNegSwitchAxis = _negSwitchButton != string.Empty;
        _hasCancelAxis = _cancelButton != string.Empty;
        _hasSwitchToMenuAxis = _menuAndGameSwitching.HasSwitchControls();
    }

    private void OnEnable()
    {
        _uiData.SubscribeToHighlightedNode(SetLastHighlighted);
        _uiData.SubscribeToSelectedNode(SetLastSelected);
        _uiData.SubscribeToActiveBranch(SaveActiveBranch);
        _uiData.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiData.SubscribeToInMenu(SaveInMenu);
    }

    private void OnDisable()
    {
        _uiAudio.OnDisable();
        _myUiCancel.OnDisable();
        _popUpController.OnDisable();
        _changeControl.OnDisable();
        _uiData.OnDisable();
    }
    
    private void Start()
    {
        SetStartPositionsAndSettings();
        CheckIfStartingInGame();
        StartCoroutine(EnableStartControls());
        OnStart?.Invoke();
    }

    private void SetStartPositionsAndSettings()
    {
        LastHighlighted = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.ThisNodeIsHighLighted();
        LastSelected = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.ThisNodeIsSelected();
        _popUpController.SetLastNodeBeforePopUp(_homeBranches[0].DefaultStartPosition);
    }

    private void CheckIfStartingInGame()
    {
        if (_menuAndGameSwitching.CanStartInGame)
        {
            ActivateAllHomeBranches(IsActive.Yes);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
            ActivateAllHomeBranches(IsActive.No);
        }
    }

    private void ActivateAllHomeBranches(IsActive activateOnStart)
    {
        foreach (var branch in _homeBranches)
        {
            if (activateOnStart == IsActive.Yes && branch == _homeBranches[0])
            {
                branch.MoveToThisBranch();
            }
            else
            {
                branch.MoveToThisBranchDontSetAsActive();
            }
        }
    }

    private IEnumerator EnableStartControls()
    {
        _changeControl.StartGame(StartingInGame);
        yield return new WaitForSeconds(_atStartDelay);
        CanStart = true;
        
        foreach (var homeBranch in _homeBranches)
        {
            homeBranch.ActivateBranch();
        }
    }

    private void Update()
    {
        if (!CanStart) return;
        if (CanPauseGame())
        {
            PauseOptionMenuPressed();
            return;
        }

        if (CanSwitchBetweenInGameAndMenu())
        {
            _menuAndGameSwitching.SwitchBetweenGameAndMenu();
            return;
        }

        if (CheckIfHotKeyAllowed()) return;

        if (_inMenu) InMenuControls();
    }
    
    private void InMenuControls()
    {
        if (CanDoCancel())
        {
            WhenCancelPressed();
            return;
        }

        if (CanSwitchBranches() && SwitchGroupProcess()) return;

        _changeControl.ChangeControlType();
    }
}


