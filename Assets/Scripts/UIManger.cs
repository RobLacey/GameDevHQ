using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

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
    [SerializeField] PowerUpUI[] _powerUpUI;
    [SerializeField] EventManager _Event_AddToScore = default;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_SetLives = default;
    [SerializeField] EventManager _Event_ActivatePowerUp = default;
    [SerializeField] EventManager _Event_DeactivatePowerUp;


    [Serializable]
    public class PowerUpUI
    {
        [SerializeField] public PowerUpTypes powerUpTypes;
        [SerializeField] public GameObject _UI;
    }


    private void OnEnable()
    {
        _Event_AddToScore.AddListener(x => AddToScore(x));
        _Event_PlayerDead.AddListener(() => GameOver());
        _Event_SetLives.AddListener(y => SetLivesDisplay(y));
        _Event_ActivatePowerUp.AddListener(x => ActivatePowerUPUI(x));
        _Event_DeactivatePowerUp.AddListener(x => DeactviatePowerUPUI(x));
    }

    private void Start()
    {
        AddToScore(0);
        SetUpAllUI();
        ActivatePowerUPUI(PowerUpTypes.SingleShot);
        _gameOverUI.SetActive(false);
        _resetKeyUI.SetActive(false);
    }

    public void AddToScore(object points)
    {
        _score += (int)points;
        _scoreText.text = _score.ToString();
    }

    public void SetLivesDisplay(object lives)
    {
        _livesUI.sprite = _livesSprites[(int)lives];
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

    private void ActivatePowerUPUI(object type)
    {
        PowerUpTypes newPowerUp = (PowerUpTypes)type;
        switch (newPowerUp)
        {
            case PowerUpTypes.SingleShot:
                FindPowerUp(newPowerUp).SetActive(true);
                FindPowerUp(PowerUpTypes.TripleShot).SetActive(false);
                break;
            case PowerUpTypes.TripleShot:
                FindPowerUp(newPowerUp).SetActive(true);
                FindPowerUp(PowerUpTypes.SingleShot).SetActive(false);
                break;
            case PowerUpTypes.SpeedBoost:
                FindPowerUp(newPowerUp).SetActive(true);
                break;
            case PowerUpTypes.Shield:
                FindPowerUp(newPowerUp).SetActive(true);
                break;
            case PowerUpTypes.Health:
                StartCoroutine(HealthDisplay(FindPowerUp(newPowerUp)));
                break;
        }
    }

    private void DeactviatePowerUPUI(object type)
    {
        PowerUpTypes newPowerUp = (PowerUpTypes)type;
        switch (newPowerUp)
        {
            case PowerUpTypes.SpeedBoost:
                FindPowerUp(newPowerUp).SetActive(false);
                break;
            case PowerUpTypes.Shield:
                FindPowerUp(newPowerUp).SetActive(false);
                break;
        }
    }

    IEnumerator HealthDisplay(GameObject healthUI)
    {
        healthUI.SetActive(true);
        yield return new WaitForSeconds(3f);
        healthUI.SetActive(false);
    }

    private GameObject FindPowerUp(PowerUpTypes powerUpToFind)
    {
        foreach (var item in _powerUpUI)
        {
            if (powerUpToFind == item.powerUpTypes)
            {
                return item._UI;
            }
        }
        return null;
    }

    private void SetUpAllUI()
    {
        foreach (var item in _powerUpUI)
        {
            item._UI.SetActive(false);
        }
    }

}
