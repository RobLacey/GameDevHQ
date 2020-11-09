
using System;
using UnityEngine;

public interface ISizeAndPositionSettings : IComponentSettings
{
    SizeAndPositionScheme Scheme { get; }
    RectTransform RectTransform { get; }
}

[Serializable]
public class SizeAndPositionSettings : ISizeAndPositionSettings
{
    [SerializeField] RectTransform _rectTransforms;
    [SerializeField] private SizeAndPositionScheme _scheme;

    public SizeAndPositionScheme Scheme => _scheme;
    public RectTransform RectTransform => _rectTransforms;

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        if ((functions & Setting.SizeAndPosition) != 0 && Scheme != null)
        {
            return new UISizeAndPosition(this, uiNodeEvents);
        }
        return new NullFunction();
    }
}
