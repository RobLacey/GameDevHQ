using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class UIManger : MonoBehaviour
{
    [SerializeField] Canvas _gameOverUI = default;
    [SerializeField] Canvas _resetKeyUI = default;
    [SerializeField] float _onMin = default;
    [SerializeField] float _onMax = default;
    [SerializeField] float _offMin = default;
    [SerializeField] float _offMax = default;
    [SerializeField] int _flashed = default;
    [SerializeField] PowerUpUI[] _powerUpUI;
    [SerializeField] WeaponUI[] _weaponUI;
    [SerializeField] Text _startCountDownText = default;
    [SerializeField] Text _weaponcountDown = default;
    [SerializeField] Text _powerUpcountDown = default;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_ActivatePowerUp = default;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_StartSpawning;
    [SerializeField] EventManager _Event_StartLevel;
    [SerializeField] EventManager _Event_DefaultWeapon;
    [SerializeField] EventManager _Event_WeaponCountDownTimer;
    [SerializeField] EventManager _Event_PowerUpCountDownTimer;


    Coroutine coroutine = null;

    [Serializable]
    public class PowerUpUI
    {
        [SerializeField] public PowerUpTypes powerUpTypes;
        [SerializeField] public Canvas _UI;
    }

    [Serializable]
    public class WeaponUI
    {
        [SerializeField] public PowerUpTypes weaponTypes;
        [SerializeField] public Canvas _UI;
    }

    private void OnEnable()
    {
        _Event_PlayerDead.AddListener(() => GameOver());
        _Event_ActivatePowerUp.AddListener(x => ActivatePowerUPUI(x));
        _Event_DeactivatePowerUp.AddListener(x => DeactviatePowerUPUI(x));
        _Event_StartLevel.AddListener(() => StartLevel());
        _Event_DefaultWeapon.AddListener(x => ActivatePowerUPUI(x));
        _Event_WeaponCountDownTimer.AddListener((x) => WeaponCountdownTimer(x));
        _Event_PowerUpCountDownTimer.AddListener((x) => PowerUpCountdownTimer(x));
    }

    private void Start()
    {
        SetUpAllUI();
        ActivatePowerUPUI(PowerUpTypes.SingleShot);
        DeactviatePowerUPUI(PowerUpTypes.SingleShot);
        _gameOverUI.enabled = false;
        _resetKeyUI.enabled = false;
    }

    private void SetUpAllUI()
    {
        foreach (var item in _powerUpUI)
        {
            item._UI.enabled = false;
        }
    }

    private void StartLevel()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        _startCountDownText.enabled = true;
        int index = 3;
        _startCountDownText.text = index--.ToString();
        yield return new WaitForSeconds(1);
        _startCountDownText.text = index--.ToString();
        yield return new WaitForSeconds(1);
        _startCountDownText.text = index--.ToString();
        yield return new WaitForSeconds(1);
        _startCountDownText.text = "GO";
        yield return new WaitForSeconds(1);
        _startCountDownText.enabled = false;
        _Event_StartSpawning.Invoke();
    }

    private void ActivatePowerUPUI(object type)
    {
        PowerUpTypes newPowerUp = (PowerUpTypes)type;
        switch (newPowerUp)
        {
            case PowerUpTypes.SingleShot:
                FindWeaponUp(PowerUpTypes.SingleShot, true);
                break;
            case PowerUpTypes.TripleShot:
                FindWeaponUp(PowerUpTypes.TripleShot, true);
                break;
            case PowerUpTypes.SideShot:
                FindWeaponUp(PowerUpTypes.SideShot, true);
                break;
            case PowerUpTypes.HomingMissle:
                FindWeaponUp(PowerUpTypes.HomingMissle, true);
                break;
            case PowerUpTypes.SpeedBoost:
                FindPowerUp(PowerUpTypes.SpeedBoost, true);
                _powerUpcountDown.enabled = true;
                break;
            case PowerUpTypes.Shield:
                FindPowerUp(PowerUpTypes.Shield, true);
                break;
            case PowerUpTypes.Health:
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
                coroutine = StartCoroutine(HealthDisplay(PowerUpTypes.Health));
                break;
        }
    }

    private void DeactviatePowerUPUI(object type)
    {
        _powerUpcountDown.enabled = false;

        PowerUpTypes newPowerUp = (PowerUpTypes)type;
        switch (newPowerUp)
        {
            case PowerUpTypes.SpeedBoost:
                FindPowerUp(PowerUpTypes.SpeedBoost, false);
                break;
            case PowerUpTypes.Shield:
                FindPowerUp(PowerUpTypes.Shield, false);
                break;
        }
    }

    IEnumerator HealthDisplay(PowerUpTypes powerUpToFind)
    {
        Canvas healthUI = FindPowerUp(powerUpToFind, true);
        yield return new WaitForSeconds(3f);
        healthUI.enabled = false;
    }

    private Canvas FindPowerUp(PowerUpTypes powerUpToFind, bool active)
    {
        foreach (var item in _powerUpUI)
        {
            if (powerUpToFind == item.powerUpTypes)
            {
                item._UI.enabled = active;
                return item._UI;
            }
        }
        return null;
    }

    private void FindWeaponUp(PowerUpTypes weaponToFind, bool active)
    {
        foreach (var item in _weaponUI)
        {
            if (weaponToFind == item.weaponTypes)
            {
                if (weaponToFind != PowerUpTypes.SingleShot)
                {
                    _weaponcountDown.enabled = true;
                }
                else
                {
                    _weaponcountDown.enabled = false;
                }

                item._UI.enabled = active;
                continue;
            }
            if (active != false)
            {
                item._UI.enabled = false;
            }
        }
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
        _gameOverUI.enabled = true;
        _resetKeyUI.enabled = true;
        yield return null;
    }

    IEnumerator FlickerTimer()
    {
        _gameOverUI.enabled = true;
        yield return new WaitForSeconds(Random.Range(_onMin, _onMax));
        _gameOverUI.enabled = false;
        yield return new WaitForSeconds(Random.Range(_offMin, _offMax));
    }

    private void WeaponCountdownTimer(object value)
    {
        float newValue = (float)value;
        _weaponcountDown.text = newValue.ToString("0.0");
    }

    private void PowerUpCountdownTimer(object value)
    {
        float newValue = (float)value;
        _powerUpcountDown.text = newValue.ToString("0.0");
    }
}
