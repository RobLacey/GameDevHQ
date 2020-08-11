using System;
using System.Collections.Generic;

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
public class PopUpController : IMono, IPopUpControls
{
    public PopUpController()
    {
        _uiData = new UIData();
        OnEnable();
    }
    
    //Variables
    private readonly UIData _uiData;
    private readonly List<UIBranch> _activeResolvePopUps = new List<UIBranch>();
    private readonly List<UIBranch> _activeOptionalPopUps = new List<UIBranch>();
    private UIBranch _lastNodeBeforePopUp;
   
    //Properties
    public bool NoActivePopUps => _activeResolvePopUps.Count == 0
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
    
    public void OnEnable()
    {
        _uiData.SubscribeToAddResolvePopUp(AddActivePopUps_Resolve);
        _uiData.SubscribeToAddOptionalPopUp(AddToActivePopUps_Optional);
        _uiData.SubscribeToHighlightedNode(SetLastBranchBeforePopUp);
    }

    public void OnDisable()
    {
        _uiData.OnDisable();
    }

    public void ActivateCurrentPopUp()
    {
        if (!NoActiveResolvePopUps)
        {
            GetNextPopUp(_activeResolvePopUps).MoveToBranchWithoutTween();
        }
        else if(!NoActiveOptionalPopUps)
        {
            GetNextPopUp(_activeOptionalPopUps).MoveToBranchWithoutTween();
        }
    }

    private UIBranch GetNextPopUp(List<UIBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index];
    }

    public void RemoveNextPopUp()
    {
        if (!NoActiveResolvePopUps)
        {
            RemoveFromActivePopUpList(_activeResolvePopUps, WhatToDoNext_Resolve);
        }
        else if(!NoActiveOptionalPopUps)
        {
            RemoveFromActivePopUpList(_activeOptionalPopUps, WhatToDoNext_Optional);
        }
    }
    
    private void AddToPopUpList(UIBranch newPopUp, List<UIBranch> popUpList,Action<bool> NoActivePopUpsEvent)
    {
        if (popUpList.Contains(newPopUp)) return;
        popUpList.Add(newPopUp);
        NoActivePopUpsEvent?.Invoke(false);
    }

    private void RemoveFromActivePopUpList(List<UIBranch> popUpList, Action<UIBranch> finishRemovalFromList)
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
