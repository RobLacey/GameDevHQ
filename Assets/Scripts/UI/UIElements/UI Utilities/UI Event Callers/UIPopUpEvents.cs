using System;

public class UIPopUpEvents : UiEventCaller
{
    private CustomEventHandler<UIBranch> _addResolvePopUp;
    private CustomEventHandler<UIBranch> _addOptionalPopUp;
    private CustomEventHandler<bool> _noResolvePopUps;
    private CustomEventHandler<bool> _noOptionalPopUps;
    private CustomEventHandler<bool> _noPopUps;


    protected override void OnExit()
    {
        if (_noResolvePopUps != null) PopUpController.NoResolvePopUps -= _noResolvePopUps.Event;
        if (_noOptionalPopUps != null) PopUpController.NoOptionalPopUps -= _noOptionalPopUps.Event;
        if (_addResolvePopUp != null) UIPopUp.AddResolvePopUp -= _addResolvePopUp.Event;
        if (_addOptionalPopUp != null) UIPopUp.AddOptionalPopUp -= _addOptionalPopUp.Event;
        if (_noPopUps != null) PopUpController.NoPopUps -= _noPopUps.Event;
    }

    public void SubscribeToAddResolvePopUp(Action<UIBranch> subscriber)
    {
        _addResolvePopUp = new CustomEventHandler<UIBranch>();
        UIPopUp.AddResolvePopUp += _addResolvePopUp.Add(subscriber);
    }

    public void SubscribeToAddOptionalPopUp(Action<UIBranch> subscriber)
    {
        _addOptionalPopUp = new CustomEventHandler<UIBranch>();
        UIPopUp.AddOptionalPopUp += _addOptionalPopUp.Add(subscriber);
    }

    public void SubscribeNoResolvePopUps(Action<bool> subscriber)
    {
        _noResolvePopUps = new CustomEventHandler<bool>();
        PopUpController.NoResolvePopUps += _noResolvePopUps.Add(subscriber);
    }

    public void SubscribeNoOptionalPopUps(Action<bool> subscriber)
    {
        _noOptionalPopUps = new CustomEventHandler<bool>();
        PopUpController.NoOptionalPopUps += _noOptionalPopUps.Add(subscriber);
    }

    public void SubscribeNoPopUps(Action<bool> subscriber)
    {
        _noPopUps = new CustomEventHandler<bool>();
        PopUpController.NoPopUps += _noPopUps.Add(subscriber);
    }

}
