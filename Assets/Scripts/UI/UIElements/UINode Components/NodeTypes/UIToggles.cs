using System;
using System.Collections.Generic;
using UnityEngine;

public class UIToggles : INodeBase, IEventUser, IServiceUser, IDisposable
{
    private readonly UINode _myNode;
    private readonly List<UINode> _toggleGroupMembers = new List<UINode>();
    private readonly ToggleGroup _groupID;
    private readonly UIBranch _tabBranch;
    private readonly bool _hasATabBranch;
    private bool _isSelected;
    private IHistoryTrack _uiHistoryTrack;

    //Events
    private static event Action<UINode> SelectedToggle;
    
    public UIToggles(UINode node)
    {
        ObserveEvents();
        SubscribeToService();
        _myNode = node;
        _groupID = _myNode.ToggleGroupID;
        _tabBranch = _myNode.ToggleBranch;
        _hasATabBranch = _tabBranch != null;
        SelectedToggle += SaveSelectedNode;
        if (_tabBranch)
            _tabBranch.IsTabBranch();
    }

    public void Dispose()
    {
        Debug.Log(_myNode);
        RemoveFromEvents();
    }

    public void ObserveEvents() => EventLocator.Subscribe<IActiveBranch>(Activate, this);

    public void RemoveFromEvents() => EventLocator.Unsubscribe<IActiveBranch>(Activate);

    public void SubscribeToService() => _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);

    private void Activate(IActiveBranch args)
    {
        if (args.ActiveBranch == _myNode.MyBranch && _isSelected)
            TurnOnTab();
    }
    
    //Main
    public void Start()
    {
        SetUpToggleGroup(_myNode.MyBranch.ThisGroupsUiNodes);
        if (!_myNode.StartAsSelected) return;
        TurnOffOtherTogglesInGroup();
        _myNode.SetNodeAsSelected_NoEffects();
    }

    public void DeactivateNode() { }

    private void SetUpToggleGroup(UINode[] thisGroupsUINodes)
    {
        foreach (var node in thisGroupsUINodes)
        {
            if (!node.IsToggleGroup || node == _myNode) continue;
            if (_groupID != node.ToggleGroupId) continue;
            _toggleGroupMembers.Add(node);
        }
    }
    
    private void SaveSelectedNode(UINode newNode)
    {
        if (!_toggleGroupMembers.Contains(newNode)) return;
        _myNode.SetNodeAsNotSelected_NoEffects();

        if (!_hasATabBranch || !_isSelected) return;
        _tabBranch.StartBranchExitProcess(OutTweenType.Cancel);
        _isSelected = false;
    }
    
    public void TurnNodeOnOff()
    {
        if (_myNode.IsSelected) return;        

        TurnOffOtherTogglesInGroup();
        ActivateNode();
        _uiHistoryTrack.SetSelected(_myNode);
    }

    private void ActivateNode() => _myNode.SetSelectedStatus(true, _myNode.DoPress);

    private void TurnOffOtherTogglesInGroup()
    {
        SelectedToggle?.Invoke(_myNode);
        _isSelected = true;
        TurnOnTab();
    }

    private void TurnOnTab()
    {
        if (!_hasATabBranch) return;
        _tabBranch.DontSetBranchAsActive();
        _tabBranch.MoveToThisBranch();
    }
}
