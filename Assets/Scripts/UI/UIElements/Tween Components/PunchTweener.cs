using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class PunchTweener
{
    [SerializeField]
    [InfoBox("DOESN'T use Global Tween Time. Changing settings DOESN'T work in RUNTIME")]
    [AllowNesting] public EffectType _punchWhen = EffectType.In;
    [SerializeField] [AllowNesting] Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [AllowNesting] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [AllowNesting] [Range(0, 1)] float _elasticity = 0.5f;
    [SerializeField] [AllowNesting] [Range(1, 10)] int _vibrato = 5;

    //Variables
    List<TweenSettings> _buildList = new List<TweenSettings>();
    private Coroutine _coroutine;

    //Properties
    public bool CheckInEffectType => _punchWhen == EffectType.In || _punchWhen == EffectType.Both;
    public bool CheckOutEffectType => _punchWhen == EffectType.Out || _punchWhen == EffectType.Both;

    public void SetUpPunchTween(List<TweenSettings> buildSettings)
    {
        _buildList = buildSettings;

        foreach (var item in _buildList)
        {
            item._punchStartScale = item._element.localScale;
        }
    }

    public void DoPunch(PunchShakeTween scaleTween, TweenType isIn, TweenCallback tweenCallback)
    {
        if (scaleTween != PunchShakeTween.Punch) return;
        StopRunningTweens();
        ResetForTweens();

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
            _coroutine = StaticCoroutine.StartCoroutine(PunchSequence(tweenCallback));
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
            DOTween.Kill("position" + item._element.GetInstanceID());
        }
    }

    // ReSharper disable once IdentifierTypo
    private void ResetForTweens()
    {
        foreach (var item in _buildList)
        {
            item._element.localScale = item._punchStartScale;
        }
    }

    public IEnumerator PunchSequence(TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _buildList)
            {
                if (index == _buildList.Count - 1)
                {
                    item._element.DOPunchScale(_strength, _duration, _vibrato, _elasticity)
                            .SetId("punch" + item._element.GetInstanceID())
                            .SetAutoKill(true)
                            .Play()
                            .OnComplete(tweenCallback);
                }
                else
                {
                    item._element.DOPunchScale(_strength, _duration, _vibrato, _elasticity)
                                                .SetId("punch" + item._element.GetInstanceID())
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
        if (isIn == IsActive.Yes) DoEndEffect(rectTransform, CheckInEffectType);
    }

    private void DoEndEffect(RectTransform rectTransform, bool checkTweenType)
    {
        if (!checkTweenType) return;
        ResetForTweens();
        rectTransform.DOPunchScale(_strength, _duration, _vibrato, _elasticity)
                     .SetId("punch" + rectTransform.gameObject.GetInstanceID())
                     .SetAutoKill(true)
                     .Play();
    }
}
