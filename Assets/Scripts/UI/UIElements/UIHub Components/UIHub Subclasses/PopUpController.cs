using System;
using System.Collections.Generic;
using EZ.Events;
using UnityEngine;

public interface IPopUpController
{
    void OnEnable();
    IBranch NextPopUp();
    void RemoveNextPopUp(IBranch popUpToRemove);
}

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
public class PopUpController : IPopUpController, IEZEventUser, INoResolvePopUp, INoPopUps, 
                               IEZEventDispatcher
{
    //Variables
    private readonly List<IBranch> _activeResolvePopUps = new List<IBranch>();
    private readonly List<IBranch> _activeOptionalPopUps = new List<IBranch>();
    private bool _noPopUps = true;

    //Properties & setters
    public bool NoActiveResolvePopUps => _activeResolvePopUps.Count == 0;
    private bool NoActiveOptionalPopUps => _activeOptionalPopUps.Count == 0;
    public bool NoActivePopUps => _noPopUps;

    //Events
    private Action<INoResolvePopUp> NoResolvePopUps { get; set; }
    private Action<INoPopUps> NoPopUps { get; set; }
    
    //Main
    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }
    
    public void FetchEvents()
    {
        NoResolvePopUps = PopUpEvents.Do.Fetch<INoResolvePopUp>();
        NoPopUps = PopUpEvents.Do.Fetch<INoPopUps>();
    }

    public void ObserveEvents()
    {
        PopUpEvents.Do.Subscribe<IAddOptionalPopUp>(AddToActivePopUps_Optional);
        PopUpEvents.Do.Subscribe<IClearOptionalPopUp>(ClearPopUpsOnLeavingHomeScreen);
        PopUpEvents.Do.Subscribe<IAddResolvePopUp>(AddActivePopUps_Resolve);
        BranchEvent.Do.Subscribe<ICloseBranch>(RemoveClosedBranch);
    }

    public void UnObserveEvents() { }

    private void RemoveClosedBranch(ICloseBranch args) => RemoveNextPopUp(args.TargetBranch);

    public IBranch NextPopUp()
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

    private static IBranch GetNextPopUp(List<IBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index];
    }
    
    public void RemoveNextPopUp(IBranch popUpToRemove)
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

    private bool HasAResolvePopUpToRemove(IBranch popUpToRemove) 
        => !NoActiveResolvePopUps && _activeResolvePopUps.Contains(popUpToRemove);

    private bool HasAOptionalPopUpToRemove(IBranch popUpToRemove) 
        => !NoActiveOptionalPopUps && _activeOptionalPopUps.Contains(popUpToRemove);

    private void ClearPopUpsOnLeavingHomeScreen(IClearOptionalPopUp args)
    {
        _activeOptionalPopUps.Remove(args.ThisPopUp);
        if (!NoActiveOptionalPopUps) return;
        
        _noPopUps = true;
        NoPopUps?.Invoke(this);
    }
    
    private void AddActivePopUps_Resolve(IAddResolvePopUp args)
    {
        AddToPopUpList(args.ThisPopUp, _activeResolvePopUps);
        NoResolvePopUps?.Invoke(this);
    }

    private void AddToActivePopUps_Optional(IAddOptionalPopUp args) 
        => AddToPopUpList(args.ThisPopUp, _activeOptionalPopUps);


    private void AddToPopUpList(IBranch newPopUp, 
                                List<IBranch> popUpList)
    {
        if(popUpList.Contains(newPopUp)) return;
        popUpList.Add(newPopUp);
        _noPopUps = false;
        NoPopUps?.Invoke(this);
    }

    private void RemoveFromActivePopUpList(List<IBranch> popUpList, 
                                                  Action<IBranch> finishRemovalFromList,
                                                  IBranch popUpToRemove)
    {
        if(!popUpList.Contains(popUpToRemove)) return;
        popUpList.Remove(popUpToRemove);
        popUpToRemove.OnDisable();
        finishRemovalFromList?.Invoke(popUpToRemove);
    }

    private void WhatToDoNext_Resolve(IBranch currentPopUpBranch)
    {
        if (!NoActiveResolvePopUps) return;
        NoResolvePopUps?.Invoke(this);
        WhatToDoNext_Optional(currentPopUpBranch);
    }

    private void WhatToDoNext_Optional(IBranch currentPopUpBranch)
    {
        if (!NoActiveOptionalPopUps) return;
        _noPopUps = true;
        NoPopUps?.Invoke(this);
    }
}
