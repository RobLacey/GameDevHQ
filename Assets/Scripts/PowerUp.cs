using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] PowerUpTypes _powerUpType = default;
    [SerializeField] AudioClip _collectSFX = default;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] string _playerTag = default;

    //Variables
    SpriteRenderer[] _myBody;
    AudioSource _audioSource;
    Collider2D _collider2D;

    protected void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _myBody = GetComponentsInChildren<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _collectSFX;
    }

    private void OnEnable()
    {
        ActivateSprite(true);
        _collider2D.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == _playerTag)
        {
            _audioSource.Play();
            _collider2D.enabled = false;
            ActivateSprite(false);
            _Event_ActivatePowerUp.Invoke(_powerUpType);
            StartCoroutine(DisableObject(_collectSFX.length));
        }
    }

    private void ActivateSprite(bool active)
    {
        foreach (var sprite in _myBody)
        {
            sprite.enabled = active;
        }
    }

    private IEnumerator DisableObject(float timer)
    {
        yield return new WaitForSeconds(timer);
        gameObject.SetActive(false);
    }
}
