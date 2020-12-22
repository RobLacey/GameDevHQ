using System;
using System.Collections.Generic;
using UnityEngine;

public interface IOptionalPopUpBranch : IBranchBase { } 

public class OptionalPopUpPopUp : BranchBase, IStartPopUp, IRemoveOptionalPopUp, 
                                  IAddOptionalPopUp, IOptionalPopUpBranch, IAdjustCanvasOrder
{
    public OptionalPopUpPopUp(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
        _myBranch.OnStartPopUp += StartPopUp;
        SetUpCanvasOrder(branch);
    }
    
    public void SetUpCanvasOrder(ICanvasOrder branch)
    {
        EVent.Do.Return<IAdjustCanvasOrder>(this);
        branch.ManualCanvasOrder = CanvasOrderOffset;
        branch.CanvasOrder = OrderInCanvas.Manual;
        optionalPopUps = new List<Canvas>();
    }
    
    //Variables
    private readonly IBranch[] _allBranches;
    private bool _restoreOnHome;
    private static List<Canvas> optionalPopUps;


    //Properties
    public IBranch ThisPopUp => _myBranch;
    public int CanvasOrderOffset { get; set; }
    public BranchType BranchType { get; } = BranchType.OptionalPopUp;

    //Events
    private Action<IAddOptionalPopUp> AddOptionalPopUp { get; set; }
    private Action<IRemoveOptionalPopUp> RemoveOptionalPopUp { get; set; }

    
    //Main
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<ILastRemovedPopUp>(AdjustCanvasOrderRemoved);
    }

    public override void FetchEvents()
    {
        base.FetchEvents();
        AddOptionalPopUp = EVent.Do.Fetch<IAddOptionalPopUp>();
        RemoveOptionalPopUp = EVent.Do.Fetch<IRemoveOptionalPopUp>();
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

    public void StartPopUp() //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
    {
        if (_gameIsPaused || !OnHomeScreen || !_canStart || _myBranch.CanvasIsEnabled || _activeResolvePopUps) return; 
        
        IfActiveResolvePopUps();        
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        if(!_myBranch.CanvasIsEnabled && !_restoreOnHome)
        {
            AddOptionalPopUp?.Invoke(this);
            AdjustCanvasOrderAdded();
        }
        
        IfActiveResolvePopUps();
        SetCanvas(ActiveCanvas.Yes);
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.No);
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

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_activeResolvePopUps) return;
        base.SetBlockRaycast(active);
    }

    private void RemoveOrStorePopUp()
    {
        if (_myBranch.CanStoreAndRestoreOptionalPoUp)
        {
            _restoreOnHome = true;
        }
        else
        {
            RemoveOptionalPopUp?.Invoke(this);
        }
    }
    
    public void AdjustCanvasOrderAdded()
    {
        optionalPopUps.Add(_myBranch.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(optionalPopUps, CanvasOrderOffset);
    }

    public void AdjustCanvasOrderRemoved(ILastRemovedPopUp args)
    {
        optionalPopUps.Remove(args.LastOptionalPopUp.MyCanvas);
        CanvasOrderCalculator.ProcessActiveCanvasses(optionalPopUps, CanvasOrderOffset);
    }
}
