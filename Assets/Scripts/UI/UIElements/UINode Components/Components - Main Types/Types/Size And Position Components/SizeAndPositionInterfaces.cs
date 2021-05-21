using EZ.Inject;
using UnityEngine;

public interface INodeTween
{
    void DoTween(IsActive activate);
}

public interface IPositionScaleTween : IParameters
{
    RectTransform MyRect { get;}
    bool IsPressed { get; }
    Vector3 StartPosition { get; }
    Vector3 StartSize { get; }
    SizeAndPositionScheme Scheme { get; }
    string GameObjectID { get; }
}

public interface IPunchShakeTween : IParameters
{
    Transform MyTransform { get; }
    Vector3 StartSize { get; }
    SizeAndPositionScheme Scheme { get; }
    string GameObjectID { get; }

}