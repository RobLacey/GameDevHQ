using UIElements;

public interface IBranchBase : IParameters, IMonoEnable, IMonoStart
{
    void SetUpAsTabBranch();
    void SetUpBranch(IBranch newParentController = null);
    void EndOfBranchStart();
    void StartBranchExit();
    void EndOfBranchExit();
    void SetCanvas(ActiveCanvas active);
    void SetBlockRaycast(BlockRaycast active);
    bool CanStartBranch();
    bool CanExitBranch(OutTweenType outTweenType);
}

public interface IBranchParams
{
    ScreenType MyScreenType { get; }
}

public interface ICanvasCalcParms : IParameters
{
    IBranch MyBranch { get; }
}

