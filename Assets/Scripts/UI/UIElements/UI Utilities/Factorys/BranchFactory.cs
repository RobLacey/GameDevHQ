public class BranchFactory
{
    private IBranch _branch;
    private static readonly IEJect ieJect = new EJect();
    
    public static BranchFactory Factory { get; } = new BranchFactory();

    public BranchFactory PassThisBranch(IBranch passedBranch)
    {
        _branch = passedBranch;
        return this;
    }

    public IBranchBase CreateType(BranchType branchType)
    {
        switch (branchType)
        {
            case BranchType.HomeScreen:
            {
                CreateHomeScreenBranch(_branch);
                return ieJect.WithParams<IHomeScreenBranch>(_branch);
            }            
            case BranchType.Standard:
            {
                CreateStandardBranch(_branch);
                return ieJect.WithParams<IStandardBranch>(_branch);
            }            
            case BranchType.ResolvePopUp:
            {
                CreateResolvePopUp(_branch);
                return ieJect.WithParams<IResolvePopUpBranch>(_branch);
            }
            case BranchType.OptionalPopUp:
            {
                CreateOptionalPopUp(_branch);
                return ieJect.WithParams<IOptionalPopUpBranch>(_branch);
            }
             case BranchType.TimedPopUp:
             {
                 CreateTimedPopUp(_branch);
                 return ieJect.WithParams<ITimedPopUpBranch>(_branch);
             }
            case BranchType.PauseMenu:
            {
                CreatePauseMenu(_branch);
                return ieJect.WithParams<IPauseBranch>(_branch);
            }
            case BranchType.Internal:
            {
                CreateInternal(_branch);
                return ieJect.WithParams<IStandardBranch>(_branch);
            }
        }

        return null;
    }
    
    private static void CreateHomeScreenBranch(IBranch branch)
    {
        branch.ScreenType = ScreenType.Normal;
        branch.EscapeKeySetting = EscapeKey.None;
    }

    private static void CreateStandardBranch(IBranch branch)
    {
        branch.TweenOnHome = DoTween.DoNothing;
    }

    private static void CreateResolvePopUp(IBranch branch)
    {
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.TweenOnHome = DoTween.DoNothing;
        branch.SetStayOn(IsActive.No);
    }

    private static void CreateOptionalPopUp(IBranch branch)
    {
        branch.ScreenType = ScreenType.Normal;
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.SetStayOn(IsActive.No);
    }
    
    private static void CreateTimedPopUp(IBranch branch)
    {
        branch.ScreenType = ScreenType.Normal;
        branch.TweenOnHome = DoTween.DoNothing;
        branch.WhenToMove = WhenToMove.Immediately;
        branch.SetStayOn(IsActive.No);
    }

    private static void CreatePauseMenu(IBranch branch)
    {
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.TweenOnHome = DoTween.DoNothing;
    }
    
    private static void CreateInternal(IBranch branch)
    {
        branch.TweenOnHome = DoTween.DoNothing;
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
    }
}
