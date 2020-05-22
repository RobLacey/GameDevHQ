﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

[System.Serializable]
public class ShakeTweener 
{
    [SerializeField]
    [InfoBox("DOESN'T use Gloabal Tween Time. Changing settings DOESN'T work in RUNTIME")]
    [AllowNesting] EffectType _shakeWhen = EffectType.In;
    [SerializeField] [AllowNesting] Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [AllowNesting] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [AllowNesting] [Range(1, 10)] int _vibrato = 5;
    [SerializeField] [AllowNesting] [Range(0, 90)] float _randomness = 45f;
    [SerializeField] [AllowNesting] bool _fadeOut;

    //Variables
    List<BuildSettings> _buildList = new List<BuildSettings>();
    int _id;
    Action<IEnumerator> _startCoroutine;

    public bool CheckInEffectType() { return _shakeWhen == EffectType.In || _shakeWhen == EffectType.Both; }

    public bool CheckOutEffectType() { return _shakeWhen == EffectType.Out || _shakeWhen == EffectType.Both;  }

    public void SetUpShakeTween(List<BuildSettings> buildSettings, Action<IEnumerator> startCoroutine)
    {
        _startCoroutine = startCoroutine;
        _buildList = buildSettings;

        foreach (var item in _buildList)
        {
            item._shakeStartScale = item._element.localScale;
        }
    }

    public void DoShake(ScaleTween scaleTween, bool isIn, TweenCallback tweenCallback = null)
    {
        if (scaleTween == ScaleTween.NoTween || scaleTween != ScaleTween.Shake) return;

        StopRunningTweens();

        if (isIn)
        {
            if (CheckInEffectType())
            {
                RewindTweens();
                _startCoroutine.Invoke(ShakeSequence(tweenCallback));
            }
            else
            {
                RewindTweens();
                tweenCallback.Invoke();
            }
        }
        else
        {
            if (CheckOutEffectType())
            {
                RewindTweens();
                _startCoroutine.Invoke(ShakeSequence(tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }
    }

    private void StopRunningTweens()
    {
        foreach (var item in _buildList)
        {
            DOTween.Kill("shake" + item._element.GetInstanceID());
        }
    }

    private void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item._element.localScale = item._shakeStartScale;
        }
    }

    public IEnumerator ShakeSequence(TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _buildList)
            {
                if (index == _buildList.Count - 1)
                {
                    item._element.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut)
                                                    .SetId("shake" + item._element.GetInstanceID())
                                                    .SetAutoKill(true)
                                                    .Play()
                                                    .OnComplete(tweenCallback);
                }
                else
                {
                    item._element.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut)
                                            .SetId("shake" + item._element.GetInstanceID())
                                            .SetAutoKill(true)
                                            .Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }
}
