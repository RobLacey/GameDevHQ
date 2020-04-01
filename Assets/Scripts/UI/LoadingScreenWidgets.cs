using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenWidgets : MonoBehaviour
{
    [SerializeField] Text _loadingText;
    [SerializeField] Scrollbar _loadingBar;
    [SerializeField] Image _loadingImage;
    [SerializeField] string _loadedText;
    [SerializeField] float _rotationSpeed = 0;
    [SerializeField] bool _pulseText;
    [SerializeField] bool _pulseImage;
    [SerializeField] [Range(0, 1)] float _minAlphaAmount = 1f;
    [SerializeField] [Range(0, 8)] float _speed = 3f;
    [SerializeField] EventManager _Event_Async_Progress;

    private void OnEnable()
    {
        _Event_Async_Progress.AddListener((value) => LoadOperation(value), this);
    }

    private void Update()
    {
        LoadingText();
        LoadingImage();
    }

    private void LoadingText()
    {
        if (_loadingText != null && _pulseText)
        {
            _loadingText.color = _loadingText.color.PulseAlpha(_minAlphaAmount, 1 - (_speed / 10f));
        }
    }

    private void LoadingImage()
    {
        if (_loadingImage != null)
        {
            if (_rotationSpeed > 0)
            {
                _loadingImage.transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
            }

            if (_pulseImage)
            {
                _loadingImage.color = _loadingImage.color.PulseAlpha(_minAlphaAmount, _speed);
            }
        }
    }


    private void LoadOperation(object value)
    {
        float loadingProgress = (float)value;

        if (_loadingBar != null)
        {
            if (loadingProgress < 0.98f)
            {
                _loadingBar.size = loadingProgress;
            }
            else
            {
                _loadingBar.size = 1f;
            }
        }

        if (_loadingText != null && loadingProgress > 0.98f)
        {
            _loadingText.text = _loadedText;
        }
    }
}
