using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedHierarchy;
using UnityEngine;

public class UIPopUp
{
    public UIPopUp(UIBranch branch, UIBranch[] branchList, UIHub newHub)
    {
        _uIHub = newHub;
        myBranch = branch;
        _allBranches = branchList;
    }

    //Variables
    UIHub _uIHub;
    UIBranch myBranch;
    UIBranch[] _allBranches;
    bool _running;
    Coroutine _coroutine;

    //Internal Classes
    private class Data
    {
        public bool _wasInMenu;
       // public bool _fromHomeScreen;
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
       // if (myBranch.IsAPopUpBranch() && !_uIHub.IsUsingMouse()) 
           // myBranch.AllowKeys = true;

        if (_uIHub.GameIsPaused) return;

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
        if (!_uIHub.ActivePopUpsResolve.Contains(myBranch))
        {
            _uIHub.ActivePopUpsResolve.Add(myBranch);
        }
        PopUpStartProcess();
    }

    private void StartActivePopUp_NonResolve()
    {
        if (!_uIHub.ActivePopUpsNonResolve.Contains(myBranch))
        {
            _uIHub.ActivePopUpsNonResolve.Add(myBranch);
        }
        PopUpStartProcess();
    }

    public void PauseMenu()
    {
        //if (myBranch.IsPause() && !_uIHub.IsUsingMouse()) 
           // myBranch.AllowKeys = true;

        if (_uIHub.GameIsPaused)
        {
            _uIHub.GameIsPaused = false;
            RestoreLastPosition(ClearedScreenData.lastHighlighted);
        }
        else
        {
            _uIHub.GameIsPaused = true;
            PopUpStartProcess();
        }
    }

