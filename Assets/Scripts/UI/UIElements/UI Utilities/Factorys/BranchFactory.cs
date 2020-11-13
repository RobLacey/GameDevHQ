public class BranchFactory
{
    private UIBranch _uiBranch;
    private UIBranch[] _allBranches;
    public static BranchFactory Factory { get; } = new BranchFactory();

    public BranchFactory PassThisBranch(UIBranch branch)
    {
        _uiBranch = branch;
        return this;
    }

    public BranchFactory PassAllBranches(UIBranch[] allBranches)
    {
        _allBranches = allBranches;
        return this;
    }

    public BranchBase CreateType(BranchType branchType)
    {
        switch (branchType)
        {
            case BranchType.HomeScreen:
                CreateHomeScreenBranch(_uiBranch);
                return new HomeScreenBranchBase(_uiBranch);
            case BranchType.Standard:
                CreateStandardBranch(_uiBranch);
                return new StandardBranchBase(_uiBranch);
            case BranchType.ResolvePopUp:
                CreateResolvePopUp(_uiBranch);
                return new ResolvePopUp(_uiBranch, _allBranches);
            case BranchType.OptionalPopUp:
                CreateOptionalPopUp(_uiBranch);
                return new OptionalPopUp(_uiBranch, _allBranches);
             case BranchType.TimedPopUp:
                 CreateTimedPopUp(_uiBranch);
                 return new Timed(_uiBranch);
            case BranchType.PauseMenu:
                CreatePauseMenu(_uiBranch);
                return new PauseMenu(_uiBranch, _allBranches);
            case BranchType.Internal:
                CreateInternal(_uiBranch);
                return new StandardBranchBase(_uiBranch);
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
