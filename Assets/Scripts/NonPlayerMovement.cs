using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerMovement : MonoBehaviour, ISpawnable
{
    [SerializeField] float _speed = default;
    [SerializeField] float _topBounds = default;
    [SerializeField] float _bottomBounds = default;
    [SerializeField] float _rightBounds = default;
    [SerializeField] float _LeftBounds = default;
    [SerializeField] bool _respawn = false;

    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);
        OffScreenCheck();
    }

    private void OffScreenCheck()
    {
        if (transform.position.y < _bottomBounds)
        {
            if (_respawn)
            {
                transform.position = SpawnPosition();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public Vector3 SpawnPosition() //ISpawnable
    {
        float randomX = Random.Range(_rightBounds, _LeftBounds);
        return new Vector3(randomX, _topBounds);
    }

    public void DontRespawn()
    {
        _respawn = false;
    }

    public void SetRandomSpeed(float randomSpeed)
    {
        _speed = randomSpeed;
    }
}
