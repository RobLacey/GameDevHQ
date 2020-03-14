using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteriod : MonoBehaviour, IKillable
{
    [SerializeField] float _rotationSpeedMin = default;
    [SerializeField] float _rotationSpeedMax = default;
    [SerializeField] float _minForce = default;
    [SerializeField] float _maxForce = default;
    [SerializeField] float _leftPathAngle = default;
    [SerializeField] float _rightPathAngle = default;
    [SerializeField] Transform _myBody;
    [SerializeField] float _minSize = default;
    [SerializeField] float _maxSize = default;
    [SerializeField] EventManager _Event_RemoveEnemyAsTarget;
    [SerializeField] GlobalVariables _screenBounds;

    //Variables
    Rigidbody2D _myRigidBody;
    float startingSize;
    float _rotationSpeed;

    private void Awake()
    {
        _myRigidBody = GetComponent<Rigidbody2D>();
        SetSize();
    }

    private void OnEnable()
    {
        SetUpStartingAngles();
        StartMotion();
        _rotationSpeed = Random.Range(_rotationSpeedMin, _rotationSpeedMax);
    }

    private void Update()
    {
        OffScreenCheck();
    }

    private void FixedUpdate()
    {
        _myBody.transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
    }

    private void SetSize()
    {
        startingSize = Random.Range(_minSize, _maxSize);
        _myBody.localScale = new Vector3(startingSize, startingSize, startingSize);
    }

    private void StartMotion()
    {
        _myRigidBody.AddForce(transform.up * Random.Range(_minForce, _maxForce), ForceMode2D.Impulse);
        transform.rotation = Quaternion.Euler(Vector3.zero); //Straightens up after push in rotations direction
    }

    private void SetUpStartingAngles()
    {
        Vector3 startAngle = new Vector3(0, 0, Random.Range(_leftPathAngle, _rightPathAngle));
        transform.rotation = Quaternion.Euler(startAngle);
    }

    public void I_Dead(bool collsionKill)
    {
        _myRigidBody.velocity = Vector2.zero;
    }

    private void OffScreenCheck()
    {
        if (!transform.StillOnScreen(_screenBounds))
        {
            _Event_RemoveEnemyAsTarget.Invoke(gameObject);
            gameObject.SetActive(false);
        }
    }
}
