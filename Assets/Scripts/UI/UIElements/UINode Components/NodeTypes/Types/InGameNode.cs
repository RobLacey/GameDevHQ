
using System.Linq;
using UnityEngine;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode, ICancelPressed
{
    private INode _parentNode;
    public InGameNode(INode node) : base(node) { }

    protected override void SaveInMenuOrInGame(IInMenu args)
    {
        if (args.InTheMenu)
        {
            OnExit();
        }
    }

    // public override void SetNodeAsActive()
    // {
    //     if (_disabledNode.IsThisNodeIsDisabled()) return;
    //     _inMenu = false;
    //     OnEnter();
    // }

    // protected override void SetAsHighlighted()
    // {
    //     if (IsDisabled) return;
    //     PointerOverNode = true;
    //     _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
    // }

    // protected override void TurnNodeOnOff()
    // {
    //     if (IsSelected)
    //     {
    //         Deactivate();
    //     }
    //     else
    //     {
    //         Activate();
    //     }
    // }

    protected override void Activate()
    {
        if(_uiNode.HasChildBranch.IsNull()) return;
        
        // if(_parentNode.IsNull())
        // {
        //     Debug.Log($"{_uiNode.HasChildBranch.MyParentBranch} : my Parent");
        //     var branchesNodes = _uiNode.HasChildBranch.MyParentBranch.ThisGroupsUiNodes;
        //     _parentNode = branchesNodes.First(node => ReferenceEquals(_uiNode.HasChildBranch, node.HasChildBranch));
        //     Debug.Log(_parentNode);
        // }
            //_parentNode.SetNodeAsActive();
            //_parentNode.SetAsHotKeyParent();
            Debug.Log("On");
            base.Activate();
           // _uiNode.HasChildBranch.MoveToThisBranch();
    }

    protected override void Deactivate()
    {
        Debug.Log("Off");
       // EVent.Do.Fetch<ICancelPressed>()?.Invoke(this);
        //_uiNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
       // _parentNode.DeactivateNode();
        base.Deactivate();
    }

    public EscapeKey EscapeKeySettings { get; } = EscapeKey.BackOneLevel;
}
