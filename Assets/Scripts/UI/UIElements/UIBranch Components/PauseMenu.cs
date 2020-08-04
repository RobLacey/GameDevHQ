/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>
public class PauseMenu : IPauseMenu, INodeData, IMono
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList)
    {
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }

    private readonly UIBranch _myBranch;
    private readonly UIBranch[] _allBranches;
    private bool _noActiveResolvePopUps = true;

    private ScreenData ClearedScreenData { get; } = new ScreenData();
    private void SetResolveCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; private set; }
    public void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    public void SaveSelected(UINode newNode) => LastSelected = newNode;


    public void OnEnable()
    {
        UIHub.GamePaused += StartPauseMenu;
        PopUpController.NoResolvePopUps += SetResolveCount;
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
    }
    public void OnDisable( )
    {
        UIHub.GamePaused -= StartPauseMenu;
        PopUpController.NoResolvePopUps -= SetResolveCount;
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
    }

    public void StartPauseMenu(bool isGamePaused)
    {
        if (isGamePaused)
            PopUpStartProcess();
        else
            RestoreLastPosition();
    }
    
    private void PopUpStartProcess()
    {
        StoreClearScreenData();
        
        foreach (var branch in _allBranches)
        {
            if (branch == _myBranch) continue;
            if (!branch.CheckAndDisableBranchCanvas(_myBranch.ScreenType)) continue;
            ClearedScreenData._clearedBranches.Add(branch);
        }
        ActivatePopUp();
    }
    
    private void ActivatePopUp()
    {
        _myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
        _myBranch.MoveToThisBranch();
    }

    private void StoreClearScreenData()
    {
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData._lastSelected = LastSelected;
        ClearedScreenData._lastHighlighted = LastHighlighted;
    }

    private void RestoreLastPosition()
    {
        if (_myBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _myBranch.StartOutTween(EndOfTweenActions);
        }
        else
        {
            _myBranch.StartOutTween();
            EndOfTweenActions();
        }
    }
    
    private void EndOfTweenActions()
    {
        var nextNode = ClearedScreenData._lastHighlighted;
        RestoreScreen();
        ClearedScreenData._lastSelected.ThisNodeIsSelected();
        nextNode.MyBranch.MoveToBranchWithoutTween();
    }

    private void RestoreScreen()
    {
        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            if (_noActiveResolvePopUps)
                branch.MyCanvasGroup.blocksRaycasts = true;

            if (branch.IsResolvePopUp)
                branch.MyCanvasGroup.blocksRaycasts = true;
            
            branch.MyCanvas.enabled = true;
        }
    }
}
