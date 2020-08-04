using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using NaughtyAttributes;

/// <summary>
/// UIHub is the core of the system and looks after shared functionality as well as tracking current UI state
/// </summary>

[RequireComponent(typeof(AudioSource))]

public partial class UIHub : MonoBehaviour, IGameToMenuSwitching
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
    // ReSharper disable once EventNeverSubscribedTo.Global
    public static event Action<bool> GamePaused; // Subscribe to trigger pause operations
    public static event Action<bool> SetInMenu; // Subscribe To track if in game

    //Variables
    private UINode _lastHomeScreenNode;
    // ReSharper disable once NotAccessedField.Local
    private UIAudioManager _uiAudio; 
    private UICancel _myUiCancel;
    private UIHomeGroup _uiHomeGroup;
    private PopUpController _popUpController;
    private bool _hasPauseAxis;
    private bool _hasPosSwitchAxis;
    private bool _hasNegSwitchAxis;
    private ChangeControl _changeControl;
    private bool _hasCancelAxis;
    private bool _hasSwitchToMenuAxis;
    private bool _onHomeScreen;

    private enum InGameSystem { On, Off }

    //Properties
    private bool NotStartingInGame => !ActiveInGameSystem || !StartInGame;
    private UIBranch[] AllBranches { get; set; }
    private bool ActiveInGameSystem => _inGameMenuSystem == InGameSystem.On;
    private bool StartInGame => _startGameWhere == StartInMenu.InGameControl;
    private PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    private void SaveHighlighted(UINode newNode) => SetLastHighlighted(newNode);
    private void SaveSelected(UINode newNode) => SetLastSelected(newNode);
    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;


    private bool MouseOnly()
    {
        if(_mainControlType == ControlMethod.Mouse) _inGameMenuSystem = InGameSystem.Off;
        return _mainControlType == ControlMethod.Mouse;
    }

    private void Awake()
    {
        AllBranches = FindObjectsOfType<UIBranch>();
        CheckForControls();
        CreateSubClasses();

        foreach (UIBranch branch in AllBranches)
        {
            branch.OnAwake(this, _uiHomeGroup);
        }
    }

    private void CreateSubClasses()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        _popUpController = new PopUpController();
        _changeControl = new ChangeControl(_mainControlType, _popUpController);
        _uiAudio = new UIAudioManager(GetComponent<AudioSource>());
        _uiHomeGroup = new UIHomeGroup(_homeBranches.ToArray(), AllBranches);
        _myUiCancel = new UICancel( _globalCancelFunction, _popUpController);
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
        _hasSwitchToMenuAxis = _switchToMenusButton != string.Empty;
    }

    private void OnEnable()
    {
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
        UIHomeGroup.DoOnHomeScreen -= SaveOnHomeScreen;
    }

    private void OnDisable()
    {
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        UIHomeGroup.DoOnHomeScreen -= SaveOnHomeScreen;
        RunOnDisableForSubClasses();
    }

    private void SaveOnHomeScreen(bool onHomeScreen)
    {
        _onHomeScreen = onHomeScreen;
    }

    private void RunOnDisableForSubClasses()
    {
        _uiAudio.OnDisable();
        _myUiCancel.OnDisable();
        _changeControl.OnDisable();
        _popUpController.OnDisable();
        
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnDisable();
        }
    }

    private void Start()
    {
        LastHighlighted = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.ThisNodeIsHighLighted();
        LastSelected = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.ThisNodeIsSelected();
        _popUpController.SetLastNodeBeforePopUp(_homeBranches[0].DefaultStartPosition);
        CheckIfStartingInGame();
        StartCoroutine(EnableStartControls());
    }

    private void CheckIfStartingInGame()
    {
        if (ActiveInGameSystem && StartInGame)
        {
            _changeControl.StartGame();
            InMenu = true;
            SwitchBetweenGameAndMenu();
            IntroAnimations(IsActive.Yes);
        }
        else
        {
            SetInMenu?.Invoke(true);
            _returnToGameControl.Invoke(InMenu);
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
            IntroAnimations(IsActive.No);
        }
    }

    private void IntroAnimations(IsActive activateOnStart)
    {
        foreach (var homeBranch in _homeBranches)
        {
            homeBranch.MyCanvasGroup.blocksRaycasts = false;
            if (activateOnStart == IsActive.Yes) _homeBranches[0].DontSetAsActive = true;
            if (homeBranch != _homeBranches[0]) homeBranch.DontSetAsActive = true;
            homeBranch.MoveToThisBranch();
        }
    }

    private IEnumerator EnableStartControls()
    {
        yield return new WaitForSeconds(_atStartDelay);
        CanStart = true;

        if (NotStartingInGame)
            _changeControl.StartGame();
        
        foreach (var homeBranch in _homeBranches)
        {
            homeBranch.MyCanvasGroup.blocksRaycasts = true;
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
            SwitchBetweenGameAndMenu();
            return;
        }

        if (CheckIfHotKeyAllowed())
        {
            if (!InMenu) 
                SwitchBetweenGameAndMenu();
            return;
        }

        if (InMenu) InMenuControls();
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
