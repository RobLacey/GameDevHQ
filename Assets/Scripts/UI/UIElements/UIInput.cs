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
    private bool _hasPauseAxis, _hasPosSwitchAxis, _hasNegSwitchAxis, 
                 _hasCancelAxis, _canStart, _inMenu, _gameIsPaused;
    private bool _noActivePopUps = true;
    private UINode _lastHomeScreenNode, _lastSelected;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private UIBranch _activeBranch;

    //Events
    public static event Action OnChangeControls, OnCancelPressed, OnPausedPressed;
    public static event Action<SwitchType> OnSwitchGroupsPressed;
    public static event Func<bool> HotKeyActivated, OnGameToMenuSwitchPressed;

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
    private void SaveGameIsPaused(bool gameIsPaused) => _gameIsPaused = gameIsPaused;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;

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

    private void PausedPressedActions() => OnPausedPressed?.Invoke();

    private static bool CanSwitchBetweenInGameAndMenu() => OnGameToMenuSwitchPressed?.Invoke() ?? false;
    
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
