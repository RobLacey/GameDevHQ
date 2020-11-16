using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// This class is the base for the major tween classes. If it involves build lists this is the one to use
/// </summary>
// ReSharper disable IdentifierTypo
[Serializable]
public abstract class TweenBase 
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")]
    protected float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")]
    protected float _outTime = 1;
    [SerializeField] protected Ease _easeIn = Ease.Linear;
    [SerializeField] protected Ease _easeOut = Ease.Linear;

    //Variables
    protected float _tweenTime;
    protected float _globalTime;
    protected Ease _tweenEase;
    protected Coroutine _coroutine;
    protected string _tweenName;
    protected List<TweenSettings> _listToUse;
    protected List<TweenSettings> _reversedBuild = new List<TweenSettings>();
    protected List<TweenSettings> _buildList = new List<TweenSettings>();

    //Delegates
    protected Action<RectTransform> _endEffectTrigger;
    protected TweenCallback _callback;

    public bool UsingGlobalTime { get; set; }

    public abstract void SetUpTweens(List<TweenSettings> buildObjectsList, 
                                     Action<RectTransform> effectCall);

    protected void SetUpTweensCommon(List<TweenSettings> buildObjectsList, 
                                     Action<RectTransform> effectCall)
    {
        _endEffectTrigger = effectCall;
        _buildList = buildObjectsList;
        _reversedBuild = new List<TweenSettings>(_buildList);
        _reversedBuild.Reverse();
    }
    
    public abstract void StartTween(Enum tweenType, float tweenTime,
                                    TweenType isIn, TweenCallback tweenCallback);
    
    protected void StartTweenCommon(float tweenTime, TweenCallback tweenCallback)
    {
        StopRunningTweens();
        StaticCoroutine.StopCoroutines(_coroutine);
        _callback = tweenCallback;
        _globalTime = tweenTime;
    }
    
    protected void InTween(TweenType isIn)
    {
        if (isIn == TweenType.In)
        {
            RewindTweens();
            DoInTween();
        }
        else
        {
            _callback?.Invoke();
        }
    }

    protected void OutTween(TweenType isIn)
    {
        if (isIn == TweenType.In)
        {
            RewindTweens();
            _callback?.Invoke();
        }
        else
        {
            DoOutTween(_buildList);
        }
    }

    protected void InAndOutTween(TweenType isIn)
    {
        if (isIn == TweenType.In)
        {
            RewindTweens();
            DoInTween();
        }
        else
        {
            DoOutTween(_reversedBuild);
        }
    }

    protected virtual void DoInTween()
    {
        _tweenEase = _easeIn;
        _listToUse = _buildList;
        SetInTime();
        InTweenTargetSettings();
        _coroutine = StaticCoroutine.StartCoroutine(TweenSequence());
    }

    protected virtual void DoOutTween(List<TweenSettings> passedBuildList)
    {
        _tweenEase = _easeOut;
        _listToUse = passedBuildList;
        SetOutTime();
        OutTweenTargetSettings();
        _coroutine = StaticCoroutine.StartCoroutine(TweenSequence());

    }

    private void StopRunningTweens()
    {
        foreach (var item in _buildList)
        {
            DOTween.Kill($"{_tweenName}{item._element.GetInstanceID()}");
        }
    }

    protected abstract IEnumerator TweenSequence();
    protected abstract void RewindTweens();

    protected abstract void OutTweenTargetSettings();
    protected abstract void InTweenTargetSettings();

    protected void SetInTime() => _tweenTime = _globalTime > 0 ? _globalTime : _inTime;

    protected void SetOutTime() => _tweenTime = _globalTime > 0 ? _globalTime : _outTime;
}
