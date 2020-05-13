using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

[Serializable]
public class UISize
{
    [SerializeField] Choose _ChangeSizeOn = Choose.None;
    [SerializeField] [AllowNesting] [ShowIf("Activate")] ScaleType _scaledType = ScaleType.ScaleDown;
    [SerializeField] [AllowNesting] [ShowIf("IsStandard")] [Range(0.01f, 0.3f)] float _scaleChangeBy = 0.05f;
    [SerializeField] [AllowNesting] [ShowIf("IsStandard")] [Range(0f, 0.3f)] float _pressedHoldTime = 0.05f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] [Range(0f, 0.3f)] float _punchTime = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] [Range(0.01f, 0.3f)] float _punchScaleByX = 0.1f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] [Range(0.01f, 0.3f)] float _punchScaleByY = 0.2f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] [Range(1f, 10f)] int _punchvibrato = 6;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] [Range(0f, 1f)] float _elastisity = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] [Range(0.01f, 0.3f)] float _shakeScaleByX = 0.1f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] [Range(0.01f, 0.3f)] float _shakeScaleByY = 0.2f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] [Range(0f, 0.3f)] float _shakeTime = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] [Range(1f, 90f)] float _shakeRandomness = 45f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] [Range(1f, 10f)] int _shakeVibrato = 6;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] bool _fadeOut = true;

    //Variables
    Transform _myTransform;
    Vector3 _startSize;
    Setting _mySettings = Setting.Size;

    //Editor Script
    #region Editor Scripts

    public bool Activate()
    {
        if (_ChangeSizeOn != Choose.None)
        {
            return true;
        }
        return false;
    }

    public bool IsStandard()
    {
        if (_ChangeSizeOn != Choose.None && _scaledType != ScaleType.Punch && _scaledType != ScaleType.Shake)
        {
            return true;
        }
        return false;
    }

    public bool IsPunch()
    {
        if (_scaledType == ScaleType.Punch && _ChangeSizeOn != Choose.None)
        {
            return true;
        }
        return false;
    }

    public bool IsShake()
    {
        if (_scaledType == ScaleType.Shake && _ChangeSizeOn != Choose.None)
        {
            return true;
        }
        return false;
    }
    #endregion

    public Action<UIEventTypes, bool, Setting> OnAwake(Transform newTransform)
    {
        _myTransform = newTransform;
        _startSize = newTransform.localScale;
        return ProcessChangeOfSize;
    }

    public Action<UIEventTypes, bool, Setting> OnDisable()
    {
        return ProcessChangeOfSize;
    }

    private void ProcessChangeOfSize(UIEventTypes uIEventTypes, bool Selected, Setting setting)
    {
        if (!((setting & _mySettings) != 0)) return;

        if (_ChangeSizeOn == Choose.Pressed) return;

        else if (_ChangeSizeOn == Choose.HighlightedAndSelected)
        {
            ProcessHighlighedAndSelected(uIEventTypes, Selected);
        }

        else if (uIEventTypes == UIEventTypes.Highlighted && _ChangeSizeOn == Choose.Highlighted)
        {
            if (_scaledType == ScaleType.Punch || _scaledType == ScaleType.Shake)
            {
                PunchOrShake();
            }
            else
            {
                SetScaleProcess();
            }
        }
        else if (uIEventTypes == UIEventTypes.Selected && _ChangeSizeOn == Choose.Selected)
        {
            if (_scaledType == ScaleType.Punch || _scaledType == ScaleType.Shake) return;
            ProcessSelected(Selected);
        }
        else if (uIEventTypes == UIEventTypes.Normal || uIEventTypes == UIEventTypes.Selected)
        {
            _myTransform.localScale = _startSize;
        }
    }

    private void ProcessSelected(bool Selected)
    {
        if (Selected)
        {
            SetScaleProcess();
        }
        else
        {
            if (_scaledType == ScaleType.ScaleUp)
            {
                ScaleDown();
            }
            else
            {
                ScaleUp();
            }
        }
    }

    private void ProcessHighlighedAndSelected(UIEventTypes uIEventTypes, bool Selected)
    {
        if (Selected)
        {
            if (uIEventTypes == UIEventTypes.Selected)
            {
                if (_scaledType == ScaleType.Punch || _scaledType == ScaleType.Shake) return;
                ProcessSelected(Selected);
            }
            else if (uIEventTypes == UIEventTypes.Highlighted)
            {
                if (_scaledType == ScaleType.Punch || _scaledType == ScaleType.Shake)
                {
                    PunchOrShake();
                }
                else
                {
                    SetScaleProcess();
                }
            }
        }
        else
        {
            if (uIEventTypes == UIEventTypes.Highlighted)
            {
                if (_scaledType == ScaleType.Punch || _scaledType == ScaleType.Shake)
                {
                    PunchOrShake();
                }
                else
                {
                    SetScaleProcess();
                }
            }

            if (uIEventTypes == UIEventTypes.Normal)
            {
                _myTransform.localScale = _startSize;
            }
        }
    }

    private void SetScaleProcess()
    {
        if (_scaledType == ScaleType.ScaleUp)
        {
            ScaleUp();
        }
        else
        {
            ScaleDown();
        }
    }

    private void ScaleUp()
    {
        float temp = _startSize.x + _scaleChangeBy;

        if (_myTransform.localScale.x >= temp) return;
        _myTransform.localScale += new Vector3(_scaleChangeBy, _scaleChangeBy, 0);
    }

    private void ScaleDown()
    {
        float temp = _startSize.x - _scaleChangeBy;

        if (_myTransform.localScale.x <= temp) return;
        _myTransform.localScale -= new Vector3(_scaleChangeBy, _scaleChangeBy, 0);
    }

    public IEnumerator PressedSequence()
    {
        if (_scaledType == ScaleType.Punch || _scaledType == ScaleType.Shake)
        {
            if (_ChangeSizeOn == Choose.Pressed || _ChangeSizeOn == Choose.Selected || _ChangeSizeOn == Choose.HighlightedAndSelected)
            {
                PunchOrShake();
            }
        }
        else if (_ChangeSizeOn == Choose.Pressed)
        {
            Vector3 difference = _myTransform.localScale - _startSize; ;
            float scaleBy = _scaleChangeBy;

            if (_scaledType == ScaleType.ScaleDown)
            {
                scaleBy = _scaleChangeBy * -1;
            }

            _myTransform.localScale += new Vector3(scaleBy, scaleBy, 0);
            yield return new WaitForSeconds(_pressedHoldTime);
            _myTransform.localScale = _startSize + difference;
        }
        else { yield return null; }
    }

    public void PunchOrShake()
    {
        if (_scaledType == ScaleType.Punch)
        {
            _myTransform.localScale = _startSize;
            Vector3 scaleBy = new Vector3(_punchScaleByX, _punchScaleByY, 0);
            _myTransform.DOPunchScale(scaleBy, _punchTime, _punchvibrato, _elastisity)
                                      .SetAutoKill(true)
                                      .Play();
        }

        if (_scaledType == ScaleType.Shake)
        {
            _myTransform.localScale = _startSize;
            Vector3 scaleBy = new Vector3(_shakeScaleByX, _shakeScaleByY, 0);
            _myTransform.DOShakeScale(_shakeTime, scaleBy, _shakeVibrato, _shakeRandomness, _fadeOut)
                                      .SetAutoKill(true)
                                      .Play();
        }
    }
}
