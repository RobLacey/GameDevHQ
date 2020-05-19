using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class PunchTweener
{
    [SerializeField]
    [InfoBox("DOESN'T use Gloabal Tween Time. Changing settings DOESN'T work in RUNTIME")]
    [AllowNesting] [DisableIf("_running")] EffectType _punchWhen = EffectType.In;
    [SerializeField] [AllowNesting] [DisableIf("_running")] Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [AllowNesting] [DisableIf("_running")] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [AllowNesting] [DisableIf("_running")] [Range(0, 1)] float _elasticity = 0.5f;
    [SerializeField] [AllowNesting] [DisableIf("_running")] [Range(1, 10)] int _vibrato = 5;

    //Variables
    Vector3 _startscale;
    bool _running = false;
    List<Tweener> _punchTweeners = new List<Tweener>();
    List<BuildSettings> _buildList = new List<BuildSettings>();

    public bool CheckInEffectType() {  return _punchWhen == EffectType.In || _punchWhen == EffectType.Both;  }

    public bool CheckOutEffectType() { return _punchWhen == EffectType.Out || _punchWhen == EffectType.Both; }

    public void SetUpPunchTween(List<BuildSettings> buildSettings)
    {
        _running = true;
        _buildList = buildSettings;

        foreach (var item in _buildList)
        {
            _startscale = item._element.transform.localScale;
            _punchTweeners.Add(item._element.transform.DOPunchScale(_strength, _duration, _vibrato, _elasticity));
        }
    }

    public void RewindTweens()
    {
        foreach (var item in _punchTweeners)
        {
            item.Rewind();
        }
        foreach (var item in _buildList)
        {
            item._element.transform.localScale = _startscale;
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
                    _punchTweeners[index].ChangeStartValue(item._element.transform.localScale).Play().OnComplete(tweenCallback);
                }
                else
                {
                    _punchTweeners[index].ChangeStartValue(item._element.transform.localScale).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            _finished = true;
        }
        yield return null;
    }
}
