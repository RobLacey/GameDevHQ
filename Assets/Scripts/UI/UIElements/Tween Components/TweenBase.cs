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
    private float _globalTime;
    protected Ease _tweenEase;
    private Coroutine _coroutine;
    protected string _tweenName;
    protected TweenStyle _tweenType;

    protected List<BuildTweenData> _listToUse;
    private List<BuildTweenData> _reversedBuild = new List<BuildTweenData>();
    protected List<BuildTweenData> _buildList = new List<BuildTweenData>();

    //Delegates
    private Action<RectTransform> _endEffectTrigger;
    private TweenCallback _callback;

    public bool UsingGlobalTime { get; set; }

    public virtual void SetUpTweens(List<BuildTweenData> buildObjectsList,
                                     Action<RectTransform> effectCall)
    {
        _tweenName = GetType().Name;
        SetUpTweensCommon(buildObjectsList, effectCall);

    }

    protected void SetUpTweensCommon(List<BuildTweenData> buildObjectsList, 
                                     Action<RectTransform> effectCall)
    {
        _endEffectTrigger = effectCall;
        _buildList = buildObjectsList;
        _reversedBuild = new List<BuildTweenData>(_buildList);
        _reversedBuild.Reverse();
    }

    public void StartTween(Enum tweenType, float tweenTime,
                           TweenType isIn, TweenCallback tweenCallback)
    {
        _tweenType = (TweenStyle)tweenType;
        if (_tweenType == TweenStyle.NoTween) return;
        StartTweenCommon(tweenTime, tweenCallback);

        switch (_tweenType)
        {
            case TweenStyle.In:
                InTween(isIn);
                break;
            case TweenStyle.Out:
                OutTween(isIn);
                break;
            case TweenStyle.InAndOut:
                InAndOutTween(isIn);
                break;
        }

    }
    
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
            DoInTween();
        }
        else
        {
            DoOutTween(_reversedBuild);
        }
    }

    private void DoInTween()
    {
        _tweenEase = _easeIn;
        _listToUse = _buildList;
        SetInTime();
        InTweenTargetSettings();
        _coroutine = StaticCoroutine.StartCoroutine(TweenSequence());
    }

    private void DoOutTween(List<BuildTweenData> passedBuildList)
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
            DOTween.Kill($"{_tweenName}{item.Element.GetInstanceID()}");
        }
    }

    private IEnumerator TweenSequence()
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (index == _listToUse.Count - 1)
                {
                    Tween tween = DoTweenProcess(item, _callback);
                    yield return tween.WaitForCompletion();
                    _endEffectTrigger?.Invoke(item.Element);
                }
                else
                {
                    DoTweenProcess(item, EndAction(item));
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }

        yield return null;

        TweenCallback EndAction(BuildTweenData tweenSettings) => () => _endEffectTrigger?.Invoke(tweenSettings.Element);
    }

    protected abstract Tween DoTweenProcess(BuildTweenData item, TweenCallback callback);
    protected abstract void RewindTweens();

    protected abstract void OutTweenTargetSettings();
    protected abstract void InTweenTargetSettings();

    protected void SetInTime() => _tweenTime = _globalTime > 0 ? _globalTime : _inTime;

    protected void SetOutTime() => _tweenTime = _globalTime > 0 ? _globalTime : _outTime;
}
