using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] int destroyTime = 2;
    [SerializeField] int damage = 1;

    Player _player;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
        _player = FindObjectOfType<Player>();
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.GetComponent<IDamageable>();
        PowerUp isAPowerUp = collision.GetComponent<PowerUp>();

        if (canDealDamage != null && !isAPowerUp)
        {
            if (collision.gameObject != _player)
            {
                canDealDamage.Damage(damage);
                Destroy(gameObject);
            }
        }
    }
}
