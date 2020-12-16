using System;
using System.Collections.Generic;
using System.Linq;

public interface ILinkedToggles : INodeBase { }

public class LinkedToggles : NodeBase, ILinkedToggles
{
    public LinkedToggles(INode node) : base(node)
    {
        var toggleData = node.ToggleData;
        _tabBranch = toggleData.ReturnTabBranch;
        _hasATabBranch = _tabBranch != null;
        _allNodes = _uiNode.MyBranch.ThisGroupsUiNodes.ToList();
        _myToggleGroupId = toggleData.ReturnToggleId;
        _startAsSelected = toggleData.StartAsSelected == IsActive.Yes;
        
        SelectedToggle += SaveSelectedNode;
        
        _autoOpenDelay = _uiNode.AutoOpenDelay;
        _canAutoOpen = MyBranch.AutoOpenCloseClass.CanAutoOpen();
        _delayTimer = EJect.Class.NoParams<IDelayTimer>();
    }

    //Variables
    private readonly List<INode> _allNodes;
    private readonly List<INode> _toggleGroupMembers = new List<INode>();
    private readonly UIBranch _tabBranch;
    private readonly bool _hasATabBranch, _canAutoOpen;
    private int _hasAGroupStartPoint;
    private readonly ToggleGroup _myToggleGroupId;
    private bool _startAsSelected;
    private readonly float _autoOpenDelay;
    private readonly IDelayTimer _delayTimer;

    //Events
    private static event Action<INode, Action> SelectedToggle;

    //Main
    public override void Start()
    {
        base.Start();
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
        foreach (var node in _allNodes.Where(node => node.IsToggleGroup)
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
        if(_uiNode.AutoOpenCloseOverride == IsActive.Yes) return;
        
        if (_canAutoOpen && !IsSelected)
        {
            _delayTimer.SetDelay(_autoOpenDelay)
                       .StartTimer(StartAutoOpen);
        }
    }
    
    private void StartAutoOpen() => TurnNodeOnOff();

    public override void OnExit()
    {
        base.OnExit();
        if (_canAutoOpen)
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
