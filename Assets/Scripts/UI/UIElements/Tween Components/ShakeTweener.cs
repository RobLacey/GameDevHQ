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
    [AllowNesting] [DisableIf("_running")] EffectType _shakeWhen = EffectType.In;
    [SerializeField] [AllowNesting] [DisableIf("_running")] Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [AllowNesting] [DisableIf("_running")] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [AllowNesting] [DisableIf("_running")] [Range(1, 10)] int _vibrato = 5;
    [SerializeField] [AllowNesting] [DisableIf("_running")] [Range(0, 90)] float _randomness = 45f;
    [SerializeField] [AllowNesting] [DisableIf("_running")] bool _fadeOut;

    //Variables
    Vector3 _startscale;
    bool _running = false;

    List<Tweener> _shakeTweeners = new List<Tweener>();
    List<BuildSettings> _buildList = new List<BuildSettings>();

    public bool CheckInEffectType() { return _shakeWhen == EffectType.In || _shakeWhen == EffectType.Both; }

    public bool CheckOutEffectType() { return _shakeWhen == EffectType.Out || _shakeWhen == EffectType.Both;  }

    public void SetUpShakeTween(List<BuildSettings> buildSettings)
    {
        _running = true;
        _buildList = buildSettings;

        foreach (var item in _buildList)
        {
            _startscale = item._element.transform.localScale;
            _shakeTweeners.Add(item._element.transform.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut));
        }
    }

    public void RewindScaleTweens()
    {
        foreach (var item in _shakeTweeners)
        {
            item.Rewind();
        }
        foreach (var item in _buildList)
        {
            item._element.transform.localScale = _startscale;
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
                    _shakeTweeners[index].ChangeStartValue(item._element.transform.localScale).Play().OnComplete(tweenCallback);
                }
                else
                {
                    _shakeTweeners[index].ChangeStartValue(item._element.transform.localScale).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }
}
