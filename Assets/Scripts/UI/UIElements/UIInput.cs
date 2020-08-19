using System;
using NaughtyAttributes;
using UnityEngine;

public class UIInput : MonoBehaviour
{
    [SerializeField] private ControlMethod _mainControlType = ControlMethod.MouseOnly;

    [Header("Pause Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Nothing to Cancel")] 
    PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    [SerializeField] 
    [Label("Pause / Option Button")] [InputAxis] string _pauseOptionButton;
    
    [Header("Home Branch Switch Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [HideIf("MouseOnly")] [InputAxis] string _posSwitchButton;
    [SerializeField] 
    [HideIf("MouseOnly")] [InputAxis] string _negSwitchButton;
    
    [Header("Cancel Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [InputAxis] string _cancelButton;
    [SerializeField]
    MenuAndGameSwitching _menuAndGameSwitching = new MenuAndGameSwitching();

    //Variables
    private bool _hasPauseAxis, _hasPosSwitchAxis, _hasNegSwitchAxis, _hasCancelAxis;
    private bool _canStart;
    private bool _inMenu;
    private bool _gameIsPaused;
    private bool _noActivePopUps = true;
    private UINode _lastSelected;
    private UINode _lastHomeScreenNode;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();

    //Events
    public static event Action OnChangeControls;
    public static event Action OnCancelPressed;
    public static event Action<SwitchType> OnSwitchGroupsPressed;
    public static event Action<bool> OnGamePaused; // Subscribe to trigger pause operations
    public static event Func<bool> HotKeyActivated;
    public static event Func<bool> OnGameToMenuSwitchPressed;

    //Properties
    private bool MouseOnly()
    {
        if(_mainControlType == ControlMethod.MouseOnly) _menuAndGameSwitching.TurnOffGameSwitchSystem();
         return _mainControlType == ControlMethod.MouseOnly;
    }
    public bool StartInGame => _menuAndGameSwitching.StartInGame;
    private void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SetLastSelected(UINode newNode) => _lastSelected = newNode;
    private void SaveOnStart() => _canStart = true;

    private void Awake()
    {
        CheckForControls();
        var unused4 = new ChangeControl(_mainControlType, StartInGame);
        _menuAndGameSwitching.OnAwake();
    }

    private void CheckForControls()
    {
        _hasPauseAxis = _pauseOptionButton != string.Empty;
        _hasPosSwitchAxis = _posSwitchButton != string.Empty;
        _hasNegSwitchAxis = _negSwitchButton != string.Empty;
        _hasCancelAxis = _cancelButton != string.Empty;
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToSelectedNode(SetLastSelected);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToOnStart(SaveOnStart);
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

    private void PausedPressedActions()
    {
        _gameIsPaused = !_gameIsPaused;
        OnGamePaused?.Invoke(_gameIsPaused);
    }

    private static bool CanSwitchBetweenInGameAndMenu() => OnGameToMenuSwitchPressed?.Invoke() ?? false;
    
    private static bool CheckIfHotKeyAllowed() => HotKeyActivated?.Invoke() ?? false;
    
    private bool CanDoCancel() => _hasCancelAxis && Input.GetButtonDown(_cancelButton);

    private void WhenCancelPressed()
    {
        if (CanEnterPauseWithNothingSelected() || _gameIsPaused)
        {
            PausedPressedActions();
        }
        else
        {
            OnCancelPressed?.Invoke();
        }
    }

    private bool CanEnterPauseWithNothingSelected()
    {
        return (_noActivePopUps && 
                !_lastSelected.HasChildBranch.CanvasIsEnabled)
               && _pauseOptionsOnEscape == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    }

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
