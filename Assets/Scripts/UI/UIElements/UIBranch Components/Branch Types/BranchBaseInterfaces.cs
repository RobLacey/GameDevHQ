public interface IBranchBase : IParameters
{
    void OnStart();
    void OnEnable();
    void SetUpAsTabBranch();
    void SetUpBranch(IBranch newParentController = null);
    void EndOfBranchStart();
    void StartBranchExit(OutTweenType outTweenType);
    void EndOfBranchExit();
    void SetCanvas(ActiveCanvas active);
    void SetBlockRaycast(BlockRaycast active);
    bool CanStartBranch();
}

public interface IBranchParams
{
    ScreenType MyScreenType { get; }
}

public interface IAdjustCanvasOrder //Inherited in PopUp Branch Types
{
    int CanvasOrderOffset { set; }
    BranchType BranchType { get; }
}
