using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UISwapper
{
    [SerializeField] [Header("Swapping UI Image Settings")] Image _imageToSwap;
    [SerializeField] Sprite _swapToThisSprite;
    [SerializeField] [Header("Swapping UI Text Settings")] Text _textToSwap;
    [SerializeField] string _swapToThisText;
    [InfoBox("Use ToggleImageIndex property to Get/Set the currentIndex. Can have as many images as needed. Use this to assign a Checkmarks, " +
     "custom toggles or make a multi select button (e.g different Button Images for Easy, Medium & Hard)")]
    [SerializeField] [Header("Toogle UI Image Settings")] Image[] _toggleImageList;

    //Variables
    Sprite _startImage;
    string _startText;
    int _currentIndex = 0;
    Setting _mySetting = Setting.Swap;

    public int ToggleImageIndex // TODO Used to set Image. NOT Tested
    { 
        get { return _currentIndex; } 
        set 
        {
            _toggleImageList[_currentIndex].enabled = false;
            _currentIndex = value;
            _toggleImageList[_currentIndex].enabled = true;
        }
    } 

    public Action<UIEventTypes, bool, Setting> OnAwake(bool selected)
    {
        if (_toggleImageList.Length == 1)
        {
            _toggleImageList[0].enabled = selected;
        }

        if (_imageToSwap)
        {
            _startImage = _imageToSwap.sprite;
        }
        if (_textToSwap)
        {
            _startText = _textToSwap.text;
        }
        if (_toggleImageList.Length > 1)
        {
            foreach (var item in _toggleImageList)
            {
                item.enabled = false;
            }
            _toggleImageList[_currentIndex].enabled = true;

        }
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

    public void CycleToggleList(bool selected, Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;

        if (_toggleImageList.Length == 1)
        {
            _toggleImageList[0].enabled = selected;
        }

        if (_toggleImageList.Length > 1)
        {
            _toggleImageList[_currentIndex].enabled = false;
            _currentIndex++;

            if (_currentIndex == _toggleImageList.Length)
            {
                _currentIndex = 0;
            }
            _toggleImageList[_currentIndex].enabled = true;
        }
    }
}
