using System;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UIElements;


public interface ISwitchGroup
{
    void OnEnable();
    void OnStart();
    bool CanSwitchBranches();
    bool SwitchGroupProcess();
    bool BranchGroupSwitchProcess();
    bool GOUISwitchProcess();
}

public class SwitchGroups : IEZEventUser, IParameters, ISwitchGroupPressed, IServiceUser, ISwitchGroup, 
                            IEZEventDispatcher, IChangeControlsSwitchPressed
{
    public SwitchGroups()
    { 
        _switcher = EZInject.Class.NoParams<IGOUISwitcher>();
        _homeGroup = EZInject.Class.NoParams<IHomeGroup>();
        UseEZServiceLocator();
    }

    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    //Variables
    private InputScheme _inputScheme;
    private SwitchFunction _currentSwitchFunction = SwitchFunction.Unset;
    private readonly IGOUISwitcher _switcher;
    private readonly IHomeGroup _homeGroup;
    private bool _onHomeScreen = true;
    private bool _allowKeys;
    private IDataHub _myDataHub;

    //Enum
    private enum SwitchFunction {  GOUI, UI, Branch, Unset }

    //Events
    private Action<IChangeControlsSwitchPressed> ChangeControls { get; set; }

    //Properties Getters / Setters
    private SwitchType SwitchType { get; set; }

    private void SaveAllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if (!_allowKeys)
            _currentSwitchFunction = SwitchFunction.Unset;
    }
    
    private void SaveOnHomeScreen(IOnHomeScreen args)
    {
        _currentSwitchFunction = SwitchFunction.Unset;
        _onHomeScreen = args.OnHomeScreen;
    }
    public bool CanSwitchBranches() => _myDataHub.NoPopups && !MouseOnly() 
                                                       && !_myDataHub.GamePaused 
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
        FetchEvents();
        ObserveEvents();
        _switcher.OnEnable();
        _homeGroup.OnEnable();
    }
    
    public void FetchEvents() => ChangeControls = InputEvents.Do.Fetch<IChangeControlsSwitchPressed>();

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
    }

    public void UnObserveEvents() { }

    public void OnStart()
    {
        _homeGroup.SetUpHomeGroup();
        _switcher.OnStart();
    }
    
    public bool SwitchGroupProcess()
    {
        if (!_onHomeScreen || _myDataHub.ActiveBranch.BranchGroupsList.Count > 0) return false;
        
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
        if (_myDataHub.ActiveBranch.BranchGroupsList.Count <= 1) return false;
        
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
            var activeBranch = _myDataHub.ActiveBranch;
            
            activeBranch.GroupIndex = BranchGroups.SwitchBranchGroup(activeBranch.BranchGroupsList,
                                                                      activeBranch.GroupIndex,
                                                                      SwitchType);
        }
    }

    public bool GOUISwitchProcess()
    {
        if (!_onHomeScreen || _switcher.GOUIPlayerCount == 0) return false;

        var nothingToSwitchToo = _currentSwitchFunction == SwitchFunction.GOUI && _switcher.GOUIPlayerCount == 1;
        
        if (_inputScheme.PressedPositiveGOUISwitch())
        {
            if (MustActivateKeysFirst_GOUI()) return true;
            if (nothingToSwitchToo) return false;

            DoSwitch(SwitchType.Positive, GOUISwitch);
            return true;
        }

        if (_inputScheme.PressedNegativeGOUISwitch())
        {
            if (MustActivateKeysFirst_GOUI()) return true;
            if (nothingToSwitchToo) return false;
            
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
        if(_currentSwitchFunction != SwitchFunction.UI)
        {
            _currentSwitchFunction = SwitchFunction.UI;
            SwitchType = SwitchType.Activate;
            ChangeControls?.Invoke(this);
            _homeGroup.SwitchHomeGroups(SwitchType);
            return true;
        }
        return false;
    }
    
    private bool MustActivateKeysFirst_GOUI()
    {
        if(_currentSwitchFunction != SwitchFunction.GOUI)
        {
            _currentSwitchFunction = SwitchFunction.GOUI;
            SwitchType = SwitchType.Activate;
            ChangeControls?.Invoke(this);
            _switcher.UseGOUISwitcher(SwitchType);
            return true;
        }
        return false;
    }
    
    private bool MustActivateKeysFirst_Branch()
    {
        if(_currentSwitchFunction != SwitchFunction.Branch)
        {
            _currentSwitchFunction = SwitchFunction.Branch;
            SwitchType = SwitchType.Activate;
            ChangeControls?.Invoke(this);
            
            var activeBranch = _myDataHub.ActiveBranch;
            activeBranch.GroupIndex = BranchGroups.SwitchBranchGroup(activeBranch.BranchGroupsList, 
                                                                      activeBranch.GroupIndex,
                                                                      SwitchType.Activate);
            return true;
        }
        return false;
    }
}