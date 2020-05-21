using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

[Serializable]
public class UISizeAndPosition
{
    [SerializeField] Choose _ChangeSizeOn = Choose.None;
    [SerializeField] [AllowNesting] [ShowIf("Activate")] ScaleType _scaledType = ScaleType.ScaleTween;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] float _punchTime = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] float _punchScaleByX = 0.1f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] float _punchScaleByY = 0.2f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] int _punchvibrato = 6;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] float _elastisity = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] float _shakeScaleByX = 0.1f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] float _shakeScaleByY = 0.2f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] float _shakeTime = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] float _shakeRandomness = 45f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] int _shakeVibrato = 6;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] bool _fadeOut = true;
    [SerializeField] [AllowNesting] [ShowIf("IsPositionTween")] Vector3 _pixelsToMoveBy;
    [SerializeField] [AllowNesting] [ShowIf("IsScaleTween")] Vector3 _scaleBy;
    [SerializeField] [AllowNesting] [ShowIf("IsTween")] float _time;
    [SerializeField] [AllowNesting] [ShowIf("IsTween")] Ease _ease;
    [SerializeField] [AllowNesting] [ShowIf("IsPositionTween")] bool _snapping;

    //Variables
    Transform _myTransform;
    RectTransform _myRect;
    Vector3 _startSize;
    Vector3 _startPosition;
    Setting _mySettings = Setting.SizeAndPosition;
    Tween _currentTween;
    int _id;


    //Editor Script
    #region Editor Scripts

    public bool Activate() { return _ChangeSizeOn != Choose.None ;  }
    public bool IsStandard() { return _ChangeSizeOn != Choose.None && _scaledType != ScaleType.ScalePunch 
                               && _scaledType != ScaleType.ScaleShake && _scaledType != ScaleType.PositionTween
                               && _scaledType != ScaleType.ScaleTween; }
    public bool IsPunch() { return _scaledType == ScaleType.ScalePunch && _ChangeSizeOn != Choose.None;  }
    public bool IsShake() {  return _scaledType == ScaleType.ScaleShake && _ChangeSizeOn != Choose.None; }
    public bool IsTween() {  return _scaledType == ScaleType.PositionTween || _scaledType == ScaleType.ScaleTween && _ChangeSizeOn != Choose.None; }
    public bool IsPositionTween() { return _scaledType == ScaleType.PositionTween && _ChangeSizeOn != Choose.None; ; }
    public bool IsScaleTween() { return _scaledType == ScaleType.ScaleTween && _ChangeSizeOn != Choose.None; ; }
    #endregion

    public Action<UIEventTypes, bool, Setting> OnAwake(Transform newTransform)
    {
        _myTransform = newTransform;
        _id = _myTransform.gameObject.GetInstanceID();
        _myRect = _myTransform.GetComponent<RectTransform>();
        _startSize = newTransform.localScale;
        _startPosition = _myRect.anchoredPosition3D;
        return HowToChangeSize;
    }

    public Action<UIEventTypes, bool, Setting> OnDisable()
    {
        return HowToChangeSize;
    }

    private void HowToChangeSize(UIEventTypes uIEventTypes, bool Selected, Setting settingsToCheck)
    {
        if (!((settingsToCheck & _mySettings) != 0)) return;

        if (_ChangeSizeOn == Choose.Pressed) return;

        else if (_ChangeSizeOn == Choose.HighlightedAndSelected)
        {
            ProcessHighlighedAndSelected(uIEventTypes, Selected);
        }
        else if (uIEventTypes == UIEventTypes.Highlighted && _ChangeSizeOn == Choose.Highlighted)
        {
            ProcessHighlighted();
        }
        else if (uIEventTypes == UIEventTypes.Selected && _ChangeSizeOn == Choose.Selected)
        {
            ProcessSelected(Selected);
        }
        else if (uIEventTypes == UIEventTypes.Normal || uIEventTypes == UIEventTypes.Selected)
        {
            if (_scaledType == ScaleType.ScalePunch || _scaledType == ScaleType.ScaleShake) return;

            if (_scaledType == ScaleType.PositionTween)
            {
               MovePositionTo(false);
            }
            else
            {
                ScaleTo(false);
            }
        }
    }

    private void ProcessHighlighedAndSelected(UIEventTypes uIEventTypes, bool Selected)
    {
        if (Selected)
        {
            if (uIEventTypes == UIEventTypes.Selected)
            {
                ProcessSelected(Selected);
            }
            else if (uIEventTypes == UIEventTypes.Highlighted)
            {
                ProcessHighlighted();
            }
        }
        else
        {
            if (uIEventTypes == UIEventTypes.Highlighted)
            {
                ProcessHighlighted();            }

            if (uIEventTypes == UIEventTypes.Normal)
            {
                if (_scaledType == ScaleType.PositionTween)
                {
                    MovePositionTo(false);
                }
                else 
                {
                    ScaleTo(false);
                }
            }
        }
    }

    private void ProcessSelected(bool Selected)
    {
        if (_scaledType == ScaleType.ScalePunch || _scaledType == ScaleType.ScaleShake) return;

        if (Selected)
        {
            if (_scaledType == ScaleType.PositionTween)
            {
                MovePositionTo(true);
            }
            else
            {
                ScaleTo(true);
            }
        }
        else
        {
            if (_scaledType == ScaleType.PositionTween)
            {
                MovePositionTo(false);
            }
            else
            {
                ScaleTo(false);
            }
        }
    }

    private void ProcessHighlighted()
    {
        if (_scaledType == ScaleType.ScalePunch || _scaledType == ScaleType.ScaleShake)
        {
            PunchOrShake();
        }
        else if (_scaledType == ScaleType.PositionTween)
        {
            MovePositionTo(true);
        }
        else
        {
            ScaleTo(true);
        }
    }

    private void MovePositionTo(bool moveOut)
    {
        DOTween.Kill("position" + _id);
        if (moveOut)
        {
            Vector3 targetPos = _startPosition + _pixelsToMoveBy;
            DoPositionTween(targetPos);
        }
        else
        {
            Vector3 targetPos = _startPosition;
            DoPositionTween(targetPos);
        }
    }

    private void ScaleTo(bool moveOut)
    {
        DOTween.Kill("scale" + _id);
        if (moveOut)
        {
            Vector3 targetPos = _startSize + _scaleBy;
            DoScaleTween(targetPos);
        }
        else
        {
            Vector3 targetPos = _startSize;
            DoScaleTween(targetPos);
        }
    }

    public IEnumerator PressedSequence(Setting settingsToCheck)
    {
        if ((settingsToCheck & _mySettings) != 0)
        {
            if (_scaledType == ScaleType.ScalePunch || _scaledType == ScaleType.ScaleShake)
            {
                if (_ChangeSizeOn == Choose.Pressed || _ChangeSizeOn == Choose.Selected || _ChangeSizeOn == Choose.HighlightedAndSelected)
                {
                    PunchOrShake();
                }
            }
            else if (_ChangeSizeOn == Choose.Pressed)
            {
                if (_scaledType == ScaleType.PositionTween)
                {
                    DoPositionTween(_startPosition + _pixelsToMoveBy);
                    yield return _currentTween.WaitForCompletion();
                    DoPositionTween(_startPosition);
                }
                if (_scaledType == ScaleType.ScaleTween)
                {
                    DoScaleTween(_startSize + _scaleBy);
                    yield return _currentTween.WaitForCompletion();
                    DoScaleTween(_startSize);
                }
            }
        }
        else { yield return null; }
    }

    private void PunchOrShake()
    {
        if (_scaledType == ScaleType.ScalePunch)
        {
            DOTween.Kill("punch" + _id);
            _myTransform.localScale = _startSize;
            Vector3 scaleBy = new Vector3(_punchScaleByX, _punchScaleByY, 0);
            _myTransform.DOPunchScale(scaleBy, _punchTime, _punchvibrato, _elastisity)
                                      .SetId("punch" + _id)
                                      .SetAutoKill(true)
                                      .Play();
        }

        if (_scaledType == ScaleType.ScaleShake)
        {
            DOTween.Kill("shake" + _id);
            _myTransform.localScale = _startSize;
            Vector3 scaleBy = new Vector3(_shakeScaleByX, _shakeScaleByY, 0);
            _myTransform.DOShakeScale(_shakeTime, scaleBy, _shakeVibrato, _shakeRandomness, _fadeOut)
                                      .SetId("shake" + _id)
                                      .SetAutoKill(true)
                                      .Play();
        }
    }

    private void DoPositionTween (Vector3 target)
    {
        _myRect.DOAnchorPos3D(target, _time, _snapping)
                                .SetId("position" + _id)
                                .SetEase(_ease)
                                .SetAutoKill(true)
                                .Play();
    }

    private void DoScaleTween (Vector3 target)
    {
        _myRect.DOScale(target, _time)
                                .SetId("scale" + _id)
                                .SetEase(_ease)
                                .SetAutoKill(true)
                                .Play();
    }
}
