using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

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
    
    private void SaveLastHighlighted(IHighlightedNode args) => _lastHighlighted = (UINode)args.Highlighted;
    private void SaveLastSelected(ISelectedNode args) => _lastSelected = (UINode)args.Selected;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = (UIBranch) args.ActiveBranch.ThisBranch;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;

    private void Awake()
    {
        _onHomeScreen = true;
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IHighlightedNode>(SaveLastHighlighted);
        EVent.Do.Subscribe<ISelectedNode>(SaveLastSelected);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Subscribe<ITestList>(ManageHistory);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
    }

    public void RemoveFromEvents()
    {
        EVent.Do.Unsubscribe<IHighlightedNode>(SaveLastHighlighted);
        EVent.Do.Unsubscribe<ISelectedNode>(SaveLastSelected);
        EVent.Do.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Unsubscribe<ITestList>(ManageHistory);
        EVent.Do.Unsubscribe<IAllowKeys>(SaveAllowKeys);
    }

    private void OnDisable() => RemoveFromEvents();

    private void ManageHistory(ITestList args)
    {
        if (args.AddNode is null)
        {
            _history.Clear();
            return;
        }
        if (_history.Contains((UINode)args.AddNode))
        {
            _history.Remove((UINode) args.AddNode);
        }
        else
        {
            _history.Add((UINode) args.AddNode);
        }
    }

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