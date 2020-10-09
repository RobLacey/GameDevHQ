public interface IHistoryTrack
{
    void SetSelected(INode node);
    void BackToHome();
    void BackOneLevel();
    void CloseAllChildNodesAfterPoint(INode newNode);
    void SetFromHotkey(UIBranch branch, INode parentNode);
    void ReverseAndClearHistory();
}