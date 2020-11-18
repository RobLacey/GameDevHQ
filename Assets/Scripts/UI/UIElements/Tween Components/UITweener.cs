using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine.Events;

public class UITweener : MonoBehaviour
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
    [ShowIf("AddUserEvents")] [Label("Event At After Start/Mid-Point of Tween")] 
    private TweenTrigger _inTweenEvent_End;
    [SerializeField] 
    [ShowIf("AddUserEvents")] [Label("Event At End of Tween")]
    private TweenTrigger _outTweenEvent_End;

    //Variables
    private int _counter, _effectCounter;
    private List<TweenBase> _activeTweens;
    //private TweenBase _endTween;
    private bool _hasScheme;
    private TweenScheme _lastTweenScheme;

    //Delegates
    private Action _finishedTweenCallback;
    private TweenTrigger _currentUserEvent;

    //Classes
    [Serializable]
    public class TweenTrigger : UnityEvent{ }
    
    //Editor
    private bool AddUserEvents =>  _addTweenEventTriggers == IsActive.Yes;

    public void Awake() 
    {
        if(_buildObjectsList.Count == 0 || _scheme is null) return;
        _activeTweens = TweenFactory.CreateTypes(_scheme);
        //_endTween = TweenFactory.CreateEndTween(_scheme);
        
        //****** EndTween not part of Count At the moment
        
        foreach (var activeTween in _activeTweens)
        {
            _counter++;
            activeTween.SetUpTweens(_buildObjectsList, _scheme, PunchOrShakeEndEffect);
        }
    }

    private void OnValidate()
    {
        PassInSchemeToBuildObjects();
        
        if(_scheme is null && _hasScheme)
        {
            SchemeHasBeenDeleted();
            return;
        }

        if (_scheme && !_hasScheme)
        {
            SchemeHasBeenAdded();
        }
        ConfigureSettings();
    }

    private void PassInSchemeToBuildObjects()
    {
        foreach (var element in _buildObjectsList)
        {
            element.SetElement();
        }
    }

    private void SchemeHasBeenDeleted()
    {
        _hasScheme = false;
        if (_lastTweenScheme != null)
            _lastTweenScheme.Unsubscribe(ConfigureSettings);
        _lastTweenScheme = null;
        ClearTweenSettings();
    }

    private void SchemeHasBeenAdded()
    {
        _hasScheme = true;
        _lastTweenScheme = _scheme;
        _scheme.Subscribe(ConfigureSettings);
    }

    private void ConfigureSettings()
    {
        if(_scheme is null) return;
        foreach (var item in _buildObjectsList)
        {
            item.ActivateTweenSettings(_scheme);
        }
    }
    
    private void ClearTweenSettings()
    {
        foreach (var item in _buildObjectsList)
        {
            item.ClearSettings(TweenStyle.NoTween);
        }
    }
    
    public void ActivateTweens(Action callBack) 
        => StartProcessingTweens(TweenType.In, callBack, _inTweenEvent_End);

    public void DeactivateTweens(Action callBack) 
        => StartProcessingTweens(TweenType.Out, callBack, _outTweenEvent_End);

    private void StartProcessingTweens(TweenType tweenType, Action callBack, TweenTrigger userEvent)
    {
        _finishedTweenCallback = callBack;
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
        _currentUserEvent = value;
        return EndTweenUserEvent;
    }
    
    private void EndTweenUserEvent()
    {
        _effectCounter--;
        if (_effectCounter > 0) return;
        _currentUserEvent?.Invoke();
        _finishedTweenCallback?.Invoke();
    }
    
    private void PunchOrShakeEndEffect(RectTransform uiObject = null)
    {
        Debug.Log("End Effect");
       // if (_scheme.PunchTween == TweenStyle.NoTween) return;

        // if (_scheme.Punch())
        // {
        //     _endTween.StartTween();
        // }
        // switch (_scheme.PunchOrShakeTween)
        // {
        //     case PunchShakeTween.Shake:
        //         _shakeTween.EndEffect(uiObject);
        //         break;
        //     case PunchShakeTween.Punch:
        //         _punchTween.EndEffect(uiObject);
        //         break;
        // }
    }
}