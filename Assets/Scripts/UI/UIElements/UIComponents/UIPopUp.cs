using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUp
{
    //Variables
    UIBranch myBranch;
    UIBranch[] _allBranches;

    private class Data
    {
        public List<UIBranch> _clearedBranches = new List<UIBranch>();
        public UINode lastSelected;
        public UINode lastHighlighted;
        public bool _fromHomeScreen;
    }

    private Data ClearedScreenData { get; set; } = new Data();

    public UIPopUp(UIBranch branch, UIBranch[] branchList)
    {
        myBranch = branch;
        _allBranches = branchList;
    }

    public void StartPopUp()
    {
        if (myBranch.IsAPopUpBranch() && myBranch.IsResolvePopUp)
        {
            AddToActivePopUps_Rsolve();
        }
        else if(myBranch.IsAPopUpBranch())
        {
            AddToActivePopUp_NonResolve();
        }

        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData._fromHomeScreen = false;
        ClearedScreenData.lastSelected = myBranch.UIHub.LastSelected;
        ClearedScreenData.lastHighlighted = myBranch.UIHub.ActiveBranch.LastHighlighted;

        foreach (var branch in _allBranches)
        {
            if (branch.MyCanvas.enabled == true && branch != myBranch)
            {
                if (!branch.IsAPopUpBranch()) ClearedScreenData._clearedBranches.Add(branch);

                if (myBranch.ScreenType == ScreenType.ToFullScreen)
                {
                    if (myBranch.UIHub.OnHomeScreen == true)
                    {
                        branch.UIHub.OnHomeScreen = false;
                        ClearedScreenData._fromHomeScreen = true;
                    }
                    branch.MyCanvas.enabled = false;
                }

                if (myBranch.IsResolvePopUp && !branch.IsResolvePopUp)
                {
                    branch.MyCanvasGroup.blocksRaycasts = false;
                }

                if (myBranch.IsPause() || branch.IsPause())
                {
                    branch.MyCanvasGroup.blocksRaycasts = false;
                    myBranch.MyCanvasGroup.blocksRaycasts = true; //Ensures Pause can't be acciently deactivated
                }
            }
        }
        myBranch.SaveLastSelected(myBranch.LastSelected);
        myBranch.MoveToNextLevel();
    }

    public void RestoreLastPosition(UINode lastHomeGroupNode = null)
    {
        if (myBranch.WhenToMove == WhenToMove.AtTweenEnd)
        {
            myBranch.OutTweenToParent(()=> EndOfTweenactions(lastHomeGroupNode));
        }
        else
        {
            myBranch.OutTweenToParent();
            EndOfTweenactions(lastHomeGroupNode);
        }
    }

    private void EndOfTweenactions(UINode lastHomeGroupNode = null)
    {
        RestoreScreen();

        if (ClearedScreenData._fromHomeScreen)
        {
            myBranch.UIHub.OnHomeScreen = true;
        }

        if (myBranch.UIHub.ActivePopUps_Resolve.Count > 0)
        {
            int index = myBranch.UIHub.ActivePopUps_Resolve.Count - 1;
            UINode nextPopUp = myBranch.UIHub.ActivePopUps_Resolve[index].LastHighlighted;
            ClearedScreenData.lastHighlighted = nextPopUp;
            ClearedScreenData.lastSelected = nextPopUp;
        }

        if (myBranch.IsResolvePopUp || myBranch.IsPause())
        {
            myBranch.UIHub.LastSelected = ClearedScreenData.lastSelected;
            myBranch.UIHub.SetLastHighlighted(ClearedScreenData.lastHighlighted);

            if (myBranch.AllowKeys)
            {
                ClearedScreenData.lastHighlighted.InitailNodeAsActive();
            }
        }
        else
        {
            myBranch.UIHub.LastSelected = lastHomeGroupNode;
            myBranch.UIHub.SetLastHighlighted(lastHomeGroupNode);

            if (myBranch.AllowKeys)
            {
                lastHomeGroupNode.InitailNodeAsActive();
            }
        }
    }

    private void RestoreScreen()
    {
        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            if (!myBranch.UIHub.GameIsPaused)
            {
                if (myBranch.UIHub.ActivePopUps_Resolve.Count == 0)
                {
                    branch.MyCanvasGroup.blocksRaycasts = true;
                }

                branch.MyCanvas.enabled = true;
            }
        }

        foreach (var item in myBranch.UIHub.ActivePopUps_Resolve)
        {
            item.MyCanvasGroup.blocksRaycasts = true;
            item.MyCanvas.enabled = true;
        }

        foreach (var item in myBranch.UIHub.ActivePopUps_NonResolve)
        {
            if (myBranch.UIHub.ActivePopUps_Resolve.Count == 0)
            {
                item.MyCanvasGroup.blocksRaycasts = true;
            }
            item.MyCanvas.enabled = true;
        }
    }

    public void AddToActivePopUps_Rsolve()
    {
        if (!myBranch.UIHub.ActivePopUps_Resolve.Contains(myBranch))
        {
            myBranch.UIHub.ActivePopUps_Resolve.Add(myBranch);
        }
    }

    public void AddToActivePopUp_NonResolve()
    {
        if (!myBranch.UIHub.ActivePopUps_NonResolve.Contains(myBranch))
        {
            myBranch.UIHub.ActivePopUps_NonResolve.Add(myBranch);
        }
    }

    public void RemoveFromActiveList_Resolve()
    {
        if(myBranch.UIHub.ActivePopUps_Resolve.Contains(myBranch))
        {
            myBranch.UIHub.ActivePopUps_Resolve.Remove(myBranch);
            RestoreLastPosition();
        }
    }

    public void RemoveFromActiveList_NonResolve()
    {
        if(myBranch.UIHub.ActivePopUps_NonResolve.Contains(myBranch))
        {
            myBranch.UIHub.ActivePopUps_NonResolve.Remove(myBranch);
            RestoreLastPosition();
        }
    }

    public void ManagePopUpRaycast()
    {
        if (myBranch.UIHub.ActivePopUps_Resolve.Count > 0 && !myBranch.IsResolvePopUp)
        {
            myBranch.MyCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            myBranch.MyCanvasGroup.blocksRaycasts = true;
        }
    }
}
