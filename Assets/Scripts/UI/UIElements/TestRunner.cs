using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEditor;

public class TestRunner : MonoBehaviour, IEventUser
{
    [SerializeField] private UINode _lastHighlighted;
    [SerializeField] private UINode _lastSelected;
    [SerializeField] private UIBranch _activeBranch;
    [SerializeField] private List<UINode> _history;
    [SerializeField] [ReadOnly] private bool _onHomeScreen = true;
    [SerializeField] [ReadOnly] private bool _allowKeys;
    [SerializeField] private EventsForTest _eventsForTest;
    [SerializeField] private string _test1Test;
    [SerializeField] private string _test2Test;
    [SerializeField] private string _test3Test;
    [SerializeField] private string _test4Test;
    [SerializeField] private string _test5Test;


    [Serializable]
    private class EventsForTest
    {
        [SerializeField] public UnityEvent _event1;
        [SerializeField] public UnityEvent _event2;
        [SerializeField] public UnityEvent _event3;
        [SerializeField] public UnityEvent _event4;
        [SerializeField] public UnityEvent _event5;
        [SerializeField] public UnityEvent _event6;
    }

    private void SaveLastHighlighted(IHighlightedNode args) => _lastHighlighted = args.Highlighted.ReturnNode;
    private void SaveLastSelected(ISelectedNode args) => _lastSelected = args.Selected.ReturnNode;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;

    private void Awake()
    {
        _onHomeScreen = true;
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        EventLocator.Subscribe<IHighlightedNode>(SaveLastHighlighted, this);
        EventLocator.Subscribe<ISelectedNode>(SaveLastSelected, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomeScreen, this);
        EventLocator.Subscribe<ITestList>(ManageHistory, this);
        EventLocator.Subscribe<IAllowKeys>(SaveAllowKeys, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IHighlightedNode>(SaveLastHighlighted);
        EventLocator.Unsubscribe<ISelectedNode>(SaveLastSelected);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EventLocator.Unsubscribe<ITestList>(ManageHistory);
        EventLocator.Unsubscribe<IAllowKeys>(SaveAllowKeys);
    }

    private void ManageHistory(ITestList args)
    {
        if (args.AddNode is null)
        {
            _history.Clear();
            return;
        }
        if (_history.Contains(args.AddNode))
        {
            _history.Remove(args.AddNode);
        }
        else
        {
            _history.Add(args.AddNode);
        }
    }
    
    private void OnDisable() => RemoveFromEvents();

    [Button ()]
    public void Button_Event1()
    {
        _eventsForTest._event1?.Invoke();
    }
    [Button]
    public void Button_Event2()
    {
        _eventsForTest._event2?.Invoke();
    }
    [Button]
    public void Button_Event3()
    {
        _eventsForTest._event3?.Invoke();
    }
    [Button]
    public void Button_Event4()
    {
        _eventsForTest._event4?.Invoke();
    }
    [Button]
    public void Button_Event5()
    {
        _eventsForTest._event5?.Invoke();
    }
    [Button]
    public void Button_Event6()
    {
        _eventsForTest._event6?.Invoke();
    }

    public void PrintTest1()
    {
        Debug.Log(_test1Test);
    }
    public void PrintTest2()
    {
        Debug.Log(_test2Test);
    }
    public void PrintTest3()
    {
        Debug.Log(_test3Test);
    }
    public void PrintTest4()
    {
        Debug.Log(_test4Test);
    }
    public void PrintTest5(bool value)
    {
        Debug.Log(_test5Test + " : " + value);
    }

}
