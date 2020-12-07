using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using NaughtyAttributes;

public interface IEndTween
{
    RectTransform EndTweenRect { get; }
    TweenScheme Scheme { get; }
}

public class UITweener : MonoBehaviour, IEndTween, IEventDispatcher
{
    [SerializeField] 
    [ReorderableList] [Label("List Of Objects To Tween")]
    private List<BuildTweenData> _buildObjectsList = new List<BuildTweenData>();
    
    [SerializeField] 
    [Expandable] 
    private TweenScheme _scheme;
    
    [Header("Event Settings")] [HorizontalLine(1, EColor.Blue , order = 3)]
    [SerializeField] 
    private IsActive _addTweenEventTriggers = IsActive.No;
    
    [SerializeField] 
    [ShowIf("UserEvents")] [BoxGroup("Tween Events")] [Label("Expand...")] 
    private TweenEvents _tweenEvents;
    
    //Variables
    private int _counter, _effectCounter;
    private List<ITweenBase> _activeTweens;
    private readonly ITweenInspector _tweenInspector = EJect.Class.NoParams<ITweenInspector>();

    public RectTransform EndTweenRect { get; private set; }
    public TweenScheme Scheme => _scheme;

    //Delegates
    private Action FinishedTweenCallback{ get; set; }
    private Action<IEndTween> EndTweenEffect { get; set; }
    private TweenTrigger CurrentUserEvent{ get; set; }

    //Editor
    private bool UserEvents =>  _addTweenEventTriggers == IsActive.Yes;
    
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
    
    public void FetchEvents() => EndTweenEffect = EVent.Do.Fetch<IEndTween>();

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
            activeTween.RemoveEvents();
        }
    }

    private void OnValidate() => _tweenInspector.CurrentScheme(_scheme)
                                                .CurrentBuildList(_buildObjectsList)
                                                .UpdateInspector();

    public void ActivateTweens(Action callBack)
    {
        if(UserEvents)
            _tweenEvents.InTweenEvent_Start?.Invoke();
        StartProcessingTweens(TweenType.In, callBack, _tweenEvents.InTweenEvent_End);
    }
    public void DeactivateTweens(Action callBack)
    {
        if(UserEvents)
            _tweenEvents.OutTweenEvent_Start?.Invoke();
        StartProcessingTweens(TweenType.Out, callBack, _tweenEvents.OutTweenEvent_End);
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
        if(UserEvents)
            CurrentUserEvent?.Invoke();
        FinishedTweenCallback?.Invoke();
    }
    
    private void PunchOrShakeEndEffect(BuildTweenData item)
    {
        EndTweenRect = item.Element;
        EndTweenEffect?.Invoke(this);
    }
}

