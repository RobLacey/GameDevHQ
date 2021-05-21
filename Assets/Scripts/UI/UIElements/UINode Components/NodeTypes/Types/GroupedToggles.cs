using System;
using System.Collections.Generic;
using System.Linq;

public interface IGroupedToggles : INodeBase { }

public class GroupedToggles : NodeBase, IGroupedToggles
{
    public GroupedToggles(INode node) : base(node)
    {
        SetUpToggleData();
        SetUpOtherVariables();
        SelectedToggle += SaveSelectedNode;
        _delayTimer = EZInject.Class.NoParams<IDelayTimer>();
    }

    private void SetUpToggleData()
    {
        var toggleData = _uiNode.ToggleData;
        _tabBranch = toggleData.ReturnTabBranch;
        _myToggleGroupId = toggleData.ReturnToggleId;
        _startAsSelected = toggleData.StartAsSelected == IsActive.Yes;
    }

    private void SetUpOtherVariables()
    {
        _hasATabBranch = _tabBranch != null;
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    //Variables
    private readonly List<INode> _toggleGroupMembers = new List<INode>();
    private IBranch _tabBranch;
    private bool _hasATabBranch;
    private int _hasAGroupStartPoint;
    private ToggleGroup _myToggleGroupId;
    private bool _startAsSelected;
    private float _autoOpenDelay;
    private readonly IDelayTimer _delayTimer;
    
    //Properties
    private List<INode> AllNodes => _uiNode.MyBranch.ThisGroupsUiNodes.ToList();

    //Events
    private static event Action<INode, Action> SelectedToggle;

    //Main
    public override void OnStart()
    {
        base.OnStart();
        if(_uiNode.HasChildBranch is null) return;
        
        SetUpToggleGroup();
        SetUpTabBranch();
        if (_startAsSelected)
        {
            SetNodeAsSelected_NoEffects();
            TurnOnTab();
        }
    }
    
    private void SetUpToggleGroup()
    {
        foreach (var node in AllNodes.Where(node => node.IsToggleGroup)
                                      .Where(node => _myToggleGroupId == node.ToggleData.ReturnToggleId))
        {
            _toggleGroupMembers.Add(node);
            CheckForStartPosition(node);
        }

        AssignStartIfNonExists();
        _toggleGroupMembers.Remove(_uiNode);
    }

    private void AssignStartIfNonExists()
    {
        if (_hasAGroupStartPoint != 0 || _toggleGroupMembers.First() != _uiNode) return;
        SetStartAsSelected();
    }

    private void SetStartAsSelected()
    {
        _uiNode.ToggleData.SetStartAsSelected();
        _startAsSelected = true;
    }

    private void CheckForStartPosition(INode node)
    {
        if (node.ToggleData.StartAsSelected == IsActive.Yes)
        {
            _hasAGroupStartPoint++;
        }        
        else if (_hasAGroupStartPoint > 1)
        {
            throw new Exception("To many start Point : " + _uiNode);
        }
    }

    private void SetUpTabBranch()
    {
        if (_hasATabBranch)
            _tabBranch.SetUpAsTabBranch();
    }
    
    public override void OnEnter()
    {
        base.OnEnter();
        
        if (_uiNode.CanAutoOpen && !IsSelected)
        {
            _delayTimer.SetDelay(_autoOpenDelay)
                       .StartTimer(StartAutoOpen);
        }
    }
    
    private void StartAutoOpen() => TurnNodeOnOff();

    public override void OnExit()
    {
        base.OnExit();
        if (_uiNode.CanAutoOpen)
        {
            _delayTimer.StopTimer();
        }
    }
    
    private void SaveSelectedNode(INode newNode, Action callback)
    {
        if (!_toggleGroupMembers.Contains(newNode)) return;
        if (!IsSelected) return;
        SetAsNotActive(callback);
    }

    protected override void TurnNodeOnOff()
    {
        if (IsSelected) return;        
        TurnOffOtherTogglesInGroup();
        Activate();
    }

    protected override void Activate()
    {
        TurnOnTab();
        SetSelectedStatus(true, DoPressOnNode);
        ThisNodeIsSelected();
    }

    private void SetAsNotActive(Action callback = null)
    {
        SetNodeAsNotSelected_NoEffects();
        if (!_hasATabBranch) return;
        _tabBranch.StartBranchExitProcess(OutTweenType.Cancel, callback);
    }

    private void TurnOffOtherTogglesInGroup() => SelectedToggle?.Invoke(_uiNode, TurnOnTab);

    private void TurnOnTab()
    {
        if (!_hasATabBranch) return;
        _tabBranch.DontSetBranchAsActive();
        _tabBranch.MoveToThisBranch();
    }
}
