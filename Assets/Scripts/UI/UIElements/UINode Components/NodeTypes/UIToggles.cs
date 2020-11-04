using System;
using System.Collections.Generic;

public class UIToggles : NodeBase
{
    private readonly List<UINode> _toggleGroupMembers = new List<UINode>();
    private readonly ToggleGroup _groupID;
    private readonly UIBranch _tabBranch;
    private readonly bool _hasATabBranch;
    private bool _isSelected;

    //Events
    private static event Action<UINode, Action> SelectedToggle;
    
    public UIToggles(UINode node) : base(node)
    {
        _groupID = _uiNode.ToggleGroupID;
        _tabBranch = _uiNode.ToggleBranch;
        _hasATabBranch = _tabBranch != null;
        SelectedToggle += SaveSelectedNode;
        if (_tabBranch)
            _tabBranch.IsTabBranch();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.Subscribe<IActiveBranch>(Activate, this);
    }

    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.Unsubscribe<IActiveBranch>(Activate);
    }

    private void Activate(IActiveBranch args)
    {
        if (args.ActiveBranch == _uiNode.MyBranch && _isSelected)
            TurnOnTab();
    }
    
    //Main
    public override void Start()
    {
        base.Start();
        SetUpToggleGroup(_uiNode.MyBranch.ThisGroupsUiNodes);
        if (!_uiNode.StartAsSelected) return;
        TurnOffOtherTogglesInGroup();
        _uiNode.SetNodeAsSelected_NoEffects();
        TurnOnTab();
    }

    public override void DeactivateNode() { }

    private void SetUpToggleGroup(UINode[] thisGroupsUINodes)
    {
        foreach (var node in thisGroupsUINodes)
        {
            if (!node.IsToggleGroup || node == _uiNode) continue;
            if (_groupID != node.ToggleGroupId) continue;
            _toggleGroupMembers.Add(node);
        }
    }
    
    private void SaveSelectedNode(UINode newNode, Action callback)
    {
        if (!_toggleGroupMembers.Contains(newNode)) return;
        Deactivate(callback);
    }

    private void Deactivate(Action callback)
    {
        _uiNode.SetNodeAsNotSelected_NoEffects();
        
        if (!_hasATabBranch || !_isSelected) return;
        _tabBranch.StartBranchExitProcess(OutTweenType.Cancel, callback);
        _isSelected = false;
    }

    protected override void TurnNodeOnOff()
    {
        if (_uiNode.IsSelected) return;        

        TurnOffOtherTogglesInGroup();
        Activate();
        _uiHistoryTrack.SetSelected(_uiNode);
    }

    private void Activate() => _uiNode.SetSelectedStatus(true, _uiNode.DoPress);

    private void TurnOffOtherTogglesInGroup()
    {
        SelectedToggle?.Invoke(_uiNode, TurnOnTab);
        _isSelected = true;
    }

    private void TurnOnTab()
    {
        if (!_hasATabBranch) return;
        _tabBranch.DontSetBranchAsActive();
        _tabBranch.MoveToThisBranch();
    }
}
