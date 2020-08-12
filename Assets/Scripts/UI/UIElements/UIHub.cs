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
    [SerializeField] private ControlMethod _mainControlType = ControlMethod.MouseOnly;
    [SerializeField] [Label("Enable Controls After..")] float _atStartDelay;
    [Header("Cancel Settings")]
    [SerializeField] [InputAxis] string _cancelButton;
    [SerializeField] [ValidateInput("ProtectEscapeKeySetting", "Can't set Global Settings to Global Settings")]
    EscapeKey _globalCancelFunction = EscapeKey.BackOneLevel;
    [SerializeField] [Label("Nothing to Cancel")] 
    PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    [Header("Pause Settings")]
    [SerializeField] [Label("Pause / Option Button")] [InputAxis] string _pauseOptionButton;
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
    public static event Action OnChangeControls;
#pragma warning disable 67
    public static event Action OnCancelPressed;
    public static event Action<SwitchType> OnSwitchGroupsPressed;
    public static event Action<bool> OnGamePaused; // Subscribe to trigger pause operations
#pragma warning restore 67

    //Variables
    private UINode _lastHomeScreenNode;
    private UIDataEvents _uiDataEvents;
    private UIPopUpEvents _uiPopUpEvents;
    private bool _hasPauseAxis, _hasPosSwitchAxis, _hasNegSwitchAxis, _hasCancelAxis, _hasSwitchToMenuAxis;
    private bool _onHomeScreen;
    private bool _inMenu;
    private bool _noActivePopUps = true;
    private bool _gameIsPaused;
    private UINode _lastSelected;
    private UINode _lastHighlighted;

    //Properties
    private bool StartingInGame => _menuAndGameSwitching.ActiveInGameSystem 
                                      && _menuAndGameSwitching.StartInGame;
    private bool MouseOnly()
    {
        if(_mainControlType == ControlMethod.MouseOnly) _menuAndGameSwitching.TurnOffGameSwitchSystem();
        return _mainControlType == ControlMethod.MouseOnly;
    }

    private void Awake()
    {
        CheckForControls();
        CreateSubClasses();
    }

    private void CreateSubClasses()
    {
        var unused = new PopUpController();
        var unused4 = new ChangeControl(_mainControlType, StartingInGame);
        var unused1 = new UIAudioManager(GetComponent<AudioSource>());
        var unused3 = new UIHomeGroup(_homeBranches.ToArray(), FindObjectsOfType<UIBranch>());
        var unused2 = new UICancel(_globalCancelFunction);
        _uiDataEvents = new UIDataEvents();
        _uiPopUpEvents = new UIPopUpEvents();
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
        _uiDataEvents.SubscribeToHighlightedNode(SetLastHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SetLastSelected);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
    }
    
    private void Start()
    {
        SetStartPositionsAndSettings();
        CheckIfStartingInGame();
        StartCoroutine(EnableStartControls());
    }

    private void SetStartPositionsAndSettings()
    {
        _lastHighlighted = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.ThisNodeIsHighLighted();
        _lastSelected = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.ThisNodeIsSelected();
    }

    private void CheckIfStartingInGame()
    {
        if (_menuAndGameSwitching.CanStartInGame)
        {
            ActivateAllHomeBranches(IsActive.Yes);
            OnStart?.Invoke();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_lastHighlighted.gameObject);
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
        //_changeControl.StartGame(StartingInGame);
        yield return new WaitForSeconds(_atStartDelay);
        CanStart = true;
        if(!_menuAndGameSwitching.CanStartInGame)
            OnStart?.Invoke();

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
            PausedPressedActions();
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

        OnChangeControls?.Invoke();
    }
}




