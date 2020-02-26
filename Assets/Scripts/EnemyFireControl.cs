using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireControl : MonoBehaviour
{
    [SerializeField] float _fireRateMin= default;
    [SerializeField] float _fireRateMax = default;
    [SerializeField] GameObject _weaponPrefab = default;
    [SerializeField] Vector3 _laserOffset = default;
    [SerializeField] string _spawnBufferName= default;//TODO fix this as uses string
    
    //Variables
    float _fireRate = 0;
    GameObject _spawnBuffer;

    private void Awake()
    {
        _spawnBuffer = GameObject.Find(_spawnBufferName);
    }

    private void Start()
    {
        _fireRate = Random.Range(_fireRateMin, _fireRateMax);
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        GameObject shot;

        while (true)
        {
            _fireRate = Random.Range(_fireRateMin, _fireRateMax);
            yield return new WaitForSeconds(_fireRate);
                shot = Instantiate(_weaponPrefab, transform.position + _laserOffset, Quaternion.identity, _spawnBuffer.transform);
                shot.GetComponent<LaserBeam>().SetTag(gameObject.tag);
        }
    }
}
