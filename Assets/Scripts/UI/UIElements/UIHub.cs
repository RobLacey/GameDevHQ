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

public partial class UIHub : MonoBehaviour, INodeData, IBranchData
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
    // ReSharper disable once EventNeverSubscribedTo.Global
    public static event Action<bool> GamePaused; // Subscribe To to trigger pause operations

    //Variables
    private UINode _lastHomeScreenNode;
    // ReSharper disable once NotAccessedField.Local
    private UIAudioManager _uiAudio; //Used to hold instance of class
    private UICancel _myUiCancel;
    private UIHomeGroup _uiHomeGroup;
    private PopUpController _popUpController;
    private bool _hasPauseAxis;
    private bool _hasPosSwitchAxis;
    private bool _hasNegSwitchAxis;
    private ChangeControl _changeControl;
    private bool _hasCancelAxis;
    private bool _hasSwitchToMenuAxis;
    private bool _activatedHotKey;

    private enum InGameSystem { On, Off }

    //Properties
    public UIBranch[] AllBranches { get; private set; }
    private bool ActiveInGameSystem => _inGameMenuSystem == InGameSystem.On;
    private bool StartInGame => _startGameWhere == StartInMenu.InGameControl;
    public PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    public PopUpController ReturnPopUpController => _popUpController;
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

    private void OnEnable()
    {
        UINode.DoCancel += _myUiCancel.CancelOrBack;
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
        // UIPopUp.AddToResolvePopUp += AddToResolveList;
        //UIPopUp.RemoveResolvePopUp += RemoveFromResolveList;
        //UIPopUp.AddToNonResolvePopUp += AddToNonResolveList;
        //UIPopUp.RemoveNonResolvePopUp += RemoveFromNonResolveList;
    }

    private void OnDisable()
    {
        UINode.DoCancel -= _myUiCancel.CancelOrBack;
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        // UIPopUp.AddToResolvePopUp -= AddToResolveList;
        //UIPopUp.RemoveResolvePopUp -= RemoveFromResolveList;
        // UIPopUp.AddToNonResolvePopUp -= AddToNonResolveList;
        //UIPopUp.RemoveNonResolvePopUp -= RemoveFromNonResolveList;

        
        _uiAudio.OnDisable();
        _myUiCancel.OnDisable();
        _changeControl.OnDisable();
        _popUpController.OnDisable();
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnDisable();
        }
    }
    
    private void CreateSubClasses()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        _popUpController = new PopUpController(this);
        _changeControl = new ChangeControl(this, _mainControlType, _popUpController);
        _changeControl.AllowKeyClasses = FindObjectsOfType<UIBranch>();
        _uiAudio = new UIAudioManager(GetComponent<AudioSource>());
        _myUiCancel = new UICancel(this, _globalCancelFunction, _homeBranches.ToArray(), _popUpController);
        _uiHomeGroup = new UIHomeGroup(this, _homeBranches.ToArray());
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake(this, _uiHomeGroup);
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

    private void Start()
    {
        LastHighlighted = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.SetThisAsHighLighted();
        LastSelected = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.SetAsSelected();
        LastNodeBeforePopUp = _homeBranches[0].DefaultStartPosition;
        //ActiveBranch = _homeBranches[0];
        OnHomeScreen = true;
        _returnToGameControl.Invoke(InMenu);
        

        CheckIfStartingInGame();
        StartCoroutine(EnableControlsStartDelay());
    }

    private void CheckIfStartingInGame()
    {
        if (ActiveInGameSystem && StartInGame)
        {
            _changeControl.StartGame();
            InMenu = true;
            GameToMenuSwitching();
            IntroAnimations(IsActive.Yes);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
            IntroAnimations(IsActive.No);
        }
    }

    private void IntroAnimations(IsActive dontActivate)
    {
        foreach (var homeBranch in _homeBranches)
        {
            homeBranch.MyCanvasGroup.blocksRaycasts = false;
            if (dontActivate == IsActive.Yes) _homeBranches[0].DontSetAsActive = true;
            if (homeBranch != _homeBranches[0]) homeBranch.DontSetAsActive = true;
            homeBranch.MoveToThisBranch();
        }
    }

    private IEnumerator EnableControlsStartDelay()
    {
        yield return new WaitForSeconds(_atStartDelay);
        CanStart = true;
        
        if (!ActiveInGameSystem || !StartInGame)
            _changeControl.StartGame();
        
        if (_mainControlType == ControlMethod.KeysOrController)
        {
            StartUsingKeysOrController();
        }
        else
        {
            StartUsingMouse();
        }
    }

    private void StartUsingKeysOrController()
    {
        foreach (var branch in AllBranches)
        {
            branch.MyCanvasGroup.blocksRaycasts = false;
        }

        _changeControl.UsingKeysOrCtrl = true;
        _changeControl.SetAllowKeys();
    }

    private void StartUsingMouse()
    {
        foreach (var homeBranch in _homeBranches)
        {
            homeBranch.MyCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Update() 
    {
        if (!CanStart) return;
        
        if (_hasPauseAxis)
            if (Input.GetButtonDown(_pauseOptionButton))
            {
                PauseOptionMenuPressed();
                return;
            }

        if (_hasSwitchToMenuAxis)
            if (Input.GetButtonDown(_switchToMenusButton) && _popUpController.NoActivePopUps)
            {
                GameToMenuSwitching();
                return;
            }

        if (HotKeyPressed())return;

        if (InMenu) InMenuOnlyControls();
    }

    private void InMenuOnlyControls()
    {
        if (_hasCancelAxis)
            if (Input.GetButtonDown(_cancelButton))
            {
                _myUiCancel.CancelPressed();
                return;
            }

        if (CanSwitchBranches() && SwitchGroupProcess()) return;

        _changeControl.ChangeControlType();
    }

    private bool SwitchGroupProcess()
    {
        if (_hasPosSwitchAxis)
            if (Input.GetButtonDown(_posSwitchButton))
            {
                SwitchingGroups(SwitchType.Positive);
                return true;
            }

        if (_hasNegSwitchAxis)
            if (Input.GetButtonDown(_negSwitchButton))
            {
                SwitchingGroups(SwitchType.Negative);
                return true;
            }

        return false;
    }
    
    public void SaveHighlighted(UINode newNode) => SetLastHighlighted(newNode);

    public void SaveSelected(UINode newNode) => SetLastSelected(newNode);

    public void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
}
