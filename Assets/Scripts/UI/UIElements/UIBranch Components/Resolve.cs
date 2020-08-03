using System;

public class Resolve : UIPopUp
{
    public Resolve(UIBranch branch, UIBranch[] branchList, IGameToMenuSwitching gameToMenu)
    {
        _gameToMenuSwitching = gameToMenu;
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }
    
    public static event Action<UIBranch> AddToResolvePopUp;
    
    protected override void SetUpPopUp()
    {
        base.SetUpPopUp();
        StartResolvePopUp();
    }
    
    private void StartResolvePopUp()
    {
        AddToResolvePopUp?.Invoke(_myBranch);
        PopUpStartProcess();
    }

    protected override void RestoreScreen()
    {
        foreach (var branch in ScreenData._clearedBranches)
        {
            if (GameIsPaused) continue;
            if (_noActiveResolvePopUps)
            {
                branch.MyCanvasGroup.blocksRaycasts = true;
            }
            
            if (_myBranch.ScreenType == ScreenType.FullScreen)
            {
                branch.MyCanvas.enabled = true;
            }
        }
    }
}
