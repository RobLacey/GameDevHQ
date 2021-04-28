using System;
using UnityEngine;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode, ICloseGOUIModule
{
    public InGameNode(INode node) : base(node)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    //Variables
    private INode _parentNode;
    private readonly float _autoOpenDelay;
    private readonly IDelayTimer _delayTimer = EJect.Class.NoParams<IDelayTimer>();
    
    //Properties
    public IBranch TargetBranch => MyBranch.MyParentBranch;

    //Events
    private Action<ICloseGOUIModule> CloseGOUIModule { get; set; }

    public override void FetchEvents()
    {
        base.FetchEvents();
        CloseGOUIModule = EVent.Do.Fetch<ICloseGOUIModule>();
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
    
    public override void DeactivateNodeByType()
    {
        if (!IsSelected) return;
        Deactivate();
        
        CloseGOUIModule?.Invoke(this);
        if(PointerOverNode && !_allowKeys) return;
        OnExit();
    }

    public override void ClearNodeCompletely()
    {
        base.ClearNodeCompletely();
        CloseGOUIModule?.Invoke(this);
    }
}

