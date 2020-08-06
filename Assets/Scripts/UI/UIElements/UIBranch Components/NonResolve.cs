using System;

public class NonResolve : UIPopUp
{
    public NonResolve(UIBranch branch, UIBranch[] branchList/*, IGameToMenuSwitching gameToMenu*/)
    {
        //_gameToMenuSwitching = gameToMenu;
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }
    
    public static event Action<UIBranch> AddToNonResolvePopUp;

    protected override void SetUpPopUp()
    {
        base.SetUpPopUp();
        StartNonResolvePopUp();
    }
    
    private void StartNonResolvePopUp()
    {
        AddToNonResolvePopUp?.Invoke(_myBranch);
        PopUpStartProcess();
    }
}
