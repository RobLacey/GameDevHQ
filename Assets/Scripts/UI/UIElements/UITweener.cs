using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using NaughtyAttributes;

public class UITweener : MonoBehaviour
{
    [SerializeField] [ReorderableList] List<BuildSettings> _buildSettings = new List<BuildSettings>();
    //[SerializeField] public float _randomness = 45f;
    //[SerializeField] public float _elasticity = 0.5f;
    //[SerializeField] public int _vibrato = 5;
    [SerializeField] DestinationAs _useStartPositionsAs;
    [SerializeField] float _inTime = 1;
    [SerializeField] float _outTime = 1;
    [SerializeField] Ease _easeIn;
    [SerializeField] Ease _easeOut;
    [SerializeField] Snapping _pixelSnapping = Snapping.False;
    List<Tweener> _inTweeners = new List<Tweener>();
    List<Tweener> _outTweeners = new List<Tweener>();

    enum DestinationAs { Start, End}
    enum Snapping { True, False }
    bool _snapping;
    Vector3 _startscale;

    [Serializable]
    public class BuildSettings
    {
        [SerializeField] public RectTransform _element;
        [SerializeField] public Vector2 _tweenAnchorPosition;
        [SerializeField] public Vector3 _sizeToTweenTo;
        [HideInInspector] public Vector2 _startPositionBuild;
        [HideInInspector] public Vector2 _endPositionBuild;
        [SerializeField] public float _buildNextAfterDelay;
    }

    private void Awake()
    {
        if (_pixelSnapping == Snapping.True)    { _snapping = true; }
    }

    public void SetUpScaleTweens(ScaleTween scaleTween)
    {
        ScaleTweens(scaleTween);
    }

    public void SetUpPositionTweens(PositionTween tweenWhen)
    {
        SetStartPositions();
        PopulateMoveTweenLists(tweenWhen);
    }

    private void ScaleTweens(ScaleTween tweenWhen)
    {
        if (tweenWhen == ScaleTween.Scale_InOnly)
        {
            foreach (var item in _buildSettings)
            {
                _startscale = item._sizeToTweenTo;
                _inTweeners.Add(item._element.transform.DOScale(item._element.transform.localScale, _inTime).SetEase(_easeIn));
                item._element.transform.localScale = item._sizeToTweenTo;
            }
        }
        if (tweenWhen == ScaleTween.Scale_OutOnly)
        {
            foreach (var item in _buildSettings)
            {
                _startscale = item._element.transform.localScale;
                _outTweeners.Add(item._element.transform.DOScale(item._sizeToTweenTo, _outTime).SetEase(_easeOut));
            }
        }
        if (tweenWhen == ScaleTween.Scale_InAndOut)
        {
            foreach (var item in _buildSettings)
            {
                _inTweeners.Add(item._element.transform.DOScale(item._element.transform.localScale, _inTime).SetEase(_easeIn));
                _outTweeners.Add(item._element.transform.DOScale(item._sizeToTweenTo, _outTime).SetEase(_easeOut));
                item._element.transform.localScale = item._sizeToTweenTo;
            }
        }
    }

    private void PopulateMoveTweenLists(PositionTween tweenWhen)
    {
        if (tweenWhen == PositionTween.Build_InOnly)
        {
            foreach (var item in _buildSettings)
            {
                _inTweeners.Add(item._element.DOAnchorPos(item._endPositionBuild, _inTime, _snapping).SetEase(_easeIn));
            }
        }

        if (tweenWhen == PositionTween.Build_OutOnly)
        {
            foreach (var item in _buildSettings)
            {
                _outTweeners.Add(item._element.DOAnchorPos(item._endPositionBuild, _outTime, _snapping).SetEase(_easeOut));
            }
        }

        if (tweenWhen == PositionTween.Build_InAndOut)
        {
            foreach (var item in _buildSettings)
            {
                _inTweeners.Add(item._element.DOAnchorPos(item._endPositionBuild, _inTime, _snapping).SetEase(_easeIn));
                _outTweeners.Add(item._element.DOAnchorPos(item._startPositionBuild, _outTime, _snapping).SetEase(_easeOut));
            }
        }
    }

