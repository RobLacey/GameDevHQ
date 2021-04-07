using System;
using UnityEngine;

[Serializable]
public class VirtualCursorSettings
{
    [SerializeField] private GameType _restrictRaycastTo = GameType.NoRestrictions;
    [SerializeField] private GameObject _virtualCursorPrefab;
    [SerializeField] private IsActive _onlyHitInGameUi = IsActive.No;
    [SerializeField] [Range(1f, 20f)] private float _cursorSpeed = 7f;
    [SerializeField] private LayerMask _inGameObjectLayersToHit;
    [SerializeField] private float _raycastLength = 1000f;

    public GameType RestrictRaycastTo => _restrictRaycastTo;

    public GameObject VirtualCursorPrefab
    {
        get => _virtualCursorPrefab;
        set => _virtualCursorPrefab = value;
    }

    public IsActive OnlyHitInGameUi
    {
        get => _onlyHitInGameUi;
        set => _onlyHitInGameUi = value;
    }

    public float CursorSpeed
    {
        get => _cursorSpeed;
        set => _cursorSpeed = value;
    }

    public LayerMask LayerToHit
    {
        get => _inGameObjectLayersToHit;
        set => _inGameObjectLayersToHit = value;
    }

    public float RaycastLength
    {
        get => _raycastLength;
        set => _raycastLength = value;
    }
}