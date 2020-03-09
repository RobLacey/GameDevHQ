using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : Projectile, ITagable
{
    [SerializeField] float speed = 10f;
    [SerializeField] Vector3 _direction;

    protected override void Update()
    {
        base.Update();
        transform.Translate(_direction * speed * Time.deltaTime);
    }
}
