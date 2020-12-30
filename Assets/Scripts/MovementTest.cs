﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UIElements;

public class MovementTest : MonoBehaviour
{
    Transform _myTransform;
    [SerializeField] [InputAxis] string _horizontal;
    [SerializeField] [InputAxis] string _vertical;
    [SerializeField] float _speed = 1f;

    float _horizontalSpeed;
    float _verticalSpeed;
    private Transform _child;
    private UIGameObject _myGameObject;

    bool _active = false;

    void Start()
    {
        //SetUp _inMenu
        _myTransform = GetComponent<Transform>();
        _child = GetComponentInChildren<SpriteRenderer>().transform;
        _myGameObject = GetComponent<UIGameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_active)
        {
            Debug.Log(_myGameObject);
            Vector3 rotationSpeed = new Vector3(0, 0, 30);
            _child.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
            _horizontalSpeed = Input.GetAxis(_horizontal) * _speed;
            _verticalSpeed = Input.GetAxis(_vertical) * _speed;

            _myTransform.position += new Vector3(_horizontalSpeed, _verticalSpeed, 0);
        }    
    }

    public void ActivateObject(UIGameObject activeObj)
    {
        _active = activeObj == _myGameObject;
    }
}
