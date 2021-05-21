using EZ.Service;
using UIElements;

public interface IHomeScreenBranch : IBranchBase { }

public class HomeScreenBranch: BranchBase, IHomeScreenBranch
{
    public HomeScreenBranch(IBranch branch) : base(branch) { }

    private ICanvasOrderData _canvasOrderData;
    private bool _justReturnedHome = false;
    
    //Properties
    private bool CannotTweenOnHome => _myBranch.TweenOnHome == DoTween.DoNothing 
                                      && (!_justReturnedHome && _myBranch.GetStayOn() == IsActive.Yes);
    private bool IsControlBar => _myBranch.IsControlBar();

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        base.SaveIfOnHomeScreen(args);
        _justReturnedHome = true;
    }

    //Main
    public override void UseEZServiceLocator()
    {
        base.UseEZServiceLocator();
        _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
    }

    public override void OnStart()
    {
        base.OnStart();
        SetCanvas(ActiveCanvas.Yes);
        SetControlBarCanvasOrder();
    }

    private void SetControlBarCanvasOrder()
    {
        if(!IsControlBar) return;
        
        var storedCondition = _myBranch.MyCanvas.enabled;
        _myBranch.MyCanvas.enabled = true;
        _myBranch.MyCanvas.overrideSorting = true;
        _myBranch.MyCanvas.sortingOrder = _canvasOrderData.ReturnControlBarCanvasOrder();
        _myBranch.MyCanvas.enabled = storedCondition;
    }

    protected override void SaveInMenu(IInMenu args)
    {
        base.SaveInMenu(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SaveOnStart(IOnStart args) 
    {
        base.SaveOnStart(args);
        SetBlockRaycast(BlockRaycast.Yes);
    }

    //Main
    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        if (args.StartBranch == _myBranch)
        {
            _myBranch.DefaultStartOnThisNode.ThisNodeIsHighLighted();
        }
        else
        {
            _myBranch.DontSetBranchAsActive();
        }
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        
        base.SetUpBranch(newParentController);
        
        if(!_canStart || !_inMenu) return;
        
        if(!IsControlBar)
            _canvasOrderCalculator.SetCanvasOrder();
        
        SetCanvas(ActiveCanvas.Yes);
        if (CannotTweenOnHome || IsControlBar)
            _myBranch.DoNotTween();
        
        if(!OnHomeScreen)
            InvokeOnHomeScreen(true);
        _justReturnedHome = false;
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        CheckChildOfControlBar();
    }

    private void CheckChildOfControlBar()
    {
        if (_myBranch.LastSelected.HasChildBranch.ScreenType == ScreenType.Normal && !OnHomeScreen)
        {
            _historyTrack.BackToHomeScreen();
        }
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(!_gameIsPaused && !_activeResolvePopUps)
        {
            base.SetBlockRaycast(IsControlBar ? BlockRaycast.Yes: active);
        }
        else
        {
            base.SetBlockRaycast(active);
        }
    }
    
    public override void SetCanvas(ActiveCanvas active)
    {
        if(!_gameIsPaused && !_activeResolvePopUps)
        {
            base.SetCanvas(IsControlBar ? ActiveCanvas.Yes : active);
        }
        else
        {
            base.SetCanvas(active);
        }
    }
}


