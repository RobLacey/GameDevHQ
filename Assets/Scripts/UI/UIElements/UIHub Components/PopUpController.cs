using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
//TODO Add Functinality to turn off and on popUps on homescreen. Add buffer for new triggers
/// <summary>
/// Need to make sure Optionals don't appear when not on main screen but buffered for return to homescreen
/// </summary>
public class PopUpController
{
    public PopUpController()
    {
        OnEnable();
    }
    
    //Variables
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private readonly List<UIBranch> _activeResolvePopUps = new List<UIBranch>();
    private readonly List<UIBranch> _activeOptionalPopUps = new List<UIBranch>();
    private UIBranch _lastBranchBeforePopUp;
    private UIBranch _activeBranch;
    private bool _noPopUps = true;

    //Properties
    private void SaveNoPopUps(bool activePopUps)
    {
        if (_noPopUps && !activePopUps)
        {
            _lastBranchBeforePopUp = _activeBranch;
        }
        _noPopUps = activePopUps;
    }

    private bool NoActiveResolvePopUps => _activeResolvePopUps.Count == 0;
    private bool NoActiveOptionalPopUps => _activeOptionalPopUps.Count == 0;

    private void SaveActiveBranch(UIBranch newBranch)
    {
        _activeBranch = newBranch;
    }

    private void AddActivePopUps_Resolve(UIBranch newResolvePopUp)
        => AddToPopUpList(newResolvePopUp, _activeResolvePopUps, NoResolvePopUps);
    private void AddToActivePopUps_Optional(UIBranch newOptionalPopUp) 
        => AddToPopUpList(newOptionalPopUp, _activeOptionalPopUps, NoOptionalPopUps);

    //Events
    public static event Action<bool> NoResolvePopUps;
    public static event Action<bool> NoOptionalPopUps;
    public static event Action<bool> NoPopUps;

    private void OnEnable()
    {
        _uiPopUpEvents.SubscribeToAddResolvePopUp(AddActivePopUps_Resolve);
        _uiPopUpEvents.SubscribeToAddOptionalPopUp(AddToActivePopUps_Optional);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        UICancel.OnBackOnePopUp += RemoveNextPopUp;
        ChangeControl.ReturnNextPopUp += NextPopUp;
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

    private void RemoveNextPopUp()
    {
        if(_noPopUps) return;
        
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
        NoPopUps?.Invoke(false);
    }

    private void RemoveFromActivePopUpList(IList<UIBranch> popUpList, Action<UIBranch> finishRemovalFromList)
    {
        var currentPopUpBranch = popUpList[popUpList.Count - 1];
        popUpList.Remove(currentPopUpBranch);
        finishRemovalFromList?.Invoke(currentPopUpBranch);
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
            currentPopUpBranch.MoveToNextPopUp(GetNextPopUp(_activeResolvePopUps));
        }
    }

    private void WhatToDoNext_Optional(UIBranch currentPopUpBranch)
    {
        if (NoActiveOptionalPopUps)
        {
            _noPopUps = true;
            NoOptionalPopUps?.Invoke(true);
            NoPopUps?.Invoke(_noPopUps);
            currentPopUpBranch.MoveToNextPopUp(_lastBranchBeforePopUp);
        }
        else
        {
            currentPopUpBranch.MoveToNextPopUp(GetNextPopUp(_activeOptionalPopUps));
        }
    }
}
