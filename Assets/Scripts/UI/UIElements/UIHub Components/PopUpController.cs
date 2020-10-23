using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPopUpController: IMonoBehaviourSub { }

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
public class PopUpController : IPopUpController, IEventUser, INoResolvePopUp
{
    public PopUpController() => OnEnable();

    //Variables
    private readonly List<UIBranch> _activeResolvePopUps = new List<UIBranch>();
    private readonly List<UIBranch> _activeOptionalPopUps = new List<UIBranch>();
    private UIBranch _lastBranchBeforePopUp;
    private bool _noPopUps = true;
    private bool _gameIsPaused;

    //Properties
    private void SaveNoPopUps(bool activePopUps) => _noPopUps = activePopUps;
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    public bool NoActiveResolvePopUps => _activeResolvePopUps.Count == 0;
    private bool NoActiveOptionalPopUps => _activeOptionalPopUps.Count == 0;

    private void SaveActiveBranch(IActiveBranch args)
    {
        if(_gameIsPaused) return;
        if (args.ActiveBranch.IsAPopUpBranch() || args.ActiveBranch.IsPauseMenuBranch()) return;
        _lastBranchBeforePopUp = args.ActiveBranch;
    }

    private void AddActivePopUps_Resolve(UIBranch newResolvePopUp)
    {
        AddToPopUpList(newResolvePopUp, _activeResolvePopUps, NoResolvePopUps);
    }    
    private void AddToActivePopUps_Optional(UIBranch newOptionalPopUp) 
        => AddToPopUpList(newOptionalPopUp, _activeOptionalPopUps);

    //Events
    private static CustomEvent<INoResolvePopUp> NoResolvePopUps { get; } 
        = new CustomEvent<INoResolvePopUp>();
    private  static  CustomEvent<INoPopUps, bool> NoPopUps { get; } = new CustomEvent<INoPopUps, bool>();
    private static CustomEvent<IMoveToNextFromPopUp, (UIBranch nextPopUp,UIBranch currentPopUp)> 
        MoveToNextFromPopUp { get; }  
        = new CustomEvent<IMoveToNextFromPopUp, (UIBranch nextPopUp, UIBranch currentPopUp)>();
    
    public void OnEnable() => ObserveEvents();
    public void OnDisable() => RemoveFromEvents();

    public void ObserveEvents()
    {
        EventLocator.Subscribe<IGameIsPaused>(SaveGameIsPaused, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<INoPopUps, bool>(SaveNoPopUps, this);
        EventLocator.SubscribeToEvent<IAddOptionalPopUp, UIBranch>(AddToActivePopUps_Optional, this);
        EventLocator.SubscribeToEvent<IRemoveOptionalPopUp, UIBranch>(OnLeavingHomeScreen, this);
        EventLocator.SubscribeToEvent<IAddResolvePopUp, UIBranch>(AddActivePopUps_Resolve, this);
        EventLocator.SubscribeToEvent<IBackToNextPopUp, UIBranch>(RemoveNextPopUp, this);
        EventLocator.SubscribeToEvent<IReturnNextPopUp, UIBranch>(NextPopUp, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IGameIsPaused>(SaveGameIsPaused);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<INoPopUps, bool>(SaveNoPopUps);
        EventLocator.UnsubscribeFromEvent<IAddOptionalPopUp, UIBranch>(AddToActivePopUps_Optional);
        EventLocator.UnsubscribeFromEvent<IRemoveOptionalPopUp, UIBranch>(OnLeavingHomeScreen);
        EventLocator.UnsubscribeFromEvent<IAddResolvePopUp, UIBranch>(AddActivePopUps_Resolve);
        EventLocator.UnsubscribeFromEvent<IBackToNextPopUp, UIBranch>(RemoveNextPopUp);
        EventLocator.UnsubscribeFromEvent<IReturnNextPopUp, UIBranch>(NextPopUp);
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
        NoPopUps?.RaiseEvent(_noPopUps);
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

    private void AddToPopUpList(UIBranch newPopUp, 
                                ICollection<UIBranch> popUpList,
                                CustomEvent<INoResolvePopUp> noActivePopUpsEvent = null)
    {
        popUpList.Add(newPopUp);
        noActivePopUpsEvent?.RaiseEvent(this);
        NoPopUps?.RaiseEvent((false));
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
            NoResolvePopUps?.RaiseEvent(this);
            WhatToDoNext_Optional(currentPopUpBranch);
        }
        else
        {
            MoveToNextFromPopUp?.RaiseEvent((GetNextPopUp(_activeResolvePopUps), currentPopUpBranch));
        }
    }

    private void WhatToDoNext_Optional(UIBranch currentPopUpBranch)
    {
        if (NoActiveOptionalPopUps)
        {
            _noPopUps = true;
            NoPopUps?.RaiseEvent(_noPopUps);
            Debug.Log(_lastBranchBeforePopUp);
            MoveToNextFromPopUp?.RaiseEvent((_lastBranchBeforePopUp, currentPopUpBranch));
        }
        else
        {
            MoveToNextFromPopUp?.RaiseEvent((GetNextPopUp(_activeOptionalPopUps), currentPopUpBranch));
        }
    }
}
