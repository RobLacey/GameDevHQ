using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpController : IMono
{
    private readonly UIHub _uiHub;
    
    public PopUpController(UIHub uiHub)
    {
        _uiHub = uiHub;
        OnEnable();
    }
    
    //Properties
    private List<UIBranch> ActiveResolvePopUpsList { get; } = new List<UIBranch>();
    private List<UIBranch> ActiveNonResolvePopUpsList { get; } = new List<UIBranch>();
    public bool NoActivePopUps => ActiveResolvePopUpsList.Count == 0
                                  & ActiveNonResolvePopUpsList.Count == 0;
    public bool NoActiveResolvePopUps => ActiveResolvePopUpsList.Count == 0;
    public bool NoActiveNonResolvePopUps => ActiveNonResolvePopUpsList.Count == 0;
    private UINode LastNodeBeforePopUp { get; set; }


    //Deleagtes
    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoNonResolvePopUps;
    
    public void OnEnable()
    {
        Resolve.AddToResolvePopUp += AddToResolveList;
        NonResolve.AddToNonResolvePopUp += AddToNonResolveList;
    }

    public void OnDisable()
    {
        Resolve.AddToResolvePopUp -= AddToResolveList;
        NonResolve.AddToNonResolvePopUp -= AddToNonResolveList;
    }

    public void ActiveNextPopUp()
    {
        if (!NoActiveResolvePopUps)
        {
            int index = ActiveResolvePopUpsList.Count - 1;
            ActiveResolvePopUpsList[index].LastHighlighted.SetNodeAsActive();
        }
        else if(!NoActiveNonResolvePopUps)
        {
            int index = ActiveNonResolvePopUpsList.Count - 1;
            ActiveNonResolvePopUpsList[index].LastHighlighted.SetNodeAsActive();
        }
    }

    private void AddToResolveList(UIBranch newResolve)
    {
        if (ActiveResolvePopUpsList.Contains(newResolve)) return;
        ActiveResolvePopUpsList.Add(newResolve);
        NoResolvePopUps?.Invoke(false);
    }
    
    private void AddToNonResolveList(UIBranch newNonResolve)
    {
        if (ActiveNonResolvePopUpsList.Contains(newNonResolve)) return;
        ActiveNonResolvePopUpsList.Add(newNonResolve);
        NoNonResolvePopUps?.Invoke(false);
    }
    
    public void RemoveFromActiveList_Resolve()
    {
        int lastIndexItem = ActiveResolvePopUpsList.Count - 1;
        var nextPopUp = ActiveResolvePopUpsList[lastIndexItem];
        ActiveResolvePopUpsList.Remove(nextPopUp);

        if (NoActiveResolvePopUps)
        {
            NoResolvePopUps?.Invoke(true);
            ActivateNextNonResolvePopUp(nextPopUp);
        }
        else
        {
            lastIndexItem = ActiveResolvePopUpsList.Count - 1;
            nextPopUp.PopUpBranch.RestoreLastPosition(ActiveResolvePopUpsList[lastIndexItem].LastHighlighted);
        }
    }

    private void ActivateNextNonResolvePopUp(UIBranch nextPopUp)
    {
        if (NoActiveNonResolvePopUps)
        {
            nextPopUp.PopUpBranch.RestoreLastPosition(LastNodeBeforePopUp);
        }
        else
        {
            var lastIndexItem = ActiveNonResolvePopUpsList.Count - 1;
            nextPopUp.PopUpBranch.RestoreLastPosition(ActiveNonResolvePopUpsList[lastIndexItem].LastHighlighted);
        }
    }

    public void RemoveFromActiveList_NonResolve()
    {
        int lastIndexItem = ActiveNonResolvePopUpsList.Count - 1;
        var nextPopUp = ActiveNonResolvePopUpsList[lastIndexItem];
        ActiveNonResolvePopUpsList.Remove(nextPopUp);

        if (NoActiveNonResolvePopUps)
        {
            NoNonResolvePopUps?.Invoke(true);
            nextPopUp.PopUpBranch.RestoreLastPosition(LastNodeBeforePopUp);
        }
        else
        {
            ActiveNextPopUp();
            nextPopUp.PopUpBranch.RestoreLastPosition(ActiveNonResolvePopUpsList[0].LastHighlighted);
        }
    }

    public void SetLastNodeBeforePopUp(UINode lastNode)
    {
        if (!lastNode.MyBranch.IsAPopUpBranch()) LastNodeBeforePopUp = lastNode;
    }
}
