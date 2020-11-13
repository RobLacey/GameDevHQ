using System;

public interface IHistoryTrack : IMonoBehaviourSub
{
    void DoCancelHoverToActivate();
    bool NoHistory { get; }
    void SetSelected(INode node);
    void BackToHome();
    void BackOneLevel();
    void MoveToLastBranchInHistory();
    void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction);
}