using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedHierarchy;
using UnityEngine;

public class UIPopUp
{
    public UIPopUp(UIBranch branch, UIBranch[] branchList, IHubData newHubData)
    {
        myHubData = newHubData;
        myBranch = branch;
        _allBranches = branchList;
    }

    //Variables
    IHubData myHubData;
    UIBranch myBranch;
    UIBranch[] _allBranches;
    bool _running;
    Coroutine _coroutine;

    //Internal Classes
    private class Data
    {
        public bool _wasInMenu;
        public bool _fromHomeScreen;
        public List<UIBranch> _clearedBranches = new List<UIBranch>();
        public UINode lastHighlighted;
        public UINode lastSelected;
    }

    //Properties
    private Data ClearedScreenData { get; set; } = new Data();
    //public bool GameIsPaused { get; private set; } = false;


    /// <summary> Starts any PopUp from other classes or Inpsector Event calls </summary>     
    public void StartPopUp()
    {
        if (myBranch.IsAPopUpBranch() && !myHubData.IsUsingMouse()) 
            myBranch.AllowKeys = true;

        if (myHubData.GameIsPaused) return;

        if (!myBranch.MyCanvas.enabled)
        {
            SetUpPopUp();
        }
        else
        {
            if (myBranch.IsTimedPopUp)
            {
                StaticCoroutine.StopCoroutines(_coroutine);
                _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
            }
        }
    }

    private void SetUpPopUp()
    {
        if (myBranch.IsResolvePopUp)
        {
            StartActivePopUps_Resolve();
        }
        else if (myBranch.IsNonResolvePopUp)
        {
            StartActivePopUp_NonResolve();
        }
        else if (myBranch.IsTimedPopUp)
        {
            _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
        }
    }

    private void StartActivePopUps_Resolve()
    {
        if (!myHubData.ActivePopUps_Resolve.Contains(myBranch))
        {
            myHubData.ActivePopUps_Resolve.Add(myBranch);
        }
        PopUpStartProcess();
    }

    private void StartActivePopUp_NonResolve()
    {
        if (!myHubData.ActivePopUps_NonResolve.Contains(myBranch))
        {
            myHubData.ActivePopUps_NonResolve.Add(myBranch);
        }
        PopUpStartProcess();
    }

    public void PauseMenu()
    {
        if (myBranch.IsPause() && !myHubData.IsUsingMouse()) 
            myBranch.AllowKeys = true;

        if (myHubData.GameIsPaused)
        {
            myHubData.GameIsPaused = false;
            RestoreLastPosition();
        }
        else
        {
            myHubData.GameIsPaused = true;
            PopUpStartProcess();
        }
    }

    /// <summary> Directly Starts up the popup. Stores data, clears screen, deactivates raycasts etc </summary>     
    private void PopUpStartProcess()
    {
        StoreClearScreenData();
        
        if (!myHubData.InMenu)
        {
            myHubData.GameToMenuSwitching();
        }

        foreach (var branch in _allBranches)
        {
            if (branch.MyCanvas.enabled == true && branch != myBranch)
            {
                if (!branch.IsAPopUpBranch()) ClearedScreenData._clearedBranches.Add(branch);
                IfBranchIsFullscreen(branch);
                HandlePopUpTypes(branch);
            }
        }
        ActivatePopUp();
    }
    private void StoreClearScreenData()
    {
        ClearedScreenData._wasInMenu = myHubData.InMenu;
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData._fromHomeScreen = false;
        ClearedScreenData.lastSelected = myHubData.LastSelected;
        ClearedScreenData.lastHighlighted = myHubData.ActiveBranch.LastHighlighted;
    }

    private void IfBranchIsFullscreen(UIBranch branch)
    {
        if (myBranch.ScreenType != ScreenType.ToFullScreen) return;
        
        if (myHubData.OnHomeScreen == true)
        {
            myHubData.OnHomeScreen = false;
            ClearedScreenData._fromHomeScreen = true;
        }
        branch.MyCanvas.enabled = false;
    }

