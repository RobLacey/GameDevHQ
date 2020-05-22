using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;


[System.Serializable]
public class RotateTween
{
    [InfoBox("Remeber to set your pivot point in the Inspector i.e Pivot X = 1 for left side hinge, 0 for right side etc.")]
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] public Ease _easeIn = Ease.Linear;
    [SerializeField] public Ease _easeOut = Ease.Linear;

    //Variables
    float _tweenTime;
    Ease _tweenEase;
    List<BuildSettings> _listToUse;
    List<BuildSettings> _reversedBuild = new List<BuildSettings>();
    List<BuildSettings> _buildList = new List<BuildSettings>();
    int _id;
    Action<IEnumerator> _startCoroutine;


    //Properties
    public bool UsingGlobalTime { get; set; }

    public void SetUpRotateTweens(List<BuildSettings> buildObjectsList, Action<IEnumerator> startCoroutine)
    {
        _buildList = buildObjectsList;
        _startCoroutine = startCoroutine;
        foreach (var item in _buildList)
        {
            item._element.localRotation = Quaternion.Euler(item._rotateFrom);
        }
        _reversedBuild = new List<BuildSettings>(_buildList);
        _reversedBuild.Reverse();
    }

    public void RotationTween(RotationTweenType rotationTweenType, float globalTime, bool isIn, TweenCallback tweenCallback = null)
    {
        if (rotationTweenType == RotationTweenType.NoTween) return;

        StopRunningTweens();

        if (rotationTweenType == RotationTweenType.In)
        {
            if (isIn)
            {
                RewindTweens();
                SetInTime(globalTime);
                InSettings();
                _startCoroutine.Invoke(MoveSequence(tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }

        if (rotationTweenType == RotationTweenType.Out)
        {
            if (isIn)
            {
                RewindTweens();
                SetOutTime(globalTime);
                tweenCallback.Invoke();
            }
            else
            {
                OutSettings();
                _startCoroutine.Invoke(MoveSequence(tweenCallback));
            }
        }

        if (rotationTweenType == RotationTweenType.InAndOut)
        {
            if (isIn)
            {
                SetInTime(globalTime);
                InSettings();
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

    public void StopRunningTweens()
    {
        foreach (var item in _buildList)
        {
            DOTween.Kill("rotation" + item._element.GetInstanceID());
        }
    }

    public void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item._element.localRotation = Quaternion.Euler(item._rotateFrom);
        }
    }

    public IEnumerator MoveSequence(TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;

        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (index == _listToUse.Count - 1)
                {
                    item._element.DOLocalRotate(item._targetRotation, _tweenTime)
                            .SetId("rotation" + item._element.GetInstanceID())
                            .SetEase(_tweenEase).SetAutoKill(true)
                            .Play()
                            .OnComplete(tweenCallback);

                }
                else
                {
                    item._element.DOLocalRotate(item._targetRotation, _tweenTime)
                                                .SetId("rotation" + item._element.GetInstanceID())
                                                .SetEase(_tweenEase).SetAutoKill(true)
                                                .Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    public void InSettings()
    {
        _tweenEase = _easeIn;
        _listToUse = _buildList;
        foreach (var item in _listToUse)
        {
            item._targetRotation = item._rotateToo;
        }
    }

    public void OutSettings()
    {
        _tweenEase = _easeOut;
        _listToUse = _buildList;
        foreach (var item in _listToUse)
        {
            item._targetRotation = item._rotateToo;
        }
    }

    public void InOutSettings()
    {
        _tweenEase = _easeOut;
        _listToUse = _reversedBuild;
        foreach (var item in _listToUse)
        {
            item._targetRotation = item._rotateFrom;
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
}
