using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] Text _loadingText;
    [SerializeField] Scrollbar _loadingBar;
    [SerializeField] Image _loadingImage;
    [SerializeField] string _hitSpaceText;
    [SerializeField] string _loadedText;
    [SerializeField] float _rotationSpeed = 0;
    [SerializeField] bool _pulseText;
    [SerializeField] bool _pulseImage;
    [SerializeField] [Range(0,2)]  float _pulsePercentage = 2f;
    [SerializeField] bool _waitOnLoad = false;
    [SerializeField] int _nextSceneIndex = 1;
    [SerializeField] bool _isLoadingScreen = false;
    [SerializeField] EventManager _Event_StartNextScene;
    [SerializeField] EventManager _Event_StartFadeOut;
    [SerializeField] EventManager _Event_StartFadeIn;
    [SerializeField] EventManager _Event_StartLoading;

    AsyncOperation asyncOperation;
    bool _finished = false;

    private void OnEnable()
    {
        _Event_StartNextScene.AddListener(() => StartNextScene());
        _Event_StartLoading.AddListener(() => StartLoadingNextScene());
    }

    private void Start()
    {
        _Event_StartFadeIn.Invoke();
    }

    private void Update()
    {
        LoadingText();
        LoadingImage();
        if (_finished)
        {
            if (_isLoadingScreen && _waitOnLoad && Input.anyKeyDown)
            {
                _Event_StartFadeOut.Invoke();
            }
        }
    }

    private void LoadingText()
    {
        if (_loadingText != null && _pulseText)
        {
            _loadingText.color = _loadingText.color.PulseAlpha(_pulsePercentage, false);
        }
    }

    private void LoadingImage()
    {
        if (_loadingImage != null && _finished)
        {
            if (_rotationSpeed > 0)
            {
                _loadingImage.transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
            }

            if (_pulseImage)
            {
                _loadingImage.color = _loadingImage.color.PulseAlpha(_pulsePercentage, true);
            }
        }
    }


    private void StartLoadingNextScene()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        asyncOperation = SceneManager.LoadSceneAsync(_nextSceneIndex);
        asyncOperation.allowSceneActivation = false;
        if (_loadingBar != null)
        {
            while (asyncOperation.progress < 0.9f)
            {
                _loadingBar.size = asyncOperation.progress / 0.9f;
            }
            _loadingBar.size = 1f;
        }
        FinishLoadOperation();
        yield return null;
    }

    private void FinishLoadOperation()
    {
        _finished = true;

        if (_waitOnLoad)
        {
            if (_loadingText != null)
            {
                _loadingText.text = _hitSpaceText;
            }
        }
        else
        {
            _loadingText.text = _loadedText;
            _Event_StartFadeOut.Invoke();
        }
    }

    private void StartNextScene()
    {
        asyncOperation.allowSceneActivation = true;
    }
}
