using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

[System.Serializable]
public class ScaleTweener 
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] Ease _easeIn = Ease.Linear;
    [SerializeField] Ease _easeOut = Ease.Linear;

    //Varibales
    float _tweenTime;
    Ease _tweenEase;
    List<TweenSettings> _listToUse;
    List<TweenSettings> _reversedBuildSettings = new List<TweenSettings>();
    List<TweenSettings> _buildList = new List<TweenSettings>();
    int _id;
    Action<IEnumerator> _startCoroutine;
    Action<RectTransform> _effectCallback;


    //Properties
    public bool UsingGlobalTime { get; set; }

    public Action SetUpScaleTweens(List<TweenSettings> buildObjectsList, 
                                 Action<IEnumerator> startCoroutine, Action<RectTransform> effectCall)
    {
        _effectCallback = effectCall;
        _startCoroutine = startCoroutine;
        _buildList = buildObjectsList;

        foreach (var item in _buildList)
        {
            item._element.transform.localScale = item._startScale;
        }
        _reversedBuildSettings = new List<TweenSettings>(_buildList);
        _reversedBuildSettings.Reverse();
        return Reset;
    }

    public void DoScaleTween(ScaleTween scaleTweenType, float globalTime, TweenType isIn, TweenCallback tweenCallback = null)
    {
        if (scaleTweenType == ScaleTween.NoTween) return;

        StopRunningTweens();

        if (scaleTweenType == ScaleTween.Scale_InOnly)
        {
            if (isIn == TweenType.In)
            {
                ResetScaleTweens();
                SetInTime(globalTime);
                InSettings();
                _startCoroutine.Invoke(ScaleSequence(tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }

        if (scaleTweenType == ScaleTween.Scale_OutOnly)
        {
            if (isIn == TweenType.In)
            {
                ResetScaleTweens();
                tweenCallback.Invoke();
            }
            else
            {
                SetOutTime(globalTime);
                OutSettings();
                _startCoroutine.Invoke(ScaleSequence(tweenCallback));
            }
        }

        if (scaleTweenType == ScaleTween.Scale_InAndOut)
        {
            if (isIn == TweenType.In)
            {
                SetInTime(globalTime);
                InSettings();
                _startCoroutine.Invoke(ScaleSequence(tweenCallback));

            }
            else
            {
                SetOutTime(globalTime);
                InOutSettings();
                _startCoroutine.Invoke(ScaleSequence(tweenCallback));
            }
        }
    }

    public IEnumerator ScaleSequence(TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (index == _listToUse.Count - 1)
                {
                    Tween tween = item._element.DOScale(item._scaleTo, _tweenTime)
                                                .SetId("scale" + item._element.GetInstanceID())
                                                .SetEase(_tweenEase).SetAutoKill(true)
                                                .Play()
                                                .OnComplete(tweenCallback);
                    yield return tween.WaitForCompletion();
                    _effectCallback?.Invoke(item._element);
                }
                else
                {
                    item._element.DOScale(item._scaleTo, _tweenTime)
                                                .SetId("scale" + item._element.GetInstanceID())
                                                .SetEase(_tweenEase).SetAutoKill(true)
                                                .Play()
                                                .OnComplete(() => _effectCallback?.Invoke(item._element));

                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    private void StopRunningTweens()
    {
        foreach (var item in _buildList)
        {
            DOTween.Kill("scale" + item._element.GetInstanceID());
        }
    }

    private void ResetScaleTweens()
    {
        foreach (var item in _buildList) 
        {
            item._element.transform.localScale = item._startScale;
        }
    }

    private void InSettings()
    {
        _tweenEase = _easeIn;
        _listToUse = _buildList;
        foreach (var item in _listToUse)
        {
            item._scaleTo = item._targetScale;
        }

    }
    private void OutSettings()
    {
        _tweenEase = _easeOut;
        _listToUse = _buildList;
        foreach (var item in _listToUse)
        {
            item._scaleTo = item._targetScale;
        }
    }
    private void InOutSettings()
    {
        _tweenEase = _easeOut;
        _listToUse = _reversedBuildSettings;

        foreach (var item in _listToUse)
        {
            item._scaleTo = item._startScale;
        }
    }

    private void SetInTime(float globalTime) 
    {
        if (globalTime > 0)
        {
            _tweenTime = globalTime;
        }
        else
        {
            _tweenTime = _inTime;
        }
    }

    private void SetOutTime(float globalTime) 
    {
        if (globalTime > 0)
        {
            _tweenTime = globalTime;
        }
        else
        {
            _tweenTime = _outTime;
        }
    }

    private void Reset()
    {
        foreach (var item in _buildList)
        {
            item._element.transform.localScale = Vector3.one;
        }
    }

}
