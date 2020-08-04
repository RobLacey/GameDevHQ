using System;
using System.Collections.Generic;

public interface IPopUpControls
{
    bool NoActivePopUps { get; }
    void ActiveNextPopUp();
    void RemoveNextPopUp();
}

public class PopUpController : IMono, IPopUpControls
{
    public PopUpController()
    {
        OnEnable();
    }
    
    //Properties
    private List<UIBranch> ActiveResolvePopUpsList { get; } = new List<UIBranch>();
    private List<UIBranch> ActiveNonResolvePopUpsList { get; } = new List<UIBranch>();
    public bool NoActivePopUps => ActiveResolvePopUpsList.Count == 0
                                  & ActiveNonResolvePopUpsList.Count == 0;

    private bool NoActiveResolvePopUps => ActiveResolvePopUpsList.Count == 0;
    private bool NoActiveNonResolvePopUps => ActiveNonResolvePopUpsList.Count == 0;
    private UINode LastNodeBeforePopUp { get; set; }

    //Delegates
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
    public void RemoveNextPopUp()
    {
        if (!NoActiveResolvePopUps)
        {
            RemoveFromActiveList_Resolve();
        }
        else if(!NoActiveNonResolvePopUps)
        {
            RemoveFromActiveList_NonResolve();
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
    
    private void RemoveFromActiveList_Resolve()
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

    private void RemoveFromActiveList_NonResolve()
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
