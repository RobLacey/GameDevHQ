using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissles : Projectile
{
    [SerializeField] Vector3 _direction;
    [SerializeField] float _speed;
    [SerializeField] float _rotationSpeed;
    [SerializeField] GameObject _target;
    [SerializeField] float _timerSetting;


    //Variables
    float _timer;
    TargetingComputer _targetingComputer;

    private void Awake()
    {
        _targetingComputer = GetComponent<TargetingComputer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _timer = _timerSetting;
        _target = null;
    }


    protected override void Update()
    {
        base.Update();
        transform.Translate(_direction * _speed * Time.deltaTime);

        if (!_target)
        {
            _target = _targetingComputer.FindTarget();
        }
        else if (_targetingComputer.CurrentTarget == null)
        {
            Debug.Log("Up to this");
            //TODO  Make missle aim at a default position
        }
        else
        {
            transform.up = Vector3.Slerp(transform.up, _target.transform.position - transform.position, 
                                         _rotationSpeed * Time.deltaTime);
        }

        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }
}
