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
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData._fromHomeScreen = false;
        ClearedScreenData.lastSelected = myBranch.UIHub.LastSelected;
        ClearedScreenData.lastHighlighted = myBranch.UIHub.ActiveBranch.LastHighlighted;

        foreach (var branch in _allBranches)
        {
            if (branch.MyCanvas.enabled == true && branch != myBranch)
            {
                if (branch.MyBranchType != BranchType.PopUp)
                {
                    ClearedScreenData._clearedBranches.Add(branch);
                }


                if (myBranch.ScreenType == ScreenType.ToFullScreen)
                {
                    if (myBranch.UIHub.OnHomeScreen == true)
                    {
                        branch.UIHub.OnHomeScreen = false;
                        ClearedScreenData._fromHomeScreen = true;
                    }
                    branch.MyCanvas.enabled = false;
                }

                if (myBranch.BlockRaycasts)
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

        if (myBranch.AllowKeys)
        {
            myBranch.UIHub.LastSelected = ClearedScreenData.lastSelected;
            myBranch.UIHub.SetLastHighlighted(ClearedScreenData.lastHighlighted);
            ClearedScreenData.lastHighlighted.InitailNodeAsActive();
        }
    }

    private void RestoreScreen()
    {
        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            if (ClearedScreenData._fromHomeScreen)
            {
                branch.MyCanvas.enabled = true;
            }
            branch.MyCanvasGroup.blocksRaycasts = true;
        }
    }
}
