
public interface INode
{
    UIBranch MyBranch { get; }
    UIBranch HasChildBranch { get; }
    UINode ReturnNode { get; }
    void SetNodeAsActive();
    void DeactivateNode();
    void SetNodeAsSelected_NoEffects();
    void ThisNodeIsSelected();
    void ThisNodeIsHighLighted();
    void PlayCancelAudio();
}


        
