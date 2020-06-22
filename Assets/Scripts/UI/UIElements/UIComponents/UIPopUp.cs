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
        if (myBranch.MyBranchType == BranchType.PopUp)
        {
            AddToActivePopUps();
        }
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData._fromHomeScreen = false;
        ClearedScreenData.lastSelected = myBranch.UIHub.LastSelected;
        ClearedScreenData.lastHighlighted = myBranch.UIHub.ActiveBranch.LastHighlighted;

        foreach (var branch in _allBranches)
        {
            if (branch.MyCanvas.enabled == true && branch != myBranch)
            {
                ClearedScreenData._clearedBranches.Add(branch);

                if (myBranch.ScreenType == ScreenType.ToFullScreen)
                {
                    if (myBranch.UIHub.OnHomeScreen == true)
                    {
                        branch.UIHub.OnHomeScreen = false;
                        ClearedScreenData._fromHomeScreen = true;
                    }
                    branch.MyCanvas.enabled = false;
                }

                if (branch.MyBranchType != BranchType.PopUp)
                {
                    branch.MyCanvasGroup.blocksRaycasts = false;
                }

                if (myBranch.MyBranchType == BranchType.PauseMenu)
                {
                    branch.MyCanvasGroup.blocksRaycasts = false;
                }
            }
        }
        myBranch.SaveLastSelected(myBranch.LastSelected);
        myBranch.MoveToNextLevel();
    }

    public void RestoreLastPosition()
    {
        if (myBranch.WhenToMove == WhenToMove.AtTweenEnd)
        {
            myBranch.OutTweenToParent(()=> EndOfTweenactions());
        }
        else
        {
            myBranch.OutTweenToParent();
            EndOfTweenactions();
        }
    }

    private void EndOfTweenactions()
    {
        RestoreScreen();

        if (ClearedScreenData._fromHomeScreen)
        {
            myBranch.UIHub.OnHomeScreen = true;
        }

        if (myBranch.UIHub.ActivePopUps.Count > 0)
        {
            UINode nextPopUp = myBranch.UIHub.ActivePopUps[myBranch.UIHub.ActivePopUps.Count - 1].LastHighlighted;
            ClearedScreenData.lastHighlighted = nextPopUp;
            ClearedScreenData.lastSelected = nextPopUp;
        }

        myBranch.UIHub.LastSelected = ClearedScreenData.lastSelected;
        myBranch.UIHub.SetLastHighlighted(ClearedScreenData.lastHighlighted);

        if (myBranch.AllowKeys)
        {
            ClearedScreenData.lastHighlighted.InitailNodeAsActive();
        }
    }

    private void RestoreScreen()
    {
        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            if (myBranch.UIHub.GameIsPaused)
            {
                if (branch.MyBranchType == BranchType.PopUp)
                {
                    branch.MyCanvasGroup.blocksRaycasts = true;
                }
            }

            else if(myBranch.UIHub.ActivePopUps.Count > 0)
            {
                if (branch.MyBranchType == BranchType.PopUp)
                {
                    branch.MyCanvasGroup.blocksRaycasts = true;
                }
            }
            else
            {
                branch.MyCanvasGroup.blocksRaycasts = true;
            }
            branch.MyCanvas.enabled = true;
        }
    }

    public void AddToActivePopUps()
    {
        myBranch.UIHub.ActivePopUps.Add(myBranch);
    }

    public void RemoveFromActiveList()
    {
        if(myBranch.UIHub.ActivePopUps.Contains(myBranch))
        {
            myBranch.UIHub.ActivePopUps.Remove(myBranch);
            RestoreLastPosition();
        }
    }
}
