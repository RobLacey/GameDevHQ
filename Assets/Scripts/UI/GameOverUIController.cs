using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class GameOverUIController : MonoBehaviour
{
    [SerializeField] Canvas _gameOverUI = default;
    [SerializeField] Canvas _resetUI = default;
    [SerializeField] float _onMin = default;
    [SerializeField] float _onMax = default;
    [SerializeField] float _offMin = default;
    [SerializeField] float _offMax = default;
    [SerializeField] int _flashed = default;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_NoHighScore;

    private void OnEnable()
    {
        _Event_PlayerDead.AddListener(() => GameOver());
        _Event_NoHighScore.AddListener(() => NoHighScore());
    }

    private void Start()
    {
        _gameOverUI.enabled = false;
        _resetUI.enabled = false;
    }

    private void GameOver()
    {
        StartCoroutine(FlashGameOver());
    }

    IEnumerator FlashGameOver()
    {
        for (int count = 0; count < _flashed; count++)
        {
            yield return StartCoroutine(FlickerTimer());
        }
        _gameOverUI.enabled = true;
        yield return null;
    }

    IEnumerator FlickerTimer()
    {
        _gameOverUI.enabled = true;
        yield return new WaitForSeconds(Random.Range(_onMin, _onMax));
        _gameOverUI.enabled = false;
        yield return new WaitForSeconds(Random.Range(_offMin, _offMax));
    }

    private void NoHighScore()
    {
        _resetUI.enabled = true;
    }
}
