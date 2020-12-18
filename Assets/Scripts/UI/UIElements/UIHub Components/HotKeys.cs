using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class HotKeys : IEventUser, IHotKeyPressed, IEventDispatcher
{
    [SerializeField] 
    private HotKey _hotKeyInput;
    [SerializeField] 
    private UIBranch _myBranch;
    
    //Variables
    private bool _hasParentNode;
    private INode _parentNode;
    private IBranch _activeBranch;
    private InputScheme _inputScheme;
    
    //Events
    private Action<IHotKeyPressed> HotKeyPressed { get; set; }

    //Properties
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    public INode ParentNode => _parentNode;
    public IBranch MyBranch => _myBranch;
    
    //Main
    public void OnAwake(InputScheme inputScheme)
    {
        _inputScheme = inputScheme;
        IsAllowedType();
    }

    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }

    public void FetchEvents() => HotKeyPressed = EVent.Do.Fetch<IHotKeyPressed>();

    public void ObserveEvents() => EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
    
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
        if(ReferenceEquals(_activeBranch, _myBranch)) return;
        
        CanvasOrderCalculator.SetCanvasOrder(_activeBranch, MyBranch);

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
            _activeBranch.StartBranchExitProcess(OutTweenType.Cancel, StartThisHotKeyBranch);
        }
    }

    private void GetParentNode()
    {
        if (_myBranch.ScreenType != ScreenType.FullScreen)
        {
            GetImmediateParent();
        }
        else
        {
            FindHomeScreenParent(_myBranch);
        }
        _hasParentNode = true;
    }

    private void GetImmediateParent()
    {
        var branchesNodes = _myBranch.MyParentBranch.ThisGroupsUiNodes;
        _parentNode = branchesNodes.First(node => _myBranch == node.HasChildBranch);
    }
    
    private void FindHomeScreenParent(IBranch branch)
    {
        while (!branch.IsHomeScreenBranch())
        {
            branch = branch.MyParentBranch;
        }
        _parentNode = branch.LastSelected;
    }
    
    private void StartThisHotKeyBranch()
    {
        HotKeyPressed?.Invoke(this);
        _parentNode.SetAsHotKeyParent();
        _myBranch.MoveToThisBranch();
    }
}
