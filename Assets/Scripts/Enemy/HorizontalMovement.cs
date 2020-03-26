using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalMovement : MonoBehaviour
{
    [SerializeField] float _maxSpeed = 4f;
    [SerializeField] float _minSpeed = 3f;
    [SerializeField] float _leftPathEdge = -6f;
    [SerializeField] float _rightPathEdge = 6f;

    //Variables
    float _duration;
    float _rightBoundsMax, _leftBoundsMax;
    float _insideLeft = -1f;
    float _insideRight = 1f;
    float _timer = 0;

    private void OnEnable()
    {
        _rightBoundsMax = Random.Range(_insideRight, _rightPathEdge);
        _leftBoundsMax = Random.Range(_insideLeft, _leftPathEdge);
        _duration = Random.Range(_minSpeed, _maxSpeed);
        StartPositionCheck();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float perc = _timer / _duration;
        transform.VectorLerp(_leftBoundsMax, _rightBoundsMax, perc.EaseINOut());
        SwapDirections();
    }

    private void SwapDirections()
    {
        if (_timer >= _duration)
        {
            float temp = _leftBoundsMax;
            _leftBoundsMax = _rightBoundsMax;
            _rightBoundsMax = temp;
            _timer = 0f;
        }
    }

    private void StartPositionCheck()
    {
        if (transform.position.x < _leftBoundsMax)
        {
            transform.position = new Vector3(_leftBoundsMax + 1, transform.position.y, transform.position.z);
        }

        if (transform.position.x > _rightBoundsMax)
        {
            transform.position = new Vector3(_rightBoundsMax - 1, transform.position.y, transform.position.z);
        }
    }


}
