
using UnityEngine;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode
{
    private INode _parentNode;
    private float _autoOpenDelay;

    public InGameNode(INode node) : base(node)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }
    
    private readonly IDelayTimer _delayTimer = EJect.Class.NoParams<IDelayTimer>();
    private bool _justCancelled;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        
        //************
        if(_uiNode.CanAutoOpen)
        {
            EVent.Do.Subscribe<ISwitchGroupPressed>(ClearJustCancelledFlag);
            EVent.Do.Subscribe<IGOUISwitchPressed>(ClearJustCancelledFlag);
        }    
    }
    
    //********
    private void ClearJustCancelledFlag(ISwitchGroupPressed args) => _justCancelled = false;
    private void ClearJustCancelledFlag(IGOUISwitchPressed args) => _justCancelled = false;


    public override void Start()
    {
        base.Start();
        if (MyBranch.CloseIfClickedOff == IsActive.Yes && _uiNode.HasChildBranch.IsNotNull())
        {
            _uiNode.HasChildBranch.CloseIfClickedOff = IsActive.Yes;
        }
    }

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
    
    private void StartAutoOpen()
    {
        MyBranch.AutoOpenCloseClass.ChildNodeHasOpenChild = _uiNode.HasChildBranch;
        TurnNodeOnOff();
    }


    public override void OnExit()
    {
        base.OnExit();
        if (_uiNode.CanAutoOpen && IsSelected)
        {
            _delayTimer.StopTimer();
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
