using System;
using System.Linq;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Serializable]
public partial class HotKeys : IEZEventUser, IHotKeyPressed, IEZEventDispatcher, IReturnHomeGroupIndex, IServiceUser,
                               IMonoEnable
{
    [SerializeField] 
    private string _name = SetName;
    [SerializeField] 
    private HotKey _hotKeyInput  = default;
    [SerializeField] 
    [AllowNesting] [OnValueChanged(IsAllowed)]
    private UIBranch _myBranch  = default;
    
    //Variables
    private bool _hasParentNode;
    private INode _parentNode;
    private IBranch _activeBranch;
    private InputScheme _inputScheme;
    private bool _makeParentActive;
    private const string SetName = "Set My Name";

    //Events
    private Action<IHotKeyPressed> HotKeyPressed { get; set; }
    private Action<IReturnHomeGroupIndex> ReturnHomeGroupIndex { get; set; }

    //Properties
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    public INode ParentNode => _parentNode;
    public IBranch MyBranch => _myBranch;
    public INode TargetNode { private get; set; }
    private INode[] ThisGroupsUiNodes => _myBranch.MyParentBranch.ThisGroupsUiNodes;



    //Main
    public void OnEnable()
    {
        IsAllowedType();
        FetchEvents();
        ObserveEvents();
        UseEZServiceLocator();
    }
    
    public void UseEZServiceLocator() => _inputScheme = EZService.Locator.Get<InputScheme>(this);

    public void FetchEvents()
    {
        HotKeyPressed = InputEvents.Do.Fetch<IHotKeyPressed>();
        ReturnHomeGroupIndex = HistoryEvents.Do.Fetch<IReturnHomeGroupIndex>();
    }

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<IActiveBranch>(SaveActiveBranch);

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
            FindHomeScreenParentNode();
        }
        _hasParentNode = true;
    }

    private void GetImmediateParentNode()
    {
        if (ReferenceEquals(_myBranch.MyParentBranch, _myBranch))
        {
            FindHomeScreenParentNode();
        }
        else
        {
            var branchesNodes = ThisGroupsUiNodes;
            _parentNode = branchesNodes.First(node => ReferenceEquals(_myBranch, node.HasChildBranch));
            _makeParentActive = true;
        }
    }

    private void FindHomeScreenParentNode()
    {
        ReturnHomeGroupIndex?.Invoke(this);
        _parentNode = TargetNode;
        _makeParentActive = false;
    }
    
    private void StartThisHotKeyBranch()
    {
        HotKeyPressed?.Invoke(this);
        _parentNode.SetAsHotKeyParent(_makeParentActive);
        _myBranch.MoveToThisBranch(_parentNode.MyBranch);
    }
}
