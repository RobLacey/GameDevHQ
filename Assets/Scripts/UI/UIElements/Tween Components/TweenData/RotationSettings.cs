using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class RotationSettings
{ 
    [SerializeField] [ReadOnly] private RectTransform _element;

    [SerializeField] [AllowNesting] [ShowIf("RotationTween")] private Vector3 _rotateFrom;
    [SerializeField] [AllowNesting] [ShowIf("MidRotation")] private Vector3 _rotateMidPoint;
    [SerializeField] [AllowNesting] [ShowIf("RotationTween")] private Vector3 _rotateToo;

    public bool RotationTween { get; set; }
    public bool MidRotation { get; set; }
    public Vector3 StartRotation => _rotateFrom;
    public Vector3 MidPoint => _rotateMidPoint;
    public Vector3 EndRotation => _rotateToo;

    public void SetRectTransform(RectTransform rectTransform) => _element = rectTransform;

    public void SetUpTween(TweenStyle rotationTween)
    {
        //Debug.Log(rotationTween);
        if (rotationTween != TweenStyle.NoTween)
        {
            RotationTween = true;
        }
        else
        {
            RotationTween = false;
        }

        if (rotationTween == TweenStyle.InAndOut)
        {
            // _rotateFrom = Vector3.zero;
            // _rotateMidPoint = Vector3.zero;
            // _rotateToo = Vector3.zero;
            MidRotation = true;
        }
        else
        {
            if (rotationTween == TweenStyle.In)
            {
                // _rotateFrom = Vector3.zero;
                // _rotateMidPoint = Vector3.zero;
                // _rotateToo = _element.localScale;
            }
            else
            {
                // _rotateFrom = _element.localScale;
                // _rotateMidPoint = Vector3.zero;
                // _rotateToo = Vector3.zero;
            }
            MidRotation = false;
        }

    }

}