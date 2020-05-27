using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

[System.Serializable]
public class PunchTweener
{
    [SerializeField]
    [InfoBox("DOESN'T use Gloabal Tween Time. Changing settings DOESN'T work in RUNTIME")]
    [AllowNesting] public EffectType _punchWhen = EffectType.In;
    [SerializeField] [AllowNesting] Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [AllowNesting] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [AllowNesting] [Range(0, 1)] float _elasticity = 0.5f;
    [SerializeField] [AllowNesting] [Range(1, 10)] int _vibrato = 5;

    //Variables
    List<BuildSettings> _buildList = new List<BuildSettings>();
    int _id;
    Action<IEnumerator> _startCoroutine;

    public bool CheckInEffectType() {  return _punchWhen == EffectType.In || _punchWhen == EffectType.Both;  }

    public bool CheckOutEffectType() { return _punchWhen == EffectType.Out || _punchWhen == EffectType.Both; }

    public void SetUpPunchTween(List<BuildSettings> buildSettings, Action<IEnumerator> startCoroutine)
    {
        _startCoroutine = startCoroutine;
        _buildList = buildSettings;

        foreach (var item in _buildList)
        {
            item._startScale = item._element.transform.localScale;
        }
    }

    public void DoPunch(PunchShakeTween scaleTween, bool isIn, TweenCallback tweenCallback = null)
    {
        if (scaleTween == PunchShakeTween.NoTween || scaleTween != PunchShakeTween.Punch) return;

        StopRunningTweens();

        if (isIn)
        {
            if (CheckInEffectType())
            {
                RewindTweens();
                _startCoroutine.Invoke(PunchSequence(tweenCallback));
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
                _startCoroutine.Invoke(PunchSequence(tweenCallback));
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
            DOTween.Kill("position" + item._element.GetInstanceID());
        }
    }

    private void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item._element.transform.localScale = item._startScale;
        }
    }

    public IEnumerator PunchSequence(TweenCallback tweenCallback = null)
    {
        bool _finished = false;
        int index = 0;
        while (!_finished)
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
            _finished = true;
        }
        yield return null;
    }

    public void EndEffect(RectTransform rectTransform, bool isIn)
    {
        if (isIn)
        {
            if (CheckInEffectType())
            {
                Debug.Log("Punch In or InOut");

                RewindTweens();
                rectTransform.DOPunchScale(_strength, _duration, _vibrato, _elasticity)
                                        .SetId("punch" + rectTransform.gameObject.GetInstanceID())
                                        .SetAutoKill(true)
                                        .Play();
            }
        }
        else
        {
            if (CheckOutEffectType())
            {
                Debug.Log("Punch Out");

                RewindTweens();
                rectTransform.DOPunchScale(_strength, _duration, _vibrato, _elasticity)
                                        .SetId("punch" + rectTransform.gameObject.GetInstanceID())
                                        .SetAutoKill(true)
                                        .Play();
            }
        }
    }

}
