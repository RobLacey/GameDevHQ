
using System;

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
            // case BranchType.Internal:
            //     return null;
            case BranchType.ResolvePopUp:
                CreateResolvePopUp(branch);
                return new ResolvePopUp(branch, allBranches);
            case BranchType.OptionalPopUp:
                CreateOptionalPopUp(branch);
                return new OptionalPopUp(branch, allBranches);
            // case BranchType.TimedPopUp:
            //     return null;
            case BranchType.PauseMenu:
                CreatePauseMenu(branch);
                return new PauseMenu(branch, allBranches);
        }

        return null;
    }
    
    private static void CreateHomeScreenBranch(UIBranch branch)
    {
        branch._screenType = ScreenType.Normal;
        branch._escapeKeyFunction = EscapeKey.None;
    }

    private static void CreateStandardBranch(UIBranch branch)
    {
        branch._tweenOnHome = IsActive.No;
    }

    private static void CreateResolvePopUp(UIBranch branch)
    {
        branch._escapeKeyFunction = EscapeKey.BackOneLevel;
        branch._tweenOnHome = IsActive.No;
    }

    private static void CreateOptionalPopUp(UIBranch branch)
    {
        branch._screenType = ScreenType.Normal;
        branch._escapeKeyFunction = EscapeKey.BackOneLevel;
        branch._tweenOnHome = IsActive.No;
    }
    
    private static void CreateTimedPopUp(UIBranch branch)
    {
        branch._screenType = ScreenType.Normal;
        branch._escapeKeyFunction = EscapeKey.BackOneLevel;
        branch._tweenOnHome = IsActive.No;
    }

    private static void CreatePauseMenu(UIBranch branch)
    {
        branch._escapeKeyFunction = EscapeKey.BackOneLevel;
        branch._tweenOnHome = IsActive.No;
    }
}
