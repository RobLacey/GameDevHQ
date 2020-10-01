using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
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
    private bool _gameIsPaused;

    //Properties
    private void SaveNoPopUps(bool activePopUps) => _noPopUps = activePopUps;
    private void SaveGameIsPaused(bool isPaused) => _gameIsPaused = isPaused;
    private bool NoActiveResolvePopUps => _activeResolvePopUps.Count == 0;
    private bool NoActiveOptionalPopUps => _activeOptionalPopUps.Count == 0;

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if(_gameIsPaused) return;
        if (newBranch.IsAPopUpBranch() || newBranch.IsPauseMenuBranch()) return;
        _lastBranchBeforePopUp = newBranch;
    }

    private void AddActivePopUps_Resolve(UIBranch newResolvePopUp)
        => AddToPopUpList(newResolvePopUp, _activeResolvePopUps, NoResolvePopUps);
    
    private void AddToActivePopUps_Optional(UIBranch newOptionalPopUp) 
        => AddToPopUpList(newOptionalPopUp, _activeOptionalPopUps);

    //Events
    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoPopUps;
    public static event Action<(UIBranch nextPopUp,UIBranch currentPopUp)> MoveToNextFromPopUp;
    
    private void OnEnable()
    {
        _uiPopUpEvents.SubscribeToAddResolvePopUp(AddActivePopUps_Resolve);
        _uiPopUpEvents.SubscribeToAddOptionalPopUp(AddToActivePopUps_Optional);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
        _uiPopUpEvents.SubscribeToBackToAPopUp(RemoveNextPopUp);
        _uiPopUpEvents.SubscribeToRemoveOptionalPopUp(OnLeavingHomeScreen);
        _uiPopUpEvents.SubscribeToReturnNextPopUp(NextPopUp);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToGameIsPaused(SaveGameIsPaused);
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

    private static void AddToPopUpList(UIBranch newPopUp, 
                                       ICollection<UIBranch> popUpList,
                                       Action<bool> noActivePopUpsEvent = null)
    {
        popUpList.Add(newPopUp);
        noActivePopUpsEvent?.Invoke(false);
        NoPopUps?.Invoke((false));
    }

    private static void RemoveFromActivePopUpList(IList<UIBranch> popUpList, 
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
            Debug.Log(NoActiveResolvePopUps);

            MoveToNextFromPopUp?.Invoke((GetNextPopUp(_activeResolvePopUps), currentPopUpBranch));
        }
    }

    private void WhatToDoNext_Optional(UIBranch currentPopUpBranch)
    {
        if (NoActiveOptionalPopUps)
        {
            _noPopUps = true;
            NoPopUps?.Invoke(_noPopUps);
            MoveToNextFromPopUp?.Invoke((_lastBranchBeforePopUp, currentPopUpBranch));
        }
        else
        {
            MoveToNextFromPopUp?.Invoke((GetNextPopUp(_activeOptionalPopUps), currentPopUpBranch));
        }
    }
}
