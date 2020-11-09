using System;
using System.Collections.Generic;
using System.Linq;

public interface ITestList
{
    UINode AddNode { get; }
}

public class HistoryTracker : IHistoryTrack, IEventUser, IReturnToHome, ITestList, ICancelHoverOverButton
{
    public HistoryTracker(EscapeKey globalCancelAction)
    {
        ServiceLocator.AddService<ICancel>(new UICancel(globalCancelAction));
        OnEnable();
    }

    //Variables
    private readonly List<UINode> _history = new List<UINode>();
    private UINode _lastSelected;
    private bool _canStart, _isPaused;
    private bool _onHomScreen = true, _noPopUps = true;
    private UIBranch _activeBranch;
    private IPopUpController _popUpController;

    public UINode AddNode { get; private set; }
    public bool ActivateBranchOnReturnHome { get; private set; }

    private void SaveOnHomScreen(IOnHomeScreen args) => _onHomScreen = args.OnHomeScreen;
    private void SaveIsGamePaused(IGameIsPaused args) => _isPaused = args.GameIsPaused;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void NoPopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;
    public bool NoHistory => _history.Count == 0;

    //Events
    private static CustomEvent<IReturnToHome> ReturnedToHome { get; } = new CustomEvent<IReturnToHome>();
    private static CustomEvent<ICancelHoverOverButton> CancelHoverToActivate { get; } 
        = new CustomEvent<ICancelHoverOverButton>();
    private static CustomEvent<ITestList> AddANode { get; } = new CustomEvent<ITestList>();
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IOnStart>(SetCanStart, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomScreen, this);
        EventLocator.Subscribe<IGameIsPaused>(SaveIsGamePaused, this);
        EventLocator.Subscribe<INoPopUps>(NoPopUps, this);
        EventLocator.Subscribe<ICancelPopUp>(CancelPopUpFromButton, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IOnStart>(SetCanStart);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomScreen);
        EventLocator.Unsubscribe<IGameIsPaused>(SaveIsGamePaused);
        EventLocator.Unsubscribe<INoPopUps>(NoPopUps);
        EventLocator.Unsubscribe<ICancelPopUp>(CancelPopUpFromButton);
    }

    public void OnEnable()
    {
        _popUpController = new PopUpController();
        ObserveEvents();
    }

    public void OnDisable()
    {
        ServiceLocator.RemoveService<IPopUpController>();
        ServiceLocator.RemoveService<ICancel>();
        RemoveFromEvents();
    }
    
    private void SetCanStart(IOnStart onStart) => _canStart = true;

    //Main
    public void SetSelected(INode node)
    {
        if(!_canStart) return;
        if(node.ReturnNode.DontStoreTheseNodeTypesInHistory) return;
        SetNullLastSelected(node);
        
        if (_history.Contains(node.ReturnNode))
        {
            CloseAllChildNodesAfterPoint(node);
            SetLastSelectedWhenNoHistory(node);
        }
        else
        {
            if(!_isPaused)
                SelectedNodeInDifferentBranch(node);
            
            AddNode = node.ReturnNode;
            AddANode?.RaiseEvent(this);
            _history.Add(node.ReturnNode);
            _lastSelected = node.ReturnNode;
        }
    }

    private void SelectedNodeInDifferentBranch(INode node)
    {
        if (_lastSelected.HasChildBranch != node.ReturnNode.MyBranch)
            ReverseAndClearHistory();
    }

    private void SetNullLastSelected(INode node)
    {
        if (_lastSelected is null)
            _lastSelected = node.ReturnNode;
    }

    public void CloseAllChildNodesAfterPoint(INode newNode)
    {
        if (!_history.Contains(newNode.ReturnNode)) return;
        for (int i = _history.Count -1; i > 0; i--)
        {
            if (_history[i] == newNode.ReturnNode) break;
            CloseThisLevel(_history[i]);
        }
        CloseThisLevel(newNode);

    }

    private void CloseThisLevel(INode node)
    {
        node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel, EndOfTweenActions);
        
        AddNode = node.ReturnNode;
        AddANode?.RaiseEvent(this);

        _history.Remove(node.ReturnNode);

        void EndOfTweenActions()
        {
            node.HasChildBranch.LastSelected.DeactivateNode();
            node.MyBranch.MoveToBranchWithoutTween();
        }
    }
    
    private void SetLastSelectedWhenNoHistory(INode node) 
        => _lastSelected = _history.Count > 0 ? _history.Last() : node.ReturnNode;
    
    public void BackOneLevel()
    {
        var lastNode = _history.Last();
        if(IfLastSelectedIsOnHomeScreen(lastNode)) return;
        
        DoMoveBackOneLevel(lastNode);
        if (_history.Count > 0)
            _lastSelected = _history.Last();
    }

    private void DoMoveBackOneLevel(INode lastNode)
    {
        lastNode.DeactivateNode();
        
        if (lastNode.MyBranch.CanvasIsEnabled)
        {
            _activeBranch.StartBranchExitProcess(OutTweenType.Cancel, lastNode.MyBranch.MoveToBranchWithoutTween);
        }
        else
        {
            _activeBranch.StartBranchExitProcess(OutTweenType.Cancel, Temp );
        }

        void Temp()
        {
            lastNode.MyBranch.MoveToThisBranch();
        }
    }

    private bool IfLastSelectedIsOnHomeScreen(UINode lastNode)
    {
        if (lastNode.MyBranch.IsHomeScreenBranch() && !_onHomScreen)
        {
            BackToHome();
            return true;
        }
        else
        {
            AddNode = lastNode;
            AddANode?.RaiseEvent(this);

            _history.Remove(lastNode);
            return false;
        }
    }

    public void BackToHome()
    {
        _activeBranch.StartBranchExitProcess(OutTweenType.Cancel, BackHomeCallBack);
        
        void BackHomeCallBack()
        {
            if (_history.Count <= 0) return;
            ReverseAndClearHistory();
            ActivateBranchOnReturnHome = true;
            ReturnedToHome.RaiseEvent(this);
        }
    }
    
    public void DoCancelHoverToActivate() => CancelHoverToActivate?.RaiseEvent(this);

    public void ReverseAndClearHistory() => HistoryProcessed(stopWhenInternalBranchReacted: true);

    private void HistoryProcessed(bool stopWhenInternalBranchReacted)
    {
        if (_history.Count == 0) return;
        _history.Reverse();
        
        foreach (var uiNode in _history)
        {
            uiNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
            uiNode.DeactivateNode();
            if (IfNodeIsInternalBranch(uiNode, stopWhenInternalBranchReacted)) return;
        }
        AddNode = null;
        AddANode?.RaiseEvent(this);
        
        _history.Clear();
    }
    
    private bool IfNodeIsInternalBranch(UINode uiNode, bool saveInternals)
    {
        if (!uiNode.HasChildBranch.IsInternalBranch() || !saveInternals) return false;

        AddNode = uiNode;
        AddANode?.RaiseEvent(this);

        _history.Remove(uiNode);
        _history.Reverse();
        return true;
    }

    public void SetFromHotkey(UIBranch branch)
    {
        BackToHomeScreenFromHotKey(branch);
        HistoryProcessed(stopWhenInternalBranchReacted: false);
        
        AddNode = FindHomeScreenRoot(branch);
        AddANode?.RaiseEvent(this);
        
        _history.Add(FindHomeScreenRoot(branch));
    }

    private void BackToHomeScreenFromHotKey(UIBranch branch)
    {
        if (branch.ScreenType == ScreenType.FullScreen || _onHomScreen) return;
        ActivateBranchOnReturnHome = false;
        ReturnedToHome.RaiseEvent(this);
    }

    private UINode FindHomeScreenRoot(UIBranch branch)
    {
        while (!branch.IsHomeScreenBranch())
        {
            branch = branch.MyParentBranch;
        }
        _lastSelected = branch.LastSelected.ReturnNode;
        return _lastSelected;
    }

    public void MoveToLastBranchInHistory()
    {
        if (!_noPopUps && !_isPaused)
        {
            _popUpController.NextPopUp().MoveToBranchWithoutTween();
            return;
        }
        if (_history.Count == 0 || _isPaused)
        {
            IfPausedOrNoHistory();
        }
        else
        {
            _activeBranch.MoveToBranchWithoutTween();
        }
    }

    private void IfPausedOrNoHistory()
    {
        if (_isPaused)
        {
            _activeBranch.MoveToBranchWithoutTween();
        }
        else
        {
            ActivateBranchOnReturnHome = true;
            ReturnedToHome?.RaiseEvent(this);
        }
    }

    public void CancelMove(Action endOfCancelAction)
    {
        if (!_noPopUps && !_isPaused)
        {
            HandlePopUps(_popUpController.NextPopUp());
        }
        else
        {
            endOfCancelAction?.Invoke();
        }
    }

    private void CancelPopUpFromButton(ICancelPopUp popUpToCancel) => HandlePopUps(popUpToCancel.MyBranch);

    private void HandlePopUps(UIBranch popUpToCancel)
    {
        if(popUpToCancel.EscapeKeySetting == EscapeKey.None) return;
        _popUpController.RemoveNextPopUp(popUpToCancel);
        popUpToCancel.StartBranchExitProcess(OutTweenType.Cancel, EndOfTweenCallback);
    }

    private void EndOfTweenCallback()
    {
        if (_noPopUps)
        {
            MoveToLastBranchInHistory();
        }
        else
        {
            _popUpController.NextPopUp().MoveToBranchWithoutTween();
        }
    }
}
