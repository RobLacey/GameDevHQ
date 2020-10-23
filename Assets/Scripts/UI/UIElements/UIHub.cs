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
        ServiceLocator.AddService<IAudioService>(new UIAudioManager(GetComponent<AudioSource>()));
        ServiceLocator.AddService<IBucketCreator>(new BucketCreator(transform, "Tooltip Holder"));
        ServiceLocator.AddService<IHistoryTrack>(new HistoryTracker());
        ServiceLocator.AddService<IHomeGroup>(new UIHomeGroup(_homeBranches.ToArray()));
        ServiceLocator.AddService<IPopUpController>(new PopUpController());
        ServiceLocator.AddService<ICancel>(new UICancel(_inputScheme.GlobalCancelAction));
    }

    private void OnDisable()
    {
        RemoveFromEvents();
        ServiceLocator.RemoveService<IAudioService>();
        ServiceLocator.RemoveService<IBucketCreator>();
        ServiceLocator.RemoveService<IHistoryTrack>();
        ServiceLocator.RemoveService<IHomeGroup>();
        ServiceLocator.RemoveService<IPopUpController>();
        ServiceLocator.RemoveService<ICancel>();
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
            EventSystem.current.SetSelectedGameObject(_homeBranches.First().DefaultStartOnThisNode.gameObject);
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
        if(_inMenu) SetEventSystem(_lastHighlighted.ReturnNode.gameObject);
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