using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class ShakeTweener 
{
    [SerializeField]
    [InfoBox("DOESN'T use Global Tween Time. Changing settings DOESN'T work in RUNTIME")]
    [AllowNesting] public EffectType _shakeWhen = EffectType.In;
    [SerializeField] [AllowNesting] Vector3 _strength = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] [AllowNesting] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [AllowNesting] [Range(1, 10)] int _vibrato = 10;
    [SerializeField] [AllowNesting] [Range(0, 90)] float _randomness = 45f;
    [SerializeField] [AllowNesting] bool _fadeOut = true;

    //Variables
    List<BuildTweenData> _buildList = new List<BuildTweenData>();
    private Coroutine _coroutine;

    //Properties
    private bool CheckInEffectType => _shakeWhen == EffectType.In || _shakeWhen == EffectType.Both;
    private bool CheckOutEffectType => _shakeWhen == EffectType.Out || _shakeWhen == EffectType.Both;

    public void SetUpShakeTween(List<BuildTweenData> buildSettings)
    {
        _buildList = buildSettings;

        foreach (var item in _buildList)
        {
            item._shakeStartScale = item.Element.localScale;
        }
    }

    public void DoShake(PunchShakeTween scaleTween, TweenType isIn, TweenCallback tweenCallback)
    {
        if (scaleTween != PunchShakeTween.Shake) return;
        StopRunningTweens();
        ResetForTween();

        if (isIn == TweenType.In)
        {
            RunTween(tweenCallback, CheckInEffectType);
        }
        else
        {
            RunTween(tweenCallback,CheckOutEffectType);
        }
    }

    private void RunTween(TweenCallback tweenCallback, bool checkForTween)
    {
        if (checkForTween)
        {
            StaticCoroutine.StopCoroutines(_coroutine);
            _coroutine = StaticCoroutine.StartCoroutine(ShakeSequence(tweenCallback));
        }
        else
        {
            tweenCallback?.Invoke();
        }
    }

    // ReSharper disable once IdentifierTypo
    private void StopRunningTweens()
    {
        foreach (var item in _buildList)
        {
            DOTween.Kill("shake" + item.Element.GetInstanceID());
        }
    }

    // ReSharper disable once IdentifierTypo
    private void ResetForTween()
    {
        foreach (var item in _buildList)
        {
            item.Element.localScale = item._shakeStartScale;
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
                    item.Element.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut)
                                                    .SetId("shake" + item.Element.GetInstanceID())
                                                    .SetAutoKill(true)
                                                    .Play()
                                                    .OnComplete(tweenCallback);
                }
                else
                {
                    item.Element.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut)
                                            .SetId("shake" + item.Element.GetInstanceID())
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

    public void EndEffect(RectTransform rectTransform, IsActive isIn)
    {
        if (isIn == IsActive.Yes) DoEndEffectTween(rectTransform, CheckInEffectType);
    }

    private void DoEndEffectTween(RectTransform rectTransform, bool checkForTween)
    {
        if (!checkForTween) return;
        ResetForTween();
        rectTransform.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut)
                     .SetId("shake" + rectTransform.gameObject.GetInstanceID())
                     .SetAutoKill(true)
                     .Play();
    }
}
