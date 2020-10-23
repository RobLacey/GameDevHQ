using System;
using System.Collections.Generic;
using System.Linq;

public interface IPopUpController : IMonoBehaviourSub
{
    UIBranch NextPopUp();
    void RemoveNextPopUp(UIBranch popUpToRemove);
}

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
public class PopUpController : IPopUpController, IEventUser, INoResolvePopUp, INoPopUps
{
    public PopUpController() => OnEnable();

    //Variables
    private readonly List<UIBranch> _activeResolvePopUps = new List<UIBranch>();
    private readonly List<UIBranch> _activeOptionalPopUps = new List<UIBranch>();
    private bool _noPopUps = true;
    private bool _gameIsPaused;

    //Properties
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    public bool ActiveResolvePopUps => _activeResolvePopUps.Count > 0;
    private bool ActiveOptionalPopUps => _activeOptionalPopUps.Count > 0;
    public bool IsThereAnyPopUps => _noPopUps;

    private void SaveActiveBranch(IActiveBranch args)
    {
        if(_gameIsPaused) return;
        if (args.ActiveBranch.IsAPopUpBranch() || args.ActiveBranch.IsPauseMenuBranch()) return;
    }

    private void AddActivePopUps_Resolve(IAddResolvePopUp args)
    {
        AddToPopUpList(args.ThisPopUp, _activeResolvePopUps, NoResolvePopUps);
    }    
    private void AddToActivePopUps_Optional(IAddOptionalPopUp args) 
        => AddToPopUpList(args.ThisPopUp, _activeOptionalPopUps);

    //Events
    private static CustomEvent<INoResolvePopUp> NoResolvePopUps { get; } 
        = new CustomEvent<INoResolvePopUp>();
    private  static  CustomEvent<INoPopUps> NoPopUps { get; } = new CustomEvent<INoPopUps>();
    
    public void OnEnable() => ObserveEvents();
    public void OnDisable() => RemoveFromEvents();

    public void ObserveEvents()
    {
        EventLocator.Subscribe<IGameIsPaused>(SaveGameIsPaused, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.Subscribe<IAddOptionalPopUp>(AddToActivePopUps_Optional, this);
        EventLocator.Subscribe<IRemoveOptionalPopUp>(OnLeavingHomeScreen, this);
        EventLocator.Subscribe<IAddResolvePopUp>(AddActivePopUps_Resolve, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IGameIsPaused>(SaveGameIsPaused);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.Unsubscribe<IAddOptionalPopUp>(AddToActivePopUps_Optional);
        EventLocator.Unsubscribe<IRemoveOptionalPopUp>(OnLeavingHomeScreen);
        EventLocator.Unsubscribe<IAddResolvePopUp>(AddActivePopUps_Resolve);
    }

    public UIBranch NextPopUp()
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

    private UIBranch GetNextPopUp(IReadOnlyList<UIBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index];
    }
    
    public void RemoveNextPopUp(UIBranch popUpToRemove)
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
        => ActiveResolvePopUps && _activeResolvePopUps.Contains(popUpToRemove);

    private bool HasAOptionalPopUpToRemove(UIBranch popUpToRemove) 
        => ActiveOptionalPopUps && _activeOptionalPopUps.Contains(popUpToRemove);

    private void OnLeavingHomeScreen(IRemoveOptionalPopUp args)
    {
        RemoveOptionalPopUp(args.ThisPopUp);
        if (ActiveOptionalPopUps) return;
        
        _noPopUps = true;
        NoPopUps?.RaiseEvent(this);
    }

    private void RemoveOptionalPopUp(UIBranch popup)
    {
        if (_activeOptionalPopUps.Any(activeOptionalPopUp => activeOptionalPopUp == popup))
        {
            _activeOptionalPopUps.Remove(popup);
        }
    }

    private void AddToPopUpList(UIBranch newPopUp, 
                                ICollection<UIBranch> popUpList,
                                CustomEvent<INoResolvePopUp> noActivePopUpsEvent = null)
    {
        popUpList.Add(newPopUp);
        noActivePopUpsEvent?.RaiseEvent(this);
        _noPopUps = false;
        NoPopUps?.RaiseEvent(this);
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
        if (ActiveResolvePopUps) return;
        NoResolvePopUps?.RaiseEvent(this);
        WhatToDoNext_Optional(currentPopUpBranch);
    }

    private void WhatToDoNext_Optional(UIBranch currentPopUpBranch)
    {
        if (ActiveOptionalPopUps) return;
        _noPopUps = true;
        NoPopUps?.RaiseEvent(this);
    }
}
