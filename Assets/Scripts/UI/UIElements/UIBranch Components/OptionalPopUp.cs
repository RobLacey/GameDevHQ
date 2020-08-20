using System;

public class OptionalPopUp : BranchBase
{
    public OptionalPopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        OnStartPopUp = StartPopUp;
        ActivateNextPopUp = EndOfTweenActions;
    }
    
    private readonly UIBranch[] _allBranches;

    public static event Action<UIBranch> AddOptionalPopUp;

    private void StartPopUp()
    {
        if (_gameIsPaused || !_onHomeScreen) return; //TODO add to buffer goes here for when paused. trigger from SaveOnHome?

        if (!_myBranch.CanvasIsEnabled)
        {
            _myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
            MoveToBranchFromPopUp();
        }
    }
    private void MoveToBranchFromPopUp() 
    {
        if (!_noResolvePopUps)
        {
            _myBranch._setAsActive = false;
        }
        _myBranch.MoveToThisBranch();
    }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        ActivateBranch();
        if (!_noResolvePopUps) _myBranch._myCanvasGroup.blocksRaycasts = false;
        
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.No);
        AddOptionalPopUp?.Invoke(_myBranch);
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
