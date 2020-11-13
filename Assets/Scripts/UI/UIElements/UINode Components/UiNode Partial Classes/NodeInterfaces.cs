
public interface INode
{
    UIBranch MyBranch { get; }
    UIBranch HasChildBranch { get; }
    UINode ReturnNode { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    bool DontStoreTheseNodeTypesInHistory { get; }
}


        
