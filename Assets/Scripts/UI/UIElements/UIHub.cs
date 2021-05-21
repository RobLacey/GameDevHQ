using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
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
    List<IBranch> HomeBranches { get; }
}

namespace UIElements
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(UIInput))]

    public class UIHub : MonoBehaviour, IHub, IEZEventUser, ISetUpStartBranches, IOnStart, 
                         IEZEventDispatcher, IIsAService, IServiceUser, ISceneIsChanging
    {
        public UIHub()
        {
            PopUpEvents.Do.Initialise(new PopUpBindings());
            HistoryEvents.Do.Initialise(new HistoryBindings());
            InputEvents.Do.Initialise(new InputBindings());
            GOUIEvents.Do.Initialise(new GOUIEventsBindings());
            BranchEvent.Do.Initialise(new BranchBindings());
            CancelEvents.Do.Initialise(new CancelBindings());
            EZService.Locator.Initialise();
        }

        [Space(10f, order = 1)]
        [Header(StartOnTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        
        [SerializeField] 
        [Tooltip(StartOnInfoBox)]
        private UIBranch _startOnThisBranch;

        [Space(10f, order = 1)] 
        [Header(CanvasOrderTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        
        [SerializeField] 
        private CanvasOrderData _canvasSortingOrderSettings;

        [Space(10f, order = 1)] 
        [Header(UIDataTitle, order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
        [SerializeField] private UIData _uiData;
        
        [SerializeField] private int _nextLevel;

        
        //Editor
        [Button("Add a New Tree Structure")]
        private void MakeTreeFolders()
        {
            new CreateNewObjects().CreateMainFolder(transform)
                                  .CreateBranch()
                                  .CreateNode();
        }
        
        [Button("Add a ToolTip Bin")]
        private void MakeTooltipFolder()
        {
            new CreateNewObjects().CreateToolTipFolder(transform);
        }
        
        [Button("Add a In Game Object UI Bin")]
        private void MakeInGameUiFolder()
        {
            new CreateNewObjects().CreateInGameUIBin(transform);
        }
    
        //Variables
        private List<IBranch> _homeBranches;
        private INode _lastHighlighted;
        private bool _startingInGame, _inMenu;
        private InputScheme _inputScheme;
        private IHistoryTrack _historyTrack;
        private IAudioService _audioService;
        private ICancel _cancelHandler;
        private IDataHub _myDataHub;
        private const string StartOnTitle = "Set On Which Branch To Start On";
        private const string StartOnInfoBox = "If left blank a random Home Group will be used";
        private const string CanvasOrderTitle = "Canvas Sorting Order Setting for Branch Types";
        private const string UIDataTitle = "UI Data";

        //Events
        private Action<IOnStart> OnStart { get; set; } 
        private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; set; } 
        public Action<ISceneIsChanging> SceneChanging { get; set; }

        
        //Properties & Getters/ Setters
        public IBranch StartBranch => _homeBranches.First();
        public GameObject ThisGameObject => gameObject;
        public List<IBranch> HomeBranches => _homeBranches;
        private RectTransform MainCanvasRect => GetComponent<RectTransform>();


        //Set / Getters
        private void SaveInMenu(IInMenu args)
        {
            _inMenu = args.InTheMenu;
            if(!_inMenu) SetEventSystem(null);
        }

        //Main
        private void Awake()
        { 
            var uIInput = GetComponent<IInput>();
            _startingInGame = uIInput.StartInGame();
            GetHomeScreenBranches();
            _historyTrack = EZInject.Class.NoParams<IHistoryTrack>();
            _cancelHandler = EZInject.Class.NoParams<ICancel>();
            _audioService = EZInject.Class.WithParams<IAudioService>(this);
            AddService();
            _myDataHub = new DataHub(MainCanvasRect);
            _myDataHub.OnAwake();
        }

        private void GetHomeScreenBranches()
        {
            var all = FindObjectsOfType<UIBranch>();
            _homeBranches = new List<IBranch>();
            foreach (var uiBranch in all)
            {
                if (!uiBranch.IsHomeScreenBranch()) continue;
                
                if(uiBranch == _startOnThisBranch)
                {
                    _homeBranches.Insert(0, uiBranch);
                }
                else
                {
                    _homeBranches.Add(uiBranch);
                }
            }
        }

        private void OnEnable()
        {
            UseEZServiceLocator();
            FetchEvents();
            ObserveEvents();
            _canvasSortingOrderSettings.OnEnable();
            _historyTrack.OnEnable();
            _cancelHandler.OnEnable();
            _audioService.OnEnable();
            _uiData.OnEnable();
            _myDataHub.OnEnable();
        }
        
        public void UseEZServiceLocator() => _inputScheme = EZService.Locator.Get<InputScheme>(this);

        public void AddService() => EZService.Locator.AddNew<IHub>(this);

        public void OnRemoveService() => _audioService.OnDisable();

        public void FetchEvents()
        {
            OnStart = HistoryEvents.Do.Fetch<IOnStart>();
            SetUpBranchesAtStart = BranchEvent.Do.Fetch<ISetUpStartBranches>();
            SceneChanging = HistoryEvents.Do.Fetch<ISceneIsChanging>();
        }

        public void ObserveEvents()
        {
            HistoryEvents.Do.Subscribe<IHighlightedNode>(SetLastHighlighted);
            HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);
            InputEvents.Do.Subscribe<IAllowKeys>(SwitchedToKeys);
        }

        private void Start() => StartCoroutine(StartUIDelay());

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator StartUIDelay()
        {
            yield return new WaitForEndOfFrame(); //Helps sync up Tweens and thread
            if(_inputScheme.DelayUIStart != 0)
                yield return new WaitForSeconds(_inputScheme.DelayUIStart);
            CheckIfStartingInGame();
            SetStartPositionsAndSettings();
            StartCoroutine(EnableStartControls());
        }

        private void CheckIfStartingInGame()
        {
            if (_startingInGame)
            {
                OnStart?.Invoke(this);
                _inMenu = false;
            }
            else
            {
                SetEventSystem(GetFirstHighlightedNodeInHomeGroup());
                _inMenu = true;
            }
        }

        private void SetStartPositionsAndSettings() => SetUpBranchesAtStart?.Invoke(this);

        private GameObject GetFirstHighlightedNodeInHomeGroup()
        {
            return _homeBranches.First().DefaultStartOnThisNode.ReturnGameObject;
        }

        private IEnumerator EnableStartControls()
        {
            if(_inputScheme.ControlActivateDelay != 0)
                yield return new WaitForSeconds(_inputScheme.ControlActivateDelay);
            
            if(!_startingInGame)
            {
                OnStart?.Invoke(this);
            }            
            SetEventSystem(GetFirstHighlightedNodeInHomeGroup());
        }
    
        private void SetLastHighlighted(IHighlightedNode args)
        {
            _lastHighlighted = args.Highlighted;
            if(_inMenu) 
                SetEventSystem(_lastHighlighted.ReturnGameObject);
        }

        private void SwitchedToKeys(IAllowKeys args) => SetEventSystem(GetCorrectLastHighlighted());

        private GameObject GetCorrectLastHighlighted()
        {
            return _lastHighlighted is null ? GetFirstHighlightedNodeInHomeGroup() : 
                _lastHighlighted.ReturnGameObject;
        }

        private static void SetEventSystem(GameObject newGameObject) 
            => EventSystem.current.SetSelectedGameObject(newGameObject);

        public void LoadNextLevel()
        {
            SceneChanging?.Invoke(this);
            CloseServices();
            StartCoroutine(Load());
        }

        private void CloseServices()
        {
            BranchEvent.Do.Purge();
            GOUIEvents.Do.Purge();
            PopUpEvents.Do.Purge();
            HistoryEvents.Do.Purge();
            InputEvents.Do.Purge();
            CancelEvents.Do.Purge();
            EZService.Locator.Purge();
        }

        private IEnumerator Load()
        {
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(_nextLevel);
        }

        private void OnApplicationQuit()
        {
            SceneChanging?.Invoke(this);
            CloseServices();
        }
    }
}