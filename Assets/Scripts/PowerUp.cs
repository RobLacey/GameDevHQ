using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : EnemyController
{
    [SerializeField] PowerUpTypes _powerUpType = default;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        Player hitPlayer = collision.GetComponent<Player>();

        if (hitPlayer)
        {
            hitPlayer.ActivatePowerUp(_powerUpType);
            Destroy(gameObject);
        }
    }
}
