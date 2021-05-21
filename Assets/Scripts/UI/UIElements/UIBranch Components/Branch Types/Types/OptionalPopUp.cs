using System;
using System.Collections.Generic;
using UnityEngine;

public interface IOptionalPopUpBranch : IBranchBase { } 

public class OptionalPopUpPopUp : BranchBase, IRemoveOptionalPopUp, IAddOptionalPopUp, IOptionalPopUpBranch
{
    public OptionalPopUpPopUp(IBranch branch) : base(branch) { }

    //Variables
    private bool _restoreOnHome;
    private static readonly List<Canvas> optionalPopUps = new List<Canvas>();

    //Properties
    public IBranch ThisPopUp => _myBranch;
    
    //Events
    private Action<IAddOptionalPopUp> AddOptionalPopUp { get; set; }
    private Action<IRemoveOptionalPopUp> RemoveOptionalPopUp { get; set; }

    //Main
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        PopUpEvents.Do.Subscribe<ILastRemovedPopUp>(AdjustCanvasOrderRemoved);
    }

    protected override void UnObserveEvents()
    {
        base.UnObserveEvents();
        PopUpEvents.Do.Unsubscribe<ILastRemovedPopUp>(AdjustCanvasOrderRemoved);
    }

    public override void FetchEvents()
    {
        base.FetchEvents();
        AddOptionalPopUp = PopUpEvents.Do.Fetch<IAddOptionalPopUp>();
        RemoveOptionalPopUp = PopUpEvents.Do.Fetch<IRemoveOptionalPopUp>();
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        base.SaveIfOnHomeScreen(args);

        if (!_restoreOnHome || !OnHomeScreen) return;
        
        if (_myBranch.TweenOnHome == DoTween.Tween)
        {
            ActivateWithTween();
        }
        else
        {
            ActivateWithoutTween();
        }
    }

    private void ActivateWithTween()
    {
        _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
    }

    private void ActivateWithoutTween()
    {
        SetCanvas(ActiveCanvas.Yes);
        if (!_activeResolvePopUps)
            SetBlockRaycast(BlockRaycast.Yes);
    }

    public override bool CanStartBranch() //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
    {
        if (_gameIsPaused || !OnHomeScreen || !_canStart || _activeResolvePopUps) return false;
        IfActiveResolvePopUps();        
        return true;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        if(!_myBranch.CanvasIsEnabled && !_restoreOnHome)
        {
            AddOptionalPopUp?.Invoke(this);
            AdjustCanvasOrderAdded();
        }
        
        IfActiveResolvePopUps();
        SetCanvas(ActiveCanvas.Yes);
        _restoreOnHome = false;
    }

    private void IfActiveResolvePopUps()
    {
        if (!_activeResolvePopUps) return;
        _myBranch.DontSetBranchAsActive();
        SetBlockRaycast(BlockRaycast.No);
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
        if(!_myBranch.CanvasIsEnabled) return;
        base.ClearBranchForFullscreen(args);
        RemoveOrStorePopUp();
    }

    private void RemoveOrStorePopUp()
    {
        if (_myBranch.CanStoreAndRestoreOptionalPoUp) 
        {
            _restoreOnHome = true;
        }
        else
        {
            _canvasOrderCalculator.ResetCanvasOrder();
            RemoveOptionalPopUp?.Invoke(this);
        }
    }

    private void AdjustCanvasOrderAdded()
    {
        optionalPopUps.Add(_myBranch.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(optionalPopUps);
    }

    private void AdjustCanvasOrderRemoved(ILastRemovedPopUp args)
    {
        optionalPopUps.Remove(args.LastOptionalPopUp.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(optionalPopUps);
    }
}
