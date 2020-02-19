using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] Vector3 _startPosition;
    [SerializeField] float _speed = 1f;
    [SerializeField] string _horizontalAxis;
    [SerializeField] string _verticalAxis;
    [SerializeField] float _topBound;
    [SerializeField] float _bottomBound;
    [SerializeField] float _rightBound;
    [SerializeField] float _leftBound;
    [SerializeField] GameObject _lazerPrefab;
    [SerializeField] Vector3 _lazerPositionOffset;
    [SerializeField] float _fireRate;
    [SerializeField] int _lives = 3;

    float _canFire = 0;

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
        Instantiate(_lazerPrefab, transform.position + _lazerPositionOffset, Quaternion.identity);
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
        Debug.Log(damage);
        _lives--;
        if (_lives <= 0)
        {
            Destroy(gameObject);
        }
    }
}
