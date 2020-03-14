using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSystem : MonoBehaviour ,IPowerUpSystem
{
    [SerializeField] GameObject _shields = default;
    [SerializeField] bool _isSpeedBoostActive = false;
    [SerializeField] bool _areShieldsActive = false;
    [SerializeField] float _speedBoost = 10f;
    [SerializeField] float _normalSpeed = 5f;
    [SerializeField] float _SpeedBoostTimer = 0f;
    [SerializeField] float _speedBoostPresence = 7f;
    [SerializeField] AudioClip _powerUpEndSFX = default;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_AddHealth;

    CircleCollider2D _shieldsCollider;
    ISpeedBoostable _speedBoostable;

    //Properties
    public bool I_ShieldsAreActive
    { get { return _areShieldsActive; } set { _areShieldsActive = value; } }

    private void Awake()
    {
        _shieldsCollider = GetComponentInChildren<CircleCollider2D>();
        _speedBoostable = GetComponent<ISpeedBoostable>();
    }

    private void OnEnable()
    {
        _Event_ActivatePowerUp.AddListener(x => I_ActivatePowerUp(x));
        _Event_DeactivatePowerUp.AddListener(x => I_DeactivatePowerUps(x));
    }

    private void Start()
    {
        SetUpShields(false);
        SetUpSpeedBoost(false);
    }

    private void Update()
    {
        if (_isSpeedBoostActive)
        {
            SpeedBoostTimer();
        }
    }

    public void I_ActivatePowerUp(object newPowerUp)
    {
        PowerUpTypes weapon = (PowerUpTypes)newPowerUp;
        switch (weapon)
        {
            case PowerUpTypes.SpeedBoost:
                _SpeedBoostTimer = _speedBoostPresence;
                SetUpSpeedBoost(true);
                break;
            case PowerUpTypes.Shield:
                if (I_ShieldsAreActive == true) return;
                SetUpShields(true);
                break;
            case PowerUpTypes.Health:
                _Event_AddHealth.Invoke();
                break;
            default:
                break;
        }
    }

    public void I_DeactivatePowerUps(object oldPowerUp)
    {
        switch ((PowerUpTypes)oldPowerUp)
        {
            case PowerUpTypes.SpeedBoost:
                SetUpSpeedBoost(false);
                break;
            case PowerUpTypes.Shield:
                SetUpShields(false);
                break;
            default:
                break;
        }
        AudioSource.PlayClipAtPoint(_powerUpEndSFX, Camera.main.transform.position);
    }


    private void SetUpSpeedBoost(bool active)
    {
        _isSpeedBoostActive = active;
        if (active)
        {
            _SpeedBoostTimer = _speedBoostPresence;
            _speedBoostable.I_SetSpeed = _speedBoost;
        }
        else
        {
            _speedBoostable.I_SetSpeed = _normalSpeed;
        }
    }

    private void SetUpShields(bool active)
    {
        I_ShieldsAreActive = active;
        _shieldsCollider.enabled = active;
        _shields.SetActive(active);
    }

    private void SpeedBoostTimer()
    {
        _SpeedBoostTimer -= Time.deltaTime;
        if (_SpeedBoostTimer <= 0)
        {
            _Event_DeactivatePowerUp.Invoke(PowerUpTypes.SpeedBoost);
        }
    }
}
