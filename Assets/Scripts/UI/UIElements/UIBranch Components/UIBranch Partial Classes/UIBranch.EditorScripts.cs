
/// <summary>
/// This partial Class looks after the editor script for UIBranch
/// </summary>
public partial class UIBranch
{
    public bool IsHome() => _branchType == BranchType.HomeScreen;
    public bool IsOptional() => _branchType == BranchType.OptionalPopUp;
    public bool IsPauseMenuBranch() => _branchType == BranchType.PauseMenu;
    private bool IsTimedPopUp => _branchType == BranchType.TimedPopUp;

}