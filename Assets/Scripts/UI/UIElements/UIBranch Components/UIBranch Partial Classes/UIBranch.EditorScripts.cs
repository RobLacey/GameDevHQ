
/// <summary>
/// This partial Class looks after the editor script for UIBranch
/// </summary>
public partial class UIBranch
{
    private bool IsEmpty(UINode uINode)
    {
        return uINode != null;
    }

    public bool IsStandard()
    {
        return _branchType == BranchType.Standard;
    }

    public bool IsHome()
    {
        if (_branchType == BranchType.HomeScreen)
        {
            _escapeKeyFunction = EscapeKey.None;
            return true;
        }

        return false;
    }

    private bool TurnOffPopUps => _screenType == ScreenType.FullScreen && !IsOptionalPopUp;
}