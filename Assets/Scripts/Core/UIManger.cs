using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class UIManger : MonoBehaviour
{
    [SerializeField] Sprite[] _livesSprites = default;
    [SerializeField] Image _livesUI = default;
    [SerializeField] GameObject _gameOverUI = default;
    [SerializeField] GameObject _resetKeyUI = default;
    [SerializeField] float _onMin = default;
    [SerializeField] float _onMax = default;
    [SerializeField] float _offMin = default;
    [SerializeField] float _offMax = default;
    [SerializeField] int _flashed = default;
    [SerializeField] PowerUpUI[] _powerUpUI;
    [SerializeField] GameObject[] _countdownUI = default;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_SetLives = default;
    [SerializeField] EventManager _Event_ActivatePowerUp = default;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_StartSpawning;
    [SerializeField] EventManager _Event_StartLevel;
    [SerializeField] EventManager _Event_DefaultWeapon;


    [Serializable]
    public class PowerUpUI
    {
        [SerializeField] public PowerUpTypes powerUpTypes;
        [SerializeField] public GameObject _UI;
    }

    private void OnEnable()
    {
        _Event_PlayerDead.AddListener(() => GameOver());
        _Event_SetLives.AddListener(y => SetLivesDisplay(y));
        _Event_ActivatePowerUp.AddListener(x => ActivatePowerUPUI(x));
        _Event_DeactivatePowerUp.AddListener(x => DeactviatePowerUPUI(x));
        _Event_StartLevel.AddListener(() => StartLevel());
        _Event_DefaultWeapon.AddListener(x => ActivatePowerUPUI(x));
    }

    private void Start()
    {
        SetUpAllUI();
        ActivatePowerUPUI(PowerUpTypes.SingleShot);
        _gameOverUI.SetActive(false);
        _resetKeyUI.SetActive(false);
    }

    private void SetUpAllUI()
    {
        foreach (var item in _powerUpUI)
        {
            item._UI.SetActive(false);
        }
    }

    private void StartLevel()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        int index = _countdownUI.Length - 1;
        _countdownUI[index].SetActive(true);
        yield return new WaitForSeconds(1);
        _countdownUI[index].SetActive(false);
        index--;
        _countdownUI[index].SetActive(true);
        yield return new WaitForSeconds(1);
        _countdownUI[index].SetActive(false);
        index--;
        _countdownUI[index].SetActive(true);
        yield return new WaitForSeconds(1);
        _countdownUI[index].SetActive(false);
        index--;
        _countdownUI[index].SetActive(true);
        yield return new WaitForSeconds(1);
        _countdownUI[index].SetActive(false);
        _Event_StartSpawning.Invoke();
    }

    public void SetLivesDisplay(object lives)
    {
        _livesUI.sprite = _livesSprites[(int)lives];
    }

    private void ActivatePowerUPUI(object type) //TODO simplify
    {
        PowerUpTypes newPowerUp = (PowerUpTypes)type;
        switch (newPowerUp)
        {
            case PowerUpTypes.SingleShot:
                FindPowerUp(PowerUpTypes.SingleShot).SetActive(true);
                FindPowerUp(PowerUpTypes.TripleShot).SetActive(false);
                FindPowerUp(PowerUpTypes.SideShot).SetActive(false);
                FindPowerUp(PowerUpTypes.HomingMissle).SetActive(false);
                break;
            case PowerUpTypes.TripleShot:
                FindPowerUp(newPowerUp).SetActive(true);
                FindPowerUp(PowerUpTypes.SingleShot).SetActive(false);
                FindPowerUp(PowerUpTypes.SideShot).SetActive(false);
                FindPowerUp(PowerUpTypes.HomingMissle).SetActive(false);
                break;
            case PowerUpTypes.SideShot:
                FindPowerUp(newPowerUp).SetActive(true);
                FindPowerUp(PowerUpTypes.SingleShot).SetActive(false);
                FindPowerUp(PowerUpTypes.TripleShot).SetActive(false);
                FindPowerUp(PowerUpTypes.HomingMissle).SetActive(false);
                break;
            case PowerUpTypes.HomingMissle:
                FindPowerUp(newPowerUp).SetActive(true);
                FindPowerUp(PowerUpTypes.SingleShot).SetActive(false);
                FindPowerUp(PowerUpTypes.TripleShot).SetActive(false);
                FindPowerUp(PowerUpTypes.SideShot).SetActive(false);
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

    public void GameOver()
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
