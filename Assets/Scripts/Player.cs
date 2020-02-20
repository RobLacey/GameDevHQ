using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] Vector3 _startPosition = default;
    [SerializeField] float _speed = 1f;
    [SerializeField] string _horizontalAxis = null;
    [SerializeField] string _verticalAxis = null;
    [SerializeField] float _topBound = default;
    [SerializeField] float _bottomBound = default;
    [SerializeField] float _rightBound = default;
    [SerializeField] float _leftBound = default;
    [SerializeField] GameObject _lazerPrefab = default;
    [SerializeField] GameObject _lazerContainer = default;
    [SerializeField] Vector3 _lazerPositionOffset = default;
    [SerializeField] float _fireRate = default;
    [SerializeField] int _lives = 3;

    float _canFire = 0;
    public static event Action _Dead;
    

    void Start()
    {
        transform.position = _startPosition;
    }

    void Update()
    {
        Movement();

        if (Input.GetKey(KeyCode.Space) && Time.time > _canFire)
        {
            Fire();
        }
    }

    private void Movement()
    {
        CalculateMovement();        
        CheckVerticalBounds();
        WrapVerticalPosition();
    }

    /// <summary> Calculates where to move player by getting inputs </summary>
    private void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis(_horizontalAxis);
        float verticalInput = Input.GetAxis(_verticalAxis);
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

        transform.Translate(direction * _speed * Time.deltaTime);        
    }

    private void Fire()
    {
        _canFire = Time.time + _fireRate;
        Vector3 newPosition = transform.position + _lazerPositionOffset;
        Instantiate(_lazerPrefab, newPosition, Quaternion.identity, _lazerContainer.transform);
    }

    /// <summary> Sets the vertical boundry of the player movement. Can be changed via the associated variables </summary>
    private void CheckVerticalBounds()
    {
        float yPosition = Mathf.Clamp(transform.position.y, _bottomBound, _topBound);
        transform.position = new Vector3(transform.position.x, yPosition, 0);
    }

    private void WrapVerticalPosition()
    {
        if (transform.position.x >= _rightBound)
        {
            transform.position = new Vector3(_leftBound, transform.position.y, 0);
        }
        else if (transform.position.x <= _leftBound)
        {
            transform.position = new Vector3(_rightBound, transform.position.y, 0);
        }
    }

    public void Damage(float damage)
    {
        _lives--;

        if (_lives <= 0)
        {
            _Dead?.Invoke();
            Destroy(gameObject);
        }
    }
}
