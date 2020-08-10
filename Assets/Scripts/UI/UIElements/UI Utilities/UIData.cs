using System;

public class CustomEventHandler<T>
{
    private Action<T> _call;
    private Action _callNoParameter;

    public Action<T> Add(Action<T> subscriber)
    {
        _call = subscriber;
        return Event;
    }
    
    public Action Add(Action subscriber)
    {
        _callNoParameter = subscriber;
        return Event;
    }

    public void Event(T variable) => _call?.Invoke(variable);
    public void Event() => _callNoParameter?.Invoke();
}

public class UIData
{
    private readonly CustomEventHandler<bool> _gameIsPaused = new CustomEventHandler<bool>();
    private readonly CustomEventHandler<UINode> _highlightedNode = new CustomEventHandler<UINode>();
    private readonly CustomEventHandler<UINode> _selectedNode = new CustomEventHandler<UINode>();
    private readonly CustomEventHandler<UIBranch> _addResolvePopUp = new CustomEventHandler<UIBranch>();
    private readonly CustomEventHandler<UIBranch> _addOptionalPopUp = new CustomEventHandler<UIBranch>();
    private readonly CustomEventHandler<UIBranch> _activeBranch = new CustomEventHandler<UIBranch>();
    private readonly CustomEventHandler<object> _onStartUp = new CustomEventHandler<object>();
    private readonly CustomEventHandler<bool> _onHomeScreen = new CustomEventHandler<bool>();
    private readonly CustomEventHandler<bool> _allowKeys = new CustomEventHandler<bool>();
    private readonly CustomEventHandler<bool> _inMenu = new CustomEventHandler<bool>();
    private readonly CustomEventHandler<bool> _noResolvePopUps = new CustomEventHandler<bool>();
    private readonly CustomEventHandler<bool> _noOptionalPopUps = new CustomEventHandler<bool>();
    private readonly CustomEventHandler<object> _fromHotKey = new CustomEventHandler<object>();

    public void OnDisable()
    {
        PauseMenu.GamePaused -= _gameIsPaused.Event;
        UINode.DoHighlighted -= _highlightedNode.Event;
        UINode.DoSelected -= _selectedNode.Event;
        UIBranch.DoActiveBranch -= _activeBranch.Event;
        UIHub.OnStart -= _onStartUp.Event;
        UIHomeGroup.DoOnHomeScreen -= _onHomeScreen.Event;
        ChangeControl.DoAllowKeys -= _allowKeys.Event;
        MenuAndGameSwitching.IsInTheMenu -= _inMenu.Event;
        PopUpController.NoResolvePopUps -= _noResolvePopUps.Event;
        PopUpController.NoOptionalPopUps -= _noOptionalPopUps.Event;
        HotKeys.FromHotKey -= _fromHotKey.Event;
        UIPopUp.AddResolvePopUp -= _addResolvePopUp.Event;
        UIPopUp.AddOptionalPopUp -= _addOptionalPopUp.Event;
    }

    public void SubscribeToGameIsPaused(Action<bool> subscriber) 
        => PauseMenu.GamePaused += _gameIsPaused.Add(subscriber);

    public void SubscribeToHighlightedNode(Action<UINode> subscriber) 
        => UINode.DoHighlighted += _highlightedNode.Add(subscriber);

    public void SubscribeToSelectedNode(Action<UINode> subscriber) 
        => UINode.DoSelected += _selectedNode.Add(subscriber);

    public void SubscribeToAddResolvePopUp(Action<UIBranch> subscriber) 
        => UIPopUp.AddResolvePopUp += _addResolvePopUp.Add(subscriber);

    public void SubscribeToAddOptionalPopUp(Action<UIBranch> subscriber) 
        => UIPopUp.AddOptionalPopUp += _addOptionalPopUp.Add(subscriber);

    public void SubscribeToActiveBranch(Action<UIBranch> subscriber) 
        => UIBranch.DoActiveBranch += _activeBranch.Add(subscriber);

    public void SubscribeToOnStart(Action subscriber) 
        => UIHub.OnStart += _onStartUp.Add(subscriber);

    public void SubscribeToOnHomeScreen(Action<bool> subscriber) 
        => UIHomeGroup.DoOnHomeScreen += _onHomeScreen.Add(subscriber);

    public void SubscribeToAllowKeys(Action<bool> subscriber) 
        => ChangeControl.DoAllowKeys += _allowKeys.Add(subscriber);

    public void SubscribeToInMenu(Action<bool> subscriber) 
        => MenuAndGameSwitching.IsInTheMenu += _inMenu.Add(subscriber);

    public void SubscribeNoResolvePopUps(Action<bool> subscriber) 
        => PopUpController.NoResolvePopUps += _noResolvePopUps.Add(subscriber);

    public void SubscribeNoOptionalPopUps(Action<bool> subscriber) 
        => PopUpController.NoOptionalPopUps += _noOptionalPopUps.Add(subscriber);
    
    public void SubscribeFromHotKey(Action subscriber) 
        => HotKeys.FromHotKey += _fromHotKey.Add(subscriber);
}
