using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class UITweener : MonoBehaviour
{
    [SerializeField] RectTransform[] _buildArray;
    [SerializeField] DestinationAs _useCurrentPositionAs;
    [SerializeField] Vector2 _tweenAnchorPosition;
    [SerializeField] float _inTime = 1;
    [SerializeField] float _outTime = 1;
    [SerializeField] Ease _easeIn;
    [SerializeField] Ease _easeOut;
    [SerializeField] Snapping _pixelSnapping = Snapping.False;

    RectTransform myRectTransform;
    enum DestinationAs { Start, End}
    enum Snapping { True, False }
    Vector2 _startPosition;
    Vector2 _endPosition;
    bool _snapping;
    string _inName = "In";
    string _outName = "Out";

    public bool CanITween { get; set; }

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();

        if (_pixelSnapping == Snapping.True)
        {
            _snapping = true;
        }

    }
    public Vector2 SetUpTween()
    {
        if (CanITween)
        {
            if (_useCurrentPositionAs == DestinationAs.Start)
            {
                _endPosition = _tweenAnchorPosition;
                _startPosition = myRectTransform.anchoredPosition;
            }

            if (_useCurrentPositionAs == DestinationAs.End)
            {
                _endPosition = myRectTransform.anchoredPosition;
                _startPosition = _tweenAnchorPosition;
                myRectTransform.anchoredPosition = _startPosition;
            }
        }
        return _startPosition;
    }

    public void MoveIn(TweenWhen tweenWhen, TweenCallback action = null)
    {
        if (tweenWhen == TweenWhen.InOnly)
        {
            myRectTransform.anchoredPosition = _startPosition;
        }

        if (tweenWhen == TweenWhen.InAndOut)
        {
            DOTween.Pause(_outName);
        }

        myRectTransform.DOAnchorPos(_endPosition, _inTime, _snapping)
                       .SetEase(_easeIn)
                       .SetId(_inName)
                       .OnComplete(action)
                       .Play();
    }

    public void MoveOut(TweenWhen tweenWhen, TweenCallback callback = null)
    {
        Vector2 tweenTargetPos = _startPosition;

        if (tweenWhen == TweenWhen.OutOnly)
        {
            tweenTargetPos = _endPosition;
        }

        if (tweenWhen == TweenWhen.InAndOut)
        {
            DOTween.Pause(_inName);
        }
           myRectTransform.DOAnchorPos(tweenTargetPos, _outTime, _snapping)
                                            .SetEase(_easeOut)
                                            .SetId(_outName)
                                            .OnComplete(callback)
                                            .Play();
    }

    public void KillAllTweens(TweenWhen tweenWhen)
    {
        if (tweenWhen == TweenWhen.InOnly)
        {
            DOTween.Rewind("In");
        }

        if (tweenWhen == TweenWhen.OutOnly)
        {
            DOTween.Rewind("Out");
        }
    }
}
