using UnityEngine;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode, ICancelPressed
{
    private INode _parentNode;
    private IGOUIModule _myGOUIModule;

    public InGameNode(INode node) : base(node) { }
    

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ISetUpUIGOBranch>(SetUpGOUIParent);
    }

    
    private void SetUpGOUIParent(ISetUpUIGOBranch args)
    {
        if(args.TargetBranch != MyBranch || _myGOUIModule.IsNotNull()) return;
        _myGOUIModule = args.UIGOModule;
    }



    protected override void Activate()
    {
        //if(_uiNode.HasChildBranch.IsNull()) return;
        
            //Debug.Log("On");
            base.Activate();
    }
    
    protected override void Deactivate()
    {
        //Debug.Log("Off");
        base.Deactivate();
    }

    public override void DeactivateNodeByType()
    {
        _myGOUIModule.ExitInGameUi();
        Deactivate();
        
        if (_allowKeys)
        {
            SetAsHighlighted();
        }
        else
        {
            OnExit();
        }

    }

    public EscapeKey EscapeKeySettings { get; } = EscapeKey.BackOneLevel;
}
