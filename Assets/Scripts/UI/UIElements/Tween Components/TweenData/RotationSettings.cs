using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class RotationSettings
{ 

    [SerializeField] [AllowNesting] [ShowIf("RotationTween")] private Vector3 _rotateFrom;
    [SerializeField] [AllowNesting] [ShowIf("MidRotation")] private Vector3 _rotateMidPoint;
    [SerializeField] [AllowNesting] [ShowIf("RotationTween")] private Vector3 _rotateToo;

    private RectTransform _element;

    //Properties
    public bool RotationTween { get; set; }
    public bool MidRotation { get; set; }
    public Vector3 StartRotation => _rotateFrom;
    public Vector3 MidPoint => _rotateMidPoint;
    public Vector3 EndRotation => _rotateToo;

    public void SetRectTransform(RectTransform rectTransform) => _element = rectTransform;

    public void SetUpTween(TweenStyle rotationTween)
    {
        if(_element is null) return;
        
        if (rotationTween != TweenStyle.NoTween)
        {
            RotationTween = true;
        }
        else
        {
            RotationTween = false;
             _rotateFrom = Vector3.zero;
            _rotateMidPoint = Vector3.zero;
             _rotateToo = Vector3.zero;
        }

        if (rotationTween == TweenStyle.InAndOut)
        {
            _rotateMidPoint = _element.localRotation.eulerAngles;
            MidRotation = true;
        }
        else
        {
            if (rotationTween == TweenStyle.In)
            {
                 _rotateToo = _element.localRotation.eulerAngles;
            }
            else
            {
                 _rotateFrom = _element.localRotation.eulerAngles;
            }
            MidRotation = false;
        }
    }
}