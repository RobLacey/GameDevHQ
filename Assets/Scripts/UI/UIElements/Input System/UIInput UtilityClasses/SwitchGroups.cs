using System;
using UIElements;

public interface IGOUISwitchSettings
{
    IGOUIModule[] GetPlayerObjects { get; }
}

public interface ISwitchGroup
{
    void OnEnable();
    void OnStart();
    bool CanSwitchBranches();
    bool SwitchGroupProcess();
    bool GOUISwitchProcess();
}

public class SwitchGroups : IEventUser, IEventDispatcher, IParameters, ISwitchGroupPressed,
                            IEServUser, IGOUISwitchSettings, ISwitchGroup
{
    public SwitchGroups(ISwitchGroupSettings settings)
    { 
        GetPlayerObjects = settings.GetPlayerObjects();
        _inputScheme = settings.ReturnScheme;
        _switcher = EJect.Class.WithParams<IGOUISwitcher>(this);
        _homeGroup = EJect.Class.NoParams<IHomeGroup>();
        ChangeControls = settings.DoChangeControlPressed;
        FetchEvents();
        UseEServLocator();
    }

    public void FetchEvents() => OnSwitchGroupPressed = EVent.Do.Fetch<ISwitchGroupPressed>();

    public void UseEServLocator() => _uiHub = EServ.Locator.Get<IHub>(this);

    //Variables
    private readonly InputScheme _inputScheme;
    private SwitchFunction _currentSwitchFunction;
    private bool _allowKeys, _gameIsPaused;
    private readonly IGOUISwitcher _switcher;
    private bool _noActivePopUps = true;
    private readonly IHomeGroup _homeGroup;
    private IHub _uiHub;
    private bool _onHomeScreen = true;

    //Enum
    private enum SwitchFunction {  GOUI, UI }

    //Events
    private Action ChangeControls { get; }
    private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }
    
    //Properties Getters / Setters
    public IGOUIModule[] GetPlayerObjects { get; }
    public SwitchType SwitchType { get; private set; }
    private void SaveAllowKeys (IAllowKeys args) { _allowKeys = args.CanAllowKeys;  }
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    private void SaveNoActivePopUps(INoPopUps args) => _noActivePopUps = args.NoActivePopUps;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    public bool CanSwitchBranches() => _noActivePopUps && !MouseOnly() 
                                                       && !_gameIsPaused 
                                                       && !_inputScheme.MultiSelectPressed();

    private bool MouseOnly()
    {
        if(_inputScheme.ControlType == ControlMethod.MouseOnly) 
            _inputScheme.TurnOffInGameMenuSystem();
        return _inputScheme.ControlType == ControlMethod.MouseOnly;
    }
    
    //Main
    public void OnEnable()
    {
        ObserveEvents();
        _switcher.OnEnable();
        _homeGroup.OnEnable();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<INoPopUps>(SaveNoActivePopUps);
        EVent.Do.Subscribe<IGameIsPaused>(GameIsPaused);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
    }

    public void OnStart() => _homeGroup.SetUpHomeGroup(_uiHub.HomeBranches);
    
    public bool SwitchGroupProcess()
    {
        if (_inputScheme.PressedPositiveSwitch())
        {
            if (MustActivateKeysFirst_Switch()) return true;
            DoSwitch(SwitchType.Positive, HomeGroupSwitch);
            return true;
        }

        if (_inputScheme.PressedNegativeSwitch())
        {
            if (MustActivateKeysFirst_Switch()) return true;
            DoSwitch(SwitchType.Negative, HomeGroupSwitch);
            return true;
        }
        
        void HomeGroupSwitch()
        {
            OnSwitchGroupPressed?.Invoke(this);
            _homeGroup.SwitchHomeGroups(SwitchType);
        }
        
        return false;
    }

    public bool GOUISwitchProcess()
    {
        if (_inputScheme.PressedPositiveGOUISwitch())
        {
            if (!_onHomeScreen) return false;
            if (MustActivateKeysFirst_GOUI()) return true;
            DoSwitch(SwitchType.Positive, GOUISwitch);
            return true;
        }

        if (_inputScheme.PressedNegativeGOUISwitch())
        {
            if (!_onHomeScreen) return false;
            if (MustActivateKeysFirst_GOUI()) return true;
            DoSwitch(SwitchType.Negative, GOUISwitch);
            return true;
        }

        void GOUISwitch()
        {
            OnSwitchGroupPressed?.Invoke(this);
            _switcher.UseGOUISwitcher(SwitchType);
        }

        return false;
    }

    private void DoSwitch(SwitchType switchType, Action switchProcess)
    {
        SwitchType = switchType;
        switchProcess.Invoke();
    }

    private bool MustActivateKeysFirst_Switch()
    {
        if(!_allowKeys || _currentSwitchFunction == SwitchFunction.GOUI)
        {
            _currentSwitchFunction = SwitchFunction.UI;
            SwitchType = SwitchType.Activate;
            
            OnSwitchGroupPressed?.Invoke(this);
            ChangeControls?.Invoke();
            
            _homeGroup.SwitchHomeGroups(SwitchType);
            return true;
        }
        return false;
    }
    
    private bool MustActivateKeysFirst_GOUI()
    {
        if(!_allowKeys || _currentSwitchFunction == SwitchFunction.UI)
        {
            _currentSwitchFunction = SwitchFunction.GOUI;
            SwitchType = SwitchType.Activate;
            
            OnSwitchGroupPressed?.Invoke(this);
            ChangeControls?.Invoke();
            
            _switcher.UseGOUISwitcher(SwitchType);
            return true;
        }
        return false;
    }
}