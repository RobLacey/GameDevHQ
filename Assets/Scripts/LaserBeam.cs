﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour, IDamageable
{
    [SerializeField] float speed = 10f;
    [SerializeField] int destroyTime = 2;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    public void Damage(float damage = 0)
    {
        Destroy(gameObject);
    }
}