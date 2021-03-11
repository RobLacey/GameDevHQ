using System;
using System.Collections.Generic;
using UnityEngine;

public interface IResolvePopUpBranch : IBranchBase { }

public class ResolvePopUp : BranchBase, IAddResolvePopUp, IResolvePopUpBranch
{
    public ResolvePopUp(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
        resolvePopUps = new List<Canvas>();
    }    

    //Variables
    private readonly IBranch[] _allBranches;
    private static List<Canvas> resolvePopUps;
    
    //Properties
    public IBranch ThisPopUp => _myBranch;

    //Events
    private Action<IAddResolvePopUp> AddResolvePopUp { get; set; }

    //Main
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ILastRemovedPopUp>(AdjustCanvasOrderRemoved);
    }

    public override void FetchEvents()
    {
        base.FetchEvents();
        AddResolvePopUp = EVent.Do.Fetch<IAddResolvePopUp>();
    }

    public override bool CanStartBranch()
    {
        if(!_canStart || _gameIsPaused) return false;  //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
        if (!OnHomeScreen && _myBranch.ReturnOnlyAllowOnHomeScreen == IsActive.Yes) return false;

        if (!_myBranch.CanvasIsEnabled)
        {
            AdjustCanvasOrderAdded();
        }

        return true;
    }
    
    public override void SetUpBranch(IBranch newParentController = null)
    {
        if(_myBranch.CanvasIsEnabled) return;
        base.SetUpBranch(newParentController);
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.Yes);
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        AddResolvePopUp?.Invoke(this);
    }
    
    private void AdjustCanvasOrderAdded()
    {
        resolvePopUps.Add(_myBranch.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(resolvePopUps);
    }

    private void AdjustCanvasOrderRemoved(ILastRemovedPopUp args)
    {
        resolvePopUps.Remove(args.LastOptionalPopUp.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(resolvePopUps);
    }
}
