using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using NaughtyAttributes;

public class UITweener : MonoBehaviour
{
    [SerializeField] [ReorderableList] List<BuildSettings> _buildSettings = new List<BuildSettings>();
    [SerializeField] float _tweenInTime = 1;
    [SerializeField] float _tweenOutTime = 1;
    [SerializeField] [BoxGroup("Punch Or Shake")] EffectType _punchOrShakeWhen = EffectType.In;
    [SerializeField] [BoxGroup("Position")] DestinationAs _currentPositionIs;
    [SerializeField] [BoxGroup("Position")] Ease _positionEaseIn = Ease.Unset;
    [SerializeField] [BoxGroup("Position")] Ease _positionEaseOut = Ease.Unset;
    [SerializeField] [BoxGroup("Position")] Snapping _pixelSnapping = Snapping.False;
    [SerializeField] [BoxGroup("Scale")] Ease _scaleEaseIn = Ease.Unset;
    [SerializeField] [BoxGroup("Scale")] Ease _scaleEaseOut = Ease.Unset;
    [SerializeField] [BoxGroup("Scale")] public Vector3 _scaleToTweenTo;
    [SerializeField] [BoxGroup("Punch")] public Vector3 _punchStrength = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] [BoxGroup("Punch")] [Range(0, 2)] float _punchDuration = 0.5f;
    [SerializeField] [BoxGroup("Punch")] [Range(0, 1)] float _elasticity = 0.5f;
    [SerializeField] [BoxGroup("Punch")] [Range(1, 10)] int _punchVibrato = 5;
    [SerializeField] [BoxGroup("Shake")] public Vector3 _shakeStrength = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] [BoxGroup("Shake")] [Range(0, 2)] float _shakeDuration = 0.5f;
    [SerializeField] [BoxGroup("Shake")] [Range(1, 10)] int _shakeVibrato = 5;
    [SerializeField] [BoxGroup("Shake")] [Range(0, 90)] float _randomness = 45f;
    [SerializeField] [BoxGroup("Shake")] bool _fadeOut;

    //Variables
    enum DestinationAs { Start, MidPointForInAndOut, End }
    enum EffectType { In, Out, Both }
    enum Snapping { True, False }
    bool _snapping;
    Vector3 _startscale;
    bool _inAndOut;
    CanvasGroup _canvasGroup;
    Tweener _canvasInTweener;
    Tweener _canvasOutTweener;
    List<Tweener> _inTweeners = new List<Tweener>();
    List<Tweener> _outTweeners = new List<Tweener>();
    List<Tweener> _scaleInTweeners = new List<Tweener>();
    List<Tweener> _scaleOutTweeners = new List<Tweener>();
    List<BuildSettings> _reversedBuildSettings = new List<BuildSettings>();

    [Serializable]
    public class BuildSettings
    {
        [SerializeField] public RectTransform _element;
        [SerializeField] public Vector2 _tweenAnchorPosition;
        [SerializeField] public float _buildNextAfterDelay;
        [HideInInspector] public Vector2 _resetStartPositionStore;
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_pixelSnapping == Snapping.True) { _snapping = true; }
    }

    public void SetUpFadeTweens(FadeTween fadeTween)
    {
        if (fadeTween == FadeTween.FadeIn || fadeTween == FadeTween.FadeInAndOut)
        {
            _canvasInTweener = _canvasGroup.DOFade(1, _tweenInTime).SetEase(Ease.Linear);
            _canvasGroup.alpha = 0;
        }
        if (fadeTween == FadeTween.FadeOut || fadeTween == FadeTween.FadeInAndOut)
        {
            _canvasOutTweener = _canvasGroup.DOFade(0, _tweenOutTime).SetEase(Ease.Linear);
            _canvasGroup.alpha = 1;
        }
    }

    public void SetUpScaleTweens(ScaleTween scaleTweenWType)
    {
        if (scaleTweenWType == ScaleTween.Scale_InOnly || scaleTweenWType == ScaleTween.Scale_InAndOut)
        {
            foreach (var item in _buildSettings)
            {
                _startscale = _scaleToTweenTo;
                _scaleInTweeners.Add(item._element.transform.DOScale(item._element.transform.localScale, _tweenInTime).SetEase(_scaleEaseIn));
                _scaleOutTweeners.Add(item._element.transform.DOScale(_scaleToTweenTo, _tweenOutTime).SetEase(_scaleEaseOut));
                item._element.transform.localScale = _scaleToTweenTo;
            }
            _inAndOut = true;
            _reversedBuildSettings = new List<BuildSettings>(_buildSettings);
            _reversedBuildSettings.Reverse();
            _scaleOutTweeners.Reverse();
        }

        if (scaleTweenWType == ScaleTween.Scale_OutOnly)
        {
            foreach (var item in _buildSettings)
            {
                _startscale = item._element.transform.localScale;
                _scaleOutTweeners.Add(item._element.transform.DOScale(_scaleToTweenTo, _tweenOutTime).SetEase(_scaleEaseOut));
            }
        }
        if (scaleTweenWType == ScaleTween.Punch)
        {
            foreach (var item in _buildSettings)
            {
                _startscale = item._element.transform.localScale;
                _scaleInTweeners.Add(item._element.transform.DOPunchScale(_punchStrength, _punchDuration, _punchVibrato, _elasticity));
            }
        }

        if (scaleTweenWType == ScaleTween.Shake)
        {
            foreach (var item in _buildSettings)
            {
                _startscale = item._element.transform.localScale;
                _scaleInTweeners.Add(item._element.transform.DOShakeScale(_shakeDuration, _shakeStrength, _shakeVibrato, _randomness, _fadeOut));
            }
        }
    }

    public void SetUpPositionTweens(PositionOutTween positioOutTween, PositionInTween positionInTween)
    {
        if (positionInTween == PositionInTween.In)
        {
            foreach (var item in _buildSettings)
            {
                if (_currentPositionIs == DestinationAs.Start)
                {
                    item._resetStartPositionStore = item._element.anchoredPosition;
                    _inTweeners.Add(item._element.DOAnchorPos(item._tweenAnchorPosition, _tweenInTime, _snapping).SetEase(_positionEaseIn));
                    _outTweeners.Add(item._element.DOAnchorPos(item._element.anchoredPosition, _tweenOutTime, _snapping).SetEase(_positionEaseOut));
                }
                else
                {
                    item._resetStartPositionStore = item._tweenAnchorPosition;
                    _inTweeners.Add(item._element.DOAnchorPos(item._element.anchoredPosition, _tweenInTime, _snapping).SetEase(_positionEaseIn));
                    _outTweeners.Add(item._element.DOAnchorPos(item._tweenAnchorPosition, _tweenOutTime, _snapping).SetEase(_positionEaseOut));
                    item._element.anchoredPosition = item._tweenAnchorPosition;
                }
            }
            _inAndOut = true;
            _reversedBuildSettings =new List<BuildSettings>(_buildSettings);
            _reversedBuildSettings.Reverse();
            _outTweeners.Reverse();
        }
        else if (positioOutTween == PositionOutTween.Out)
        {
            foreach (var item in _buildSettings)
            {
                if (_currentPositionIs == DestinationAs.Start)
                {
                    item._resetStartPositionStore = item._element.anchoredPosition;
                    _outTweeners.Add(item._element.DOAnchorPos(item._tweenAnchorPosition, _tweenOutTime, _snapping).SetEase(_positionEaseOut));
                }
                else
                {
                    item._resetStartPositionStore = item._tweenAnchorPosition;
                    _outTweeners.Add(item._element.DOAnchorPos(item._element.anchoredPosition, _tweenOutTime, _snapping).SetEase(_positionEaseOut));
                    item._element.anchoredPosition = item._tweenAnchorPosition;
                }
            }
        }
    }

    public void PauseAllTweens(List<Tweener> tweeners)
    {
        foreach (var item in tweeners)
        {
            item.Pause();
        }
    }
    public void RewindTweens(List<Tweener> tweeners)
    {
        foreach (var item in tweeners)
        {
            item.Rewind();
        }

        foreach (var item in _buildSettings)
        {
            item._element.anchoredPosition = item._resetStartPositionStore;
        }
    }

    public void RewindScaleTweens(List<Tweener> tweeners)
    {
        foreach (var item in tweeners)
        {
            item.Rewind();
        }
        foreach (var item in _buildSettings)
        {
            item._element.transform.localScale = _startscale;
        }
    }

    public void DoInTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            RewindTweens(_inTweeners);
            PauseAllTweens(_outTweeners);
            StartCoroutine(MoveSequence(_buildSettings, _inTweeners, tweenCallback));
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    public void DoOutTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            RewindTweens(_outTweeners);
        }
        else
        {
            PauseAllTweens(_inTweeners);
            if (_inAndOut)
            {
                StartCoroutine(MoveSequence(_reversedBuildSettings, _outTweeners, tweenCallback));

            }
            else
            {
                StartCoroutine(MoveSequence(_buildSettings, _outTweeners, tweenCallback));
            }
        }
    }

    public void DoPunchOrShake(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            if (_punchOrShakeWhen == EffectType.In || _punchOrShakeWhen == EffectType.Both)
            {
                RewindScaleTweens(_scaleInTweeners);
                StartCoroutine(ScaleSequence(_buildSettings, _scaleInTweeners));
            }
        }
        else
        {
            if (_punchOrShakeWhen == EffectType.Out || _punchOrShakeWhen == EffectType.Both)
            {
                RewindScaleTweens(_scaleInTweeners);
                StartCoroutine(ScaleSequence(_buildSettings, _scaleInTweeners, tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }
    }

    public void ScaleInTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            PauseAllTweens(_scaleOutTweeners);
            StartCoroutine(ScaleSequence(_buildSettings, _scaleInTweeners));
        }
        else
        {
            RewindScaleTweens(_scaleInTweeners);
            tweenCallback.Invoke();
        }
    }

    public void ScaleOutTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            RewindScaleTweens(_scaleOutTweeners);
        }
        else
        {
            PauseAllTweens(_scaleInTweeners);
            if (_inAndOut)
            {
                StartCoroutine(ScaleSequence(_reversedBuildSettings, _scaleOutTweeners, tweenCallback));
            }
            else
            {
                StartCoroutine(ScaleSequence(_buildSettings, _scaleOutTweeners, tweenCallback));
            }
        }
    }

    public void DoCanvasFade(FadeTween fadeTween, bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            if (fadeTween == FadeTween.FadeIn || fadeTween == FadeTween.FadeInAndOut)
            {
                _canvasGroup.alpha = 0;
                Debug.Log("on");

                _canvasOutTweener.Rewind();
                _canvasInTweener.Play();
            }
            else
            {
                Debug.Log("on");

                _canvasOutTweener.Rewind();
                _canvasGroup.alpha = 1;
            }
        }
        else
        {
            if (fadeTween == FadeTween.FadeOut || fadeTween == FadeTween.FadeInAndOut)
            {
                Debug.Log("off");
                _canvasGroup.alpha = 1;
                _canvasInTweener.Rewind();
                _canvasOutTweener.Play().OnComplete(tweenCallback);
            }
            else
            {
                _canvasInTweener.Rewind();
                tweenCallback.Invoke();
            }
        }
    }

    private IEnumerator MoveSequence(List<BuildSettings> buildSettings, List<Tweener> tweeners, TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in buildSettings)
            {
                if (index == buildSettings.Count - 1)
                {
                    tweeners[index].ChangeStartValue(item._element.anchoredPosition).Play().OnComplete(tweenCallback);
                }
                else
                {
                tweeners[index].ChangeStartValue(item._element.anchoredPosition).Play();
                yield return new WaitForSeconds(item._buildNextAfterDelay);
                index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    public IEnumerator ScaleSequence(List<BuildSettings> buildSettings, List<Tweener> tweeners, TweenCallback tweenCallback = null)
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
