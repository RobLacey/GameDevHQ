﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.GetComponent<IDamageable>();

        if (canDealDamage != null)
        {
            canDealDamage.Damage();            
            Destroy(gameObject);
        }
    }
}
