using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteriod : MonoBehaviour
{
    [SerializeField] float _rotationSpeedMin = default;
    [SerializeField] float _rotationSpeedMax = default;
    [SerializeField] float _minSpeed = default;
    [SerializeField] float _maxSpeed = default;
    [SerializeField] float _leftPathAngle = default;
    [SerializeField] float _rightPathAngle = default;
    [SerializeField] float _startAngleMin = default;
    [SerializeField] float _startAngleMax = default;
    [SerializeField] float _minSize = default;
    [SerializeField] float _maxSize = default;
    [SerializeField] GameObject _asteriod = default;

    //Variables
    float _rotationSpeed = default;
    NonPlayerMovement _myMovement = default;

    private void Awake()
    {
        _myMovement = GetComponent<NonPlayerMovement>();
    }

    private void Start()
    {
        SetUpStartingAngles();
        float scale = Random.Range(_minSize, _maxSize);
        transform.localScale = new Vector3(scale, scale, scale);
        _myMovement.SetRandomSpeed(Random.Range(_minSpeed, _maxSpeed));
    }

    private void SetUpStartingAngles()
    {
        _rotationSpeed = Random.Range(_rotationSpeedMin, _rotationSpeedMax);
        _asteriod.transform.localEulerAngles = new Vector3(0, 0, Random.Range(_startAngleMin, _startAngleMax));
        transform.eulerAngles = new Vector3(0, 0, Random.Range(_leftPathAngle, _rightPathAngle));
    }

    private void Update()
    {
        _asteriod.transform.Rotate(0, 0, _rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {        
        IDamageable canDealDamage = collision.gameObject.GetComponent<IDamageable>();

        if (canDealDamage != null)
        {
            canDealDamage.Damage();
        }
    }
}
