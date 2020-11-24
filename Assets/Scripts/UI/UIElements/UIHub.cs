using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;
using Debug = UnityEngine.Debug;

/// <summary>
/// UIHub is the core of the system and looks after starting the system Up and general state management 
/// </summary>

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UIInput))]

public class UIHub : MonoBehaviour, IEventUser, ISetUpStartBranches, IOnStart
{
    [SerializeField]
    [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")]
    private List<UIBranch> _homeBranches;

    //Events
    private static CustomEvent<IOnStart> OnStart { get; } = new CustomEvent<IOnStart>();
    private static CustomEvent<ISetUpStartBranches> SetUpBranchesAtStart { get; } 
        = new CustomEvent<ISetUpStartBranches>();
    
    //Variables
    private INode _lastHighlighted;
    private bool _inMenu, _startingInGame;
    private InputScheme _inputScheme;
    
    //private static ClassCreator<ICreate> newClass { get; } = new ClassCreator<ICreate>(typeof(TestClass));
    private ITestClass _testClass1;
    private readonly IInjectClass _newIoC = new IoC();

    //Properties
    private void SaveInMenu(IInMenu args)
    {
        
        _inMenu = args.InTheMenu;
        if(!_inMenu) SetEventSystem(null);
    }
    public UIBranch StartBranch => _homeBranches.First();

    //Main
    private void Awake()
    { 
        var counter = 0;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
       
        
       for (int i = 0; i < 1; i++)
       { 
           var args = new TestClassArgs(4, NavigationType.RightAndLeft);
            _testClass1 = _newIoC.WithParams<ITestClass>(args);
           counter++;
       }
       stopwatch.Stop();
       //Debug.Log($"Test took {stopwatch.ElapsedMilliseconds} milliseconds");
       Debug.Log($"Test took {stopwatch.Elapsed} milliseconds");
       Debug.Log(counter);
       _testClass1.CheckConstruction();
       
        _inputScheme = GetComponent<UIInput>().ReturnScheme;
        _startingInGame = GetComponent<UIInput>().StartInGame();
        ObserveEvents();
    }
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IHighlightedNode>(SetLastHighlighted, this);
        EventLocator.Subscribe<IInMenu>(SaveInMenu, this);
    }
    
    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IHighlightedNode>(SetLastHighlighted);
        EventLocator.Unsubscribe<IInMenu>(SaveInMenu);
    }

    private void OnEnable()
    {
        ServiceLocator.Bind<IAudioService>(new UIAudioManager(GetComponent<AudioSource>()));
        ServiceLocator.Bind<IBucketCreator>(new BucketCreator(transform, "Tooltip Holder"));
        ServiceLocator.Bind<IHistoryTrack>(new HistoryTracker(_inputScheme.GlobalCancelAction));
        ServiceLocator.Bind<IHomeGroup>(new UIHomeGroup(_homeBranches.ToArray()));
    }

    private void OnDisable()
    {
        RemoveFromEvents();
        ServiceLocator.Remove<IAudioService>();
        ServiceLocator.Remove<IBucketCreator>();
        ServiceLocator.Remove<IHistoryTrack>();
        ServiceLocator.Remove<IHomeGroup>();
    }

    private void Start()
    {
        SetStartPositionsAndSettings();
        CheckIfStartingInGame();
        StartCoroutine(EnableStartControls());
    }

    private void SetStartPositionsAndSettings() 
        => SetUpBranchesAtStart?.RaiseEvent(this);

    private void CheckIfStartingInGame()
    {
        if (_startingInGame)
        {
            OnStart?.RaiseEvent(this);
            _inMenu = false;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_homeBranches.First()
                               .DefaultStartOnThisNode.ReturnGameObject);
            _inMenu = true;
        }
    }
    
    private IEnumerator EnableStartControls()
    {
        yield return new WaitForSeconds(_inputScheme.StartDelay);
        if(!_startingInGame)
            OnStart?.RaiseEvent(this);
    }
    
    private void SetLastHighlighted(IHighlightedNode args)
    {
        _lastHighlighted = args.Highlighted;
        if(_inMenu) SetEventSystem(_lastHighlighted.ReturnGameObject);
    }

    public static void SetEventSystem(GameObject newGameObject) 
        => EventSystem.current.SetSelectedGameObject(newGameObject);
    
    [Button("Add a New Branch")]
    // ReSharper disable once UnusedMember.Local
    private void MakeFolder()
    {
        var newTree = CreateMainFolder();
        var newBranch = CreateNewBranch(newTree);
        CreateFirstNode(newBranch);
    }

    private static void CreateFirstNode(GameObject newBranch)
    {
        var newNode = new GameObject();
        newNode.transform.parent = newBranch.transform;
        newNode.name = "New Node";
        newNode.AddComponent<UINode>();
    }

    private static GameObject CreateNewBranch(GameObject newTree)
    {
        var newBranch = new GameObject();
        newBranch.transform.parent = newTree.transform;
        newBranch.name = "New Branch";
        newBranch.AddComponent<UIBranch>();
        return newBranch;
    }

    private GameObject CreateMainFolder()
    {
        var newTree = new GameObject();
        newTree.transform.parent = transform;
        newTree.name = "New Tree Folder";
        newTree.AddComponent<RectTransform>();
        return newTree;
    }
}