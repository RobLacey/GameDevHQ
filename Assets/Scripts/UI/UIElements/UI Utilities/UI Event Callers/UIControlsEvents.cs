using System;

public class UIControlsEvents : UiEventCaller
{
    //private CustomEventHandler<bool> _allowKeys;
   // private CustomEventHandler _onCancel;
    //private CustomEventHandler _onChangeControls;
    //private CustomEventHandler<EscapeKey> _cancelOrBackButtonPressed;
   // private CustomEventHandler<SwitchType> _switchGroups;
    //private CustomEventHandler _menuAndGameSwitching;
   // private CustomEventHandler _pausedPressed;

    protected override void OnExit()
    {
        //if (_allowKeys != null) ChangeControl.DoAllowKeys -= _allowKeys.Event;
        //if (_cancelOrBackButtonPressed != null) UINode.DoCancelButtonPressed -= _cancelOrBackButtonPressed.Event;
        //if (_onCancel != null) UIInput.OnCancelPressed -= _onCancel.Event;
       // if (_onChangeControls != null) UIInput.OnChangeControls -= _onChangeControls.Event;
       // if (_switchGroups != null) UIInput.OnSwitchGroupsPressed -= _switchGroups.Event;
        //if (_menuAndGameSwitching != null) UIInput.OnGameToMenuSwitchPressed -= _menuAndGameSwitching.Event;
       // if (_pausedPressed != null) UIInput.OnPausedPressed -= _pausedPressed.Event;
    }
    
    // public void SubscribeToAllowKeys(Action<bool> subscriber)
    // {
    //     _allowKeys = new CustomEventHandler<bool>();
    //     ChangeControl.DoAllowKeys += _allowKeys.Add(subscriber);
    // }

    // public void SubscribeOnCancel(Action subscriber)
    // {
    //     _onCancel = new CustomEventHandler();
    //     UIInput.OnCancelPressed += _onCancel.Add(subscriber);
    // }

    // public void SubscribeOnChangeControls(Action subscriber)
    // {
    //     _onChangeControls = new CustomEventHandler();
    //     UIInput.OnChangeControls += _onChangeControls.Add(subscriber);
    // }

    // public void SubscribeCancelOrBackButtonPressed(Action<EscapeKey> subscriber)
    // {
    //     _cancelOrBackButtonPressed = new CustomEventHandler<EscapeKey>();
    //     UINode.DoCancelButtonPressed += _cancelOrBackButtonPressed.Add(subscriber);
    // }

    // public void SubscribeSwitchGroups(Action<SwitchType> subscriber)
    // {
    //     _switchGroups = new CustomEventHandler<SwitchType>();
    //     UIInput.OnSwitchGroupsPressed += _switchGroups.Add(subscriber);
    // }
    
    // public void SubscribeMenuGameSwitching(Action subscriber)
    // {
    //     _menuAndGameSwitching = new CustomEventHandler();
    //     UIInput.OnGameToMenuSwitchPressed += _menuAndGameSwitching.Add(subscriber);
    // }
    
    // public void SubscribePausedPressed(Action subscriber)
    // {
    //     _pausedPressed = new CustomEventHandler();
    //     UIInput.OnPausedPressed += _pausedPressed.Add(subscriber);
    // }
}