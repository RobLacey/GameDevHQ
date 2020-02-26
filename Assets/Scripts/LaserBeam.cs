using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] int destroyTime = 2;
    [SerializeField] int damage = 1;
    [SerializeField] string _myTag = default;
    [SerializeField] bool _enemyWeapon = default;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        if (_enemyWeapon)
        {
            transform.Translate(-transform.up * speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(transform.up * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.GetComponent<IDamageable>();
        PowerUp isAPowerUp = collision.GetComponent<PowerUp>();

        if (canDealDamage != null && !isAPowerUp)
        {
            if (collision.gameObject.tag != _myTag)
            {
                canDealDamage.Damage(damage);
                Destroy(gameObject);
            }
        }
    }

    public void SetTag(string newTag)
    {
        _myTag = newTag;
    }
}
