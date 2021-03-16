

using System;
using UIElements;
using UnityEngine;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode, ICancelPressed
{
    private INode _parentNode;
    private GOUIModule _myGOUIModule;

    public InGameNode(INode node) : base(node) { }

    protected override void SaveInMenuOrInGame(IInMenu args)
    {
        // Debug.Log(args.InTheMenu);
        // if (args.InTheMenu)
        // {
        //     Debug.Log(MyBranch);
        //     _myGOUIModule.ExitInGameUi();
        // }
        // if (args.InTheMenu)
        // {
        //     OnExit();
        // }
    }
    
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
        
           // Debug.Log("On");
            base.Activate();
    }
    
    protected override void Deactivate()
    {
        //Debug.Log("Off");
        base.Deactivate();
    }

    public void DeactivateNodeByType()
    {
        _myGOUIModule.ExitInGameUi();
        //MyBranch.StartBranchExitProcess(OutTweenType.Cancel);
        
        Deactivate();
       //OnExit();
    }

    public EscapeKey EscapeKeySettings { get; } = EscapeKey.BackOneLevel;
}
