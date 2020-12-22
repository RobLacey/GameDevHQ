using UnityEngine;

public interface IStandardBranch : IBranchBase { }

public class StandardBranch : BranchBase, IStandardBranch
{
    public StandardBranch(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
    }

    private readonly IBranch[] _allBranches;

    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetBlockRaycast(BlockRaycast.No);
        if (_isTabBranch) return;
        SetCanvas(ActiveCanvas.No);
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        _myBranch.SetHighlightedNode();
        SetNewParentBranch(newParentController);
        
        if(_myBranch.BlockOtherNode == IsActive.Yes || _myBranch.ScreenType == ScreenType.FullScreen)
            _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.Yes);
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        if(_isTabBranch) return;
        base.ClearBranchForFullscreen(args);
    }

    private void SetNewParentBranch(IBranch newParentController) 
    {
        if(newParentController is null) return;
        _myBranch.MyParentBranch = newParentController;
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_activeResolvePopUps) return;
        base.SetBlockRaycast(active);
    }
}
