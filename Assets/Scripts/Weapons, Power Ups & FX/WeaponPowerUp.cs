using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPowerUp : PowerUp
{
    [SerializeField] WeaponSpec _weaponSpec;
    [SerializeField] EventManager _Event_NewWeapon;

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
                _Event_NewWeapon.Invoke(_weaponSpec, this);
                StartCoroutine(DisableObject(_collectSFX.length));
            }
        }
    }
}
