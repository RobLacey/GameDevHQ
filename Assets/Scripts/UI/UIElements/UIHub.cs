using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

/// <summary>
/// UIHub is the core of the system and looks after starting the system Up and general state management 
/// </summary>

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UIInput))]

public partial class UIHub : MonoBehaviour
{
    [SerializeField]
    [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")]
    private List<UIBranch> _homeBranches;

    //Events
    public static event Action OnStart;
    public static event Action<UIBranch> SetUpBranchesAtStart;

    //Variables
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private INode _lastHighlighted;
    private bool _inMenu, _startingInGame;
    private InputScheme _inputScheme;

    //Properties
    private void SaveInMenu(bool isInMenu)
    {
        _inMenu = isInMenu;
        if(!_inMenu) SetEventSystem(null);
    }

    private void Awake()
    {
        _inputScheme = GetComponent<UIInput>().ReturnScheme;
        _startingInGame = GetComponent<UIInput>().StartInGame();
        ServiceLocator.AddService<IBucketCreator>(new BucketCreator(transform, "Tooltip Holder"));
        ServiceLocator.AddService<IAudioService>(new UIAudioManager(GetComponent<AudioSource>()));
        CreateSubClasses();
    }

    private void CreateSubClasses()
    {
        var unused = new PopUpController();
        var unused3 = new UIHomeGroup(_homeBranches.ToArray());
        var unused2 = new UICancel(_inputScheme.GlobalCancelAction);
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SetLastHighlighted);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
    }

    private void Start()
    {
        SetStartPositionsAndSettings();
        CheckIfStartingInGame();
        StartCoroutine(EnableStartControls());
    }

    private void SetStartPositionsAndSettings()
    {
        _lastHighlighted = _homeBranches[0].DefaultStartOnThisNode;
        _homeBranches[0].DefaultStartOnThisNode.ThisNodeIsSelected();
        _homeBranches[0].DefaultStartOnThisNode.ThisNodeIsHighLighted();
        SetUpBranchesAtStart?.Invoke(_homeBranches[0]);
    }

    private void CheckIfStartingInGame()
    {
        if (_startingInGame)
        {
            OnStart?.Invoke();
            _inMenu = false;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_homeBranches[0].DefaultStartOnThisNode.gameObject);
            _inMenu = true;
        }
    }
    
    private IEnumerator EnableStartControls()
    {
        yield return new WaitForSeconds(_inputScheme.StartDelay);
        if(!_startingInGame)
            OnStart?.Invoke();
    }
    
    private void SetLastHighlighted(INode newNode)
    {
        _lastHighlighted = newNode;
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