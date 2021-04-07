
public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode
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
        _myGOUIModule = args.ReturnGOUIModule;
    }

    public override void DeactivateNodeByType()
    {
        Deactivate();
        OnExit();
    }
}
