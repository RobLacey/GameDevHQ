using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UISwapper
{
    [Header("Toggle Settings")]
    [InfoBox(" Toggle Text is OPTIONAL!!", order = 0)]
    [SerializeField] Image _toggleIsOff;
    [SerializeField] Image _toggleIsOn;
    [SerializeField] Text _toggleText;
    [SerializeField] string _changeToggleTextTo;
    [SerializeField] [Header("Swapping UI Image Settings")] Image _imageToSwap;
    [SerializeField] Sprite _swapToThisSprite;
    [SerializeField] string _swapToThisText;
    [SerializeField] [Header("Swapping UI Text Settings")] Text _textToSwap;

    //Variables
    Sprite _startImage;
    string _startText;
    string _startToggleText;
    Setting _mySetting = Setting.SwapImageOrText;

    //TODO Add Multi Select Toggle - Will need to be a new UIElement type so a lot of work

    public Action<UIEventTypes, bool, Setting> OnAwake(bool selected)
    {
        if (_imageToSwap)
        {
            _startImage = _imageToSwap.sprite;
        }
        if (_textToSwap)
        {
            _startText = _textToSwap.text;
        }

        if (_toggleText)
        {
            _startToggleText = _toggleText.text;
        }

        CycleToggle(selected, Setting.SwapImageOrText);
        return Swap;
    }

    public Action<UIEventTypes, bool, Setting> OnDisable()
    {
        return Swap;
    }

    private void Swap(UIEventTypes uIEventTypes, bool selected, Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;

        if (_imageToSwap)
        {
            if (selected)
            {
                _imageToSwap.sprite = _swapToThisSprite;
            }
            else
            {
                _imageToSwap.sprite = _startImage;
            }
        }

        if (_textToSwap)
        {
            if (selected)
            {
                _textToSwap.text = _swapToThisText;
            }
            else
            {
                _textToSwap.text = _startText;
            }
        }
    }

    public void CycleToggle(bool selected, Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;
        if (_toggleIsOff == null) return;

        if (selected)
        {
            _toggleIsOff.enabled = false;
            _toggleIsOn.enabled = true;
            if(_toggleText) _toggleText.text = _changeToggleTextTo;
        }
        else
        {
           _toggleIsOff.enabled = true;
           _toggleIsOn.enabled = false;
            if (_toggleText) _toggleText.text = _startToggleText;
        }
    }
}
