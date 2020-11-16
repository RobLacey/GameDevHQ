using System;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class TweenSettings
{
    [SerializeField] [AllowNesting] /*[OnValueChanged("ReturnElement")]*/ public RectTransform _element;

    [SerializeField] public PositionSettings _positionSettings;
    [SerializeField] public ScaleSettings _scaleSettings;
    [SerializeField] public RotationSettings _rotationSettings;
    [SerializeField] public float _buildNextAfterDelay;
    [HideInInspector] public Vector3 _moveTo;
    [HideInInspector] public Vector3 _scaleTo;
    [HideInInspector] public Vector3 _targetRotation;
    [HideInInspector] public Vector3 _punchStartScale;
    [HideInInspector] public Vector3 _shakeStartScale;

    public PositionSettings PositionSettings => _positionSettings;
    public ScaleSettings ScaleSettings => _scaleSettings;
    public RotationSettings RotationSettings => _rotationSettings;

    public void ReturnElement()
    {
        if (_element == null)
        {
            //_positionSettings.ClearPositionTween();
            _positionSettings.SetRectTransform(null);
            _scaleSettings.SetRectTransform(null);
            _rotationSettings.SetRectTransform(null);
        }
        else
        {
            _positionSettings.SetRectTransform(_element);
            _scaleSettings.SetRectTransform(_element);
            _rotationSettings.SetRectTransform(_element);
        }
    }

   // public bool ScaleTween { get; set; }
    // public bool RotationTween { get; set; }
}