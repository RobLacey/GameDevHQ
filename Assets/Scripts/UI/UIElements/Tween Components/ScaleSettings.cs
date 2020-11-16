using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ScaleSettings
{
    [SerializeField] [ReadOnly] RectTransform _element;

    [SerializeField] [AllowNesting] [ShowIf("DoScaleTween")]
    private Vector3 _startScale;

    [SerializeField] [AllowNesting] [ShowIf("MidTween")]
    private Vector3 _fullSize = Vector3.one;

    [SerializeField] [AllowNesting] [ShowIf("DoScaleTween")] [Label("End Scale")]
    private Vector3 _endScale;

    public bool DoScaleTween { get; set; }
    private bool MidTween { get; set; }

    public Vector3 StartScale => _startScale;
    public Vector3 MidScale => _fullSize;
    public Vector3 EndScale => _endScale;

    public void SetRectTransform(RectTransform rectTransform) => _element = rectTransform;

    public void SetScaleTween(ScaleTween scaleTween)
    {
        if (scaleTween != ScaleTween.NoTween)
        {
            DoScaleTween = true;
        }
        else
        {
            DoScaleTween = false;
        }

        if (scaleTween == ScaleTween.Scale_InAndOut)
        {
            // _startScale = Vector3.zero;
            // _fullSize = _element.localScale;
            // _endScale = Vector3.zero;
            MidTween = true;
        }
        else
        {
            if (scaleTween == ScaleTween.Scale_InOnly)
            {
                // _startScale = Vector3.zero;
                // _fullSize = Vector3.zero;
                // _endScale = _element.localScale;
            }
            else
            {
                // _startScale = _element.localScale;
                // _fullSize = Vector3.zero;
                // _endScale = Vector3.zero;
            }
            MidTween = false;
        }

    }
}