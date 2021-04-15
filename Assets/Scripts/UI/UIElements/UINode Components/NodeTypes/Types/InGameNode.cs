using System;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode, ICloseGOUIModule
{
    public InGameNode(INode node) : base(node)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    private INode _parentNode;
    private readonly float _autoOpenDelay;
    private readonly IDelayTimer _delayTimer = EJect.Class.NoParams<IDelayTimer>();
    private bool _justCancelled;
    
    //Properties
    public IBranch TargetBranch => MyBranch.MyParentBranch;

    //Events
    private Action<ICloseGOUIModule> CloseGOUIModule { get; set; }

    public override void FetchEvents()
    {
        base.FetchEvents();
        CloseGOUIModule = EVent.Do.Fetch<ICloseGOUIModule>();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ICloseInGameNode>(CloseThisNode);
    }
    
    private void CloseThisNode(ICloseInGameNode args)
    {
        if (args.TargetBranch == MyBranch && IsSelected)
        {
            _justCancelled = true;
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

        _justCancelled = false;
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
        
        CloseGOUIModule?.Invoke(this);
        
        if (_uiNode.CanAutoOpen)
        {
            _justCancelled = true;
            OnExit();
        }
        else
        {
            OnExit();
        }
    }
}

