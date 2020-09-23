using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class UIInput : MonoBehaviour
{
    [SerializeField] 
    private ControlMethod _mainControlType = ControlMethod.MouseOnly;

    [SerializeField] private ScriptableObject _inputScheme;

    [Header("Pause Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Nothing to Cancel")]
    private PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    [SerializeField] 
    [Label("Pause / Option Button")] [InputAxis]
    private string _pauseOptionButton;
    
    [Header("Home Branch Switch Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField]
    [HideIf("MouseOnly")] [InputAxis] private string _posSwitchButton;
    [SerializeField] 
    [HideIf("MouseOnly")] [InputAxis] private string _negSwitchButton;
    
    [Header("Cancel Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [InputAxis] private string _cancelButton;
    
    [Header("In-Game Menu Settings")]
    [SerializeField]
    [DisableIf("MouseOnly")] private InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] 
    private StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] 
    [Label("Switch To/From Game Menus")] [InputAxis] private string _switchToMenusButton;
    [SerializeField] 
    private InGameOrInMenu _returnToGameControl;


    //Variables
    private bool _hasPauseAxis, _hasPosSwitchAxis, _hasNegSwitchAxis, 
                 _hasCancelAxis,_hasSwitchToMenusButton, _canStart, 
                 _inMenu, _gameIsPaused;
    private bool _noActivePopUps = true;
    private UINode _lastHomeScreenNode;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private UIBranch _activeBranch;
    private MenuAndGameSwitching _menuToGameSwitching;

    //Events
    public static event Action OnChangeControls, OnCancelPressed, OnPausedPressed;
    public static event Action<SwitchType> OnSwitchGroupsPressed;
    public static event Func<bool> HotKeyActivated, OnGameToMenuSwitchPressed;
    [Serializable]
    public class InGameOrInMenu : UnityEvent<bool> { }


    //Properties
    private bool MouseOnly()
    {
        if(_mainControlType == ControlMethod.MouseOnly) _inGameMenuSystem = InGameSystem.Off;
         return _mainControlType == ControlMethod.MouseOnly;
    }

    public bool StartInGame => _startGameWhere == StartInMenu.InGameControl && _inGameMenuSystem == InGameSystem.On;
    private void SaveInMenu(bool isInMenu)
    {
        _returnToGameControl?.Invoke(isInMenu);
        _inMenu = isInMenu;
    }
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SaveOnStart() => _canStart = true;
    private void SaveGameIsPaused(bool gameIsPaused) => _gameIsPaused = gameIsPaused;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    
    private void Awake()
    {
        CheckForControls();
        var unused4 = new ChangeControl(_mainControlType, StartInGame);
        if(_inGameMenuSystem == InGameSystem.On)
            _menuToGameSwitching = new MenuAndGameSwitching(_startGameWhere);
    }

    private void CheckForControls()
    {
        _hasPauseAxis = _pauseOptionButton != string.Empty;
        _hasPosSwitchAxis = _posSwitchButton != string.Empty;
        _hasNegSwitchAxis = _negSwitchButton != string.Empty;
        _hasCancelAxis = _cancelButton != string.Empty;
        _hasSwitchToMenusButton = _switchToMenusButton != string.Empty;
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToOnStart(SaveOnStart);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToGameIsPaused(SaveGameIsPaused);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
    }
    
    private void Update()
    {
        if (!_canStart) return;
        if (CanPauseGame())
        {
            PausedPressedActions();
            return;
        }

        if (CanSwitchBetweenInGameAndMenu()) return;

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
    
    private bool CanPauseGame() => _hasPauseAxis && Input.GetButtonDown(_pauseOptionButton);

    private static void PausedPressedActions() => OnPausedPressed?.Invoke();

    private bool CanSwitchBetweenInGameAndMenu()
    {
        if (CantSwitchBetweenGameAndMenu()) return false;
        if (!Input.GetButtonDown(_switchToMenusButton)) return false;
        OnGameToMenuSwitchPressed?.Invoke(); //Might need to move
        return true;
    }

    private bool CantSwitchBetweenGameAndMenu() 
        => _inGameMenuSystem == InGameSystem.Off || !_hasSwitchToMenusButton;

    private static bool CheckIfHotKeyAllowed() => HotKeyActivated?.Invoke() ?? false;
    
    private bool CanDoCancel() => _hasCancelAxis && Input.GetButtonDown(_cancelButton);

    private void WhenCancelPressed()
    {
        if (CanEnterPauseWithNothingSelected() || CanUnpauseGame())
        {
            PausedPressedActions();
        }
        else
        {
            OnCancelPressed?.Invoke();
        }
    }

    private bool CanUnpauseGame() => _gameIsPaused && _activeBranch.IsPauseMenuBranch();

    private bool CanEnterPauseWithNothingSelected() =>
        (_noActivePopUps && _activeBranch == _activeBranch.MyParentBranch)
        && _pauseOptionsOnEscape == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;

    private bool CanSwitchBranches() => _noActivePopUps && !MouseOnly();

    private bool SwitchGroupProcess()
    {
        if (_hasPosSwitchAxis && Input.GetButtonDown(_posSwitchButton))
        {
            OnSwitchGroupsPressed?.Invoke(SwitchType.Positive);
            return true;
        }

        if (_hasNegSwitchAxis && Input.GetButtonDown(_negSwitchButton))
        {
            OnSwitchGroupsPressed?.Invoke(SwitchType.Negative);
            return true;
        }
        return false;
    }
}
