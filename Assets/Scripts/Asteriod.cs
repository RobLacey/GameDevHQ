using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteriod : EnemyController
{
    [SerializeField] float _rotationSpeedMin = default;
    [SerializeField] float _rotationSpeedMax = default;
    [SerializeField] float _minForce = default;
    [SerializeField] float _maxForce = default;
    [SerializeField] float _leftPathAngle = default;
    [SerializeField] float _rightPathAngle = default;
    [SerializeField] float _minSize = default;
    [SerializeField] float _maxSize = default;
    [SerializeField] EventManager _Event_ReturnBottomBounds = default;

    //Variables
    Rigidbody2D _myRigidBody;
    float startingSize;
    float _bounds;
    float _rotationSpeed;

    protected override void Awake()
    {
        base.Awake();
        _myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _bounds = (float)_Event_ReturnBottomBounds.Return_Parameter();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetUpStartingAngles();
        SetSize();
        StartMotion();
        _rotationSpeed = Random.Range(_rotationSpeedMin, _rotationSpeedMax);
    }

    private void Update()
    {
        if (transform.position.y < _bounds)
        {
            gameObject.SetActive(false);
        }
        transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
    }

    private void SetSize()
    {
        startingSize = Random.Range(_minSize, _maxSize);
        transform.localScale = new Vector3(startingSize, startingSize, startingSize);
    }

    private void StartMotion()
    {
        _myRigidBody.AddForce(transform.up * Random.Range(_minForce, _maxForce), ForceMode2D.Impulse);
    }

    private void SetUpStartingAngles()
    {
        Vector3 startAngle = new Vector3(0, 0, Random.Range(_leftPathAngle, _rightPathAngle));
        transform.rotation = Quaternion.Euler(startAngle);
    }

    public override void ProcessCollision() //IDamagable
    {
        if (_instanceHealth <= 0)
        {
            base.ProcessCollision();
            _myRigidBody.velocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}
