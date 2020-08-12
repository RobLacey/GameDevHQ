using System;
using UnityEngine;

public class UIDataEvents : UiEventCaller
{
    private CustomEventHandler<UINode> _highlightedNode;
    private CustomEventHandler<UINode> _selectedNode;
    private CustomEventHandler<UIBranch> _activeBranch;
    private CustomEventHandler<UIBranch> _currentHomeScreen;
    private CustomEventHandler<object> _onStartUp;
    private CustomEventHandler<UIBranch> _onBackOneLevel;
    private CustomEventHandler<bool> _onHomeScreen;
    private CustomEventHandler<bool> _inMenu;

    protected override void OnExit()
    {
        if (_highlightedNode != null) UINode.DoHighlighted -= _highlightedNode.Event;
        if (_selectedNode != null) UINode.DoSelected -= _selectedNode.Event;
        if (_activeBranch != null) UIBranch.DoActiveBranch -= _activeBranch.Event;
        if (_onStartUp != null) UIHub.OnStart -= _onStartUp.Event;
        if (_onHomeScreen != null) UIBranch.SetIsOnHomeScreen -= _onHomeScreen.Event;
        if (_inMenu != null) MenuAndGameSwitching.IsInTheMenu -= _inMenu.Event;
        if (_currentHomeScreen != null) UIHomeGroup.DoCurrentHomeBranch -= _currentHomeScreen.Event;
    }


    public void SubscribeToHighlightedNode(Action<UINode> subscriber)
    {
        _highlightedNode = new CustomEventHandler<UINode>();
        UINode.DoHighlighted += _highlightedNode.Add(subscriber);
    }

    public void SubscribeToSelectedNode(Action<UINode> subscriber)
    {
        _selectedNode = new CustomEventHandler<UINode>();
        UINode.DoSelected += _selectedNode.Add(subscriber);
    }

    public void SubscribeToCurrentHomeScreen(Action<UIBranch> subscriber)
    {
        _currentHomeScreen = new CustomEventHandler<UIBranch>();
        UIHomeGroup.DoCurrentHomeBranch += _currentHomeScreen.Add(subscriber);
    }

    public void SubscribeToActiveBranch(Action<UIBranch> subscriber)
    {
        _activeBranch = new CustomEventHandler<UIBranch>();
        UIBranch.DoActiveBranch += _activeBranch.Add(subscriber);
    }

    public void SubscribeToOnStart(Action subscriber)
    {
        _onStartUp = new CustomEventHandler<object>();
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
        UIBranch.SetIsOnHomeScreen += _onHomeScreen.Add(subscriber);
    }

    public void SubscribeToInMenu(Action<bool> subscriber)
    {
        _inMenu = new CustomEventHandler<bool>();
        MenuAndGameSwitching.IsInTheMenu += _inMenu.Add(subscriber);
    }
}
