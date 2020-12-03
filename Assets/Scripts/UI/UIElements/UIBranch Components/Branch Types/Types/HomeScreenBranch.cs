using UnityEngine;

public interface IHomeScreenBranch : IBranchBase { }

public class HomeScreenBranch: BranchBase, IHomeScreenBranch
{
    private bool _startBranch;
    public HomeScreenBranch(IBranch branch) : base(branch) { }
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == DoTween.DoNothing;
    
    //Main
    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SaveOnStart(IOnStart args) 
    {
        base.SaveOnStart(args);
        if (_startBranch)
        {
            _myBranch.DoNotTween();
            _myBranch.MoveToThisBranch();
        }
        SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        if (!OnHomeScreen && args.OnHomeScreen)
        {
            SetCanvas(ActiveCanvas.Yes);
            SetBlockRaycast(BlockRaycast.Yes);
        }
        base.SaveIfOnHomeScreen(args);
    }

    //Main
    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.No);

        if (args.StartBranch == _myBranch)
        {
            _startBranch = true;
            _myBranch.DefaultStartOnThisNode.ThisNodeIsHighLighted();
        }
        _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        _myBranch.SetHighlightedNode();
        
        if(!_canStart || !_inMenu) return;
        
        SetCanvas(ActiveCanvas.Yes);
        
        if (CannotTweenOnHome && !OnHomeScreen)
            _myBranch.DoNotTween();
        
        if (OnHomeScreen && _myBranch.GetStayOn() == IsActive.Yes)
            _myBranch.DoNotTween();
        
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_resolvePopUps) return;
        base.SetBlockRaycast(active);
    }
}


