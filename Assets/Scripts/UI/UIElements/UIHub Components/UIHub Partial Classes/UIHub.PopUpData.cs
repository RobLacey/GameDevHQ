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
    //private int PopIndex { get; set; }
    public bool NoActivePopUps => ActivePopUpsResolve.Count == 0
                                  & ActivePopUpsNonResolve.Count == 0;

    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoNonResolvePopUps;

    public void ActiveNextPopUp(List<UIBranch> popUpList)
    {
        int index = popUpList.Count -1;
        popUpList[index].LastHighlighted.SetNodeAsActive();
    }

    private void AddToResolveList(UIBranch newResolve)
    {
        if (ActivePopUpsResolve.Contains(newResolve)) return;
        ActivePopUpsResolve.Add(newResolve);
        NoResolvePopUps?.Invoke(false);
        Debug.Log(ActivePopUpsResolve.Count + " : Add");
    }
    
    private void AddToNonResolveList(UIBranch newNonResolve)
    {
        if (ActivePopUpsNonResolve.Contains(newNonResolve)) return;
        ActivePopUpsNonResolve.Add(newNonResolve);
        NoNonResolvePopUps?.Invoke(false);
        Debug.Log(ActivePopUpsNonResolve.Count + " : Add Non");
    }
    
    public void RemoveFromActiveList_Resolve()
    {
        int lastIndexItem = ActivePopUpsResolve.Count - 1;
        var nextPopUp = ActivePopUpsResolve[lastIndexItem];
        ActivePopUpsResolve.Remove(nextPopUp);
    
        if (ActivePopUpsResolve.Count > 0)
        {
            lastIndexItem = ActivePopUpsResolve.Count - 1;
            nextPopUp.PopUpClass.RestoreLastPosition(ActivePopUpsResolve[lastIndexItem].LastHighlighted);
        }
        else
        {
            NoResolvePopUps?.Invoke(true);

            if (ActivePopUpsNonResolve.Count > 0)
            {
                lastIndexItem = ActivePopUpsNonResolve.Count - 1;
                //PopIndex = lastIndexItem;
                nextPopUp.PopUpClass.RestoreLastPosition(ActivePopUpsNonResolve[lastIndexItem].LastHighlighted);
            }
            else
            {
                nextPopUp.PopUpClass.RestoreLastPosition(LastNodeBeforePopUp);
            }
        }
    }
    
    public void RemoveFromActiveList_NonResolve(/*UIBranch PopUp*/)
    {
        UIBranch nextPopUp = LastHighlighted.MyBranch;
        ActivePopUpsNonResolve.Remove(nextPopUp);

        if (ActivePopUpsNonResolve.Count > 0)
        {
            //PopIndex = 0;
            ActiveNextPopUp(ActivePopUpsNonResolve);
            nextPopUp.PopUpClass.RestoreLastPosition(ActivePopUpsNonResolve[0].LastHighlighted);
        }
        else
        {
            NoNonResolvePopUps?.Invoke(true);
            nextPopUp.PopUpClass.RestoreLastPosition(LastNodeBeforePopUp);
        }
    }

    public void ActivateNonResolvePopUps()
    {
        foreach (var item in ActivePopUpsNonResolve)
        {
            //if (_uIHub.ActivePopUpsResolve.Count == 0)
            if (ActivePopUpsResolve.Count == 0)
            {
                item.MyCanvasGroup.blocksRaycasts = true;
            }

            item.MyCanvas.enabled = true;
        }
    }

    public void ActiveResolvePopUps()
    {
        foreach (var item in ActivePopUpsResolve)
        {
            item.MyCanvasGroup.blocksRaycasts = true;
            item.MyCanvas.enabled = true;
        }
    }

}
