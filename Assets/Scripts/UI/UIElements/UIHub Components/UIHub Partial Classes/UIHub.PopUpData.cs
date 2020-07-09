using System.Collections.Generic;

/// <summary>
/// This partial class handles the tracking of PopUpdata for project wise use
/// </summary>
public partial class UIHub
{
    //Properties
    public List<UIBranch> ActivePopUpsResolve { get; } = new List<UIBranch>();
    public List<UIBranch> ActivePopUpsNonResolve { get; } = new List<UIBranch>();
    public int PopIndex { get; set; }
    public bool NoActivePopUps => ActivePopUpsResolve.Count == 0
                                  & ActivePopUpsNonResolve.Count == 0;
    
    public void ActiveNextPopUp()
    {
        int groupLength = ActivePopUpsNonResolve.Count;
        SetLastHighlighted(ActivePopUpsNonResolve[PopIndex].LastHighlighted);
        ActivePopUpsNonResolve[PopIndex].LastHighlighted.SetNodeAsActive();
        PopIndex = PopIndex.PositiveIterate(groupLength);
    }

}
