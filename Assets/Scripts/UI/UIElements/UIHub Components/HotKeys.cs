using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class HotKeys : IServiceUser, IEventUser
{
    [SerializeField] 
    private HotKey _hotKeyInput;
    [SerializeField] 
    private UIBranch _myBranch;
    
    //Variables
    private bool _hasParentNode;
    private bool _gameIsPaused;
    private bool _noActivePopUps = true;
    private INode _parentNode;
    private UIBranch _activeBranch;
    private InputScheme _inputScheme;
    private IHistoryTrack _uiHistoryTrack;
    
    //Properties
    private void SaveIsPaused(bool isPaused) => _gameIsPaused = isPaused;
    private void SaveActiveBranch(UIBranch branch) => _activeBranch = branch;
    private void SaveNoActivePopUps(bool noaActivePopUps) => _noActivePopUps = noaActivePopUps;
    
    public void OnAwake(InputScheme inputScheme)
    {
        _inputScheme = inputScheme;
        IsAllowedType();
        SubscribeToService();
        ObserveEvents();
    }
    
    public void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<IGameIsPaused, bool>(SaveIsPaused, this);
        EventLocator.SubscribeToEvent<IActiveBranch, UIBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<INoPopUps, bool>(SaveNoActivePopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(SaveIsPaused);
        EventLocator.UnsubscribeFromEvent<IActiveBranch, UIBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<INoPopUps, bool>(SaveNoActivePopUps);
    }
    
    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
       // return _uiHistoryTrack is null;
    }

    private void IsAllowedType()
    {
        if (_myBranch.IsAPopUpBranch())
            throw new Exception("Can't have a PopUp as a Hot Key");
        if (_myBranch.IsPauseMenuBranch())
            throw new Exception("Can't have Pause as a Hot Key");
        if (_myBranch.IsInternalBranch())
            throw new Exception("Can't have an Internal Branch as a Hot Key");
    }

    public bool CheckHotKeys()
    {
        if (!_inputScheme.HotKeyChecker(_hotKeyInput)) return false;
        if (_myBranch.CanvasIsEnabled || _gameIsPaused || !_noActivePopUps) return false;
        HotKeyActivation();
        return true;
    }

    private void HotKeyActivation()
    {    
        if(!_hasParentNode)
        {
            GetParentNode();
        }
        _activeBranch.StartBranchExitProcess(OutTweenType.Cancel,StartThisHotKeyBranch);
    }

    private void GetParentNode()
    {
        var branchesNodes = _myBranch.MyParentBranch.ThisGroupsUiNodes;
        _parentNode = branchesNodes.First(node => _myBranch == node.HasChildBranch);
        _hasParentNode = true;
    }
    
    private void StartThisHotKeyBranch()
    {
        SetHotKeyAsSelectedActions();
        _myBranch.MoveToThisBranch();
        _uiHistoryTrack.SetFromHotkey(_myBranch, _parentNode);
    }

    private void SetHotKeyAsSelectedActions()
    { 
        _parentNode.ThisNodeIsSelected();
        _parentNode.ThisNodeIsHighLighted();
        _parentNode.SetNodeAsSelected_NoEffects();
    }
}
