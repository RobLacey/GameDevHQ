using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player hitPlayer = collision.GetComponent<Player>();

        if (hitPlayer)
        {
            hitPlayer.ActivatePowerUp();
            Destroy(gameObject);
        }
    }
}
