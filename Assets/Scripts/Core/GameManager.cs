using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] EventManager _Event_StartLevel;
    [SerializeField] EventManager _Event_SetMusicVolume;
    [SerializeField] EventManager _Event_NoHighScore;
    [SerializeField] EventManager _Event_StartFadeOut;
    [SerializeField] GameObject _backgroundScene;
    [SerializeField] ParticleSystem _speedBlur1;
    [SerializeField] ParticleSystem _speedBlur2;
    [SerializeField] float _firstPhaseDelay;
    [SerializeField] float _secondPhaseDelay;
    [SerializeField] int _mainGameScene = 3;
    [SerializeField] float _musicFadeIn = 2f;


    //Variables
    AudioSource _myAudioSource;

    public bool IsGameOver { get; set; }

    private void Awake()
    {
        if (_backgroundScene != null)
        {
            _backgroundScene.SetActive(false);
        }
        _myAudioSource = GetComponent<AudioSource>();
        IsGameOver = false;
    }

    private void OnEnable()
    {
        _Event_NoHighScore.AddListener(() => GameOver());
    }

    private void Start()
    {
        _Event_SetMusicVolume.Invoke(PlayerPrefs.GetFloat(PlayerSettings._musicSettings));
        //TODO Add Audio Mixer for SFX to set level
        //TODO Add Pause Menu to Change Music and SFX too
        if (SceneManager.GetActiveScene().buildIndex == _mainGameScene)
        {
            StartCoroutine(StartSequence());
        }
    }

    IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(_firstPhaseDelay);
        _Event_StartLevel.Invoke();
        yield return new WaitForSeconds(_secondPhaseDelay);
        StartCoroutine(FadeOutSFX());
        _backgroundScene.SetActive(true);
        var temp1 = _speedBlur1.emission;
        temp1.enabled = false;
        var temp2 = _speedBlur2.emission;
        temp2.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quit");
        }

        if (Input.GetKeyDown(KeyCode.R) && IsGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.M) && IsGameOver)
        {
            _Event_StartFadeOut.Invoke();
        }
    }

    private void GameOver()
    {
        IsGameOver = true;
    }

    IEnumerator FadeOutSFX()
    {
        float timer = 0;
        float perc = 0;
        float endvolume = _myAudioSource.volume;

        _myAudioSource.volume = 0;

        while (_myAudioSource.volume < endvolume)
        {
            timer += Time.deltaTime;
            perc = timer / _musicFadeIn;
            _myAudioSource.volume = Mathf.Lerp(endvolume, 0, perc);
            yield return null;
        }
    }
}
