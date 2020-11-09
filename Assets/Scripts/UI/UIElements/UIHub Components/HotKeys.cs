using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class HotKeys : IServiceUser, IEventUser, IHotKeyPressed
{
    [SerializeField] 
    private HotKey _hotKeyInput;
    [SerializeField] 
    private UIBranch _myBranch;
    
    //Variables
    private bool _hasParentNode;
    private INode _parentNode;
    private UIBranch _activeBranch;
    private InputScheme _inputScheme;
    private IHistoryTrack _uiHistoryTrack;
    
    //Events
    private static CustomEvent<IHotKeyPressed> HotKeyPressed { get; } = new CustomEvent<IHotKeyPressed>();
    
    //Properties
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    public UINode ParentNode => _parentNode.ReturnNode;
    public UIBranch MyBranch => _myBranch;
    
    //Main
    public void OnAwake(InputScheme inputScheme)
    {
        _inputScheme = inputScheme;
        IsAllowedType();
        SubscribeToService();
        ObserveEvents();
    }
    
    public void ObserveEvents() => EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);

    public void RemoveFromEvents() => EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);

    public void SubscribeToService() => _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);

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
        if (_myBranch.CanvasIsEnabled) return false;
        HotKeyActivation();
        return true;
    }

    private void HotKeyActivation()
    {    
        if(!_hasParentNode)
        {
            GetParentNode();
        }

        if (_activeBranch.IsHomeScreenBranch())
        {
            StartThisHotKeyBranch();
        }
        else
        {
            _activeBranch.StartBranchExitProcess(OutTweenType.Cancel,StartThisHotKeyBranch);
        }
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
        _uiHistoryTrack.SetFromHotkey(_myBranch);
        HotKeyPressed?.RaiseEvent(this);
    }

    private void SetHotKeyAsSelectedActions()
    { 
        _parentNode.ThisNodeIsSelected();
        _parentNode.ThisNodeIsHighLighted();
        if(_myBranch.ScreenType != ScreenType.FullScreen)
        {
            _parentNode.SetNodeAsSelected_NoEffects();
        }    
    }
}
