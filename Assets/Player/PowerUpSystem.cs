using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSystem : MonoBehaviour
{
    [SerializeField] GameObject _shields = default;
    [SerializeField] float _speedBoostPresence = 7f;
    [SerializeField] AudioClip _powerUpEndSFX = default;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_AddHealth;
    [SerializeField] EventManager _Event_CountDownTimer;
    [SerializeField] EventManager _Event_Are_Shields_Active;

    //Variables
    CapsuleCollider2D _shieldsCollider;
    bool _isSpeedBoostActive = false;
    bool _areShieldsActive = false;
    float _SpeedBoostTimer = 0f;

    //Properties
    public bool ShieldsAreActive
    { get { return _areShieldsActive; } 
      set { 
            _areShieldsActive = value;
            _shieldsCollider.enabled = value;
            _shields.SetActive(value);
          }
    }

    private void Awake()
    {
        _shieldsCollider = GetComponentInChildren<CapsuleCollider2D>();
    }

    private void OnEnable()
    {
        _Event_ActivatePowerUp.AddListener(x => I_ActivatePowerUp(x), this);
        _Event_DeactivatePowerUp.AddListener(x => I_DeactivatePowerUps(x), this);
        _Event_Are_Shields_Active.AddReturnParameter(() => ShieldsAreActive, this);
    }

    private void Start()
    {
        ShieldsAreActive = false;
    }

    private void Update()
    {
        if (_isSpeedBoostActive)
        {
            SpeedBoostTimer();
        }
    }

    private void I_ActivatePowerUp(object newPowerUp)
    {
        PowerUpTypes weapon = (PowerUpTypes)newPowerUp;
        switch (weapon)
        {
            case PowerUpTypes.SpeedBoost:
                _SpeedBoostTimer = _speedBoostPresence;
                _isSpeedBoostActive = true;
                break;
            case PowerUpTypes.Shield:
                if (ShieldsAreActive == true) return;
                ShieldsAreActive = true;
                break;
            case PowerUpTypes.Health:
                _Event_AddHealth.Invoke(this);
                break;
            default:
                break;
        }
    }

    private void I_DeactivatePowerUps(object oldPowerUp)
    {
        switch ((PowerUpTypes)oldPowerUp)
        {
            case PowerUpTypes.SpeedBoost:
                _isSpeedBoostActive = false;
                break;
            case PowerUpTypes.Shield:
                ShieldsAreActive = false;
                break;
            default:
                break;
        }
        AudioSource.PlayClipAtPoint(_powerUpEndSFX, Camera.main.transform.position);
    }

    private void SpeedBoostTimer()
    {
        _SpeedBoostTimer -= Time.deltaTime;
        _Event_CountDownTimer.Invoke(_SpeedBoostTimer, this);
        if (_SpeedBoostTimer <= 0)
        {
            _Event_DeactivatePowerUp.Invoke(PowerUpTypes.SpeedBoost, this);
        }
    }
}
