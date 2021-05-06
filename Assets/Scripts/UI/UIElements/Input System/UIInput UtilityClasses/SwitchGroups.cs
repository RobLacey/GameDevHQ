using System;
using UIElements;
using UnityEngine;


public interface ISwitchGroup
{
    void OnEnable();
    void OnStart();
    bool CanSwitchBranches();
    bool SwitchGroupProcess();
    bool BranchGroupSwitchProcess();
    bool GOUISwitchProcess();
}

public class SwitchGroups : IEventUser, IParameters, ISwitchGroupPressed, IEServUser, ISwitchGroup
{
    public SwitchGroups(ISwitchGroupSettings settings)
    { 
        _switcher = EJect.Class.NoParams<IGOUISwitcher>();
        _homeGroup = EJect.Class.NoParams<IHomeGroup>();
        ChangeControls = settings.DoChangeControlPressed;
        UseEServLocator();
    }

    public void UseEServLocator()
    {
        _inputScheme = EServ.Locator.Get<InputScheme>(this);
        _uiHub = EServ.Locator.Get<IHub>(this);
    }

    //Variables
    private InputScheme _inputScheme;
    private SwitchFunction _currentSwitchFunction = SwitchFunction.Unset;
    private bool _allowKeys, _gameIsPaused;
    private readonly IGOUISwitcher _switcher;
    private bool _noActivePopUps = true;
    private readonly IHomeGroup _homeGroup;
    private IHub _uiHub;
    private bool _onHomeScreen = true;
    private IBranch _activeBranch;

    //Enum
    private enum SwitchFunction {  GOUI, UI, Branch, Unset }

    //Events
    private Action ChangeControls { get; }
    
    //Properties Getters / Setters
    public SwitchType SwitchType { get; private set; }
    private void SaveAllowKeys (IAllowKeys args) { _allowKeys = args.CanAllowKeys;  }
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    private void SaveNoActivePopUps(INoPopUps args) => _noActivePopUps = args.NoActivePopUps;
    private void SaveOnHomeScreen(IOnHomeScreen args)
    {
        _currentSwitchFunction = SwitchFunction.Unset;
        _onHomeScreen = args.OnHomeScreen;
    }
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
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
    }

    private void SaveActiveBranch(IActiveBranch args)
    {
        _activeBranch = args.ActiveBranch;
    }

    public void OnStart()
    {
        _homeGroup.SetUpHomeGroup(_uiHub.HomeBranches);
        _switcher.OnStart();
    }
    
    public bool SwitchGroupProcess()
    {
        if (!_onHomeScreen) return false;

        if (_activeBranch.BranchGroupsList.Count > 0) return false;
        
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
        
        void HomeGroupSwitch() => _homeGroup.SwitchHomeGroups(SwitchType);

        return false;
    }

    public bool BranchGroupSwitchProcess()
    {
        if (_activeBranch.BranchGroupsList.Count <= 1) return false;
        
        if (_inputScheme.PressedPositiveSwitch())
        {
            if (MustActivateKeysFirst_Branch()) return true;
            DoSwitch(SwitchType.Positive, BranchSwitch);
            return true;
        }

        if (_inputScheme.PressedNegativeSwitch())
        {
            if (MustActivateKeysFirst_Branch()) return true;
            DoSwitch(SwitchType.Negative, BranchSwitch);
            return true;
        }

        return false;

        void BranchSwitch()
        {
            _activeBranch.GroupIndex = BranchGroups.SwitchBranchGroup(_activeBranch.BranchGroupsList,
                                                                      _activeBranch.GroupIndex,
                                                                      SwitchType);
        }
    }

    public bool GOUISwitchProcess()
    {
        
        if (!_onHomeScreen) return false;

        if (_inputScheme.PressedPositiveGOUISwitch())
        {
            if (MustActivateKeysFirst_GOUI()) return true;
            DoSwitch(SwitchType.Positive, GOUISwitch);
            return true;
        }

        if (_inputScheme.PressedNegativeGOUISwitch())
        {
            if (MustActivateKeysFirst_GOUI()) return true;
            DoSwitch(SwitchType.Negative, GOUISwitch);
            return true;
        }

        void GOUISwitch() => _switcher.UseGOUISwitcher(SwitchType);

        return false;
    }

    private void DoSwitch(SwitchType switchType, Action switchProcess)
    {
        SwitchType = switchType;
        switchProcess.Invoke();
    }
    
    private bool MustActivateKeysFirst_Switch()
    {
        if(!_allowKeys && _currentSwitchFunction != SwitchFunction.UI)
        {
            _currentSwitchFunction = SwitchFunction.UI;
            SwitchType = SwitchType.Activate;
            ChangeControls?.Invoke();
            _homeGroup.SwitchHomeGroups(SwitchType);
            return true;
        }
        return false;
    }
    
    private bool MustActivateKeysFirst_GOUI()
    {
        if(!_allowKeys && _currentSwitchFunction != SwitchFunction.GOUI)
        {
            _currentSwitchFunction = SwitchFunction.GOUI;
            SwitchType = SwitchType.Activate;
            ChangeControls?.Invoke();
            _switcher.UseGOUISwitcher(SwitchType);
            return true;
        }
        return false;
    }
    
    private bool MustActivateKeysFirst_Branch()
    {
        if(!_allowKeys && _currentSwitchFunction != SwitchFunction.Branch)
        {
            _currentSwitchFunction = SwitchFunction.Branch;
            SwitchType = SwitchType.Activate;
            ChangeControls?.Invoke();
            _activeBranch.GroupIndex = BranchGroups.SwitchBranchGroup(_activeBranch.BranchGroupsList, 
                                                                      _activeBranch.GroupIndex,
                                                                      SwitchType.Activate);
            return true;
        }
        return false;
    }
}