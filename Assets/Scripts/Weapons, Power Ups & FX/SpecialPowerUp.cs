using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialPowerUp : PowerUp
{
    [SerializeField] PowerUpTypes _powerUpType = default;
    [SerializeField] EventManager _Event_ActivatePowerUp;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canCollide = collision.GetComponentInParent<IDamageable>();

        if (canCollide != null)
        {
            if (canCollide.I_TeamTag == TeamTag)
            {
                _audioSource.Play();
                _collider2D.enabled = false;
                ActivateSprite(false);
                _Event_ActivatePowerUp.Invoke(_powerUpType);
                StartCoroutine(DisableObject(_collectSFX.length));
            }
        }
    }
}
