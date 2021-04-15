
public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    private bool _isToggle;
    private bool _justCancelled;
    private readonly IDelayTimer _delayTimer = EJect.Class.NoParams<IDelayTimer>();
    private readonly float _autoOpenDelay;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        if(_uiNode.CanAutoOpen)
            EVent.Do.Subscribe<ISwitchGroupPressed>(ClearJustCancelledFlag);
    }

    private void ClearJustCancelledFlag(ISwitchGroupPressed args) => _justCancelled = false;

    public override void OnEnter()
    {
        base.OnEnter();

        if (CheckIfHasJustBeenCancelled()) return;

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

    private bool CheckIfHasJustBeenCancelled()
    {
        if (!_justCancelled) return false;
        
        _justCancelled = false;
        return true;
    }

    public override void DeactivateNodeByType()
    {
        if (!IsSelected) return;
        Deactivate();
        
        if (_uiNode.CanAutoOpen && _allowKeys)
        {
            _justCancelled = true;
            SetNotHighlighted();
        }
        else
        {
            OnExit();
        }
    }
}
