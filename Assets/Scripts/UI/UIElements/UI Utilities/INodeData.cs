interface INodeData
{
    UINode LastHighlighted { get; }
    UINode LastSelected { get; }
    void SaveHighlighted(UINode newNode);
    void SaveSelected(UINode newNode);
}

interface IBranchData
{
    UIBranch ActiveBranch { get; }
    void SaveActiveBranch(UIBranch newBranch);
}
