
public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    private bool _isToggle;
    private readonly IDelayTimer _delayTimer = EZInject.Class.NoParams<IDelayTimer>();
    private readonly float _autoOpenDelay;

    public override void OnEnter()
    {
        base.OnEnter();

        if (_uiNode.CanAutoOpen && !IsSelected)
        {
            _delayTimer.SetDelay(_autoOpenDelay)
                       .StartTimer(StartAutoOpen);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        if (_uiNode.CanAutoOpen)
        {
            _delayTimer.StopTimer();
        }
    }

    private void StartAutoOpen()
    {
        MyBranch.AutoOpenCloseClass.ChildNodeHasOpenChild = _uiNode.HasChildBranch;
        TurnNodeOnOff();
    }

    public override void DeactivateNodeByType()
    {
        if (!IsSelected) return;
        Deactivate();
        
        if(PointerOverNode && !_allowKeys) return;
        OnExit();
    }
}
