using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;

/// <summary>
/// UIHub is the core of the system and looks after starting the system Up and general state management 
/// </summary>

public interface IHub : IParameters
{
    GameObject ThisGameObject { get; }
    IBranch[] HomeBranches { get; }
    InputScheme Scheme { get; }
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UIInput))]

public class UIHub : MonoBehaviour, IHub, IEventUser, ISetUpStartBranches, IOnStart
{
    [SerializeField]
    [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")]
    private List<UIBranch> _homeBranches;

    //Events
    private Action<IOnStart> OnStart { get; } = EVent.Do.FetchEVent<IOnStart>();
    private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; } = EVent.Do.FetchEVent<ISetUpStartBranches>();
    
    //Variables
    private INode _lastHighlighted;
    private bool _inMenu, _startingInGame;
    private InputScheme _inputScheme;

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
        ObserveEvents();
    }
    
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IHighlightedNode>(SetLastHighlighted);
        EVent.Do.Subscribe<IInMenu>(SaveInMenu);
    }
    
    public void RemoveFromEvents()
    {
        EVent.Do.Unsubscribe<IHighlightedNode>(SetLastHighlighted);
        EVent.Do.Unsubscribe<IInMenu>(SaveInMenu);
    }

    private void OnEnable()
    {
        ServiceLocator.Bind(EJect.Class.WithParams<IAudioService>(this));
        ServiceLocator.Bind(EJect.Class.WithParams<IHistoryTrack>(this));
        ServiceLocator.Bind(EJect.Class.WithParams<IHomeGroup>(this));
        SetUpBucketCreatorService();
    }

    private void SetUpBucketCreatorService()
    {
        var bucket = EJect.Class.NoParams<IBucketCreator>();
        bucket.SetName("ToolTip Holder")
              .SetParent(transform)
              .CreateBucket();
        ServiceLocator.Bind(bucket);
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
        => SetUpBranchesAtStart?.Invoke(this);

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