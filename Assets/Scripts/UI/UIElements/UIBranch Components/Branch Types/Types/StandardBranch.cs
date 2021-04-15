using UnityEngine;

public interface IStandardBranch : IBranchBase { }

public class StandardBranch : BranchBase, IStandardBranch
{
    public StandardBranch(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
    }

    private readonly IBranch[] _allBranches;

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        _canvasOrderCalculator.SetCanvasOrder();
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        SetNewParentBranch(newParentController);
        
        if(_myBranch.ScreenType == ScreenType.FullScreen)
            _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.Yes);
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        if(_isTabBranch) return;
        base.ClearBranchForFullscreen(args);
        _canvasOrderCalculator.ResetCanvasOrder();
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
