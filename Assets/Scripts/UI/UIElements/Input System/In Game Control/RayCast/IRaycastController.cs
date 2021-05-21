using EZ.Inject;
using UnityEngine;
using UnityEngine.UI;

public interface IRaycastController : IParameters
{
    LayerMask LayerToHit { get; }
    float LaserLength { get; }
}