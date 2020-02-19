using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float _topBounds;
    [SerializeField] float _bottomBounds;
    [SerializeField] float _rightBounds;
    [SerializeField] float _LeftBounds;
    [SerializeField] string _playerTag;
    [SerializeField] string _lazerTag;

    void Start()
    {
        Spawn();
    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y < _bottomBounds)
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        float randomX = Random.Range(_rightBounds, _LeftBounds);

        transform.position = new Vector3(randomX, _topBounds, 0);
    }

    private void OnTriggerEnter(Collider triggeredBy)
    {
        if (triggeredBy.tag == _playerTag)
        {
            IDamageable damagable = triggeredBy.GetComponent<IDamageable>();

            if (damagable != null)
            {
                damagable.Damage();            
                Destroy(gameObject);
            }
        }
        else if (triggeredBy.tag == _lazerTag)
        {
            Destroy(triggeredBy.gameObject);
            Destroy(gameObject);
        }
    }
}
