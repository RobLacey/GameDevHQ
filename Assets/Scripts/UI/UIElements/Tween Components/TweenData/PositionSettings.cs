using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class PositionSettings
{
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GrabStartPos")]
    private bool _grabStartPosition;
    [SerializeField] 
    [AllowNesting] [ShowIf("MiddleTween")] [OnValueChanged("GrabMidPos")]
    private bool _grabMidPosition;
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GrabEndPos")]
    private bool _grabEndPosition;
    
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GotToStart")]
    private bool _goToStartPosition;
    [SerializeField] 
    [AllowNesting] [ShowIf("MiddleTween")] [OnValueChanged("GotToMid")]
    private bool _goToMidPosition;
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GotToEnd")]
    private bool _goToEndPosition;

    [SerializeField] 
    [ShowIf("PositionTween")] [Label("Start Position")] [ReadOnly]  
    private Vector3 _tweenStartPosition;
    [SerializeField] 
    [ShowIf("MiddleTween")] [Label("Mid Position")] [ReadOnly] 
    private Vector3 _tweenMiddlePosition;
    [SerializeField] 
    [ShowIf("PositionTween")] [Label("End Position")] [ReadOnly]  
    private Vector3 _tweenTargetPosition;

    private RectTransform _element;
    
    //Properties
    public Vector3 StartPos => _tweenStartPosition;
    public Vector3 MidPos => _tweenMiddlePosition;
    public Vector3 EndPos => _tweenTargetPosition;
    
    //Editor
    private bool PositionTween { get; set; }
    private bool MiddleTween { get; set; }
    
    
    public void SetRectTransform(RectTransform rectTransform) => _element = rectTransform;

    private void GrabStartPos()
    {
        if (_grabStartPosition)
        {
            Debug.Log("Start value changed");
            _tweenStartPosition = _element.anchoredPosition3D;
        }
        else
        {
            Debug.Log("Start value reset");
            _tweenStartPosition = Vector3.zero;
        }
    }
    private void GrabMidPos()
    {
        if (_grabMidPosition)
        {
            Debug.Log("Mid value changed");
            _tweenMiddlePosition = _element.anchoredPosition3D;
        }
        else
        {
            Debug.Log("Mid value reset");
            _tweenMiddlePosition = Vector3.zero;
        }

    }
    private void GrabEndPos()
    {
        if (_grabEndPosition)
        {
            Debug.Log("End value changed");
            _tweenTargetPosition = _element.anchoredPosition3D;
        }
        else
        {
            Debug.Log("End value reset");
            _tweenTargetPosition = Vector3.zero;
        }
    }

    private void GotToStart()
    {
        _element.anchoredPosition3D = _tweenStartPosition;
        _goToStartPosition = false;
    }
    private void GotToMid()
    {
        _element.anchoredPosition3D = _tweenMiddlePosition;
        _goToMidPosition = false;
    }
    private void GotToEnd()
    {
        _element.anchoredPosition3D = _tweenTargetPosition;
        _goToEndPosition = false;
    }
    
    public void SetUpTween(TweenStyle tween)
    {
        if (tween != TweenStyle.NoTween)
        {
            PositionTween = true;
        }
        else
        {
            PositionTween = false;
        }

        if (tween == TweenStyle.InAndOut)
        {
            MiddleTween = true;
        }
        else
        {
            MiddleTween = false;
        }
    }
    //TODO Not Used
    public void ClearPositionTween()
    {
        _grabStartPosition = false;
        _grabMidPosition = false;
        _grabEndPosition = false;
        _tweenStartPosition = Vector3.zero;
        _tweenMiddlePosition = Vector3.zero;
        _tweenTargetPosition = Vector3.zero;
    }
}

