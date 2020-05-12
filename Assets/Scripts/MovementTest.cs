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

    [SerializeField] bool _inMenu = false;

    public bool InMenu { get { return _inMenu; } set { _inMenu = value; } }

    void Start()
    {
        //SetUp _inMenu
        _myTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_inMenu)
        {
            _horizontalSpeed = Input.GetAxis(_horizontal) * _speed;
            _verticalSpeed = Input.GetAxis(_vertical) * _speed;

            _myTransform.position += new Vector3(_horizontalSpeed, _verticalSpeed, 0);
        }    
    }
}
