using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] protected float _speed = default;
    [SerializeField] bool _allowSidewaysMovement;
    [SerializeField] float farRight, farLeft;
    [SerializeField] float insideRight, insideLeft;
    [SerializeField] float _horizontalSpeedMax;
    [SerializeField] EventManager _Event_RemoveEnemyAsTarget;
    [SerializeField] GlobalVariables _screenBounds;

    //Variables
    float rightBoundsMax, leftBoundsMax;
    float horizontalSpeed;
    IEnemyWave _partofWave;

    private void OnEnable()
    {
        rightBoundsMax = Random.Range(insideRight, farRight); //TODI fix this to us global vars
        leftBoundsMax = Random.Range(insideLeft,farLeft);
        horizontalSpeed = Random.Range(-_horizontalSpeedMax, _horizontalSpeedMax);
        StartPositionCheck();
    }

    private void Start()
    {
        _partofWave = GetComponent<IEnemyWave>();
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
        if (!transform.StillOnScreen(_screenBounds))
        {
            if (_partofWave != null)
            {
                foreach (Transform child in transform)
                {
                    _Event_RemoveEnemyAsTarget.Invoke(child.gameObject);
                }
                _partofWave.I_DeactivateChildObjects();
            }
            else if(_partofWave == null)
            {
                gameObject.SetActive(false);
                Debug.Log("non wave objects need script on top layer");
            }
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