    private void HandlePopUpTypes(UIBranch branch)
    {
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
    private void ActivatePopUp()
    {
        if (!myBranch.IsResolvePopUp && !myBranch.IsPause() && myHubData.ActivePopUps_Resolve.Count != 0)
        {
            myBranch.DontSetAsActive = true;
        }
        myBranch.SaveLastSelected(myBranch.LastSelected);
        myBranch.SaveLastHighlighted(myBranch.LastSelected);
        myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
        myBranch.MoveToNextLevel();
    }
    private IEnumerator TimedPopUpProcess()
    {
        if (!_running)
        {
            myBranch.DontSetAsActive = true;
            myBranch.MoveToNextLevel();
            _running = true;
        }
        yield return new WaitForSeconds(myBranch.Timer);
        _running = false;
        myBranch.StartOutTween();
    }

    private void RestoreLastPosition(UINode lastHomeGroupNode = null)
    {
        if (myBranch.WhenToMove == WhenToMove.AtTweenEnd)
        {
            myBranch.StartOutTween(()=> EndOfTweenActions(lastHomeGroupNode));
        }
        else
        {
            myBranch.StartOutTween();
            EndOfTweenActions(lastHomeGroupNode);
        }
    }

    private void EndOfTweenActions(UINode lastHomeGroupNode)
    {
        RestoreScreen();

        if (!ClearedScreenData._wasInMenu)
        {
            myHubData.SetLastSelected(ClearedScreenData.lastSelected);
            myHubData.SetLastHighlighted(myHubData.LastNodeBeforePopUp);
            myHubData.GameToMenuSwitching();
            return;
        }
        
        if (myBranch.IsPause())
        {
            myHubData.SetLastSelected(ClearedScreenData.lastSelected);
            myBranch.SaveLastHighlighted(ClearedScreenData.lastHighlighted);
            if (myBranch.AllowKeys)
            {
                ClearedScreenData.lastHighlighted.SetNodeAsActive();
            }
        }
        else
        {
            myHubData.SetLastSelected(lastHomeGroupNode.MyBranch.MyParentBranch.LastSelected);
            myBranch.SaveLastHighlighted(lastHomeGroupNode);
            if (myBranch.AllowKeys)
            {
                lastHomeGroupNode.SetNodeAsActive();
            }
        }
    }

    private void RestoreScreen()
    {
        if (ClearedScreenData._fromHomeScreen)
        {
            myHubData.OnHomeScreen = true;
        }

        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            if (!myHubData.GameIsPaused)
            {
                if (myHubData.ActivePopUps_Resolve.Count == 0)
                {
                    branch.MyCanvasGroup.blocksRaycasts = true;
                }
                branch.MyCanvas.enabled = true;
            }
        }

        foreach (var item in myHubData.ActivePopUps_Resolve)
        {
            item.MyCanvasGroup.blocksRaycasts = true;
            item.MyCanvas.enabled = true;
        }

        foreach (var item in myHubData.ActivePopUps_NonResolve)
        {
            if (myHubData.ActivePopUps_Resolve.Count == 0)
            {
                item.MyCanvasGroup.blocksRaycasts = true;
            }
            item.MyCanvas.enabled = true;
        }
    }
    public void RemoveFromActiveList_Resolve()
    {
        if(myHubData.ActivePopUps_Resolve.Contains(myBranch))
        {
            myHubData.ActivePopUps_Resolve.Remove(myBranch);

            if (myHubData.ActivePopUps_Resolve.Count > 0)
            {
                int lastIndexItem = myHubData.ActivePopUps_Resolve.Count - 1;
                RestoreLastPosition(myHubData.ActivePopUps_Resolve[lastIndexItem].LastHighlighted);
            }
            else
            {
                if (myHubData.ActivePopUps_NonResolve.Count > 0)
                {
                    int lastIndexItem = myHubData.ActivePopUps_NonResolve.Count - 1;
                    myHubData.PopIndex = lastIndexItem;
                    RestoreLastPosition(myHubData.ActivePopUps_NonResolve[lastIndexItem].LastHighlighted);
                }
                else
                {
                    RestoreLastPosition(myHubData.LastNodeBeforePopUp);
                }
            }
        }
    }

    public void RemoveFromActiveList_NonResolve()
    {
        if(myHubData.ActivePopUps_NonResolve.Contains(myBranch))
        {
            myHubData.ActivePopUps_NonResolve.Remove(myBranch);

            if (myHubData.ActivePopUps_NonResolve.Count > 0)
            {
                myHubData.PopIndex = 0;
                myHubData.HandleActivePopUps();
                RestoreLastPosition(myHubData.ActivePopUps_NonResolve[0].LastHighlighted);
            }
            else
            {
                RestoreLastPosition(myHubData.LastNodeBeforePopUp);
            }
        }
    }

    public void ManagePopUpRaycast()
    {
        if (myHubData.ActivePopUps_Resolve.Count > 0 && !myBranch.IsResolvePopUp)
        {
            myBranch.MyCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            myBranch.MyCanvasGroup.blocksRaycasts = true;
        }
    }
}

