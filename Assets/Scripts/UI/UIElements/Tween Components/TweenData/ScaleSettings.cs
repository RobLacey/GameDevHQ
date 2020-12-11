using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ScaleSettings
{

    [SerializeField] [AllowNesting] [ShowIf("DoScaleTween")]
    private Vector3 _startScale;

    [SerializeField] [AllowNesting] [ShowIf("MidTween")]
    private Vector3 _fullSize = Vector3.one;

    [SerializeField] [AllowNesting] [ShowIf("DoScaleTween")] [Label("End Scale")]
    private Vector3 _endScale;

    private RectTransform _element;
    
    public bool DoScaleTween { get; set; }
    private bool MidTween { get; set; }
    public Vector3 StartScale => _startScale;
    public Vector3 MidScale => _fullSize;
    public Vector3 EndScale => _endScale;

    public void SetRectTransform(RectTransform rectTransform) => _element = rectTransform;

    public void SetUpTween(TweenStyle scaleTween)
    {
        if (scaleTween != TweenStyle.NoTween)
        {
            DoScaleTween = true;
        }
        else
        {
            DoScaleTween = false;
            var localScale = _element.localScale;
            _startScale = localScale;
            _fullSize = localScale;
            _endScale = localScale;
        }

        if (scaleTween == TweenStyle.InAndOut)
        {
             _startScale = Vector3.zero;
             _fullSize = _element.localScale;
             _endScale = Vector3.zero;
            MidTween = true;
        }
        else
        {
            if (scaleTween == TweenStyle.In)
            {
                 _startScale = Vector3.zero;
                 _fullSize = Vector3.zero;
                 _endScale = _element.localScale;
            }
            else
            {
                 _startScale = _element.localScale;
                 _fullSize = Vector3.zero;
                 _endScale = Vector3.zero;
            }
            MidTween = false;
        }
    }
}