using System;
using System.Collections.Generic;
using UnityEngine;

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

    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoNonResolvePopUps;

    public void ActiveNextPopUp() //Todo Move to popup 
    {
        int groupLength = ActivePopUpsNonResolve.Count;
        //SetLastHighlighted(ActivePopUpsNonResolve[PopIndex].LastHighlighted);
        ActivePopUpsNonResolve[PopIndex].LastHighlighted.SetNodeAsActive();
        PopIndex = PopIndex.PositiveIterate(groupLength);
    }

    public void AddToResolveList(UIBranch newResolve)
    {
        ActivePopUpsResolve.Add(newResolve);
        NoResolvePopUps?.Invoke(false);
        Debug.Log(ActivePopUpsResolve.Count + " : Add");
    }
    public void RemoveFromResolveList(UIBranch oldResolve)
    {
        ActivePopUpsResolve.Remove(oldResolve);
        if (ActivePopUpsResolve.Count == 0)
        {
            NoResolvePopUps?.Invoke(true);
            Debug.Log(ActivePopUpsResolve.Count + " : remove");

        }
    }
    
    public void AddToNonResolveList(UIBranch newNonResolve)
    {
        ActivePopUpsNonResolve.Add(newNonResolve);
        NoResolvePopUps?.Invoke(false);
        Debug.Log(ActivePopUpsNonResolve.Count + " : Add");
    }
    public void RemoveFromNonResolveList(UIBranch oldNonResolve)
    {
        ActivePopUpsNonResolve.Remove(oldNonResolve);
        if (ActivePopUpsNonResolve.Count == 0)
        {
            NoResolvePopUps?.Invoke(true);
            Debug.Log(ActivePopUpsNonResolve.Count + " : remove");

        }
    }

    public bool ReturnResolveList()
    {
        return ActivePopUpsResolve.Count == 0;
    }
}
