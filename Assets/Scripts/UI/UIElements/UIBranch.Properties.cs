using UnityEngine;

/// <summary>
/// This partial class holds all the classes properties as UIBranch is an important link between higher and lower parts of the system
/// </summary>
public partial class UIBranch
{
    public bool IsAPopUpBranch()
    {
        return _branchType == BranchType.PopUp_NonResolve
               || _branchType == BranchType.PopUp_Resolve
               || _branchType == BranchType.PopUp_Timed;
    }

    public bool IsPause()
    {
        return _branchType == BranchType.PauseMenu;
    }

    public UINode DefaultStartPosition
    {
        get => _userDefinedStartPosition;
        private set => _userDefinedStartPosition = value;
    }

    public Canvas MyCanvas { get; private set; }
    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; private set; }
    public UIBranch MyParentBranch { get; private set; }
    public bool DontSetAsActive { get; set; }
    public UINode[] ThisGroupsUiNodes { get; private set; }
    public CanvasGroup MyCanvasGroup { get; private set; }
    public EscapeKey EscapeKeySetting => _escapeKeyFunction;
    public BranchType MyBranchType => _branchType;
    public WhenToMove WhenToMove => _moveType;
    public bool StayOn => _stayOn == IsActive.Yes;
    public bool FromHotKey { get; set; }
    public bool IsResolvePopUp => _branchType == BranchType.PopUp_Resolve;
    public bool IsNonResolvePopUp => _branchType == BranchType.PopUp_NonResolve;
    public bool IsTimedPopUp => _branchType == BranchType.PopUp_Timed;
    public ScreenType ScreenType => _screenType;
    public UIPopUp PopUpClass { get; private set; }
    public UIPopUp PauseMenuClass { get; private set; }
    public int GroupListCount => _groupsList.Count;
    public float Timer => _timer;
    private UIHomeGroup HomeGroup { get; set; }
    public bool AllowKeys { get; set; }
    public bool TweenOnChange { get; set; } = true;
    private bool TweenOnHome => _tweenOnHome == IsActive.Yes;

}