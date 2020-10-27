using UnityEngine;

public interface INodeTween
{
    void DoTween(IsActive activate);
}

public interface IPositionScaleTween
{
    RectTransform MyRect { get;}
    bool IsPressed { get; }
    Vector3 StartPosition { get; }
    Vector3 StartSize { get; }
    SizeAndPositionScheme Scheme { get; }
}

public interface IPunchShakeTween
{
    Transform MyTransform { get; }
    Vector3 StartSize { get; }
    SizeAndPositionScheme Scheme { get; }

}