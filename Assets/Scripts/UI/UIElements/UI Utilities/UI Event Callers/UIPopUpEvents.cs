using System;

public class UIPopUpEvents : UiEventCaller
{
    private CustomEventHandler<UIBranch> _addResolvePopUp;
    private CustomEventHandler<UIBranch> _addOptionalPopUp;
    private CustomEventHandler<UIBranch> _backToNextPopUp;
    private CustomEventHandler<UIBranch> _removeOptionalPopUp;
    private CustomEventHandler<bool> _noResolvePopUps;
    private CustomEventHandler<bool> _noPopUps;
    private CustomEventHandler<(UIBranch nextPopUp,UIBranch currentPopUp)> _toNextNodeFromPopUp;
    private CustomReturnEventHandler<UIBranch> _returnNextPopUp;


    protected override void OnExit()
    {
        if (_addResolvePopUp != null) ResolvePopUp.AddResolvePopUp -= _addResolvePopUp.Event;
        if (_addOptionalPopUp != null) OptionalPopUp.AddOptionalPopUp -= _addOptionalPopUp.Event;
        if (_backToNextPopUp != null) UICancel.OnBackToAPopUp -= _backToNextPopUp.Event;
        if (_removeOptionalPopUp != null) OptionalPopUp.RemoveOptionalPopUp -= _removeOptionalPopUp.Event;
        if (_noResolvePopUps != null) PopUpController.NoResolvePopUps -= _noResolvePopUps.Event;
        if (_noPopUps != null) PopUpController.NoPopUps -= _noPopUps.Event;
        if (_toNextNodeFromPopUp != null) PopUpController.MoveToNextFromPopUp -= _toNextNodeFromPopUp.Event;
        if (_returnNextPopUp != null) ChangeControl.ReturnNextPopUp -= _returnNextPopUp.Event;
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
    
    public void SubscribeToBackToAPopUp(Action<UIBranch> subscriber)
    {
        _backToNextPopUp = new CustomEventHandler<UIBranch>();
        UICancel.OnBackToAPopUp += _backToNextPopUp.Add(subscriber);
    }
    
    public void SubscribeToRemoveOptionalPopUp(Action<UIBranch> subscriber)
    {
        _removeOptionalPopUp = new CustomEventHandler<UIBranch>();
        OptionalPopUp.RemoveOptionalPopUp += _removeOptionalPopUp.Add(subscriber);
    }

    public void SubscribeNoResolvePopUps(Action<bool> subscriber)
    {
        _noResolvePopUps = new CustomEventHandler<bool>();
        PopUpController.NoResolvePopUps += _noResolvePopUps.Add(subscriber);
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
    public void SubscribeToReturnNextPopUp(Func<UIBranch> subscriber)
    {
        _returnNextPopUp = new CustomReturnEventHandler<UIBranch>();
        ChangeControl.ReturnNextPopUp += _returnNextPopUp.Add(subscriber);
    }

}
