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
    //[SerializeField] GameObject _expolsion = default;

    //Variables
    Rigidbody2D _myRigidBody;

    protected override void Awake()
    {
        base.Awake();
        _myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SetUpStartingAngles();
        float startingSize = Random.Range(_minSize, _maxSize);
        transform.localScale = new Vector3(startingSize, startingSize, startingSize);
        _myRigidBody.mass = _myRigidBody.mass * startingSize;
        _myRigidBody.AddRelativeForce(transform.up * Random.Range(_minForce, _maxForce), ForceMode2D.Impulse);
        _myRigidBody.AddTorque(Random.Range(_rotationSpeedMin, _rotationSpeedMax));
    }

    private void SetUpStartingAngles()
    {
        Vector3 startAngle = new Vector3(0, 0, Random.Range(_leftPathAngle, _rightPathAngle));
        transform.eulerAngles = startAngle;
    }

    public override void ProcessCollision() //IDamagable
    {
        if (_health <= 0)
        {
            base.ProcessCollision();
            //TODO add animation death to player
            _myRigidBody.velocity = Vector2.zero;
            //Instantiate(_expolsion, transform.position, Quaternion.identity, transform);
        }
    }
}
