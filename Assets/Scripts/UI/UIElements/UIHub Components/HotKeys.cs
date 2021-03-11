using System;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class HotKeys : IEventUser, IHotKeyPressed, IEventDispatcher, IStartBranch
{
    [SerializeField] 
    private HotKey _hotKeyInput  = default;
    [SerializeField] [AllowNesting] [OnValueChanged("IsAllowedType")]
    private UIBranch _myBranch  = default;
    
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
    public UIBranch TargetBranch => _myBranch;

    
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
        string message = $"Can't have \"{_myBranch.name}\" as a Hot Key. " +
                         $"{Environment.NewLine}" +
                         $"{Environment.NewLine}" +
                         "Only Standard or HomeScreen branch Types allowed";
        
        const string title = "Invalid Hot Key Type";

        if (_myBranch.ReturnBranchType == BranchType.Standard 
            || _myBranch.ReturnBranchType == BranchType.HomeScreen) return;
        
        UIEditorDialogue.WarningDialogue(title, message, "OK");
        _myBranch = null;
    }

    public bool CheckHotKeys()
    {
        if (!_inputScheme.HotKeyChecker(_hotKeyInput)) return false;
        if (_myBranch.CanvasIsEnabled) return false;
        HotKeyActivation();
        return true;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void HotKeyActivation()
    {    
        if(ReferenceEquals(_activeBranch, _myBranch)) return;

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
            GetImmediateParentNode();
        }
        else
        {
            FindHomeScreenParentNode(_myBranch);
        }
        _hasParentNode = true;
    }

    private void GetImmediateParentNode()
    {
        var branchesNodes = _myBranch.MyParentBranch.ThisGroupsUiNodes;
        _parentNode = branchesNodes.First(node => ReferenceEquals(_myBranch, node.HasChildBranch));
    }
    
    private void FindHomeScreenParentNode(IBranch branch)
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
