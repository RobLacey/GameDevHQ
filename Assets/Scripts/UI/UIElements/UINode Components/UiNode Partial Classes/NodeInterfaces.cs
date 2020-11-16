
public interface INode : IToggles
{
    UIBranch MyBranch { get; }
    UIBranch HasChildBranch { get; }
    UINode ReturnNode { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    bool DontStoreTheseNodeTypesInHistory { get; }
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
    IsActive ReturnStartAsSelected { get; }
    IsActive SetStartAsSelected { set; }
    UINode ReturnNode { get; }

}
        
