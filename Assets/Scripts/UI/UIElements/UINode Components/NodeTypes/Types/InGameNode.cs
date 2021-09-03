using System;
using UnityEngine;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode, ICloseThisGOUIModule
{
    public InGameNode(INode node) : base(node)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    //Variables
    private INode _parentNode;
    private readonly float _autoOpenDelay;
    private readonly IDelayTimer _delayTimer = EZInject.Class.NoParams<IDelayTimer>();

    //Properties
    public IBranch TargetBranch => MyBranch.MyParentBranch;

    //Events
    private Action<ICloseThisGOUIModule> CloseGOUIModule { get; set; }

    public override void FetchEvents()
    {
        base.FetchEvents();
        CloseGOUIModule = GOUIEvents.Do.Fetch<ICloseThisGOUIModule>();
    }

    public override void SetUpGOUIParent(IGOUIModule module) => _uiNode.InGameObject = module.GOUITransform.gameObject;

    protected override void TurnNodeOnOff()
    {
        if(!AllowKeys)
            MyBranch.SetBranchAsActive();
        base.TurnNodeOnOff();
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
        if(PointerOverNode && !AllowKeys) return;
        OnExit();
    }

    public override void ClearNodeCompletely()
    {
        base.ClearNodeCompletely();
        CloseGOUIModule?.Invoke(this);
    }
}

