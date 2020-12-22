using System;
using System.Diagnostics.CodeAnalysis;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class PositionSettings
{
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GrabStartPos")]
    private bool _grabStartPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf("MiddleTween")] [OnValueChanged("GrabMidPos")]
    private bool _grabMidPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GrabEndPos")]
    private bool _grabEndPosition  = default;
    
#pragma warning disable 414 
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GotToStart")]
    private bool _goToStartPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf("MiddleTween")] [OnValueChanged("GotToMid")]
    private bool _goToMidPosition  = default;
    [SerializeField] 
    [AllowNesting] [ShowIf("PositionTween")] [OnValueChanged("GotToEnd")]
    private bool _goToEndPosition  = default;
#pragma warning restore 414 

    [SerializeField] 
    [ShowIf("PositionTween")] [Label("Start Position")] [ReadOnly]  
    private Vector3 _tweenStartPosition  = default;
    [SerializeField] 
    [ShowIf("MiddleTween")] [Label("Mid Position")] [ReadOnly] 
    private Vector3 _tweenMiddlePosition  = default;
    [SerializeField] 
    [ShowIf("PositionTween")] [Label("End Position")] [ReadOnly]  
    private Vector3 _tweenTargetPosition  = default;

    private RectTransform _element;
    
    //Properties
    public Vector3 StartPos => _tweenStartPosition;
    public Vector3 MidPos => _tweenMiddlePosition;
    public Vector3 EndPos => _tweenTargetPosition;
    
    //Editor
    private bool PositionTween { get; set; }
    private bool MiddleTween { get; set; }
    
    
    public void SetRectTransform(RectTransform rectTransform) 
        => _element = rectTransform;

    private void GrabStartPos() 
        => _tweenStartPosition = _grabStartPosition ? _element.anchoredPosition3D : Vector3.zero;
    
    private void GrabMidPos() 
        => _tweenMiddlePosition = _grabMidPosition ? _element.anchoredPosition3D : Vector3.zero;
    
    private void GrabEndPos() 
        => _tweenTargetPosition = _grabEndPosition ? _element.anchoredPosition3D : Vector3.zero;
    

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
        PositionTween = tween != TweenStyle.NoTween;
        MiddleTween = tween == TweenStyle.InAndOut;
        ClearPositionTween();
    }
    //TODO Not Used
    public void ClearPositionTween()
    {
        if(PositionTween) return;
        
        _grabStartPosition = false;
        _grabMidPosition = false;
        _grabEndPosition = false;
        _tweenStartPosition = Vector3.zero;
        _tweenMiddlePosition = Vector3.zero;
        _tweenTargetPosition = Vector3.zero;
    }
}

