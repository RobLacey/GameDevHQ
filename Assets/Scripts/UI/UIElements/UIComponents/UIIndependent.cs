using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIIndependent
{
    UIBranch myBranch;
    UIBranch[] _allBranches;

    public class Data
    {
        public List<UIBranch> _clearedBranches = new List<UIBranch>();
        public UINode lastSelected;
        public UINode lastHighlighted;
        public bool _fromHomeScreen;
    }

    public Data ClearedScreenData { get; private set; } = new Data();

    public UIIndependent(UIBranch branch, UIBranch[] branchList)
    {
        myBranch = branch;
        _allBranches = branchList;
    }

    public void StartIndie()
    {
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData._fromHomeScreen = false;
        ClearedScreenData.lastSelected = myBranch.UIMaster.LastSelected;
        ClearedScreenData.lastHighlighted = myBranch.UIMaster.ActiveBranch.LastHighlighted;

        foreach (var branch in _allBranches)
        {
            if (branch.MyCanvas.enabled == true && branch != myBranch)
            {
                ClearedScreenData._clearedBranches.Add(branch);

                if (branch.UIMaster.OnHomeScreen == true)
                {
                    branch.UIMaster.OnHomeScreen = false;
                    ClearedScreenData._fromHomeScreen = true;
                }

                if (myBranch.ScreenType == ScreenType.ToFullScreen)
                {
                    branch.MyCanvas.enabled = false;
                }
                branch.MyCanvasGroup.blocksRaycasts = false;
            }
        }
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
            myBranch.UIMaster.OnHomeScreen = true;
        }
        myBranch.UIMaster.LastSelected = ClearedScreenData.lastSelected;
        myBranch.UIMaster.SetLastHighlighted(ClearedScreenData.lastHighlighted);
        ClearedScreenData.lastHighlighted.SetUpNodeWhenActive();
    }

    public void PassClearedDetails(Data passedData)
    {
        ClearedScreenData = passedData;
        if (myBranch.ScreenType == ScreenType.Normal)
        {
            foreach (var branch in ClearedScreenData._clearedBranches)
            {
                if (branch.MyCanvas.enabled == false)
                {
                    branch.MyCanvas.enabled = true;
                }
            }
        }
    }

    public void RestoreScreen()
    {
        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            branch.MyCanvas.enabled = true;
            branch.MyCanvasGroup.blocksRaycasts = true;
        }
    }
}
