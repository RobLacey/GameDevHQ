
public interface IDisabled
{
    bool IsDisabled { get; }
    void DisableObject();
    void EnableObject();
}

public interface INode
{
    UIBranch MyBranch { get; }
    UIBranch HasChildBranch { get; }
    UINode ReturnNode { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    void SetNodeAsSelected_NoEffects();
    void SetNodeAsNotSelected_NoEffects();
    void ThisNodeIsSelected();
    void ThisNodeIsHighLighted();
    void PlayCancelAudio();
}


        
