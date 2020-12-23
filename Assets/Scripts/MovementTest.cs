using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MovementTest : MonoBehaviour
{
    Transform _myTransform;
    [SerializeField] [InputAxis] string _horizontal;
    [SerializeField] [InputAxis] string _vertical;
    [SerializeField] float _speed = 1f;

    float _horizontalSpeed;
    float _verticalSpeed;
    private Transform _child;

    bool _inMenu = true;

    public bool InMenu { get { return _inMenu; } set { _inMenu = value; } }

    void Start()
    {
        //SetUp _inMenu
        _myTransform = GetComponent<Transform>();
        _child = GetComponentInChildren<SpriteRenderer>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rotationSpeed = new Vector3(0, 0, 30);
        _child.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        if (!_inMenu)
        {
            _horizontalSpeed = Input.GetAxis(_horizontal) * _speed;
            _verticalSpeed = Input.GetAxis(_vertical) * _speed;

            _myTransform.position += new Vector3(_horizontalSpeed, _verticalSpeed, 0);
        }    
    }
}
