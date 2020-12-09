
public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode) => _uiNode = uiNode;

    private bool _isToggle;
    private bool _justCancelled;
    private bool _canAutoOpen;
    private bool _canAutoClose;

    public override void Start()
    {
        base.Start();
        _canAutoOpen = MyBranch.AutoOpenCloseClass.CanAutoOpen();
        _canAutoClose = MyBranch.AutoOpenCloseClass.CanAutoClose();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        if(_canAutoClose)
            EVent.Do.Subscribe<ISwitchGroupPressed>(ClearJustCancelledFlag);
    }

    public override void RemoveEvents()
    {
        base.RemoveEvents();
        if(_canAutoClose)
            EVent.Do.Unsubscribe<ISwitchGroupPressed>(ClearJustCancelledFlag);
    }

    private void ClearJustCancelledFlag(ISwitchGroupPressed args) => _justCancelled = false;

    public override void OnEnter()
    {
        base.OnEnter();
        if(_uiNode.AutoOpenCloseOverride == IsActive.Yes) return;

        if (CheckIfHasJustBeenCancelled()) return;
        
        if (_canAutoOpen && !IsSelected)
        {
            MyBranch.AutoOpenCloseClass.ChildNodeHasOpenChild = _uiNode.HasChildBranch;
            TurnNodeOnOff();
        }
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
        
        if (_canAutoClose && _allowKeys)
        {
            _justCancelled = true;
            SetAsHighlighted();
        }
        else
        {
            OnExit();
        }
    }
}
