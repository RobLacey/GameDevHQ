using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class BuildSettings
{
    [SerializeField] public RectTransform _element;
    [SerializeField] [AllowNesting] [ShowIf("PositionTween")] public Vector3 _tweenStartPosition;
    [SerializeField] [AllowNesting] [ShowIf("PositionTween")] [Label("End/Mid Position")] public Vector3 _tweenTargetPosition;
    [SerializeField] [AllowNesting] [ShowIf("ScaleTween")] public Vector3 _startScale;
    [SerializeField] [AllowNesting] [ShowIf("ScaleTween")] [Label("End/Mid Scale")] public Vector3 _targetScale;
    [SerializeField] [AllowNesting] [ShowIf("RotationTween")] public Vector3 _rotateFrom;
    [SerializeField] [AllowNesting] [ShowIf("RotationTween")] public Vector3 _rotateToo;
    [SerializeField] public float _buildNextAfterDelay;
    [HideInInspector] public Vector3 _moveTo;
    [HideInInspector] public Vector3 _scaleTo;
    [HideInInspector] public Vector3 _targetRotation;
    [HideInInspector] public Vector3 _punchStartscale;
    [HideInInspector] public Vector3 _shakeStartScale;

    public bool PositionTween { get; set; }
    public bool ScaleTween { get; set; }
    public bool RotationTween { get; set; }
}

