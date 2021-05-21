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
                               IEZEventDispatcher, ILastRemovedPopUp
{
    //Variables
    private readonly List<IBranch> _activeResolvePopUps = new List<IBranch>();
    private readonly List<IBranch> _activeOptionalPopUps = new List<IBranch>();
    private bool _noPopUps = true;

    //Properties & setters
    public bool ActiveResolvePopUps => _activeResolvePopUps.Count > 0;
    private bool ActiveOptionalPopUps => _activeOptionalPopUps.Count > 0;
    public bool NoActivePopUps => _noPopUps;
    public IBranch LastOptionalPopUp { get; private set; }


    //Events
    private Action<INoResolvePopUp> NoResolvePopUps { get; set; }
    private Action<INoPopUps> NoPopUps { get; set; }
    private Action<ILastRemovedPopUp> LastRemovedPopUp { get; set; }
    
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
        LastRemovedPopUp = PopUpEvents.Do.Fetch<ILastRemovedPopUp>();
    }

    public void ObserveEvents()
    {
        PopUpEvents.Do.Subscribe<IAddOptionalPopUp>(AddToActivePopUps_Optional);
        PopUpEvents.Do.Subscribe<IRemoveOptionalPopUp>(OnLeavingHomeScreen);
        PopUpEvents.Do.Subscribe<IAddResolvePopUp>(AddActivePopUps_Resolve);
    }

    public IBranch NextPopUp()
    {
        if (ActiveResolvePopUps)
        {
            return GetNextPopUp(_activeResolvePopUps);
        }

        if (ActiveOptionalPopUps)
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
        => ActiveResolvePopUps && _activeResolvePopUps.Contains(popUpToRemove);

    private bool HasAOptionalPopUpToRemove(IBranch popUpToRemove) 
        => ActiveOptionalPopUps && _activeOptionalPopUps.Contains(popUpToRemove);

    private void OnLeavingHomeScreen(IRemoveOptionalPopUp args)
    {
        _activeOptionalPopUps.Remove(args.ThisPopUp);
        if (ActiveOptionalPopUps) return;
        
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
        LastOptionalPopUp = popUpToRemove;
        LastRemovedPopUp?.Invoke(this);
        finishRemovalFromList?.Invoke(popUpToRemove);
    }

    private void WhatToDoNext_Resolve(IBranch currentPopUpBranch)
    {
        if (ActiveResolvePopUps) return;
        NoResolvePopUps?.Invoke(this);
        WhatToDoNext_Optional(currentPopUpBranch);
    }

    private void WhatToDoNext_Optional(IBranch currentPopUpBranch)
    {
        if (ActiveOptionalPopUps) return;
        _noPopUps = true;
        NoPopUps?.Invoke(this);
    }

}
