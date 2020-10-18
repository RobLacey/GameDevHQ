using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class TestRunner : MonoBehaviour, IEventUser
{
    [SerializeField] private UINode _lastHighlighted;
    [SerializeField] private UINode _lastSelected;
    [SerializeField] private UIBranch _activeBranch;
    [SerializeField] [ReadOnly] private bool _onHomeScreen = true;
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
    }

    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();

    private void SaveLastHighlighted(INode newNode) => _lastHighlighted = newNode.ReturnNode;
    private void SaveLastSelected(INode newNode) => _lastSelected = newNode.ReturnNode;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private void SaveOnHomeScreen(bool onHome) => _onHomeScreen = onHome;

    private void Awake()
    {
        _onHomeScreen = true;
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<IHighlightedNode, INode>(SaveLastHighlighted, this);
        EventLocator.SubscribeToEvent<ISelectedNode, INode>(SaveLastSelected, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IHighlightedNode, INode>(SaveLastHighlighted);
        EventLocator.UnsubscribeFromEvent<ISelectedNode, INode>(SaveLastSelected);
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
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
