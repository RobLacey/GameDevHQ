using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UIElements;
using UIElements.Input_System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TestRunner : MonoBehaviour, IEventUser
{
    [SerializeField] private UINode _lastHighlighted = default;
    [SerializeField] private UINode _lastSelected = default;
    [SerializeField] private UIBranch _activeBranch = default;
    [SerializeField] [Space(20, order = 1)] private int _nextScene  = 6;
    [SerializeField] private List<UINode> _history = default;
    [SerializeField] private List<UINode> _multiSelect = default;
    [SerializeField] [ReadOnly] private bool _onHomeScreen = true;
    [SerializeField] [ReadOnly] private bool _allowKeys = default;
    [SerializeField] [ReadOnly] private bool _inMenu;
    [SerializeField] private EventsForTest _eventsForTest = default;
    [SerializeField] private string _test1Test = default;
    [SerializeField] private string _test2Test = default;
    [SerializeField] private string _test3Test = default;
    [SerializeField] private string _test4Test = default;
    [SerializeField] private string _test5Test = default;
    [SerializeField] private GameObject _newObject;
    [SerializeField] private Transform _parentFolder;
    
    
    private int minV = -5;
    private int maxV = 5;
    private int minH = -9;
    private int maxH = 9;
    private int _counter;

    [Serializable]
    private class EventsForTest
    {
        [SerializeField] public UnityEvent _event1 = default;
        [SerializeField] public UnityEvent _event2 = default;
        [SerializeField] public UnityEvent _event3 = default;
        [SerializeField] public UnityEvent _event4 = default;
        [SerializeField] public UnityEvent _event5 = default;
        [SerializeField] public UnityEvent _event6 = default;
    }
    
    private void SaveLastHighlighted(IHighlightedNode args)
    {
        //Debug.Log(args.Highlighted);
        _lastHighlighted = (UINode) args.Highlighted;
    }
    private void SaveLastSelected(ISelectedNode args)  
    {
        _lastSelected = (UINode) args.UINode;
    }
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = (UIBranch) args.ActiveBranch.ThisBranch;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
    private void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;

    private void Awake() => _onHomeScreen = true;

    private void OnEnable() => ObserveEvents();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IHighlightedNode>(SaveLastHighlighted);
        EVent.Do.Subscribe<ISelectedNode>(SaveLastSelected);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Subscribe<ITestList>(ManageHistory);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IInMenu>(SaveInMenu);

        MultiSelectSystem.MultiSelectChange += ManageMultiSelect;
    }


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
    
    private void ManageMultiSelect(INode newNode)
    {
        if (newNode is null)
        {
            _multiSelect.Clear();
            return;
        }
        if (_multiSelect.Contains((UINode)newNode))
        {
            _multiSelect.Remove((UINode) newNode);
        }
        else
        {
            _multiSelect.Add((UINode) newNode);
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
    [Button]
    public void AddANewInGameObject()
    {
        Vector2 randomPos = new Vector2(Random.Range(minH, maxH), Random.Range(minV, maxV));
        var newObject = Instantiate(_newObject, randomPos, Quaternion.identity, _parentFolder);
        newObject.name = "New Object : " + _counter;
        newObject.GetComponent<RunTimeGetter>().BufferMainTextFroCreation(_counter.ToString());
        _counter++;
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
    
    /// <summary>
    /// Used by UnityEvent in Inspector
    /// </summary>
    public void LoadNextScene()
    {
        StartCoroutine(StartOut());
    }

    private IEnumerator StartOut()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(_nextScene);
    }
}


