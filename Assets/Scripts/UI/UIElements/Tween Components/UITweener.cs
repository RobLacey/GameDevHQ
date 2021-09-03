using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using EZ.Events;
using NaughtyAttributes;

public class UITweener : MonoBehaviour, IEndTween, IEZEventDispatcher
{
    
    [SerializeField] 
    [ReorderableList] [Label(BuildListName)]
    private List<BuildTweenData> _buildObjectsList = new List<BuildTweenData>();
    
    [SerializeField] 
    [BoxGroup("Tween Settings")] [HorizontalLine(1, EColor.Blue , order = 2)]
    private TweenScheme _scheme;
    
    [SerializeField]
    [BoxGroup("Events")] [HorizontalLine(1, EColor.Blue , order = 3)]
    private TweenEvents _tweenEvents;
    
    //Variables
    private int _counter, _effectCounter;
    private List<ITweenBase> _activeTweens;
    private readonly ITweenInspector _tweenInspector = EZInject.Class.NoParams<ITweenInspector>();

    //Properties
    public RectTransform EndTweenRect { get; private set; }
    public TweenScheme Scheme => _scheme;

    //Delegates
    private Action FinishedTweenCallback{ get; set; }
    private Action<IEndTween> EndTweenEffect { get; set; }
    private TweenTrigger CurrentUserEvent{ get; set; }
    public bool HasInAndOutTween() => !(_scheme is null) && _scheme.InAndOutTween();
    
    //Editor
    private const string BuildListName = "List Of Objects To Tween"; 

    //Main
    public void Awake()
    {
        if(_buildObjectsList.Count == 0 || _scheme is null) return;
        _activeTweens = TweenFactory.CreateTypes(_scheme);
        FetchEvents();
        
        foreach (var activeTween in _activeTweens)
        {
            _counter++;
            activeTween.SetUpTweens(_buildObjectsList, _scheme, PunchOrShakeEndEffect);
        }
    }
    
    public void FetchEvents() => EndTweenEffect = BranchEvent.Do.Fetch<IEndTween>();

    private void OnEnable()
    {
        if (_activeTweens is null) return;
        foreach (var activeTween in _activeTweens)
        {
            activeTween.ObserveEvents();
        }
    }

    private void OnDisable()
    {
        if (_activeTweens is null) return;
        foreach (var activeTween in _activeTweens)
        {
            activeTween.UnObserveEvents();
        }
    }

    private void OnValidate() => _tweenInspector.CurrentScheme(_scheme)
                                                .CurrentBuildList(_buildObjectsList)
                                                .UpdateInspector();

    public void StartInTweens(Action callBack)
    {
        _tweenEvents.InTweenEventStart?.Invoke();
        StartProcessingTweens(TweenType.In, callBack, _tweenEvents.InTweenEventEnd);
    }
    public void StartOutTweens(Action callBack)
    {
        StopAllCoroutines();
        _tweenEvents.OutTweenEventStart?.Invoke();
        StartProcessingTweens(TweenType.Out, callBack, _tweenEvents.OutTweenEventEnd);
    }
    private void StartProcessingTweens(TweenType tweenType, Action callBack, TweenTrigger userEvent)
    {
        FinishedTweenCallback = callBack;

        if (IfTweenCounterIsZero_In(userEvent)) return;

        ResetCounterAndCoroutines();
        DoTweens(tweenType, DoAtEndOfTweens(userEvent));
    }

    private bool IfTweenCounterIsZero_In(TweenTrigger userEvent)
    {
        if (_counter > 0 && _scheme) return false;
        DoAtEndOfTweens(userEvent);
        EndTweenUserEvent();
        return true;
    }

    private void ResetCounterAndCoroutines()
    {
        StopAllCoroutines();
        _effectCounter = _counter;
    }

    private void DoTweens(TweenType tweenType, TweenCallback endOfTweenActions)
    {
        foreach (var activeTween in _activeTweens)
        {
            activeTween.StartTween(tweenType, endOfTweenActions);
        }
    }
    
    private TweenCallback DoAtEndOfTweens(TweenTrigger value)
    {
        CurrentUserEvent = value;
        return EndTweenUserEvent;
    }
    
    private void EndTweenUserEvent()
    {
        _effectCounter--;
        if (_effectCounter > 0) return;
        CurrentUserEvent?.Invoke();
        FinishedTweenCallback?.Invoke();
    }
    
    private void PunchOrShakeEndEffect(BuildTweenData item)
    {
        EndTweenRect = item.Element;
        EndTweenEffect?.Invoke(this);
    }
}

