using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] bool _waitOnLoad = false;
    [SerializeField] bool _isLoadingScreen = false;
    [SerializeField] float _loadingScreenLoadDelay = 3f;
    [SerializeField] int _nextSceneIndex = 1;
    [SerializeField] EventManager _Event_StartLoading;
    [SerializeField] EventManager _Event_Async_Progress;
    [SerializeField] EventManager _Event_Start_Level_Exit_Process;

    //Varibales
    AsyncOperation asyncOperation;
    bool _finished = false;
    Action Callback;
    bool _startLoad = true;

    private void OnEnable()
    {
        _Event_StartLoading.AddListener((value) => LoadFromGameScene(value), this);
        Callback = StartNextScene;
    }

    private void Update()
    {
        if (_isLoadingScreen)
        {
            if (_startLoad)
            {
                StartCoroutine(LoadScene(_nextSceneIndex));
                _startLoad = false;
            }
            if (_finished && _waitOnLoad && Input.anyKeyDown)
            {
                _Event_Start_Level_Exit_Process.Invoke(Callback, this);
                _finished = false;
            }
            else if (_finished && !_waitOnLoad)
            {
                _Event_Start_Level_Exit_Process.Invoke(Callback, this);
                _finished = false;
            }
        }    
    }

    public void LoadFromButton()
    {
        StartCoroutine(LoadScene(_nextSceneIndex));
        _Event_Start_Level_Exit_Process.Invoke(Callback, this);
    }

    public void LoadFromGameScene(object nextLevel)
    {
        int level = (int)nextLevel;
        StartCoroutine(LoadScene(level));
        _Event_Start_Level_Exit_Process.Invoke(Callback, this);
    }

    private IEnumerator LoadScene(int level)
    {
        if (_isLoadingScreen)
        {
            yield return new WaitForSeconds(_loadingScreenLoadDelay);
        }
        asyncOperation = SceneManager.LoadSceneAsync(level);
        asyncOperation.allowSceneActivation = false;

        while (asyncOperation.progress < 0.9f)
        {
            _Event_Async_Progress.Invoke(asyncOperation.progress / 0.9f, this);
        }
        _finished = true;
        yield return null;
    }


    private void StartNextScene()
    {
        StartCoroutine(StartWhenLoaded());
    }

    IEnumerator StartWhenLoaded()
    {
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }
        asyncOperation.allowSceneActivation = true;
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
