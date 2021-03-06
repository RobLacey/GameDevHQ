﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class BuildSettings
{
    [SerializeField] public RectTransform _element;
    [SerializeField] [AllowNesting] [ShowIf("PositionTween")] [Label("Start Position")] public Vector3 _tweenStartPosition;
    [SerializeField] [AllowNesting] [ShowIf("MiddleTween")] [Label("Mid Position")] public Vector3 _tweenMiddlePosition;
    [SerializeField] [AllowNesting] [ShowIf("PositionTween")] [Label("End Position")] public Vector3 _tweenTargetPosition;
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

    private bool PositionTween { get; set; }
    private bool MiddleTween { get; set; }
    public bool ScaleTween { get; set; }
    public bool RotationTween { get; set; }

    public void SetPositionTween(PositionTweenType positionTween)
    {
        if (positionTween != PositionTweenType.NoTween)
        {
            PositionTween = true;
        }
        else
        {
            PositionTween = false;
        }

        if (positionTween == PositionTweenType.InAndOut)
        {
            MiddleTween = true;
        }
        else
        {
            MiddleTween = false;
        }
    }
}

