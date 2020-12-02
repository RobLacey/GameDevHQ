using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// UIHub is the core of the system and looks after starting the system Up and general state management 
/// </summary>

public interface IHub : IParameters
{
    GameObject ThisGameObject { get; }
    IBranch[] HomeBranches { get; }
    InputScheme Scheme { get; }
}

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UIInput))]

public class UIHub : MonoBehaviour, IHub, IEventUser, ISetUpStartBranches, IOnStart, ISceneChange, 
                     IEServUser, IEventDispatcher
{
    [SerializeField] private int _nextScene;
    [SerializeField]
    [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")]
    private List<UIBranch> _homeBranches;

    //Events
    private Action<IOnStart> OnStart { get; set; }
    private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; set; }
    private Action<ISceneChange> SceneChanging { get; set; }

    //Variables
    private INode _lastHighlighted;
    private bool _inMenu, _startingInGame;
    private InputScheme _inputScheme;
    private EVentBindings _eVentBindings = new EVentBindings();
    private IHistoryTrack _historyTrack;
    private IAudioService _audioService;
    private IHomeGroup _homeGroup;

    //Properties
    private void SaveInMenu(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if(!_inMenu) SetEventSystem(null);
    }
    public IBranch StartBranch => _homeBranches.First();
    public GameObject ThisGameObject => gameObject;
    public IBranch[] HomeBranches => _homeBranches.ToArray<IBranch>();
    public InputScheme Scheme => _inputScheme;

    //Main
    private void Awake()
    { 
       var uIInput = GetComponent<IInput>();
        _inputScheme = uIInput.ReturnScheme;
        _startingInGame = uIInput.StartInGame();
        _historyTrack = EJect.Class.WithParams<IHistoryTrack>(this);
        _audioService = EJect.Class.WithParams<IAudioService>(this);
        _homeGroup = EJect.Class.WithParams<IHomeGroup>(this);
        UseEServLocator();
    }


    private void OnEnable()
    {
        FetchEvents();
        _historyTrack.OnEnable();
        _homeGroup.OnEnable();
        ObserveEvents();
        SetUpBucketCreatorService();
    }

    private void OnDisable()
    {
        _historyTrack.OnDisable();
        _homeGroup.OnDisable();
        _audioService.OnDisable();
        RemoveEvents();
    }

    public void FetchEvents()
    {
        OnStart = EVent.Do.Fetch<IOnStart>();
        SetUpBranchesAtStart  = EVent.Do.Fetch<ISetUpStartBranches>();
        SceneChanging = EVent.Do.Fetch<ISceneChange>();
    }

    public void UseEServLocator()
    {
        EServ.Locator.AddNew(_historyTrack);
        EServ.Locator.AddNew(_audioService);
        EServ.Locator.AddNew(_homeGroup);
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IHighlightedNode>(SetLastHighlighted);
        EVent.Do.Subscribe<IInMenu>(SaveInMenu);
        EVent.Do.Subscribe<IAllowKeys>(SwitchedToKeys);
    }

    public void RemoveEvents()
    {
        EVent.Do.Unsubscribe<IHighlightedNode>(SetLastHighlighted);
        EVent.Do.Unsubscribe<IInMenu>(SaveInMenu);
        EVent.Do.Unsubscribe<IAllowKeys>(SwitchedToKeys);
    }

    private void SetUpBucketCreatorService()
    {
        var bucket = EJect.Class.NoParams<IBucketCreator>();
        bucket.SetName("ToolTip Holder")
              .SetParent(transform)
              .CreateBucket();
        EServ.Locator.AddNew(bucket);
    }

    private void Start()
    {
        SetStartPositionsAndSettings();
        CheckIfStartingInGame();
        StartCoroutine(EnableStartControls());
    }

    private void SetStartPositionsAndSettings() => SetUpBranchesAtStart?.Invoke(this);

    private void CheckIfStartingInGame()
    {
        if (_startingInGame)
        {
            OnStart?.Invoke(this);
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
        yield return new WaitForSeconds(Scheme.StartDelay);
        if(!_startingInGame)
            OnStart?.Invoke(this);
        SetEventSystem(_lastHighlighted.ReturnGameObject);
    }
    
    private void SetLastHighlighted(IHighlightedNode args)
    {
        _lastHighlighted = args.Highlighted;
        if(_inMenu) SetEventSystem(_lastHighlighted.ReturnGameObject);
    }

    private void SwitchedToKeys(IAllowKeys args)
    {
        SetEventSystem(_lastHighlighted.ReturnGameObject);
    }

    private static void SetEventSystem(GameObject newGameObject) 
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

    public void LoadNextScene()
    {
        StartCoroutine(StartOut());
    }

    private IEnumerator StartOut()
    {
        yield return new WaitForSeconds(0.5f);
        SceneChanging?.Invoke(this);
        SceneManager.LoadScene(_nextScene);
    }

}