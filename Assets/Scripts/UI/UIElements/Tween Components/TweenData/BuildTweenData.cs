using System;
using UnityEngine;

[Serializable]
public class BuildTweenData
{
    [SerializeField] private RectTransform _element;
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
    public RectTransform Element => _element;
    public CanvasGroup MyCanvasGroup { get; private set; }

    public void SetElement()
    {
        if (_element == null)
        {
            //_positionSettings.ClearPositionTween();
            MyCanvasGroup = null;
            _positionSettings.SetRectTransform(null);
            _scaleSettings.SetRectTransform(null);
            _rotationSettings.SetRectTransform(null);
        }
        else
        {
            MyCanvasGroup = SetCanvasGroup(_element);
            _positionSettings.SetRectTransform(_element);
            _scaleSettings.SetRectTransform(_element);
            _rotationSettings.SetRectTransform(_element);
        }
    }

    public void ActivateTweenSettings(TweenScheme scheme)
    {
        _positionSettings.SetUpTween(scheme.PositionTween);
        _rotationSettings.SetUpTween(scheme.RotationTween);
        _scaleSettings.SetUpTween(scheme.ScaleTween);
    }
    
    public void ClearSettings(TweenStyle clear)
    {
        _positionSettings.SetUpTween(clear);
        _rotationSettings.SetUpTween(clear);
        _scaleSettings.SetUpTween(clear);
    }

    private CanvasGroup SetCanvasGroup(RectTransform element) => element.GetComponent<CanvasGroup>();
}