    private void SetStartPositions()
    {
        if (_useStartPositionsAs == DestinationAs.Start)
        {
            foreach (var item in _buildSettings)
            {
                item._startPositionBuild = item._element.anchoredPosition;
                item._endPositionBuild = item._tweenAnchorPosition;
            }
        }

        if (_useStartPositionsAs == DestinationAs.End)
        {
            foreach (var item in _buildSettings)
            {
                item._startPositionBuild = item._tweenAnchorPosition;
                item._endPositionBuild = item._element.anchoredPosition;
                item._element.anchoredPosition = item._startPositionBuild;
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
    public void RewindAllTweens(List<Tweener> tweeners)
    {
        foreach (var item in tweeners)
        {
            item.Rewind();
        }
    }

    public void ActivatePositionTweens(PositionTween currentTweenType, bool activate, TweenCallback tweenCallback = null)
    {
        switch (currentTweenType)
        {
            case PositionTween.Build_InOnly:
                if (activate)
                {
                    StartCoroutine(MoveSequence(_inTweeners, currentTweenType, tweenCallback));
                }
                else
                {
                    StopAllCoroutines();
                    RewindAllTweens(_inTweeners);
                    tweenCallback.Invoke();
                }
                break;
            case PositionTween.Build_OutOnly:
                if (activate)
                {
                    StopAllCoroutines();
                    RewindAllTweens(_outTweeners);
                }
                else
                {
                    StartCoroutine(MoveSequence(_outTweeners, currentTweenType, tweenCallback));
                }
                break;
            case PositionTween.Build_InAndOut:
                StopAllCoroutines();
                if (activate)
                {
                    PauseAllTweens(_outTweeners);
                    StartCoroutine(MoveSequence(_inTweeners, currentTweenType));
                }
                else
                {
                    PauseAllTweens(_inTweeners);
                    StartCoroutine(MoveSequence(_outTweeners, currentTweenType, tweenCallback));
                }
                break;
        }
    }
    public void ActivateScaleTweens(ScaleTween currentTweenType, bool activate, TweenCallback tweenCallback = null)
    {
        switch (currentTweenType)
        {
            case ScaleTween.Scale_InOnly:
                if (activate)
                {
                    StartCoroutine(ScaleSequence(_inTweeners, currentTweenType));
                }
                else
                {
                    StopAllCoroutines();
                    RewindAllTweens(_inTweeners);
                    tweenCallback.Invoke();
                }
                break;
            case ScaleTween.Scale_OutOnly:
                if (activate)
                {
                    StopAllCoroutines();
                    RewindAllTweens(_outTweeners);
                }
                else
                {
                    StartCoroutine(ScaleSequence(_outTweeners, currentTweenType, tweenCallback));
                }
                break;
            case ScaleTween.Scale_InAndOut:
                StopAllCoroutines();
                if (activate)
                {
                    PauseAllTweens(_outTweeners);
                    StartCoroutine(ScaleSequence(_inTweeners, currentTweenType));
                }
                else
                {
                    PauseAllTweens(_inTweeners);
                    StartCoroutine(ScaleSequence(_outTweeners, currentTweenType, tweenCallback));
                }
                break;
            default:
                break;
        }
    }

    private IEnumerator MoveSequence(List<Tweener> tweeners, PositionTween tweenWhen, TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _buildSettings)
            {
                if (tweenWhen == PositionTween.Build_InAndOut) item._startPositionBuild = item._element.anchoredPosition;

                if (index == _buildSettings.Count - 1)
                {
                    tweeners[index].ChangeStartValue(item._startPositionBuild).Play().OnComplete(tweenCallback);
                }
                else
                {
                tweeners[index].ChangeStartValue(item._startPositionBuild).Play();
                yield return new WaitForSeconds(item._buildNextAfterDelay);
                index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    public IEnumerator ScaleSequence(List<Tweener> tweeners, ScaleTween tweenWhen, TweenCallback tweenCallback = null)
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _buildSettings)
            {
                if (tweenWhen == ScaleTween.Scale_InAndOut) _startscale = item._element.transform.localScale;

                if (index == _buildSettings.Count - 1)
                {
                    tweeners[index].ChangeStartValue(_startscale).Play().OnComplete(tweenCallback);
                }
                else
                {
                    tweeners[index].ChangeStartValue(_startscale).Play();
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }

    public IEnumerator PunchOrShake() //TO Addd
    {
        foreach (var item in _buildSettings)
        {
            //item._element.transform.DOShakeScale(_inTime, _scaleAmount, _vibrato, _randomness, true)
            //item._element.transform.DOPunchScale(_scaleAmount, _outTime, _vibrato, _elasticity)

                         //.SetId(_outName)
                         //.OnComplete(tweenCallback)
                         //.Play();
        }

        yield return null;
    }
}
