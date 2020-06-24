using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUp
{
    private class CoRunner : MonoBehaviour
    {
        public void StartCo(IEnumerator corouitne)
        {
            StartCoroutine(corouitne);
        }
    }

    //Variables
    UIBranch myBranch;
    UIBranch[] _allBranches;
    CoRunner _CoRunner;
    bool _running;
    Coroutine _coroutine;

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
        if (myBranch.UIHub.ActivePopUps_NonResolve.Count == 0 && myBranch.UIHub.ActivePopUps_Resolve.Count == 0)
        {
            myBranch.UIHub.LastNodeBeforePopUp = myBranch.UIHub.LastHighlighted;
        }

        if (myBranch.IsResolvePopUp && !myBranch.MyCanvas.enabled)
        {
            AddToActivePopUps_Rsolve();
        }
        else if (myBranch.IsNonResolvePopUp && !myBranch.MyCanvas.enabled)
        {
            AddToActivePopUp_NonResolve();
        }
        else if (myBranch.IsTimedPopUp)
        {
            if (!_running)
            {
                _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
                return;
            }
            else
            {
                StaticCoroutine.StopCoroutines(_coroutine);
                _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
                return;
            }
        }
    }

    private void MainProcess()
    {
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

        if (!myBranch.IsResolvePopUp && !myBranch.IsPause() && myBranch.UIHub.ActivePopUps_Resolve.Count != 0)
        {
            myBranch.DontSetAsActive = true;
        }
        myBranch.SaveLastSelected(myBranch.LastSelected);
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
        myBranch.OutTweenToParent();
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

        if (myBranch.IsPause())
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
        MainProcess();

        if (!myBranch.UIHub.ActivePopUps_Resolve.Contains(myBranch))
        {
            myBranch.UIHub.ActivePopUps_Resolve.Add(myBranch);
        }
    }

    public void AddToActivePopUp_NonResolve()
    {
        MainProcess();

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

            if (myBranch.UIHub.ActivePopUps_Resolve.Count > 0)
            {
                int lastIndexItem = myBranch.UIHub.ActivePopUps_Resolve.Count - 1;
                RestoreLastPosition(myBranch.UIHub.ActivePopUps_Resolve[lastIndexItem].LastHighlighted);
            }
            else
            {
                if (myBranch.UIHub.ActivePopUps_NonResolve.Count > 0)
                {
                    int lastIndexItem = myBranch.UIHub.ActivePopUps_NonResolve.Count - 1;
                    myBranch.UIHub.PopIndex = lastIndexItem;
                    RestoreLastPosition(myBranch.UIHub.ActivePopUps_NonResolve[lastIndexItem].LastHighlighted);
                }
                else
                {
                    RestoreLastPosition(myBranch.UIHub.LastNodeBeforePopUp);
                }
            }
        }
    }

    public void RemoveFromActiveList_NonResolve()
    {
        if(myBranch.UIHub.ActivePopUps_NonResolve.Contains(myBranch))
        {
            myBranch.UIHub.ActivePopUps_NonResolve.Remove(myBranch);

            if (myBranch.UIHub.ActivePopUps_NonResolve.Count > 0)
            {
                myBranch.UIHub.PopIndex = 0;
                myBranch.UIHub.HandleActivePopUps();
                RestoreLastPosition(myBranch.UIHub.ActivePopUps_NonResolve[0].LastHighlighted);
            }
            else
            {
                RestoreLastPosition(myBranch.UIHub.LastHomePosition);
            }
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

