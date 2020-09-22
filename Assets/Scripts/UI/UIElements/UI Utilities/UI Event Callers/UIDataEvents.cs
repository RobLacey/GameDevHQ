using System;

public class UIDataEvents : UiEventCaller
{
    private CustomEventHandler<bool> _gameIsPaused;
    private CustomEventHandler<INode> _highlightedNode;
    private CustomEventHandler<INode> _selectedNode;
    private CustomEventHandler<UIBranch> _activeBranch;
    private CustomEventHandler<UIBranch> _currentHomeScreen;
    private CustomEventHandler<UIBranch> _setUpBranchesAtStart;
    private CustomEventHandler _onStartUp;
    private CustomEventHandler<UIBranch> _onBackOneLevel;
    private CustomEventHandler<bool> _onHomeScreen;
    private CustomEventHandler<bool> _inMenu;

    protected override void OnExit()
    {
        if (_gameIsPaused != null) PauseMenu.OnGamePaused -= _gameIsPaused.Event;
        if (_highlightedNode != null) UINode.DoHighlighted -= _highlightedNode.Event;
        if (_selectedNode != null) UINode.DoSelected -= _selectedNode.Event;
        if (_activeBranch != null) UIBranch.DoActiveBranch -= _activeBranch.Event;
        if (_currentHomeScreen != null) UIHomeGroup.DoSetCurrentHomeBranch -= _currentHomeScreen.Event;
        if (_setUpBranchesAtStart != null) UIHub.SetUpBranchesAtStart -= _setUpBranchesAtStart.Event;
        if (_onStartUp != null) UIHub.OnStart -= _onStartUp.Event;
        if (_onBackOneLevel != null) UICancel.OnBackOneLevel -= _onBackOneLevel.Event;
        if (_onHomeScreen != null) BranchBase.SetIsOnHomeScreen -= _onHomeScreen.Event;
        if (_inMenu != null) MenuAndGameSwitching.IsInTheMenu -= _inMenu.Event;
    }

    public void SubscribeToGameIsPaused(Action<bool> subscriber)
    {
        _gameIsPaused = new CustomEventHandler<bool>();
        PauseMenu.OnGamePaused += _gameIsPaused.Add(subscriber);
    }

    public void SubscribeToHighlightedNode(Action<INode> subscriber)
    {
        _highlightedNode = new CustomEventHandler<INode>();
        UINode.DoHighlighted += _highlightedNode.Add(subscriber);
    }

    public void SubscribeToSelectedNode(Action<INode> subscriber)
    {
        _selectedNode = new CustomEventHandler<INode>();
        UINode.DoSelected += _selectedNode.Add(subscriber);
    }

    public void SubscribeToCurrentHomeScreen(Action<UIBranch> subscriber)
    {
        _currentHomeScreen = new CustomEventHandler<UIBranch>();
        UIHomeGroup.DoSetCurrentHomeBranch += _currentHomeScreen.Add(subscriber);
    }

    public void SubscribeToActiveBranch(Action<UIBranch> subscriber)
    {
        _activeBranch = new CustomEventHandler<UIBranch>();
        UIBranch.DoActiveBranch += _activeBranch.Add(subscriber);
    }

    public void SubscribeToOnStart(Action subscriber)
    {
        _onStartUp = new CustomEventHandler();
        UIHub.OnStart += _onStartUp.Add(subscriber);
    }
    
    public void SubscribeToBackOneLevel(Action<UIBranch> subscriber)
    {
        _onBackOneLevel = new CustomEventHandler<UIBranch>();
        UICancel.OnBackOneLevel += _onBackOneLevel.Add(subscriber);
    }

    public void SubscribeToOnHomeScreen(Action<bool> subscriber)
    {
        _onHomeScreen = new CustomEventHandler<bool>();
        BranchBase.SetIsOnHomeScreen += _onHomeScreen.Add(subscriber);
    }

    public void SubscribeToInMenu(Action<bool> subscriber)
    {
        _inMenu = new CustomEventHandler<bool>();
        MenuAndGameSwitching.IsInTheMenu += _inMenu.Add(subscriber);
    }
    
    public void SubscribeSetUpBranchesAtStart(Action<UIBranch> subscriber)
    {
        _setUpBranchesAtStart = new CustomEventHandler<UIBranch>();
        UIHub.SetUpBranchesAtStart += _setUpBranchesAtStart.Add(subscriber);
    }

}
