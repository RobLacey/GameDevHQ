
public static class BranchFactory
{

    public static BranchBase AssignType(UIBranch branch, BranchType branchType, UIBranch[] allBranches)
    {
        switch (branchType)
        {
            case BranchType.HomeScreen:
                CreateHomeScreenBranch(branch);
                return new HomeScreenBranchBase(branch);
            case BranchType.Standard:
                CreateStandardBranch(branch);
                return new StandardBranchBase(branch);
            case BranchType.ResolvePopUp:
                CreateResolvePopUp(branch);
                return new ResolvePopUp(branch, allBranches);
            case BranchType.OptionalPopUp:
                CreateOptionalPopUp(branch);
                return new OptionalPopUp(branch, allBranches);
             case BranchType.TimedPopUp:
                 CreateTimedPopUp(branch);
                 return new Timed(branch);
            case BranchType.PauseMenu:
                CreatePauseMenu(branch);
                return new PauseMenu(branch, allBranches);
            case BranchType.Internal:
                CreateInternal(branch);
                return new StandardBranchBase(branch);
        }

        return null;
    }
    
    private static void CreateHomeScreenBranch(UIBranch branch)
    {
        branch.ScreenType = ScreenType.Normal;
        branch.EscapeKeySetting = EscapeKey.None;
    }

    private static void CreateStandardBranch(UIBranch branch)
    {
        branch.TweenOnHome = IsActive.No;
    }

    private static void CreateResolvePopUp(UIBranch branch)
    {
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.TweenOnHome = IsActive.No;
        branch.SetStayOn(IsActive.No);
    }

    private static void CreateOptionalPopUp(UIBranch branch)
    {
        branch.ScreenType = ScreenType.Normal;
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.SetStayOn(IsActive.No);
    }
    
    private static void CreateTimedPopUp(UIBranch branch)
    {
        branch.ScreenType = ScreenType.Normal;
        branch.TweenOnHome = IsActive.No;
        branch.WhenToMove = WhenToMove.Immediately;
        branch.SetStayOn(IsActive.No);
    }

    private static void CreatePauseMenu(UIBranch branch)
    {
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
        branch.TweenOnHome = IsActive.No;
    }
    
    private static void CreateInternal(UIBranch branch)
    {
        branch.TweenOnHome = IsActive.No;
        branch.EscapeKeySetting = EscapeKey.BackOneLevel;
    }
}
