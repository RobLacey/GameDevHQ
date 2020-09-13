using DG.Tweening;
using UnityEngine;

public interface INodeTween
{
    void DoTween(IsActive activate);
}

public interface IPositionScaleTween
{
    RectTransform MyRect { get;}
    bool IsPressed { get; }
    float Time { get; }
    bool CanLoop  { get; }
    Ease Ease { get; }
    Vector3 StartPosition { get; }
    Vector3 StartSize { get; }
    Vector3 ChangeBy { get; }
    Vector3 PixelsToMoveBy { get; }
    bool Snapping { get; }
}

public interface IPunchShakeTween
{
    bool CanLoop  { get; }
    float Time { get; }
    Transform MyTransform { get; }
    Vector3 ChangeBy { get; }
    Vector3 StartSize { get; }
    int Vibrato { get; }
    float Elasticity { get; }
    float Randomness { get; }
    bool FadeOut { get; }
}