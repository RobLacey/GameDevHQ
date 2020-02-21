using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int _points = default;
    [SerializeField] string _playerTag = default;

    //Variables
    UIManger _uiManger;

    private void Start()
    {
        _uiManger = FindObjectOfType<UIManger>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.GetComponent<IDamageable>();

        if (canDealDamage != null)
        {
            if (collision.tag != _playerTag)
            {
                _uiManger.AddToScore(_points);
            }
            canDealDamage.Damage();            
            Destroy(gameObject);
        }
    }
}
