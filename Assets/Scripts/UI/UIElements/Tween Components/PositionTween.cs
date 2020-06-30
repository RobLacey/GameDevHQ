using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

[System.Serializable]
public class PositionTween
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] Ease _easeIn = Ease.Linear;
    [SerializeField] Ease _easeOut = Ease.Linear;
    [SerializeField] [AllowNesting] bool _pixelSnapping = false;

    //Variables
    float _tweenTime;
    Ease _tweenEase;
    List<TweenSettings> _listToUse;
    List<TweenSettings> _reversedBuild = new List<TweenSettings>();
    List<TweenSettings> _buildList = new List<TweenSettings>();
    int _id;
    Action<IEnumerator> _startCoroutine;
    Action<RectTransform> _effectCallback;
    PositionTweenType _tweenTypeStore;

    //Properties
    public bool UsingGlobalTime { get; set; }

    public Action SetUpPositionTweens(List<TweenSettings> buildObjectsList, 
                                    Action<IEnumerator> startCoroutine, 
                                    Action<RectTransform> effectCall)
    {
        _effectCallback = effectCall;
        _startCoroutine = startCoroutine;
        _buildList = buildObjectsList;
        foreach (var item in _buildList)
        {
            item._element.anchoredPosition3D = item._tweenStartPosition;
        }
        _reversedBuild = new List<TweenSettings>(_buildList);
        _reversedBuild.Reverse();
        return Reset;
    }

    public void DoPositionTween(PositionTweenType positionTween, float globalTime, bool isIn, TweenCallback tweenCallback = null)
    {
        _tweenTypeStore = positionTween;

        if (positionTween == PositionTweenType.NoTween) return;

        StopRunningTweens();

        if (positionTween == PositionTweenType.In)
        {
            if (isIn)
            {
                ResetStartPosition();
                SetInTime(globalTime);
                InSettings(positionTween);
                _startCoroutine.Invoke(MoveSequence(tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }

        if (positionTween == PositionTweenType.Out)
        {

            if (isIn)
            {
                ResetStartPosition();
                tweenCallback.Invoke();
            }
            else
            {
                SetOutTime(globalTime);
                OutSettings();
                _startCoroutine.Invoke(MoveSequence(tweenCallback));
            }
        }

        if (positionTween == PositionTweenType.InAndOut)
        {
            if (isIn)
            {
                ResetStartPosition();
                SetInTime(globalTime);
                InSettings(positionTween);
                _startCoroutine.Invoke(MoveSequence(tweenCallback));
            }
            else
            {
                SetOutTime(globalTime);
                InOutSettings();
                _startCoroutine.Invoke(MoveSequence(tweenCallback));
            }
        }
    }

    public IEnumerator MoveSequence(TweenCallback callback)
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
                                                .OnComplete(callback);
                    yield return tween.WaitForCompletion();
                    _effectCallback?.Invoke(item._element);
                }
                else
                {
                    item._element.DOAnchorPos3D(item._moveTo, _tweenTime, _pixelSnapping)
                                                .SetId("position" + item._element.GetInstanceID())
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

    private void ResetStartPosition()
    {
        foreach (var item in _buildList)
        {
            item._element.anchoredPosition3D = item._tweenStartPosition;
        }
    }

    private void StopRunningTweens()
    {
        foreach (var item in _buildList)
        {
            DOTween.Kill("position" + item._element.GetInstanceID());
        }
    }

    private void InSettings(PositionTweenType positionTween)
    {
        _tweenEase = _easeIn;
        _listToUse = _buildList;

        if (positionTween == PositionTweenType.InAndOut)
        {
            foreach (var item in _listToUse)
            {
                item._moveTo = item._tweenMiddlePosition;
            }
        }
        else
        {
            foreach (var item in _listToUse)
            {
                item._moveTo = item._tweenTargetPosition;
            }
        }
    }
    private void OutSettings()
    {
        _tweenEase = _easeOut;
        _listToUse = _buildList;

        foreach (var item in _listToUse)
        {
            item._moveTo = item._tweenTargetPosition;
        }
    }

    private void InOutSettings()
    {
        _tweenEase = _easeOut;
        _listToUse = _reversedBuild;

        foreach (var item in _listToUse)
        {
            item._moveTo = item._tweenTargetPosition;
        }
    }

    private void SetInTime(float globalTime) //Need to fix
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

    private void SetOutTime(float globalTime) //Need to fix
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
        if (_tweenTypeStore == PositionTweenType.In)
        {
            foreach (var item in _buildList)
            {
                item._element.anchoredPosition3D = item._tweenTargetPosition;
            }
        }
        if (_tweenTypeStore == PositionTweenType.InAndOut)
        {
            foreach (var item in _buildList)
            {
                item._element.anchoredPosition3D = item._tweenMiddlePosition;
            }
        }
    }
}
