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
    
    public UINode DefaultStartPosition
    {
        get => _userDefinedStartPosition;
        private set => _userDefinedStartPosition = value;
    }

    public bool CanvasIsEnabled => _myCanvas.enabled;
    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; set; }
    public UIBranch MyParentBranch { get; internal set; }
    public UINode[] ThisGroupsUiNodes { get; private set; }
    public EscapeKey EscapeKeySetting => _escapeKeyFunction;
    public BranchType MyBranchType => _branchType;
    public WhenToMove WhenToMove => _moveType;
    public ScreenType ScreenType => _screenType;
    public float Timer => _timer;

}