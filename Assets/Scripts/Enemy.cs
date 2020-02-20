using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float speed = default;
    [SerializeField] float _topBounds = default;
    [SerializeField] float _bottomBounds = default;
    [SerializeField] float _rightBounds = default;
    [SerializeField] float _LeftBounds = default;

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y < _bottomBounds)
        {
            transform.position = Spawn();
        }
    }

    public Vector3 Spawn()
    {
        float randomX = Random.Range(_rightBounds, _LeftBounds);
        return new Vector3(randomX, _topBounds, 0);
    }

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
