using System;
using UIElements;
using UnityEngine;

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
                            IEServUser, IGOUISwitchSettings, ISwitchGroup, IGOUISwitchPressed
{
    public SwitchGroups(ISwitchGroupSettings settings)
    { 
        GetPlayerObjects = settings.GetPlayerObjects();
        _inputScheme = settings.ReturnScheme;
        _switcher = EJect.Class.WithParams<IGOUISwitcher>(this);
        _homeGroup = EJect.Class.NoParams<IHomeGroup>();
        FetchEvents();
        UseEServLocator();
    }

    public void FetchEvents()
    {
        OnSwitchGroupPressed = EVent.Do.Fetch<ISwitchGroupPressed>();
        OnGOUISwitchPressed = EVent.Do.Fetch<IGOUISwitchPressed>();
    }
    
    public void UseEServLocator() => _uiHub = EServ.Locator.Get<IHub>(this);

    //Variables
    private readonly InputScheme _inputScheme;
    private SwitchFunction _currentSwitchFunction;
    private bool _allowKeys, _gameIsPaused;
    private readonly IGOUISwitcher _switcher;
    private bool _noActivePopUps = true;
    private readonly IHomeGroup _homeGroup;
    private IHub _uiHub;
    private INode _lastHighlighted;

    //Enum
    private enum SwitchFunction {  GOUI, UI }

    //Events
    private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }
    private Action<IGOUISwitchPressed> OnGOUISwitchPressed { get; set; }
    
    //Properties Getters / Setters
    public IGOUIModule[] GetPlayerObjects { get; }
    public SwitchType SwitchType { get; private set; }
    private void SaveAllowKeys (IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if(!_allowKeys) return;
        _currentSwitchFunction = _lastHighlighted.MyBranch.IsInGameBranch() ? SwitchFunction.GOUI : SwitchFunction.UI;
    }
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    private void SaveLastHighlighted(IHighlightedNode args) => _lastHighlighted = args.Highlighted;
    private void SaveNoActivePopUps(INoPopUps args) => _noActivePopUps = args.NoActivePopUps;

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
        EVent.Do.Subscribe<IHighlightedNode>(SaveLastHighlighted);
    }
    
    public void OnStart() => _homeGroup.SetUpHomeGroup(_uiHub.HomeBranches);
    
    public bool CanSwitchBranches() => _noActivePopUps && !MouseOnly() && !_gameIsPaused;

    public bool SwitchGroupProcess()
    {
        if (_inputScheme.PressedPositiveSwitch())
        {
            if (MustActivateKeysFirst_Switch()) return false;
            DoSwitch(SwitchType.Positive, HomeGroupSwitch);
            return true;
        }

        if (_inputScheme.PressedNegativeSwitch())
        {
            if (MustActivateKeysFirst_Switch()) return false;
            DoSwitch(SwitchType.Negative, HomeGroupSwitch);
            return true;
        }
        
        void HomeGroupSwitch() => OnSwitchGroupPressed?.Invoke(this);
        
        return false;
    }

    public bool GOUISwitchProcess()
    {
        if (_inputScheme.PressedPositiveGOUISwitch())
        {
            if (MustActivateKeysFirst_GOUI()) return false;
            DoSwitch(SwitchType.Positive, GOUISwitch);
            return true;
        }

        if (_inputScheme.PressedNegativeGOUISwitch())
        {
            if (MustActivateKeysFirst_GOUI()) return false;
            DoSwitch(SwitchType.Negative, GOUISwitch);
            return true;
        }

        void GOUISwitch()
        {
            OnGOUISwitchPressed?.Invoke(this);
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
            _switcher.UseGOUISwitcher(SwitchType);
            return true;
        }
        return false;
    }
}