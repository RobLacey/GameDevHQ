using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] protected float _speed = default;
    [SerializeField] EventManager _Event_ReturnBottomBounds = default;
    [SerializeField] bool _allowSidewaysMovement;
    [SerializeField] float farRight, farLeft;
    [SerializeField] float insideRight, insideLeft;
    [SerializeField] float _horizontalSpeedMax;

    float rightBoundsMax, leftBoundsMax;
    float horizontalSpeed;
    ISpawnable _spawnable;
    float _bounds;

    private void OnEnable()
    {
        rightBoundsMax = Random.Range(insideRight, farRight);
        leftBoundsMax = Random.Range(insideLeft,farLeft);
        horizontalSpeed = Random.Range(-_horizontalSpeedMax, _horizontalSpeedMax);
        StartPositionCheck();
    }

    private void Start()
    {
        _bounds = (float)_Event_ReturnBottomBounds.Return_Parameter();
        _spawnable = GetComponent<ISpawnable>();
    }

    private void Update()
    {
        if (_allowSidewaysMovement)
        {
            transform.Translate(new Vector3(horizontalSpeed, -1, 0) * _speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
        }
        SwapDirections();
        OffScreenCheck();

    }

    private void OffScreenCheck() 
    {
        if (transform.position.y < _bounds)
        {
            if (_spawnable != null)
            {
                _spawnable.ActivateChildObjects(false);
            }            
            gameObject.SetActive(false);
        }
    }

    private void StartPositionCheck()
    {
        if (_allowSidewaysMovement)
        {
            if (transform.position.x < leftBoundsMax)
            {
                transform.position = new Vector3(leftBoundsMax + 1, transform.position.y, transform.position.z);
            }

            if (transform.position.x > rightBoundsMax)
            {
                transform.position = new Vector3(rightBoundsMax - 1, transform.position.y, transform.position.z);
            }
        }    
    }

    private void SwapDirections()
    {
        if (transform.position.x <= leftBoundsMax)
        {
            horizontalSpeed = -horizontalSpeed;
            return;
        }

        if (transform.position.x >= rightBoundsMax)
        {
            horizontalSpeed = -horizontalSpeed;
        }

    }
}
