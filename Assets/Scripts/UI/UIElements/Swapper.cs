using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Swapper
{
    [SerializeField] Image _mainImage;
    [SerializeField] Text _textToSwap;
    [SerializeField] Sprite _swapToImage;
    [SerializeField] string _swapToText;

    Sprite _startImage;
    string _startText;

    public void OnAwake()
    {
        if (_mainImage)
        {
            _startImage = _mainImage.sprite;
        }
        if (_textToSwap)
        {
            _startText = _textToSwap.text;
        }
    }

    public void Swap()
    {
            if (_mainImage)
            {
                _mainImage.sprite = _swapToImage;
            }

            if (_textToSwap)
            {
                _textToSwap.text = _swapToText;
            }
    }

    public void Default()
    {
        if (_mainImage)
        {
            _mainImage.sprite = _startImage;
        }

        if (_textToSwap)
        {
            _textToSwap.text = _startText;
        }

    }
}
