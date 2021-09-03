using System;
using System.Collections.Generic;
using UnityEngine;

public interface IResolvePopUpBranch : IBranchBase { }

public class ResolvePopUp : BranchBase, IAddResolvePopUp, IResolvePopUpBranch
{
    public ResolvePopUp(IBranch branch) : base(branch) { }   

    //Variables
    private static readonly List<Canvas> resolvePopUps = new List<Canvas>();
    
    //Properties
    public IBranch ThisPopUp => _myBranch;
    private IBranch[] AllBranches => _myDataHub.AllBranches;

    //Events
    private Action<IAddResolvePopUp> AddResolvePopUp { get; set; }

    //Main
    public override void FetchEvents()
    {
        base.FetchEvents();
        AddResolvePopUp = PopUpEvents.Do.Fetch<IAddResolvePopUp>();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        AdjustCanvasOrderRemoved();
    }

    public override bool CanStartBranch()
    {
        //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
        if(!CanStart || GameIsPaused) return false;  
        if (!OnHomeScreen && _myBranch.ReturnOnlyAllowOnHomeScreen == IsActive.Yes) return false;
        return true;
    }
    
    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if (!_myBranch.CanvasIsEnabled)
        {
            AdjustCanvasOrderAdded();
        }

        AddResolvePopUp?.Invoke(this);
        _screenData.StoreClearScreenData(AllBranches, _myBranch, BlockRaycast.Yes);
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(!CanStart) return;
        
        if (CanAllowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
        }
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        ActivateStoredPosition();
    }

    private void AdjustCanvasOrderAdded()
    {
        resolvePopUps.Add(_myBranch.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(resolvePopUps);
    }
    
    private void AdjustCanvasOrderRemoved()
    {
        resolvePopUps.Remove(_myCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(resolvePopUps);
    }
}
