using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public interface ITweenBase
{
    void StartTween(TweenType tweenType, TweenCallback tweenCallback);

    void SetUpTweens(List<BuildTweenData> buildObjectsList,
                     TweenScheme tweenScheme,
                     Action<RectTransform> effectCall);
}

/// <summary>
/// This class is the base for the major tween classes. If it involves build lists this is the one to use
/// </summary>
// ReSharper disable IdentifierTypo
[Serializable]
public abstract class TweenBase 
{
    //Variables
    protected float _tweenTime;
    protected Ease _tweenEase;
    private Coroutine _coroutine;
    protected string _tweenName;
    protected TweenStyle _tweenStyle;
    protected TweenScheme _scheme;

    protected List<BuildTweenData> _listToUse;
    private List<BuildTweenData> _reversedBuild = new List<BuildTweenData>();
    protected List<BuildTweenData> _buildList = new List<BuildTweenData>();

    //Delegates
    private Action<RectTransform> _endEffectTrigger;
    private TweenCallback _callback;

    public virtual void SetUpTweens(List<BuildTweenData> buildObjectsList,
                                    TweenScheme tweenScheme,
                                    Action<RectTransform> effectCall)
    {
        _tweenName = GetType().Name;
        _scheme = tweenScheme;
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

    public virtual void StartTween(TweenType tweenType, TweenCallback tweenCallback)
    {
        StartTweenCommon(tweenCallback);

        switch (_tweenStyle)
        {
            case TweenStyle.In:
                InTween(tweenType);
                break;
            case TweenStyle.Out:
                OutTween(tweenType);
                break;
            case TweenStyle.InAndOut:
                InAndOutTween(tweenType);
                break;
            case TweenStyle.NoTween:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }
    
    protected void StartTweenCommon(TweenCallback tweenCallback)
    {
        StopRunningTweens();
        StaticCoroutine.StopCoroutines(_coroutine);
        _callback = tweenCallback;
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

    protected void OutTween(TweenType tweenType)
    {
        if (tweenType == TweenType.In)
        {
            RewindTweens();
            _callback?.Invoke();
        }
        else
        {
            DoOutTween(_buildList);
        }
    }

    protected void InAndOutTween(TweenType tweenType)
    {
        if (tweenType == TweenType.In)
        {
            DoInTween();
        }
        else
        {
            DoOutTween(_reversedBuild);
        }
    }

    protected virtual void DoInTween()
    {
        _listToUse = _buildList;
        InTweenTargetSettings();
        _coroutine = StaticCoroutine.StartCoroutine(TweenSequence());
    }

    protected virtual void DoOutTween(List<BuildTweenData> passedBuildList)
    {
        _listToUse = passedBuildList;
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

    protected abstract void InTweenTargetSettings();
    protected abstract void OutTweenTargetSettings();

    // protected void SetInTime() => _tweenTime = _globalTime > 0 ? _globalTime : _inTime;
    //
    // protected void SetOutTime() => _tweenTime = _globalTime > 0 ? _globalTime : _outTime;
}
