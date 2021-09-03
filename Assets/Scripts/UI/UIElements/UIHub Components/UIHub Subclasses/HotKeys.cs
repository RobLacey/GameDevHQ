using System;
using System.Linq;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Serializable]
public partial class HotKeys : IHotKeyPressed, IEZEventDispatcher, IServiceUser,
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
    private InputScheme _inputScheme;
    private bool _makeParentActive;
    private IDataHub _myDatHub;
    private const string SetName = "Set My Name";

    //Events
    private Action<IHotKeyPressed> HotKeyPressed { get; set; }

    //Properties
    private IBranch ActiveBranch => _myDatHub.ActiveBranch;
    public INode ParentNode => _parentNode;
    public IBranch MyBranch => _myBranch;
    private INode[] ThisGroupsUiNodes => _myBranch.MyParentBranch.ThisGroupsUiNodes;

    //Main
    public void OnEnable()
    {
        IsAllowedType();
        FetchEvents();
        UseEZServiceLocator();
    }
    
    public void UseEZServiceLocator()  
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDatHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void FetchEvents() => HotKeyPressed = InputEvents.Do.Fetch<IHotKeyPressed>();

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
        if(ReferenceEquals(ActiveBranch, _myBranch)) return;
        if(!_hasParentNode)
        {
            GetParentNode();
        }
        
        if (ActiveBranch.IsHomeScreenBranch())
        {
            StartThisHotKeyBranch();
        }
        else
        {
            ActiveBranch.StartBranchExitProcess(OutTweenType.Cancel, StartThisHotKeyBranch);
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
        var branchesNodes = ThisGroupsUiNodes;
        _parentNode = branchesNodes.First(node => ReferenceEquals(_myBranch, node.HasChildBranch));
        _makeParentActive = true;
    }

    private void FindHomeScreenParentNode()
    {
        var toTest = _myBranch.MyParentBranch;

        while (!toTest.IsHomeScreenBranch())
        {
            toTest = toTest.MyParentBranch;
        }
        
        _makeParentActive = false;
        _parentNode = toTest.LastHighlighted;
    }
    
    private void StartThisHotKeyBranch()
    {
        HotKeyPressed?.Invoke(this);
        _parentNode.SetAsHotKeyParent(_makeParentActive);
        _myBranch.MoveToThisBranch(_parentNode.MyBranch);
    }
}
