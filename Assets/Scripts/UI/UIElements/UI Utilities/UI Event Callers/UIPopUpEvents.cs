using System;

public class UIPopUpEvents : UiEventCaller
{
    private CustomEventHandler<UIBranch> _addResolvePopUp;
    private CustomEventHandler<UIBranch> _addOptionalPopUp;
    private CustomEventHandler<bool> _noResolvePopUps;
    private CustomEventHandler<bool> _noOptionalPopUps;
    private CustomEventHandler<bool> _noPopUps;
    private CustomEventHandler<(UIBranch nextPopUp,UIBranch currentPopUp)> _toNextNodeFromPopUp;


    protected override void OnExit()
    {
        if (_noResolvePopUps != null) PopUpController.NoResolvePopUps -= _noResolvePopUps.Event;
        if (_noOptionalPopUps != null) PopUpController.NoOptionalPopUps -= _noOptionalPopUps.Event;
        if (_addResolvePopUp != null) ResolvePopUp.AddResolvePopUp -= _addResolvePopUp.Event;
        if (_addOptionalPopUp != null) OptionalPopUp.AddOptionalPopUp -= _addOptionalPopUp.Event;
        if (_noPopUps != null) PopUpController.NoPopUps -= _noPopUps.Event;
        if (_toNextNodeFromPopUp != null) PopUpController.MoveToNextFromPopUp -= _toNextNodeFromPopUp.Event;
    }

    public void SubscribeToAddResolvePopUp(Action<UIBranch> subscriber)
    {
        _addResolvePopUp = new CustomEventHandler<UIBranch>();
        ResolvePopUp.AddResolvePopUp += _addResolvePopUp.Add(subscriber);
    }

    public void SubscribeToAddOptionalPopUp(Action<UIBranch> subscriber)
    {
        _addOptionalPopUp = new CustomEventHandler<UIBranch>();
        OptionalPopUp.AddOptionalPopUp += _addOptionalPopUp.Add(subscriber);
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
    public void SubscribeToNextNodeFromPopUp(Action<(UIBranch nextPopUp, UIBranch currentPopUp)> subscriber)
    {
        _toNextNodeFromPopUp = new CustomEventHandler<(UIBranch nextPopUp, UIBranch currentPopUp)>();
        PopUpController.MoveToNextFromPopUp += _toNextNodeFromPopUp.Add(subscriber);
    }

    public void SubscribeNoPopUps(Action<bool> subscriber)
    {
        _noPopUps = new CustomEventHandler<bool>();
        PopUpController.NoPopUps += _noPopUps.Add(subscriber);
    }

}
