using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : EnemyController
{
    [SerializeField] PowerUpTypes _powerUpType = default;
    [SerializeField] AudioClip _collectSFX = default;
    [SerializeField] AudioClip _powerUpEndSFX = default;
    [SerializeField] float _tripleShotTimer = 5f;
    [SerializeField] float _speedBoostTimer = 7f;

    Collider2D _myCollider;
    Player _player;
    float timer = 0;

    protected override void Awake()
    {
        base.Awake();
        _audioSource.clip = _collectSFX;
        _player = FindObjectOfType<Player>();
        _myCollider = GetComponent<Collider2D>();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == _player.tag)
        {
            _myBody.enabled = false;
            _myCollider.enabled = false;
            SetSpeed(0);
            _audioSource.Play();
            ActivatePowerUp();
        }
    }

    private void PowerUpOver()
    {
        _audioSource.clip = _powerUpEndSFX;
        timer = _audioSource.clip.length;
        _audioSource.Play();
        Destroy(gameObject, timer);
    }

    public void ActivatePowerUp()
    {
        _player.CheckForActivePowerUps(this);
        _player.ActivatePowerUp(_powerUpType);

        switch (_powerUpType)
        {
            case PowerUpTypes.TripleShot:
                //StartCoroutine(TripleShot());
                StartCoroutine(PowerTimer(_tripleShotTimer));
                break;
            case PowerUpTypes.SpeedBoost:
                //StartCoroutine(SpeedBoost());
                StartCoroutine(PowerTimer(_speedBoostTimer));
                break;
            default:
                break;
        }
    }

    private IEnumerator PowerTimer(float timer)
    {
        yield return new WaitForSeconds(_tripleShotTimer);
        _player.DeactivatePowerUps(this);
        PowerUpOver();
    }
    //private IEnumerator TripleShot()
    //{
    //    yield return new WaitForSeconds(_tripleShotTimer);
    //    _player.DeactivatePowerUps(this);
    //    PowerUpOver();
    //}

    //private IEnumerator SpeedBoost()
    //{
    //    yield return new WaitForSeconds(_speedBoostTimer);
    //    _player.DeactivatePowerUps(this);
    //    PowerUpOver();
    //}

    public PowerUpTypes ReturnPowerUpType()
    {
        return _powerUpType;
    }
}
