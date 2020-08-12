using UnityEngine;

/// <summary>
/// This partial class holds all the classes properties as UIBranch is an important link between higher and lower parts of the system
/// </summary>
public partial class UIBranch
{
    public bool IsAPopUpBranch()
    {
        return _branchType == BranchType.OptionalPopUp
               || _branchType == BranchType.ResolvePopUp
               || _branchType == BranchType.TimedPopUp;
    }

    public bool IsPauseMenuBranch()
    {
        return _branchType == BranchType.PauseMenu;
    }

    public UINode DefaultStartPosition
    {
        get => _userDefinedStartPosition;
        private set => _userDefinedStartPosition = value;
    }

    public bool CanvasIsEnabled => _myCanvas.enabled;
    public bool TurnOffPopUPs => _turnOffPopUps == IsActive.Yes;
    public bool IgnoreThisBranch { get; private set; }

    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; private set; }
    public UIBranch MyParentBranch { get; private set; }
    public UINode[] ThisGroupsUiNodes { get; private set; }
    public EscapeKey EscapeKeySetting => _escapeKeyFunction;
    public BranchType MyBranchType => _branchType;
    public WhenToMove WhenToMove => _moveType;
    public bool IsResolvePopUp => _branchType == BranchType.ResolvePopUp;
    public bool IsOptionalPopUp => _branchType == BranchType.OptionalPopUp;
    public ScreenType ScreenType => _screenType;
    public float Timer => _timer;
    private bool IsTimedPopUp => _branchType == BranchType.TimedPopUp;

}