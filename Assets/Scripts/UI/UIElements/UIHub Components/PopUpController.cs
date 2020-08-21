using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
// TODO Need to make sure Optionals don't appear when not on main screen but buffered for return to homescreen
public class PopUpController
{
    public PopUpController() => OnEnable();

    //Variables
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private readonly List<UIBranch> _activeResolvePopUps = new List<UIBranch>();
    private readonly List<UIBranch> _activeOptionalPopUps = new List<UIBranch>();
    private UIBranch _lastBranchBeforePopUp;
    private bool _noPopUps = true;

    //Properties
    private void SaveNoPopUps(bool activePopUps) => _noPopUps = activePopUps;
    private bool NoActiveResolvePopUps => _activeResolvePopUps.Count == 0;
    private bool NoActiveOptionalPopUps => _activeOptionalPopUps.Count == 0;

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if(_noPopUps) _lastBranchBeforePopUp = newBranch;
    }

    private void AddActivePopUps_Resolve(UIBranch newResolvePopUp)
        => AddToPopUpList(newResolvePopUp, _activeResolvePopUps, NoResolvePopUps);
    private void AddToActivePopUps_Optional(UIBranch newOptionalPopUp) 
        => AddToPopUpList(newOptionalPopUp, _activeOptionalPopUps, NoOptionalPopUps);

    //Events
    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoOptionalPopUps;
    public static event Action<bool> NoPopUps;
    public static event Action<(UIBranch nextPopUp,UIBranch currentPopUp)> MoveToNextFromPopUp;
    
    private void OnEnable()
    {
        _uiPopUpEvents.SubscribeToAddResolvePopUp(AddActivePopUps_Resolve);
        _uiPopUpEvents.SubscribeToAddOptionalPopUp(AddToActivePopUps_Optional);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        UICancel.OnBackToAPopUp += RemoveNextPopUp;
        ChangeControl.ReturnNextPopUp += NextPopUp;
        OptionalPopUp.RemoveOptionalPopUp += OnLeavingHomeScreen;
    }

    private UIBranch NextPopUp()
    {
        if (!NoActiveResolvePopUps)
        {
            return GetNextPopUp(_activeResolvePopUps);
        }

        if (!NoActiveOptionalPopUps)
        {
            return GetNextPopUp(_activeOptionalPopUps);
        }

        return null;
    }

    private UIBranch GetNextPopUp(IReadOnlyList<UIBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index];
    }
    
    private void RemoveNextPopUp(UIBranch popUpToRemove)
    {
        if(_noPopUps) return;
        
        if (HasAResolvePopUpToRemove(popUpToRemove))
        {
            RemoveFromActivePopUpList(_activeResolvePopUps, WhatToDoNext_Resolve, popUpToRemove);
        }
        else if(HasAOptionalPopUpToRemove(popUpToRemove))
        {
            RemoveFromActivePopUpList(_activeOptionalPopUps, WhatToDoNext_Optional, popUpToRemove);
        }
    }

    private bool HasAResolvePopUpToRemove(UIBranch popUpToRemove) 
        => !NoActiveResolvePopUps && _activeResolvePopUps.Contains(popUpToRemove);

    private bool HasAOptionalPopUpToRemove(UIBranch popUpToRemove) 
        => !NoActiveOptionalPopUps && _activeOptionalPopUps.Contains(popUpToRemove);

    private void OnLeavingHomeScreen(UIBranch popup)
    {
        RemoveOptionalPopUp(popup);
        if (!NoActiveOptionalPopUps) return;
        
        _noPopUps = true;
        NoOptionalPopUps?.Invoke(true);
        NoPopUps?.Invoke(_noPopUps);
    }

    private void RemoveOptionalPopUp(UIBranch popup)
    {
        foreach (UIBranch activeOptionalPopUp in _activeOptionalPopUps)
        {
            if (activeOptionalPopUp != popup) continue;
            _activeOptionalPopUps.Remove(popup);
            break;
        }
    }

    private void AddToPopUpList(UIBranch newPopUp, ICollection<UIBranch> popUpList,Action<bool> noActivePopUpsEvent)
    {
        popUpList.Add(newPopUp);
        noActivePopUpsEvent?.Invoke(false);
        NoPopUps?.Invoke((false));
    }

    private void RemoveFromActivePopUpList(IList<UIBranch> popUpList, 
                                           Action<UIBranch> finishRemovalFromList,
                                           UIBranch popUpToRemove)
    {
        if(!popUpList.Contains(popUpToRemove)) return;
        popUpList.Remove(popUpToRemove);
        finishRemovalFromList?.Invoke(popUpToRemove);
    }

    private void WhatToDoNext_Resolve(UIBranch currentPopUpBranch)
    {
        if (NoActiveResolvePopUps)
        {
            NoResolvePopUps?.Invoke(true);
            WhatToDoNext_Optional(currentPopUpBranch);
        }
        else
        {
            MoveToNextFromPopUp?.Invoke((GetNextPopUp(_activeResolvePopUps), currentPopUpBranch));
        }
    }

    private void WhatToDoNext_Optional(UIBranch currentPopUpBranch)
    {
        if (NoActiveOptionalPopUps)
        {
            _noPopUps = true;
            NoOptionalPopUps?.Invoke(true);
            NoPopUps?.Invoke(_noPopUps);
            MoveToNextFromPopUp?.Invoke((_lastBranchBeforePopUp, currentPopUpBranch));
        }
        else
        {
            MoveToNextFromPopUp?.Invoke((GetNextPopUp(_activeOptionalPopUps), currentPopUpBranch));
        }
    }
}
