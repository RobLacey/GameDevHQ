using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedHierarchy;
using UnityEngine;

public class UIPopUp : IHUbData, IMono
{
    public UIPopUp(UIBranch branch, UIBranch[] branchList, UIHub newHub)
    {
        _uIHub = newHub;
        myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }

    //Variables
    UIHub _uIHub;
    private UIBranch myBranch;
    UIBranch[] _allBranches;
    bool _running;
    Coroutine _coroutine;
     private bool _noActiveResolvePopUps;
     private bool _noActiveNonResolvePopUps;
    
    public static event Action<UIBranch> AddToResolvePopUp;
   // public static event Action<UIBranch> RemoveResolvePopUp;
    public static event Action<UIBranch> AddToNonResolvePopUp;
    //public static event Action<UIBranch> RemoveNonResolvePopUp;
   // public static event Func<UIBranch> ReturnNextResolve;
    //public static event Func<UIBranch> ReturnNextNonResolve;


    //Internal Classes
    private class Data
    {
        //public bool _wasInMenu;
        public List<UIBranch> _clearedBranches = new List<UIBranch>();
        public UINode lastHighlighted;
        public UINode lastSelected;
    }

    //Properties
    private Data ClearedScreenData { get; set; } = new Data();
    private bool InMenuBeforePopUp { get; set; }
    public bool GameIsPaused { get; private set; }

    public void OnEnable()
    {
        UIHub.GamePaused += IsGamePaused;
        UIHub.NoResolvePopUps += SetResolveCount;
        UIHub.NoNonResolvePopUps += SetNonResolveCount;
    }
    public void OnDisable( )
    {
        UIHub.GamePaused -= IsGamePaused;
        UIHub.NoResolvePopUps -= SetResolveCount;
        UIHub.NoNonResolvePopUps -= SetNonResolveCount;
    }

    public void IsGamePaused(bool paused) => GameIsPaused = paused;
    private void SetResolveCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
    private void SetNonResolveCount(bool activeNonResolvePopUps) => _noActiveNonResolvePopUps = activeNonResolvePopUps;

    /// <summary> Starts any PopUp from other classes or Inpsector Event calls </summary>     
    public void StartPopUp()
    {
        if (/*_uIHub.*/GameIsPaused) return;
        
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
        if (!_uIHub.InMenu && _uIHub.NoActivePopUps)
        {
            InMenuBeforePopUp = false;
            _uIHub.GameToMenuSwitching();
        }

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
        //if (!_uIHub.ActivePopUpsResolve.Contains(myBranch))
        //{
            //_uIHub.ActivePopUpsResolve.Add(myBranch);
            AddToResolvePopUp?.Invoke(myBranch);
       // }
        PopUpStartProcess();
    }

    private void StartActivePopUp_NonResolve()
    {
        //if (!_uIHub.ActivePopUpsNonResolve.Contains(myBranch))
         // {
            //_uIHub.ActivePopUpsNonResolve.Add(myBranch);
            AddToNonResolvePopUp?.Invoke(myBranch);
         // }
        PopUpStartProcess();
    }

    public void PauseMenu()
    {
        if (/*_uIHub.*/GameIsPaused)
        {
            //_uIHub.GameIsPaused = false;
            PopUpStartProcess();
        }
        else
        {
            RestoreLastPosition(ClearedScreenData.lastHighlighted);
            //_uIHub.GameIsPaused = true;
        }
    }

    /// <summary> Directly Starts up the popup. Stores data, clears screen, deactivates raycasts etc </summary>     
    private void PopUpStartProcess()
    {
        StoreClearScreenData();
        
        //if (!_uIHub.InMenu)

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
        //ClearedScreenData._wasInMenu = _uIHub.InMenu;
        //ClearedScreenData._wasInMenu = _uIHub.inMenuBeforePopUp;
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData.lastSelected = _uIHub.LastSelected;
        ClearedScreenData.lastHighlighted = _uIHub.ActiveBranch.LastHighlighted;
    }

    private void IfBranchIsFullscreen(UIBranch branch)
    {
        if (myBranch.ScreenType != ScreenType.FullScreen) return;
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
        //if (myBranch.IsNonResolvePopUp && _uIHub.ActivePopUpsResolve.Count > 0)
        if (myBranch.IsNonResolvePopUp && _noActiveNonResolvePopUps)
        {
            myBranch.DontSetAsActive = true;
        }
        myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
        //if(myBranch.IsPause()) myBranch.LastSelected.SetAsSelected();    
        myBranch.MoveToThisBranch();
    }
    
    private IEnumerator TimedPopUpProcess()
    {
        if (!_running)
        {
            myBranch.DontSetAsActive = true;
            myBranch.MoveToThisBranch();
            _running = true;
        }
        yield return new WaitForSeconds(myBranch.Timer);
        _running = false;
        myBranch.StartOutTween();
    }

    public void RestoreLastPosition(UINode lastHomeGroupNode = null)
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
        if (myBranch.ScreenType == ScreenType.FullScreen) RestoreScreen();
        
        if (lastHomeGroupNode.MyBranch.MyParentBranch)
        {
            //_uIHub.SetLastSelected(lastHomeGroupNode.MyBranch.MyParentBranch.LastSelected);                            
            lastHomeGroupNode.MyBranch.MyParentBranch.LastSelected.SetAsSelected();                            
        }
        else
        {
            //_uIHub.SetLastSelected(ClearedScreenData.lastSelected);
            ClearedScreenData.lastSelected.SetAsSelected();
        }

        //TODO Check there is a highlight action
        //_uIHub.SetLastHighlighted(lastHomeGroupNode);

        if (_uIHub.NoActivePopUps && !InMenuBeforePopUp)
        {
            _uIHub.GameToMenuSwitching();
            InMenuBeforePopUp = true;
        }

        lastHomeGroupNode.MyBranch.TweenOnChange = false;
        lastHomeGroupNode.MyBranch.MoveToThisBranch();
        /*if (myBranch.AllowKeys && _uIHub.InMenu)*/ //lastHomeGroupNode.SetNodeAsActive();
    }

    private void RestoreScreen()
    {
        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            if (/*_uIHub.*/GameIsPaused) continue;
            //if (_uIHub.ActivePopUpsResolve.Count == 0)
            if (_noActiveResolvePopUps)
            {
                branch.MyCanvasGroup.blocksRaycasts = true;
            }
            branch.MyCanvas.enabled = true;
        }

        _uIHub.ActiveResolvePopUps();
        _uIHub.ActivateNonResolvePopUps();
    }
    
    public void ManagePopUpResolve()
    {
        //if (_uIHub.ActivePopUpsResolve.Count > 0 && !myBranch.IsResolvePopUp)
        if (!_noActiveResolvePopUps || myBranch.IsResolvePopUp)
        {
            myBranch.MyCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            myBranch.MyCanvasGroup.blocksRaycasts = false;
        }
    }
}

