using System;
using UnityEngine;

public class ResolvePopUp : BranchBase
{
    public ResolvePopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        OnStartPopUp = StartPopUp;
        ActivateNextPopUp = EndOfTweenActions;
    }
    
    private readonly UIBranch[] _allBranches;

    public static event Action<UIBranch> AddResolvePopUp;

    private void StartPopUp()
    {
        if (_gameIsPaused) return; //TODO add to buffer goes here for when paused. trigger from SaveOnHome?

        if (!_myBranch.CanvasIsEnabled)
        {
            _myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
            _myBranch.MoveToThisBranch();
        }
    }
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        ActivateBranch();
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.Yes);
        AddResolvePopUp?.Invoke(_myBranch);
    }
    
    private void EndOfTweenActions(UIBranch lastActiveBranch)
    {
        _screenData.RestoreScreen();
        
        if (ReturnToGame())
        {
            lastActiveBranch.LastHighlighted.ThisNodeIsHighLighted();
        }
        else
        {
            lastActiveBranch.MoveToBranchWithoutTween();
        }
    }

    private bool ReturnToGame()
    {
        return !_screenData._wasInTheMenu && _noActivePopUps;
    }
}
