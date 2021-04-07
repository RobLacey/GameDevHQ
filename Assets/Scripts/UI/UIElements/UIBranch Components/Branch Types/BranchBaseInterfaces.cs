public interface IBranchBase : IParameters
{
    void OnEnable();
    void SetUpAsTabBranch();
    void SetUpBranch(IBranch newParentController = null);
    void EndOfBranchStart();
    void StartBranchExit();
    void EndOfBranchExit();
    void SetCanvas(ActiveCanvas active);
    void SetBlockRaycast(BlockRaycast active);
    bool CanStartBranch();
    bool CanExitBranch();
}

public interface IBranchParams
{
    ScreenType MyScreenType { get; }
}

