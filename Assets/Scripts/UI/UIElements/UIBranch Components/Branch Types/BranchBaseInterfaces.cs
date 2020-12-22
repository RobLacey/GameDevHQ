public interface IBranchBase : IParameters
{
    void OnStart();
    void OnEnable();
    void SetUpAsTabBranch();
    void SetUpBranch(IBranch newParentController = null);
    void SetCanvas(ActiveCanvas active);
    void SetBlockRaycast(BlockRaycast active);
    void ActivateStoredPosition();
}

public interface IBranchParams
{
    ScreenType MyScreenType { get; }
}

public interface IAdjustCanvasOrder //Inherited in PopUp Branch Types
{
    int CanvasOrderOffset { set; }
    BranchType BranchType { get; }
    void SetUpCanvasOrder(ICanvasOrder branch);
    void AdjustCanvasOrderAdded();
    void AdjustCanvasOrderRemoved(ILastRemovedPopUp args);
}
