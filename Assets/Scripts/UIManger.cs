using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManger : MonoBehaviour
{
    [SerializeField] int _score = 0;
    [SerializeField] Text _scoreText = default;
    [SerializeField] Sprite[] _livesSprites = default;
    [SerializeField] Image _livesUI = default;
    [SerializeField] GameObject _gameOverUI = default;
    [SerializeField] float _onMin = default;
    [SerializeField] float _onMax = default;
    [SerializeField] float _offMin = default;
    [SerializeField] float _offMax = default;
    [SerializeField] int _flashed = default;
    [SerializeField] GameObject _resetKeyUI = default;

    private void Start()
    {
        AddToScore(0);
        _gameOverUI.SetActive(false);
        _resetKeyUI.SetActive(false);
    }

    public void AddToScore(int points)
    {
        _score += points;
        _scoreText.text = _score.ToString();
    }

    public void SetLivesDisplay(int lives)
    {
        _livesUI.sprite = _livesSprites[lives];
    }

    public void GameOver()//UE
    {
        StartCoroutine(FlashGameOver());
    }

    IEnumerator FlashGameOver()
    {
        for (int count = 0; count < _flashed; count++)
        {
            yield return StartCoroutine(FlickerTimer());
        }        
        _gameOverUI.SetActive(true);
        _resetKeyUI.SetActive(true);
        yield return null;
    }

    IEnumerator FlickerTimer()
    {
        _gameOverUI.SetActive(true);
        yield return new WaitForSeconds(Random.Range(_onMin, _onMax));
        _gameOverUI.SetActive(false);
        yield return new WaitForSeconds(Random.Range(_offMin, _offMax));
    }
}
