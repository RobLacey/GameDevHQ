using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissles : Projectile
{
    [SerializeField] float _speed;
    [SerializeField] float _rotationSpeed;
    [SerializeField] Vector3 _defaultTarget;
    [SerializeField] float _timerSetting;
    [SerializeField] EventManager _Event_RemoveEnemyAsTarget;
    [SerializeField] EventManager _Event_ReturnActiveEnemies;
    [SerializeField] GlobalVariables _targetingBounds;

    //Variables
    float _timer;
    List<GameObject> _activeEnemeiesList;
    GameObject _target;

    protected override void OnEnable()
    {
        base.OnEnable();
        _activeEnemeiesList = new List<GameObject>();
        _timer = _timerSetting;
        _target = null;
        _Event_RemoveEnemyAsTarget.AddListener((x) => RemoveTarget(x));
    }

    protected override void Update()
    {
        base.Update();

        transform.Translate(Vector3.up * _speed * Time.deltaTime);

        CheckForTarget();
        Timer();
    }

    private void CheckForTarget()
    {
        if (!_target || _target.transform.parent.StillOnScreen(_targetingBounds))
        {
            _target = FindTarget();
        }

        if (_target == null)
        {
            transform.up = Vector3.Slerp(transform.up, transform.Direction(_defaultTarget),
                                         _rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.up = Vector3.Slerp(transform.up, transform.Direction(_target.transform),
                                         _rotationSpeed * Time.deltaTime);
        }
    }

    private void Timer()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }

    public GameObject FindTarget()
    {
        _activeEnemeiesList = (List<GameObject>)_Event_ReturnActiveEnemies.Return_Parameter();
        float shortestDistance = Mathf.Infinity;
        GameObject bestTarget = null;

        foreach (var potentialTarget in _activeEnemeiesList)
        {
            if (_activeEnemeiesList.Count > 0 && potentialTarget.transform.StillOnScreen(_targetingBounds))
            {
                float thisDistance = Vector3.Distance(potentialTarget.transform.position, transform.position);

                if (thisDistance < shortestDistance)
                {
                    shortestDistance = thisDistance;
                    bestTarget = potentialTarget;
                }
            }
        }
        return bestTarget;
    }

    private void RemoveTarget(object target)
    {
        GameObject deadTarget = (GameObject)target;
        if (_target = deadTarget)
        {
            _target = null;
        }
    }
}
