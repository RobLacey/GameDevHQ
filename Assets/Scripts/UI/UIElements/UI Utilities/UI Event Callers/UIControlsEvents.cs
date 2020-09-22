using System;

public class UIControlsEvents : UiEventCaller
{
    private CustomEventHandler<bool> _allowKeys;
    private CustomEventHandler _fromHotKey;
    private CustomEventHandler _onCancel;
    private CustomEventHandler _onChangeControls;
    private CustomEventHandler<EscapeKey> _cancelOrBackButtonPressed;
    private CustomEventHandler<SwitchType> _switchGroups;
    private CustomReturnEventHandler<bool> _menuAndGameSwitching;
    private CustomReturnEventHandler<bool> _hotKeyActivation;
    private CustomEventHandler _pausedPressed;
    private CustomEventHandler<UIBranch> _turnOffChildBranches;
    private CustomEventHandler<(UIBranch moveFrom, UIBranch moveToo)> _moveToNewBranch;

    protected override void OnExit()
    {
        if (_allowKeys != null) ChangeControl.DoAllowKeys -= _allowKeys.Event;
        if (_fromHotKey != null) HotKeys.FromHotKey -= _fromHotKey.Event;
        if (_cancelOrBackButtonPressed != null) UINode.DoCancelButtonPressed -= _cancelOrBackButtonPressed.Event;
        if (_onCancel != null) UIInput.OnCancelPressed -= _onCancel.Event;
        if (_onChangeControls != null) UIInput.OnChangeControls -= _onChangeControls.Event;
        if (_switchGroups != null) UIInput.OnSwitchGroupsPressed -= _switchGroups.Event;
        if (_menuAndGameSwitching != null) UIInput.OnGameToMenuSwitchPressed -= _menuAndGameSwitching.Event;
        if (_hotKeyActivation != null) UIInput.HotKeyActivated -= _hotKeyActivation.Event;
        if (_pausedPressed != null) UIInput.OnPausedPressed -= _pausedPressed.Event;
        if (_moveToNewBranch != null) UINavigation.onMoveToBranch -= _moveToNewBranch.Event;
        if (_turnOffChildBranches != null) UINavigation.onTurnOffChildBranches -= _turnOffChildBranches.Event;
    }
    
    public void SubscribeToAllowKeys(Action<bool> subscriber)
    {
        _allowKeys = new CustomEventHandler<bool>();
        ChangeControl.DoAllowKeys += _allowKeys.Add(subscriber);
    }

    public void SubscribeFromHotKey(Action subscriber)
    {
        _fromHotKey = new CustomEventHandler();
        HotKeys.FromHotKey += _fromHotKey.Add(subscriber);
    }

    public void SubscribeOnCancel(Action subscriber)
    {
        _onCancel = new CustomEventHandler();
        UIInput.OnCancelPressed += _onCancel.Add(subscriber);
    }

    public void SubscribeOnChangeControls(Action subscriber)
    {
        _onChangeControls = new CustomEventHandler();
        UIInput.OnChangeControls += _onChangeControls.Add(subscriber);
    }

    public void SubscribeCancelOrBackButtonPressed(Action<EscapeKey> subscriber)
    {
        _cancelOrBackButtonPressed = new CustomEventHandler<EscapeKey>();
        UINode.DoCancelButtonPressed += _cancelOrBackButtonPressed.Add(subscriber);
    }

    public void SubscribeSwitchGroups(Action<SwitchType> subscriber)
    {
        _switchGroups = new CustomEventHandler<SwitchType>();
        UIInput.OnSwitchGroupsPressed += _switchGroups.Add(subscriber);
    }
    
    public void SubscribeMenuGameSwitching(Func<bool> subscriber)
    {
        _menuAndGameSwitching = new CustomReturnEventHandler<bool>();
        UIInput.OnGameToMenuSwitchPressed += _menuAndGameSwitching.Add(subscriber);
    }
    
    public void SubscribeHotKeyActivation(Func<bool> subscriber)
    {
        _hotKeyActivation = new CustomReturnEventHandler<bool>();
        UIInput.HotKeyActivated += _hotKeyActivation.Add(subscriber);
    }
    
    public void SubscribePausedPressed(Action subscriber)
    {
        _pausedPressed = new CustomEventHandler();
        UIInput.OnPausedPressed += _pausedPressed.Add(subscriber);
    }
    
    public void SubscribeMoveToChildBranch(Action<(UIBranch moveFrom, UIBranch moveToo)> subscriber)
    {
        _moveToNewBranch = new CustomEventHandler<(UIBranch, UIBranch)>();
        UINavigation.onMoveToBranch += _moveToNewBranch.Add(subscriber);
    }
    
    public void SubscribeTurnOffChildBranches(Action<UIBranch> subscriber)
    {
        _turnOffChildBranches = new CustomEventHandler<UIBranch>();
        UINavigation.onTurnOffChildBranches += _turnOffChildBranches.Add(subscriber);
    }

}

