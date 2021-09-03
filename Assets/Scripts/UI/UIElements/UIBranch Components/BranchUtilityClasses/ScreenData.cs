using System.Collections.Generic;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface IScreenData : IMonoEnable,IMonoDisable
{
    bool WasOnHomeScreen { get; }
    void RestoreScreen();
    void StoreClearScreenData(IBranch[] allBranches, IBranch thisBranch, BlockRaycast blockRaycast);
}

public class ScreenData : IScreenData, IServiceUser, IEZEventUser
{
    public ScreenData(IBranchParams branch)
    {
        if (branch.MyScreenType == ScreenType.FullScreen)
            _isFullscreen = true;
    }

    //Variables
    private readonly List<IBranch> _clearedBranches = new List<IBranch>();
    private readonly bool _isFullscreen;
    private IDataHub _myDataHub;

    //Propertues
    public bool WasOnHomeScreen { get; private set; } = true;

    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }
    
    public void ObserveEvents() => BranchEvent.Do.Subscribe<ICloseBranch>(RemoveBranch);
    public void UnObserveEvents() => BranchEvent.Do.Unsubscribe<ICloseBranch>(RemoveBranch);

    private void RemoveBranch(ICloseBranch args)
    {
        if (_clearedBranches.Contains(args.TargetBranch))
            _clearedBranches.Remove(args.TargetBranch);
    }

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void OnDisable()
    {
        UnObserveEvents();
        _clearedBranches.Clear();
    }

    //Main
    public void StoreClearScreenData(IBranch[] allBranches, IBranch thisBranch, BlockRaycast blockRaycast)
    {
        WasOnHomeScreen = _myDataHub.OnHomeScreen;
        StoreActiveBranches(allBranches, thisBranch, blockRaycast == BlockRaycast.Yes);
    }

    private void StoreActiveBranches(IBranch[] allBranches, IBranch thisBranch, bool blockRaycast)
    {
        foreach (var branchToClear in allBranches)
        {
            if(branchToClear == thisBranch) continue;
            
            if (branchToClear.CanvasIsEnabled)
                _clearedBranches.Add(branchToClear);
            
            if (blockRaycast) 
                branchToClear.SetBlockRaycast(BlockRaycast.No);
        }
    }

    public void RestoreScreen()
    {
        if(_clearedBranches.Count == 0) return;
        
        foreach (var branch in _clearedBranches)
        {
            if(_isFullscreen)
                branch.SetCanvas(ActiveCanvas.Yes);
            branch.SetBlockRaycast(BlockRaycast.Yes);
        }
        _clearedBranches.Clear();
    }
}
