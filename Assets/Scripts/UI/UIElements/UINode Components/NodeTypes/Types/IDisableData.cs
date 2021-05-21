using EZ.Inject;

public interface IDisableData : IParameters
{
    INode UINode { get; }
    UINavigation Navigation { get; }
    void SetNodeAsNotSelected_NoEffects();
}