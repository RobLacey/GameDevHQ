using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

[Serializable]
public class PositionTween
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] Ease _easeIn = Ease.Linear;
    [SerializeField] Ease _easeOut = Ease.Linear;
    [SerializeField] [AllowNesting] bool _pixelSnapping;

    //Variables
    private float _tweenTime;
    private Ease _tweenEase;
    private List<TweenSettings> _listToUse;
    private List<TweenSettings> _reversedBuild = new List<TweenSettings>();
    private List<TweenSettings> _buildList = new List<TweenSettings>();

    //Delegates
    private Action<RectTransform> _endEffectTrigger;
    private Coroutine _coroutine;
    private TweenCallback _callback;

    //Properties
    public bool UsingGlobalTime { get; set; }

    // ReSharper disable once IdentifierTypo
    public void SetUpPositionTweens(List<TweenSettings> buildObjectsList, Action<RectTransform> effectCall)
    {
        _endEffectTrigger = effectCall;
        _buildList = buildObjectsList;
        foreach (var uIObject in _buildList)
        {
            uIObject._element.anchoredPosition3D = uIObject._tweenStartPosition;
        }
        _reversedBuild = new List<TweenSettings>(_buildList);
        _reversedBuild.Reverse();
    }

    public void DoPositionTween(PositionTweenType positionTween, float tweenTime, 
                                TweenType isIn, TweenCallback tweenCallback)
    {
        if (positionTween == PositionTweenType.NoTween) return;
        StopRunningTweens();
        StaticCoroutine.StopCoroutines(_coroutine);
        _callback = tweenCallback;
        InTween(positionTween, isIn, tweenTime);
        OutTween(positionTween, isIn, tweenTime);
        InAndOutTween(positionTween, isIn, tweenTime);
    }

    private void InTween(PositionTweenType positionTween, TweenType isIn, float tweenTime)
    {
        if (positionTween != PositionTweenType.In) return;
        if (isIn == TweenType.In)
        {
            DoInTween(positionTween, tweenTime);
        }
        else
        {
            _callback?.Invoke();
        }
    }

    private void OutTween(PositionTweenType positionTween, TweenType isIn, float tweenTime)
    {
        if (positionTween != PositionTweenType.Out) return;
        if (isIn == TweenType.In)
        {
            ResetStartPosition();
            _callback?.Invoke();
        }
        else
        {
            DoOutTween(_buildList, tweenTime);
        }
    }

    private void InAndOutTween(PositionTweenType tweenType, TweenType isIn, float tweenTime)
    {
        if (tweenType != PositionTweenType.InAndOut) return;
        if (isIn == TweenType.In)
        {
            DoInTween(tweenType, tweenTime);
        }
        else
        {
            DoOutTween(_reversedBuild, tweenTime);
        }
    }

    private void DoInTween(PositionTweenType tweenType, float tweenTime)
    {
        ResetStartPosition();
        SetInTime(tweenTime);
        InTweenTargetSettings(tweenType);
        _coroutine = StaticCoroutine.StartCoroutine(PositionTweenSequence());
    }

    private void DoOutTween(List<TweenSettings> outList, float tweenTime)
    {
        SetOutTime(tweenTime);
        OutTweenTargetSettings(outList);
        _coroutine = StaticCoroutine.StartCoroutine(PositionTweenSequence());
    }

    public IEnumerator PositionTweenSequence()
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (index == _listToUse.Count - 1)
                {
                    Tween tween = item._element.DOAnchorPos3D(item._moveTo, _tweenTime, _pixelSnapping)
                                                .SetId("position" + item._element.GetInstanceID())
                                                .SetEase(_tweenEase).SetAutoKill(true)
                                                .Play()
                                                .OnComplete(_callback);
                    yield return tween.WaitForCompletion();
                    _endEffectTrigger?.Invoke(item._element);
                }
                else
                {
                    item._element.DOAnchorPos3D(item._moveTo, _tweenTime, _pixelSnapping)
                                                .SetId("position" + item._element.GetInstanceID())
                                                .SetEase(_tweenEase).SetAutoKill(true)
                                                .Play()
                                                .OnComplete(() => _endEffectTrigger?.Invoke(item._element));
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    private void ResetStartPosition()
    {
        foreach (var item in _buildList)
        {
            item._element.anchoredPosition3D = item._tweenStartPosition;
        }
    }

    // ReSharper disable once IdentifierTypo
    private void StopRunningTweens()
    {
        foreach (var item in _buildList)
        {
            DOTween.Kill("position" + item._element.GetInstanceID());
        }
    }

    private void InTweenTargetSettings(PositionTweenType positionTween)
    {
        _tweenEase = _easeIn;
        _listToUse = _buildList;

        if (positionTween == PositionTweenType.InAndOut)
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._moveTo = uIObject._tweenMiddlePosition;
            }
        }
        else
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._moveTo = uIObject._tweenTargetPosition;
            }
        }
    }
    private void OutTweenTargetSettings(List<TweenSettings> outList)
    {
        _tweenEase = _easeOut;
        _listToUse = outList;

        foreach (var uIObject in _listToUse)
        {
            uIObject._moveTo = uIObject._tweenTargetPosition;
        }
    }

    private void SetInTime(float globalTime) => _tweenTime = globalTime > 0 ? globalTime : _inTime;

    private void SetOutTime(float globalTime) => _tweenTime = globalTime > 0 ? globalTime : _outTime;
}
