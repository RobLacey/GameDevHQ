using System;
using System.Collections.Generic;

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
public class PopUpController : IPopUpController, IEventUser, INoResolvePopUp, INoPopUps, IEventDispatcher
{
    //Variables
    private readonly List<IBranch> _activeResolvePopUps = new List<IBranch>();
    private readonly List<IBranch> _activeOptionalPopUps = new List<IBranch>();
    private bool _noPopUps = true;

    //Properties & setters
    public bool ActiveResolvePopUps => _activeResolvePopUps.Count > 0;
    private bool ActiveOptionalPopUps => _activeOptionalPopUps.Count > 0;
    public bool NoActivePopUps => _noPopUps;
    public int ActiveResolvePopUpCount => _activeResolvePopUps.Count;
    public int ActiveOptionalPopUpCount => _activeOptionalPopUps.Count;
    
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
        NoResolvePopUps = EVent.Do.Fetch<INoResolvePopUp>();
        NoPopUps = EVent.Do.Fetch<INoPopUps>();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IAddOptionalPopUp>(AddToActivePopUps_Optional);
        EVent.Do.Subscribe<IRemoveOptionalPopUp>(OnLeavingHomeScreen);
        EVent.Do.Subscribe<IAddResolvePopUp>(AddActivePopUps_Resolve);
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
        RemoveOptionalPopUp(args.ThisPopUp);
        if (ActiveOptionalPopUps) return;
        
        _noPopUps = true;
        NoPopUps?.Invoke(this);
    }

    private void RemoveOptionalPopUp(IBranch popup)
    {
        foreach (var optionalPopUp in _activeOptionalPopUps)
        {
            if (optionalPopUp == popup)
            {
                _activeOptionalPopUps.Remove(popup);
                break;
            }
        }
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

    private static void RemoveFromActivePopUpList(List<IBranch> popUpList, 
                                                  Action<IBranch> finishRemovalFromList,
                                                  IBranch popUpToRemove)
    {
        if(!popUpList.Contains(popUpToRemove)) return;
        popUpList.Remove(popUpToRemove);
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