    /// <summary> Directly Starts up the popup. Stores data, clears screen, deactivates raycasts etc </summary>     
    private void PopUpStartProcess()
    {
        StoreClearScreenData();
        
        if (!_uIHub.InMenu)
        {
            _uIHub.GameToMenuSwitching();
        }

        foreach (var branch in _allBranches)
        {
            if (branch.MyCanvas.enabled && branch != myBranch)
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
        ClearedScreenData._wasInMenu = _uIHub.InMenu;
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData.lastSelected = _uIHub.LastSelected;
        ClearedScreenData.lastHighlighted = _uIHub.ActiveBranch.LastHighlighted;
    }

    private void IfBranchIsFullscreen(UIBranch branch)
    {
        if (myBranch.ScreenType != ScreenType.ToFullScreen) return;
        
      //  if (_uIHub.OnHomeScreen)
       // {
           // _uIHub.OnHomeScreen = false;
      //  }
        branch.MyCanvas.enabled = false;
    }

    private void HandlePopUpTypes(UIBranch branch)
    {
        if (myBranch.IsResolvePopUp && !branch.IsResolvePopUp)
        {
            branch.MyCanvasGroup.blocksRaycasts = false;
        }

        if (!myBranch.IsPause() && !branch.IsPause()) return;
        branch.MyCanvasGroup.blocksRaycasts = false;
        myBranch.MyCanvasGroup.blocksRaycasts = true; //Ensures Pause can't be acciently deactivated
    }
    private void ActivatePopUp() //Todo maybe add to wait list
    {
        if (myBranch.IsNonResolvePopUp && _uIHub.ActivePopUpsResolve.Count > 0)
        {
            myBranch.DontSetAsActive = true;
        }
        if(myBranch.IsPause()) myBranch.SaveLastSelected(myBranch.LastSelected);
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
        if (myBranch.WhenToMove == WhenToMove.AfterEndOfTween)
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
        if(myBranch.ScreenType == ScreenType.ToFullScreen) RestoreScreen();

        if (!ClearedScreenData._wasInMenu)
        {
            _uIHub.SetLastSelected(ClearedScreenData.lastSelected);
            _uIHub.SetLastHighlighted(_uIHub.LastNodeBeforePopUp);
            _uIHub.GameToMenuSwitching();
            //return;
        }
        
        // if (myBranch.IsPause())
        // {
        //     myHubData.SetLastSelected(ClearedScreenData.lastSelected);
        //     myBranch.SaveLastHighlighted(ClearedScreenData.lastHighlighted);
        //     if (myBranch.AllowKeys)
        //     {
        //         ClearedScreenData.lastHighlighted.SetNodeAsActive();
        //     }
        // }
        else
        {
            if (!lastHomeGroupNode.MyBranch.MyParentBranch)
            {
                _uIHub.SetLastSelected(ClearedScreenData.lastSelected);
            }
            else
            {
                _uIHub.SetLastSelected(lastHomeGroupNode.MyBranch.MyParentBranch.LastSelected);
            }
            myBranch.SaveLastHighlighted(lastHomeGroupNode);
            if (myBranch.AllowKeys)
            {
                lastHomeGroupNode.SetNodeAsActive();
            }
        }
        // else if(!myHubData.OnHomeScreen)
        // {
        //     myHubData.SetLastSelected(ClearedScreenData.lastSelected);
        //     myBranch.SaveLastHighlighted(ClearedScreenData.lastHighlighted);
        //     if (myBranch.AllowKeys)
        //     {
        //         lastHomeGroupNode.SetNodeAsActive();
        //     }
        //
        // }
    }

    private void RestoreScreen()
    {
      //  if (ClearedScreenData._fromHomeScreen)
       // if (!_uIHub.OnHomeScreen) { _uIHub.OnHomeScreen = true; }

        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            if (!_uIHub.GameIsPaused)
            {
                if (_uIHub.ActivePopUpsResolve.Count == 0)
                {
                    branch.MyCanvasGroup.blocksRaycasts = true;
                }
                branch.MyCanvas.enabled = true;
            }
        }

        foreach (var item in _uIHub.ActivePopUpsResolve)
        {
            item.MyCanvasGroup.blocksRaycasts = true;
            item.MyCanvas.enabled = true;
        }

        foreach (var item in _uIHub.ActivePopUpsNonResolve)
        {
            if (_uIHub.ActivePopUpsResolve.Count == 0)
            {
                item.MyCanvasGroup.blocksRaycasts = true;
            }
            item.MyCanvas.enabled = true;
        }
    }
    public void RemoveFromActiveList_Resolve()
    {
        if(_uIHub.ActivePopUpsResolve.Contains(myBranch))
        {
            _uIHub.ActivePopUpsResolve.Remove(myBranch);

            if (_uIHub.ActivePopUpsResolve.Count > 0)
            {
                int lastIndexItem = _uIHub.ActivePopUpsResolve.Count - 1;
                RestoreLastPosition(_uIHub.ActivePopUpsResolve[lastIndexItem].LastHighlighted);
            }
            else
            {
                if (_uIHub.ActivePopUpsNonResolve.Count > 0)
                {
                    int lastIndexItem = _uIHub.ActivePopUpsNonResolve.Count - 1;
                    _uIHub.PopIndex = lastIndexItem;
                    RestoreLastPosition(_uIHub.ActivePopUpsNonResolve[lastIndexItem].LastHighlighted);
                }
                else
                {
                    RestoreLastPosition(_uIHub.LastNodeBeforePopUp);
                }
            }
        }
    }

    public void RemoveFromActiveList_NonResolve()
    {
        if (!_uIHub.ActivePopUpsNonResolve.Contains(myBranch)) return;
        _uIHub.ActivePopUpsNonResolve.Remove(myBranch);

        if (_uIHub.ActivePopUpsNonResolve.Count > 0)
        {
            _uIHub.PopIndex = 0;
            _uIHub.HandleActivePopUps();
            RestoreLastPosition(_uIHub.ActivePopUpsNonResolve[0].LastHighlighted);
        }
        else
        {
            RestoreLastPosition(_uIHub.LastNodeBeforePopUp);
        }
    }

    public void ManagePopUpResolve()
    {
        if (_uIHub.ActivePopUpsResolve.Count > 0 && !myBranch.IsResolvePopUp)
        {
            myBranch.MyCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            myBranch.MyCanvasGroup.blocksRaycasts = true;
        }
    }
}

