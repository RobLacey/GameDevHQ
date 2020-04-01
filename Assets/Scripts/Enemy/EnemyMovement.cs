using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] protected float _speed = default;
    [SerializeField] EventManager _Event_RemoveEnemyAsTarget;
    [SerializeField] GlobalVariables _screenBounds;

    bool _canMove = true;
    IEnemyWave _partofWave;

    private void Start()
    {
        _partofWave = GetComponent<IEnemyWave>();
    }

    private void OnEnable()
    {
        _canMove = true;
    }

    private void Update()
    {
        if (_canMove)
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
            OffScreenCheck();
        }    
    }

    private void OffScreenCheck()
    {
        if (!transform.StillOnScreen(_screenBounds))
        {
            if (_partofWave != null)
            {
                foreach (Transform child in transform)
                {
                    _Event_RemoveEnemyAsTarget.Invoke(child.gameObject, this);
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
}
