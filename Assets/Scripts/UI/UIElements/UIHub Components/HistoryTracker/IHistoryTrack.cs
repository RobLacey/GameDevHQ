using System;

public interface IHistoryTrack : IMonoBehaviourSub
{
    void DoCancelHoverToActivate();
    bool NoHistory { get; }
    void SetSelected(INode node);
    void BackToHome();
    void BackOneLevel();
    void CloseAllChildNodesAfterPoint(INode newNode);
    void SetFromHotkey(UIBranch branch);
    void ReverseAndClearHistory();
    void MoveToLastBranchInHistory();
    void CancelMove(Action endOfCancelAction);
}