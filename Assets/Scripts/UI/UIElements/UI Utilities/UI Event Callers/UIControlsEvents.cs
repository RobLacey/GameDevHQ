using System;

public class UIControlsEvents : UiEventCaller
{
    private CustomEventHandler<bool> _gameIsPaused;
    private CustomEventHandler<bool> _allowKeys;
    private CustomEventHandler<object> _fromHotKey;
    private CustomEventHandler<object> _onCancel;
    private CustomEventHandler<object> _onChangeControls;
    private CustomEventHandler<EscapeKey> _cancelOrBackButtonPressed;
    private CustomEventHandler<SwitchType> _switchGroups;

    protected override void OnExit()
    {
        if (_gameIsPaused != null) UIHub.OnGamePaused -= _gameIsPaused.Event;
        if (_allowKeys != null) ChangeControl.DoAllowKeys -= _allowKeys.Event;
        if (_fromHotKey != null) HotKeys.FromHotKey -= _fromHotKey.Event;
        if (_cancelOrBackButtonPressed != null) UINode.DoCancelButtonPressed -= _cancelOrBackButtonPressed.Event;
        if (_onCancel != null) UIHub.OnCancelPressed -= _onCancel.Event;
        if (_onChangeControls != null) UIHub.OnChangeControls -= _onChangeControls.Event;
        if (_switchGroups != null) UIHub.OnSwitchGroupsPressed -= _switchGroups.Event;
    }

    public void SubscribeToGameIsPaused(Action<bool> subscriber)
    {
        _gameIsPaused = new CustomEventHandler<bool>();
        UIHub.OnGamePaused += _gameIsPaused.Add(subscriber);
    }

    public void SubscribeToAllowKeys(Action<bool> subscriber)
    {
        _allowKeys = new CustomEventHandler<bool>();
        ChangeControl.DoAllowKeys += _allowKeys.Add(subscriber);
    }

    public void SubscribeFromHotKey(Action subscriber)
    {
        _fromHotKey = new CustomEventHandler<object>();
        HotKeys.FromHotKey += _fromHotKey.Add(subscriber);
    }

    public void SubscribeOnCancel(Action subscriber)
    {
        _onCancel = new CustomEventHandler<object>();
        UIHub.OnCancelPressed += _onCancel.Add(subscriber);
    }

    public void SubscribeOnChangeControls(Action subscriber)
    {
        _onChangeControls = new CustomEventHandler<object>();
        UIHub.OnChangeControls += _onChangeControls.Add(subscriber);
    }

    public void SubscribeCancelOrBackButtonPressed(Action<EscapeKey> subscriber)
    {
        _cancelOrBackButtonPressed = new CustomEventHandler<EscapeKey>();
        UINode.DoCancelButtonPressed += _cancelOrBackButtonPressed.Add(subscriber);
    }

    public void SubscribeSwitchGroups(Action<SwitchType> subscriber)
    {
        _switchGroups = new CustomEventHandler<SwitchType>();
        UIHub.OnSwitchGroupsPressed += _switchGroups.Add(subscriber);
    }

}

