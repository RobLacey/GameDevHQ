using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrapnelController : MonoBehaviour, IShrapnel
{
    [SerializeField] GameObject _shrapnel;
    [SerializeField] int _minSpawn = 3;
    [SerializeField] int _maxSpawn = 7;

    //Variables
    private int _StartingShrapnelCount;
    private int _shrapnelLeft;
    GameObject[] _shrapnelCloud;

    private void Awake()
    {
        int index = Random.Range(_minSpawn, _maxSpawn);
        _shrapnelCloud = new GameObject[index];

        for (int i = 0; i < index; i++)
        {
            GameObject newObject = Instantiate(_shrapnel, transform.position, Quaternion.identity, transform);
            _shrapnelCloud[i] = newObject;
            _StartingShrapnelCount++;
        }
        _shrapnelLeft = _StartingShrapnelCount;
    }

    private void OnEnable()
    {
        _shrapnelLeft = _StartingShrapnelCount;
        foreach (var item in _shrapnelCloud)
        {
            item.SetActive(true);
        }
    }

    public void I_DeactivateChildObjects()
    {
        _shrapnelLeft--;
        if (_shrapnelLeft <= 0)
        {
            foreach (var piece in _shrapnelCloud)
            {
                piece.transform.localPosition = Vector3.zero;
            }
            gameObject.SetActive(false);
        }
    }
}
