using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : EnemyController
{
    [SerializeField] PowerUpTypes _powerUpType = default;
    [SerializeField] AudioClip _collectSFX = default;

    //Variables
    Collider2D _myCollider;
    WeaponsSystem _weaponSystem;
    float timer = 0;

    protected override void Awake()
    {
        base.Awake();
        _audioSource.clip = _collectSFX;
        _weaponSystem = FindObjectOfType<WeaponsSystem>();
        _myCollider = GetComponent<Collider2D>();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == _weaponSystem.tag)
        {
            ActivatePowerUp();
            _weaponSystem.ActivatePowerUp(_powerUpType);
        }
    }

    public void ActivatePowerUp()
    {
        _myCollider.enabled = false;
        _myBody.enabled = false;
        SetSpeed(0);
        timer = _audioSource.clip.length;
        _audioSource.Play();
        _weaponSystem.ActivatePowerUp(_powerUpType);
        Destroy(gameObject, timer);
    }
}
