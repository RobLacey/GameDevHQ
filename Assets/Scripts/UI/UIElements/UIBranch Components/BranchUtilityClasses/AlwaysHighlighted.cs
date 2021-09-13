using System;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UnityEngine;

public interface IAlwaysHighlightSettings : IParameters
{
    IBranch MyBranch { get; }
    Override Overridden { get; }
    bool FunctionNotActive();
    INode UiNode { get; }
    bool IsSelected { get; }
    Action DoPointerOverSetUp { get; }
    Action DoPointerNotOver { get; }
    bool OptionalStartConditions { get; }
}

public interface IAlwaysHighlight: IEZEventUser
{
    bool CanAllow();
    void ShowStartingHighlightedNode();
}

public class AlwaysHighlighted : IServiceUser, IAlwaysHighlight
{
    public AlwaysHighlighted(IAlwaysHighlightSettings settings)
    {
        _myBranch = settings.MyBranch;
        _settings = settings;
        UseEZServiceLocator();
    }

    private readonly IBranch _myBranch;
    private IDataHub _myDataHub;
    private readonly IAlwaysHighlightSettings _settings;

    //Properties, Set/Getters
    private bool AlwaysHighlightedOverride => _settings.Overridden == Override.Override;
    private bool IsAlwaysHighlighted => _myBranch.AlwaysHighlighted == IsActive.Yes;
    private void SetUpOnStart(IOnStart args) => ShowStartingHighlightedNode();

    //Main
    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<IOnStart>(SetUpOnStart);

    public void UnObserveEvents() => HistoryEvents.Do.Unsubscribe<IOnStart>(SetUpOnStart);

    public bool CanAllow()
    {
        if (_myBranch.LastHighlighted.IsToggleGroup || _myDataHub.Selected == _myBranch.LastSelected)
        {
            _settings.DoPointerNotOver.Invoke();
            return true;
        }
    
        return AlwaysHighlightedAndNotSelected() && !AlwaysHighlightedOverride;
    
        bool AlwaysHighlightedAndNotSelected() => IsAlwaysHighlighted && !_settings.IsSelected;
    }

    public void ShowStartingHighlightedNode()
    {
        if(_settings.FunctionNotActive() || AlwaysHighlightedOverride || _settings.OptionalStartConditions) return;

        if (IsAlwaysHighlighted && _myBranch.DefaultStartOnThisNode == _settings.UiNode)
        {
            _settings.DoPointerOverSetUp.Invoke();
        }
    }
}