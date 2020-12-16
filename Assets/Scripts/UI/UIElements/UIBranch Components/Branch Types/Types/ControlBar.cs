using UnityEngine;
public interface IControlBar : IBranchBase { }



public class ControlBar : BranchBase, IControlBar
{
    public ControlBar(IBranch branch) : base(branch)
    {
    }
    
    private bool IsControlBar => _myBranch.IsControlBar();

    protected override void SaveOnStart(IOnStart args)
    {
        base.SaveOnStart(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        base.SetUpBranchesOnStart(args);
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.No);
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        
    }
    
    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_resolvePopUps) return;
        base.SetBlockRaycast(IsControlBar ? BlockRaycast.Yes : active);
    }
    
    public override void SetCanvas(ActiveCanvas active)
    {
        Debug.Log($"{_myBranch} : {IsControlBar}");
        base.SetCanvas(IsControlBar ? ActiveCanvas.Yes : active);
    }

}
