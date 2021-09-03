using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TestRunner : MonoBehaviour
{
    [SerializeField] private UIData _uiData;
    [SerializeField] [Space(20, order = 1)] private int _nextScene  = 6;
    [SerializeField] private EventsForTest _eventsForTest = default;
    [SerializeField] [Foldout("Test Text Settings")] private string _test1Text = default;
    [SerializeField] [Foldout("Test Text Settings")] private string _test2Text = default;
    [SerializeField] [Foldout("Test Text Settings")] private string _test3Text = default;
    [SerializeField] [Foldout("Test Text Settings")] private string _test4Text = default;
    [SerializeField] [Foldout("Test Text Settings")] private string _test5Text = default;
    
    [Header("New GameObject")]
    [SerializeField] private GameObject _newObject;
    [SerializeField] private Transform _gouiParentFolder;
    
    [Header("New Node")]
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private UIBranch _targetBranch;
    [SerializeField] private Transform _parentTransform;

    [SerializeField] private PopUpTester _popUpPrefabs;

    [Header("Disable This Node Test")] 
    [SerializeField] private UINode _testCase;
    
    
    private int minV = -5;
    private int maxV = 5;
    private int minH = -9;
    private int maxH = 9;
    private int _gouiCounter;
    private int _nodeCounter;
    private List<GOUIModule> _gouiModules;

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

    private void Awake()
    {
        _gouiModules = FindObjectsOfType<GOUIModule>().ToList();
    }

    private void OnEnable()
    {
        _uiData.OnEnable();
        _popUpPrefabs.OnEnable();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddANewInGameObject();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            RemoveInGameObject();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddANewNodeAtRuntime();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            RemoveNodeAtRuntime();
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
    private void AddANewNodeAtRuntime()
    {
        var newObject = Instantiate(_nodePrefab, _parentTransform);
        newObject.name = "New Object : " + _nodeCounter;
        _nodeCounter++;
    }
    
    [Button]
    private void RemoveNodeAtRuntime()
    {
        if(_targetBranch.ThisGroupsUiNodes.Length > 0)
        {
            var last = _targetBranch.ThisGroupsUiNodes.Last();
            Destroy(last.ReturnGameObject);
        }
    }
    
    [Button]
    private void AddANewInGameObject()
    {
        GameObject newObject;
        
        foreach (var gouiModule in _gouiModules.Where(gouiModule => !gouiModule.gameObject.activeSelf))
        {
            newObject = gouiModule.gameObject;
            newObject.SetActive(true);
            return;
        }
        
        Vector2 randomPos = new Vector2(Random.Range(minH, maxH), Random.Range(minV, maxV));
        newObject = Instantiate(_newObject, randomPos, Quaternion.identity, _gouiParentFolder);
        newObject.NewName("New Object : " + _gouiCounter);
        _gouiCounter++;
        var module = newObject.GetComponent<GOUIModule>();
        module.RenameGouiBranch();
        _gouiModules.Add(module);

    }
    [Button]
    private void RemoveInGameObject()
    {
        if (_gouiModules.Count <= 0) return;
        
        foreach (var gouiModule in _gouiModules.Where(gouiModule => gouiModule.gameObject.activeSelf))
        {
            gouiModule.gameObject.SetActive(false);
            return;
        }
    }

    [Button] 
    public void CreateResolvePopUp() => _popUpPrefabs.MakeResolvePopUp();

    [Button]
    public void CreateOptionalPopUp() => _popUpPrefabs.MakeOptionalPopUp();

    [Button]
    public void CreateTimedPopUp() => _popUpPrefabs.MakeTimedPopUp();

    [Button]
    public void RemoveResolvePopUp() => _popUpPrefabs.RemoveResolvePopUp();

    [Button]
    public void RemoveOptionalPopUp() => _popUpPrefabs.RemoveOptionalPopUp();

    [Button]
    public void RemoveTimedPopUp() => _popUpPrefabs.RemoveTimedPopUp();

    [Button]
    public void DisableButton() => _testCase.DisableNode();
    [Button]
    public void EnableButton() => _testCase.EnableNode();

    public void PrintTest1()
    {
        Debug.Log(_test1Text);
    }
    public void PrintTest2()
    {
        Debug.Log(_test2Text);
    }
    public void PrintTest3()
    {
        Debug.Log(_test3Text);
    }
    public void PrintTest4()
    {
        Debug.Log(_test4Text);
    }
    public void PrintTest5(bool value)
    {
        Debug.Log(_test5Text + " : " + value);
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


