using System;
using System.Collections.Generic;
/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
public class PopUpController : IMono, IPopUpControls
{
    private UIData _uiData;
    
    public PopUpController()
    {
        _uiData = new UIData();
        OnEnable();
    }
   
    //Properties
    private List<UIBranch> ActiveResolvePopUps { get; } = new List<UIBranch>();
    private List<UIBranch> ActiveOptionalPopUps { get; } = new List<UIBranch>();
    public bool NoActivePopUps => ActiveResolvePopUps.Count == 0
                                  & ActiveOptionalPopUps.Count == 0;
    private bool NoActiveResolvePopUps => ActiveResolvePopUps.Count == 0;
    private bool NoActiveOptionalPopUps => ActiveOptionalPopUps.Count == 0;
    private UINode LastNodeBeforePopUp { get; set; }

    //Delegates
    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoOptionalPopUps;
    
    public void OnEnable()
    {
        _uiData.SubscribeToAddResolvePopUp(AddActivePopUps_Resolve);
        _uiData.SubscribeToAddOptionalPopUp(AddToActivePopUps_Optional);
    }

    public void OnDisable()
    {
        _uiData.OnDisable();
    }

    public void ActivateCurrentPopUp()
    {
        if (!NoActiveResolvePopUps)
        {
            GetNextPopUp(ActiveResolvePopUps).SetNodeAsActive();
        }
        else if(!NoActiveOptionalPopUps)
        {
            GetNextPopUp(ActiveOptionalPopUps).SetNodeAsActive();
        }
    }

    private UINode GetNextPopUp(List<UIBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index].LastHighlighted;
    }

    public void RemoveNextPopUp()
    {
        if (!NoActiveResolvePopUps)
        {
            RemoveFromActivePopUpList(ActiveResolvePopUps, WhatToDoNext_Resolve);
        }
        else if(!NoActiveOptionalPopUps)
        {
            RemoveFromActivePopUpList(ActiveOptionalPopUps, WhatToDoNext_Optional);
        }
    }

    private void AddActivePopUps_Resolve(UIBranch newResolvePopUp)
       => AddToPopUpList(newResolvePopUp, ActiveResolvePopUps, NoResolvePopUps);

    private void AddToActivePopUps_Optional(UIBranch newOptionalPopUp) 
        => AddToPopUpList(newOptionalPopUp, ActiveOptionalPopUps, NoOptionalPopUps);

    private void AddToPopUpList(UIBranch newPopUp, List<UIBranch> popUpList,Action<bool> clearListEvent)
    {
        if (popUpList.Contains(newPopUp)) return;
        popUpList.Add(newPopUp);
        clearListEvent?.Invoke(false);
    }

    private void RemoveFromActivePopUpList(List<UIBranch> popUpList, Action<UIBranch> finishRemovalFromList)
    {
        var lastPopUp = popUpList[popUpList.Count - 1];
        popUpList.Remove(lastPopUp);
        finishRemovalFromList?.Invoke(lastPopUp);
    }

    private void WhatToDoNext_Resolve(UIBranch lastPopUp)
    {
        if (NoActiveResolvePopUps)
        {
            NoResolvePopUps?.Invoke(true);
            WhatToDoNext_Optional(lastPopUp);
        }
        else
        {
            lastPopUp.PopUpBranch.MoveToNextPopUp(GetNextPopUp(ActiveResolvePopUps));
        }
    }

    private void WhatToDoNext_Optional(UIBranch currentPopUp)
    {
        if (NoActiveOptionalPopUps)
        {
            NoOptionalPopUps?.Invoke(true);
            currentPopUp.PopUpBranch.MoveToNextPopUp(LastNodeBeforePopUp);
        }
        else
        {
            currentPopUp.PopUpBranch.MoveToNextPopUp(GetNextPopUp(ActiveOptionalPopUps));
        }
    }

    public void SetLastNodeBeforePopUp(UINode lastNode)
    {
        if (!lastNode.MyBranch.IsAPopUpBranch()) LastNodeBeforePopUp = lastNode;
    }
}
