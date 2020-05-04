using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;


[System.Serializable]
public class ShakeTweener 
{
    [SerializeField]
    [InfoBox("DOESN'T use Gloabal Tween Time. Changing settings DOESN'T work in RUNTIME")]
    EffectType _shakeWhen = EffectType.In;
    [SerializeField] public Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [Range(1, 10)] int _vibrato = 5;
    [SerializeField] [Range(0, 90)] float _randomness = 45f;
    [SerializeField] bool _fadeOut;

    //Variables
    public List<Tweener> _shakeTweeners = new List<Tweener>();
    Vector3 _startscale;

    public bool CheckInEffectType
    {
        get
        {
            if (_shakeWhen == EffectType.In || _shakeWhen == EffectType.Both)
            {
                return true;
            }
            return false;
        }
    }

    public bool CheckOutEffectType
    {
        get
        {
            if (_shakeWhen == EffectType.Out || _shakeWhen == EffectType.Both)
            {
                return true;
            }
            return false;
        }
    }

    public void SetUpShakeTween(List<BuildSettings> buildSettings)
    {
        foreach (var item in buildSettings)
        {
            _startscale = item._element.transform.localScale;
            _shakeTweeners.Add(item._element.transform.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut));
        }
    }

    public void RewindScaleTweens(List<BuildSettings> buildSettings, List<Tweener> tweeners)
    {
        foreach (var item in tweeners)
        {
            item.Rewind();
        }
        foreach (var item in buildSettings)
        {
            item._element.transform.localScale = _startscale;
        }
    }

    public IEnumerator ShakeSequence(List<BuildSettings> buildSettings, List<Tweener> tweeners, TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in buildSettings)
            {
                if (index == buildSettings.Count - 1)
                {
                    tweeners[index].ChangeStartValue(item._element.transform.localScale).Play().OnComplete(tweenCallback);
                }
                else
                {
                    tweeners[index].ChangeStartValue(item._element.transform.localScale).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }
}
