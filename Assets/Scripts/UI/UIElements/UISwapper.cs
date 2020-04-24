using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UISwapper
{
    [SerializeField] Image _mainImage;
    [SerializeField] Text _textToSwap;
    [SerializeField] Sprite _swapToSprite;
    [SerializeField] string _swapToText;
    [SerializeField] Canvas[] _toggleList;

    Sprite _startImage;
    string _startText;
    int _index = 0;

    public Action<UIEventTypes, bool> OnAwake()
    {
        if (_mainImage)
        {
            _startImage = _mainImage.sprite;
        }
        if (_textToSwap)
        {
            _startText = _textToSwap.text;
        }
        if (_toggleList.Length > 0)
        {
            foreach (var item in _toggleList)
            {
                item.enabled = false;
            }
            _toggleList[_index].enabled = true;

        }
        return Swap;
    }

    public Action<UIEventTypes, bool> OnDisable()
    {
        return Swap;
    }

    public void Swap(UIEventTypes uIEventTypes, bool selected)
    {
        if (_mainImage)
        {
            if (selected)
            {
                _mainImage.sprite = _swapToSprite;
            }
            else
            {
                _mainImage.sprite = _startImage;
            }
        }

        if (_textToSwap)
        {
            if (selected)
            {
                _textToSwap.text = _swapToText;
            }
            else
            {
                _textToSwap.text = _startText;
            }
        }

    }

    public void CycleToggleList()
    {
        if (_toggleList.Length > 0)
        {
                _toggleList[_index].enabled = false;
                _index++;

                if (_index == _toggleList.Length)
                {
                    _index = 0;
                }
                _toggleList[_index].enabled = true;
        }
    }
}
