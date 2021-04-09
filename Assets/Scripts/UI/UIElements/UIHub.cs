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
    RectTransform MainCanvas { get; }
}

namespace UIElements
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(UIInput))]

    public class UIHub : MonoBehaviour, IHub, IEventUser, ISetUpStartBranches, IOnStart, ISceneChange, 
                         IEServUser, IEventDispatcher
    {
        [SerializeField] private UIBranch _startOnThisBranch;

        [Header("Canvas Sorting Order Setting for Branch Types", order = 2)]
        [HorizontalLine(1f, EColor.Blue, order = 3)]
        [Space(10, order = 1)]
        [SerializeField]
        private CanvasOrderData _canvasSortingOrderSettings; 
        
        //Editor
        [Button("Add a New Tree Structure")]
        private void MakeTreeFolders()
        {
            new CreateNewObjects().CreateMainFolder(transform)
                                  .CreateBranch()
                                  .CreateNode();
            MakeTooltipFolder();
        }
        
        [Button("Add a ToolTip Bin")]
        private void MakeTooltipFolder()
        {
            new CreateNewObjects().CreateToolTipFolder(transform);
        }
    
        //Variables
        private List<UIBranch> _homeBranches;
        private INode _lastHighlighted;
        private bool _inMenu, _startingInGame;
        private InputScheme _inputScheme;
        private EVentBindings _eVentBindings = new EVentBindings(new EVent());
        private IHistoryTrack _historyTrack;
        private IAudioService _audioService;
        private ICancel _cancelHandler;

        //Events
        private Action<IOnStart> OnStart { get; set; }
        private Action<ISetUpStartBranches> SetUpBranchesAtStart { get; set; }
        private Action<ISceneChange> SceneChanging { get; set; }
        
        //Properties
        public IBranch StartBranch => _homeBranches.First();
        public GameObject ThisGameObject => gameObject;
        public IBranch[] HomeBranches => _homeBranches.ToArray<IBranch>();
        public InputScheme Scheme => _inputScheme;
        public RectTransform MainCanvas => GetComponent<RectTransform>();

        //Set / Getters
        private void SaveInMenu(IInMenu args)
        {
            _inMenu = args.InTheMenu;
            if(!_inMenu) SetEventSystem(null);
        }

        private void ReturnHomeBranches(IGetHomeBranches args) => args.HomeBranches = _homeBranches;

        //Main
        private void Awake()
        { 
            var uIInput = GetComponent<IInput>();
            _inputScheme = GetComponent<IInput>().ReturnScheme;
            _startingInGame = uIInput.StartInGame();
            GetHomeScreenBranches();
            _historyTrack = EJect.Class.NoParams<IHistoryTrack>();
            _cancelHandler = EJect.Class.WithParams<ICancel>(this);
            _audioService = EJect.Class.WithParams<IAudioService>(this);
        }

        private void GetHomeScreenBranches()
        {
            var all = FindObjectsOfType<UIBranch>();
            _homeBranches = new List<UIBranch>();
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
            _canvasSortingOrderSettings.OnEnable();
            UseEServLocator();
            FetchEvents();
            _historyTrack.OnEnable();
            _cancelHandler.OnEnable();
            ObserveEvents();
        }

        private void OnDisable()
        {
            _audioService.OnDisable();
            SceneChanging?.Invoke(this);
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
            EServ.Locator.AddNew((IHub)this);
        }

        public void ObserveEvents()
        {
            EVent.Do.Subscribe<IHighlightedNode>(SetLastHighlighted);
            EVent.Do.Subscribe<IInMenu>(SaveInMenu);
            EVent.Do.Subscribe<IAllowKeys>(SwitchedToKeys);
            EVent.Do.Subscribe<IGetHomeBranches>(ReturnHomeBranches);
        }

        private void Start()
        {
            _canvasSortingOrderSettings.OnStart();
            StartCoroutine(StartUIDelay());
        }

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
                EventSystem.current.SetSelectedGameObject(GetFirstHighlightedNodeInHomeGroup());
                _inMenu = true;
            }
        }

        private GameObject GetFirstHighlightedNodeInHomeGroup()
        {
            return _homeBranches.First().DefaultStartOnThisNode.ReturnGameObject;
        }

        private IEnumerator EnableStartControls()
        {
            if(_inputScheme.ControlActivateDelay != 0)
                yield return new WaitForSeconds(Scheme.ControlActivateDelay);
            if(!_startingInGame)
                OnStart?.Invoke(this);
            SetEventSystem(GetFirstHighlightedNodeInHomeGroup());
        }
    
        private void SetLastHighlighted(IHighlightedNode args)
        {
            _lastHighlighted = args.Highlighted;
            if(_inMenu) SetEventSystem(_lastHighlighted.ReturnGameObject);
        }

        private void SwitchedToKeys(IAllowKeys args) => SetEventSystem(GetCorrectLastHighlighted());

        private GameObject GetCorrectLastHighlighted()
        {
            return _lastHighlighted is null ? GetFirstHighlightedNodeInHomeGroup() : 
                _lastHighlighted.ReturnGameObject;
        }

        private static void SetEventSystem(GameObject newGameObject)
        {
            EventSystem.current.SetSelectedGameObject(newGameObject);
        }    
    }
}