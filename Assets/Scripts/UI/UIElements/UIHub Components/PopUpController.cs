using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
public class PopUpController
{
    public PopUpController()
    {
        _uiDataEvents = new UIDataEvents();
        _uiPopUpEvents = new UIPopUpEvents();
        OnEnable();
    }
    
    //Variables
    private readonly UIDataEvents _uiDataEvents;
    private readonly UIPopUpEvents _uiPopUpEvents;
    private readonly List<UIBranch> _activeResolvePopUps = new List<UIBranch>();
    private readonly List<UIBranch> _activeOptionalPopUps = new List<UIBranch>();
    private UIBranch _lastNodeBeforePopUp;
   
    //Properties
    private bool NoActivePopUps => _activeResolvePopUps.Count == 0
                                  & _activeOptionalPopUps.Count == 0;
    private bool NoActiveResolvePopUps => _activeResolvePopUps.Count == 0;
    private bool NoActiveOptionalPopUps => _activeOptionalPopUps.Count == 0;
    private void AddActivePopUps_Resolve(UIBranch newResolvePopUp)
        => AddToPopUpList(newResolvePopUp, _activeResolvePopUps, NoResolvePopUps);
    private void AddToActivePopUps_Optional(UIBranch newOptionalPopUp) 
        => AddToPopUpList(newOptionalPopUp, _activeOptionalPopUps, NoOptionalPopUps);

    //Delegates
    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoOptionalPopUps;
    public static event Action<bool> NoPopUps;

    private void OnEnable()
    {
        _uiPopUpEvents.SubscribeToAddResolvePopUp(AddActivePopUps_Resolve);
        _uiPopUpEvents.SubscribeToAddOptionalPopUp(AddToActivePopUps_Optional);
        _uiDataEvents.SubscribeToSelectedNode(SetLastBranchBeforePopUp);
        _uiDataEvents.SubscribeToBackOneLevel(RemoveNextPopUp);
    }

    private UIBranch GetNextPopUp(IReadOnlyList<UIBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index];
    }

    private void RemoveNextPopUp(UIBranch lastBranch = null)
    {
        if(NoActivePopUps) return;
        
        if (!NoActiveResolvePopUps)
        {
            RemoveFromActivePopUpList(_activeResolvePopUps, WhatToDoNext_Resolve);
        }
        else if(!NoActiveOptionalPopUps)
        {
            RemoveFromActivePopUpList(_activeOptionalPopUps, WhatToDoNext_Optional);
        }
    }
    
    private void AddToPopUpList(UIBranch newPopUp, ICollection<UIBranch> popUpList,Action<bool> noActivePopUpsEvent)
    {
        if (popUpList.Contains(newPopUp)) return;
        popUpList.Add(newPopUp);
        noActivePopUpsEvent?.Invoke(false);
        NoPopUps?.Invoke(NoActivePopUps);
    }

    private void RemoveFromActivePopUpList(IList<UIBranch> popUpList, Action<UIBranch> finishRemovalFromList)
    {
        var lastPopUp = popUpList[popUpList.Count - 1];
        popUpList.Remove(lastPopUp);
        finishRemovalFromList?.Invoke(lastPopUp);
    }

    private void WhatToDoNext_Resolve(UIBranch lastPopUpBranch)
    {
        if (NoActiveResolvePopUps)
        {
            NoResolvePopUps?.Invoke(true);
            NoPopUps?.Invoke(NoActivePopUps);
            WhatToDoNext_Optional(lastPopUpBranch);
        }
        else
        {
            lastPopUpBranch.MoveToNextPopUp(GetNextPopUp(_activeResolvePopUps));
        }
    }

    private void WhatToDoNext_Optional(UIBranch currentPopUpBranch)
    {
        if (NoActiveOptionalPopUps)
        {
            NoOptionalPopUps?.Invoke(true);
            NoPopUps?.Invoke(NoActivePopUps);
            currentPopUpBranch.MoveToNextPopUp(_lastNodeBeforePopUp);
        }
        else
        {
            currentPopUpBranch.MoveToNextPopUp(GetNextPopUp(_activeOptionalPopUps));
        }
    }

    private void SetLastBranchBeforePopUp(UINode lastBranch)
    {
        if (IsNotAPopUpOrPauseMenu(lastBranch)) 
            _lastNodeBeforePopUp = lastBranch.MyBranch;
    }

    private static bool IsNotAPopUpOrPauseMenu(UINode lastBranch)
    {
        return !lastBranch.MyBranch.IsAPopUpBranch() || !lastBranch.MyBranch.IsPauseMenuBranch();
    }

}
