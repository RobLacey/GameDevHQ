using System;

public interface IHistoryTrack : IParameters, IEServService
{
    void OnEnable();
    bool IsPaused { get; }
    bool NoHistory { get; }
    IHistoryManagement HistoryListManagement { get; }
    void BackToHome();
    void BackOneLevel();
    void MoveToLastBranchInHistory();
    void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction);
    void UpdateHistoryData(INode node);
    void BackToHomeScreen();
}