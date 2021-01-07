using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IInput : IParameters
{
    InputScheme ReturnScheme { get; }
    bool StartInGame();
}

public class UIInput : MonoBehaviour, IInput, IEventUser, IPausePressed, ISwitchGroupPressed, 
                       ICancelPressed, IChangeControlsPressed, IMenuGameSwitchingPressed, 
                       IEServUser, IEventDispatcher, IClearAll
{
    public UIInput() => _validationCheck = new ValidationCheck(this);

    [Header("Input Scheme(Expandable)", order = 2)][HorizontalLine(1f, EColor.Blue, order = 3)] 
    [SerializeField] 
    [Expandable] private InputScheme _inputScheme  = default;
    [SerializeField] 
    [Space(20, order = 1)]
    private SwitchGameOrMenu _switchBetweenMenuAndGame  = default;
    [SerializeField] 
    [ReorderableList] private List<HotKeys> _hotKeySettings = new List<HotKeys>();

    //Variables
    private bool _canStart, _inMenu, _gameIsPaused, _allowKeys, _nothingSelected;
    private bool _noActivePopUps = true;
    private bool _onHomeScreen = true;
    private UINode _lastHomeScreenNode;
    private IBranch _activeBranch;
    private IMenuAndGameSwitching _menuToGameSwitching;
    private IChangeControl _changeControl;
    private IHistoryTrack _historyTrack;
    private readonly ValidationCheck _validationCheck;

    //Events
    private Action<IPausePressed> OnPausedPressed { get; set; }
    private Action<IMenuGameSwitchingPressed> OnMenuAndGameSwitch { get; set; }
    private Action<ICancelPressed> OnCancelPressed { get; set; }
    private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }
    private Action<IChangeControlsPressed> OnChangeControlPressed { get; set; }
    private Action<IClearAll> OnClearAll { get; set; }

    [Serializable]
    public class SwitchGameOrMenu : UnityEvent<InMenuOrGame> { }

    //Properties
    private bool MouseOnly()
    {
        if(_inputScheme.ControlType == ControlMethod.MouseOnly) _inputScheme.TurnOffInGameMenuSystem();
         return _inputScheme.ControlType == ControlMethod.MouseOnly;
    }

    public bool StartInGame()
    {
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
        {
            return _inputScheme.WhereToStartGame == InMenuOrGame.InGameControl;
        }
        return false;
    }
    private void SaveInMenu(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if(_inMenu)
        {
            _switchBetweenMenuAndGame?.Invoke(InMenuOrGame.InMenu);
        }
        else
        {
            _switchBetweenMenuAndGame?.Invoke(InMenuOrGame.InGameControl);
        }
    }
    private void SaveNoActivePopUps(INoPopUps args) => _noActivePopUps = args.NoActivePopUps;
    private void SaveOnStart(IOnStart onStart) => _canStart = true;
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void SaveOnHomeScreen (IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveAllowKeys (IAllowKeys args) => _allowKeys = args.CanAllowKeys;
    public SwitchType SwitchType { get; private set; }
    public InputScheme ReturnScheme => _inputScheme;
    public EscapeKey EscapeKeySettings => _activeBranch.EscapeKeyType;
    private bool NothingSelectedAction => _inputScheme.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;

    //Main
    private void Awake()
    {
        _inputScheme.OnAwake();
        _changeControl = EJect.Class.WithParams<IChangeControl>(this);
        _menuToGameSwitching = EJect.Class.WithParams<IMenuAndGameSwitching>(this);
        UseEServLocator();
        _inputScheme.SetCursor();
    }

    public void DoValidation() => _validationCheck.ValidateDialogue();

    private void OnEnable()
    {
        FetchEvents();
        SetUpHotKeys();
        _changeControl.OnEnable();
        _menuToGameSwitching.OnEnable();
        ObserveEvents();
    }

    public void UseEServLocator() => _historyTrack = EServ.Locator.Get<IHistoryTrack>(this);

    private void SetUpHotKeys()
    {
        if (_hotKeySettings.Count == 0) return;
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake(_inputScheme);
            hotKey.OnEnable();
        }
    }
    
    public void FetchEvents()
    {
        OnPausedPressed = EVent.Do.Fetch<IPausePressed>();
        OnMenuAndGameSwitch = EVent.Do.Fetch<IMenuGameSwitchingPressed>();
        OnCancelPressed = EVent.Do.Fetch<ICancelPressed>();
        OnSwitchGroupPressed  = EVent.Do.Fetch<ISwitchGroupPressed>();
        OnChangeControlPressed = EVent.Do.Fetch<IChangeControlsPressed>();
        OnClearAll = EVent.Do.Fetch<IClearAll>();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IGameIsPaused>(SaveGameIsPaused);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<IOnStart>(SaveOnStart);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Subscribe<IInMenu>(SaveInMenu);
        EVent.Do.Subscribe<INoPopUps>(SaveNoActivePopUps);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
    }

    private void Update()
    {
        if (!_canStart) return;

        if(ReturnControlToGameIfKeysOnly()) return;
        
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            OnClearAll?.Invoke(this);
        }

        if (CanPauseGame())
        {
            PausedPressedActions();
            return;
        }
        
        if (CanSwitchBetweenInGameAndMenu() && _onHomeScreen) return;
        if (CheckIfHotKeyAllowed()) return;

        if (_inMenu) InMenuControls();
    }

    private bool ReturnControlToGameIfKeysOnly()
    {
        if(!_inMenu) return false;
        if (Input.GetMouseButtonDown(0) && _inputScheme.ControlType == ControlMethod.KeysOrControllerOnly)
        {
            EventSystem.current.SetSelectedGameObject(_activeBranch.LastSelected.ReturnGameObject);
            return true;
        }
        return false;
    }

    private void InMenuControls()
    {
        if (CanDoCancel())
        {
            WhenCancelPressed();
            return;
        }
        
        if (CanSwitchBranches() && SwitchGroupProcess()) return;
        
        OnChangeControlPressed?.Invoke(this);
    }

    private bool CanPauseGame() => _inputScheme.PressPause();

    private void PausedPressedActions() => OnPausedPressed?.Invoke(this);

    private bool CanSwitchBetweenInGameAndMenu()
    {
        if (!_inputScheme.PressedMenuToGameSwitch()) return false;
        OnMenuAndGameSwitch?.Invoke(this);
        return true;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (_gameIsPaused || !_noActivePopUps) return false;
        if (!HasMatchingHotKey()) return false;
        if(!_inMenu)
            OnMenuAndGameSwitch?.Invoke(this);
        return true;
    }

    private bool HasMatchingHotKey()
    {
        bool hasHotKey = false;
        for (var index = 0; index < _hotKeySettings.Count; index++)
        {
            var hotKeySetting = _hotKeySettings[index];
            if (hotKeySetting.CheckHotKeys())
            {
                hasHotKey = true;
                break;
            }
        }

        return hasHotKey;
    }

    private bool CanDoCancel() => _inputScheme.PressedCancel();

    private void WhenCancelPressed()
    {
         if (CanEnterPauseWithNothingSelected() || CanUnpauseGame())
         {
            PausedPressedActions();
         }
         else
         {
             CancelPressed();
         }
    }

    private bool CanUnpauseGame() => _gameIsPaused && _activeBranch.IsPauseMenuBranch();
    
    private void CancelPressed() => OnCancelPressed?.Invoke(this);
    
    private bool CanEnterPauseWithNothingSelected() =>
        (_noActivePopUps && !_gameIsPaused && _historyTrack.NoHistory) && NothingSelectedAction;
    
    private bool CanSwitchBranches() => _noActivePopUps && !MouseOnly() && _allowKeys;

    private bool SwitchGroupProcess()
    {
        if (_inputScheme.PressedPositiveSwitch())
        {
            SwitchType = SwitchType.Positive;
            OnSwitchGroupPressed?.Invoke(this);
            return true;
        }

        if (_inputScheme.PressedNegativeSwitch())
        {
            SwitchType = SwitchType.Negative;
            OnSwitchGroupPressed?.Invoke(this);
            return true;
        }
        return false;
    }

}
