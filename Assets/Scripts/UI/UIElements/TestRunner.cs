using System;
using System.Collections;
using EZ.Events;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestRunner : MonoBehaviour, IEZEventUser
{
    [SerializeField] private UIData _uiData;
    [SerializeField] [Space(20, order = 1)] private int _nextScene  = 6;
    [SerializeField] private EventsForTest _eventsForTest = default;
    [SerializeField] private string _test1Test = default;
    [SerializeField] private string _test2Test = default;
    [SerializeField] private string _test3Test = default;
    [SerializeField] private string _test4Test = default;
    [SerializeField] private string _test5Test = default;
    
    [Header("New GameObject")]
    [SerializeField] private GameObject _newObject;
    [SerializeField] private Transform _parentFolder;
    
    [Header("New Node")]
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private UIBranch _targetBranch;
    [SerializeField] private Transform _parentTransform;
    
    
    private int minV = -5;
    private int maxV = 5;
    private int minH = -9;
    private int maxH = 9;
    private int _counter;
    private int _nodeCounter;

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

    private void OnEnable()
    {
        ObserveEvents();
        _uiData.OnEnable();
    }

    public void ObserveEvents()
    {
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
    [Button ("Add A New Node At Runtime")]
    public void Button_Event7()
    {
        var newObject = Instantiate(_nodePrefab, _parentTransform);
        newObject.name = "New Object : " + _nodeCounter;
        _targetBranch.AddNodeToBranch();
        _nodeCounter++;
    }
    
    [Button]
    public void AddANewInGameObject()
    {
        Vector2 randomPos = new Vector2(Random.Range(minH, maxH), Random.Range(minV, maxV));
        var newObject = Instantiate(_newObject, randomPos, Quaternion.identity, _parentFolder);
        newObject.name = "New Object : " + _counter;
        newObject.GetComponent<RunTimeGetter>().BufferMainTextForCreation(_counter.ToString());
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


