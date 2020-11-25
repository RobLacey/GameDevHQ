public class BranchFactory
{
    private IBranch _branch;
    private static readonly IInjectClass injectClass = new InjectClass();
    
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
                return injectClass.WithParams<IHomeScreenBranch>(_branch);
            }            
            case BranchType.Standard:
            {
                CreateStandardBranch(_branch);
                return injectClass.WithParams<IStandardBranch>(_branch);
            }            
            case BranchType.ResolvePopUp:
            {
                CreateResolvePopUp(_branch);
                return injectClass.WithParams<IResolvePopUpBranch>(_branch);
            }
            case BranchType.OptionalPopUp:
            {
                CreateOptionalPopUp(_branch);
                return injectClass.WithParams<IOptionalPopUpBranch>(_branch);
            }
             case BranchType.TimedPopUp:
             {
                 CreateTimedPopUp(_branch);
                 return injectClass.WithParams<ITimedPopUpBranch>(_branch);
             }
            case BranchType.PauseMenu:
            {
                CreatePauseMenu(_branch);
                return injectClass.WithParams<IPauseBranch>(_branch);
            }
            case BranchType.Internal:
            {
                CreateInternal(_branch);
                return injectClass.WithParams<IStandardBranch>(_branch);
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
        branch.TweenOnHome = IsActive.No;
    }

    private static void CreateResolvePopUp(IBranch branch)
    {
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.TweenOnHome = IsActive.No;
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
        branch.TweenOnHome = IsActive.No;
        branch.WhenToMove = WhenToMove.Immediately;
        branch.SetStayOn(IsActive.No);
    }

    private static void CreatePauseMenu(IBranch branch)
    {
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.TweenOnHome = IsActive.No;
    }
    
    private static void CreateInternal(IBranch branch)
    {
        branch.TweenOnHome = IsActive.No;
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
    }
}
