using System;

public interface IHistoryTrack : IParameters, IIsAService
{
    void OnEnable();
    bool IsPaused { get; }
    bool NoHistory { get; }
    IHistoryManagement HistoryListManagement { get; }
    void BackToHome();
    void BackOneLevel();
    void MoveToLastBranchInHistory();
    void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction);
    void AddNodeToTestRunner(INode node);
    void BackToHomeScreen(ActivateNodeOnReturnHome activate);
}