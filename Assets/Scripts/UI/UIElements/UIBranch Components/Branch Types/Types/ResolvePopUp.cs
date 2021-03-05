using System;
using System.Collections.Generic;
using UnityEngine;

public interface IResolvePopUpBranch : IBranchBase { }

public class ResolvePopUp : BranchBase, IAddResolvePopUp, IResolvePopUpBranch, IAdjustCanvasOrder
{
    public ResolvePopUp(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
        SetUpCanvasOrder(branch);
    }    

    //Variables
    private readonly IBranch[] _allBranches;
    private static List<Canvas> resolvePopUps;
    
    //Properties
    public IBranch ThisPopUp => _myBranch;
    public int CanvasOrderOffset { get; set; }
    public BranchType BranchType { get; } = BranchType.ResolvePopUp;

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

        if (!_myBranch.CanvasIsEnabled)
        {
            AdjustCanvasOrderAdded();
        }

        return true;
    }
    
    public override void SetUpBranch(IBranch newParentController = null)
    {
        if(_myBranch.CanvasIsEnabled) return;
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.Yes);
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        AddResolvePopUp?.Invoke(this);
    }

    public override void EndOfBranchStart()
    {
        base.EndOfBranchStart();
        CanvasOrderCalculator.ResetCanvasOrder(_myBranch, _myCanvas);
    }

    private void SetUpCanvasOrder(ICanvasOrder branch)
    {
        EVent.Do.Return<IAdjustCanvasOrder>(this);
        branch.ManualCanvasOrder = CanvasOrderOffset;
        branch.CanvasOrder = OrderInCanvas.Manual;
        resolvePopUps = new List<Canvas>();
    }

    private void AdjustCanvasOrderAdded()
    {
        resolvePopUps.Add(_myBranch.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(resolvePopUps, CanvasOrderOffset);
    }

    private void AdjustCanvasOrderRemoved(ILastRemovedPopUp args)
    {
        resolvePopUps.Remove(args.LastOptionalPopUp.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(resolvePopUps, CanvasOrderOffset);
    }
}
