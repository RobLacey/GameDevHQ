using System;

public interface IHistoryTrack : IParameters, IIsAService
{
    void OnEnable();
    void OnDisable();
    bool IsPaused { get; }
    bool NoHistory { get; }
    IHistoryManagement HistoryListManagement { get; }
    void DoCancelHoverToActivate();
    void SetSelected(INode node);
    void BackToHome();
    void BackOneLevel();
    void MoveToLastBranchInHistory();
    void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction);
    void AddNodeToTestRunner(INode node);
    void BackToHomeScreen(ActivateNodeOnReturnHome activate);
}