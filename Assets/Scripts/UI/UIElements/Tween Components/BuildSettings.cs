using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildSettings
{
    [SerializeField] public RectTransform _element;
    [SerializeField] public Vector3 _tweenAnchorPosition;
    [SerializeField] public Vector3 _tweenRotateAngle;
    [SerializeField] public float _buildNextAfterDelay;
    [HideInInspector] public Vector2 _resetStartPositionStore;
    [HideInInspector] public Vector3 _resetStartRotationStore;
    [HideInInspector] public Vector3 rotateTo;
    [HideInInspector] public Vector3 rotateFrom;
}

