using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingComputer : MonoBehaviour
{
    [SerializeField] EventManager _Event_RemoveEnemyAsTarget;
    [SerializeField] GameObject _currentTarget = default;
    [SerializeField] GlobalVariables _targetingBounds;

    SpawnManager _spawnManager;


    //TODO Combine into Homing Missle script
    public GameObject CurrentTarget { get { return _currentTarget; } private set { _currentTarget = value; } }

    private void Awake()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
    }

    private void OnEnable()
    {
        CurrentTarget = null;
        _Event_RemoveEnemyAsTarget.AddListener((x) => RemoveTarget(x));
    }

    private void Update()
    {
        if (CurrentTarget != null)
        {
            if (!CurrentTarget.transform.parent.StillOnScreen(_targetingBounds))
            {
                CurrentTarget = null;
            }
        }   
    }

    private void RemoveTarget(object target)
    {
        GameObject deadTarget = (GameObject)target;
        if (CurrentTarget = deadTarget)
        {
            CurrentTarget = null;
        }
    }

    public GameObject FindTarget()
    {
        float distance = Mathf.Infinity;
        GameObject bestTarget = null;

        foreach (var target in _spawnManager.ActiveTargets)
        {
            if (_spawnManager.ActiveTargets.Count == 0)
            {
                break;
            }
            if (!target.transform.StillOnScreen(_targetingBounds))
            {
                continue;
            }
            float thisDistance = Vector3.Distance(target.transform.position, transform.position);
            if (thisDistance < distance)
            {
                distance = thisDistance;
                bestTarget = target;
            }
        }
        if (bestTarget != null)
        {
            CurrentTarget = bestTarget;
        }

        return bestTarget;
    }

}
