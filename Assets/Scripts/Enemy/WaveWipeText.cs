using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveWipeText : MonoBehaviour
{
    [SerializeField] float _duration = default;
    [SerializeField] EventManager _Event_GetTarget = default;
    [SerializeField] AudioClip _waveWipeSFX;
    [SerializeField] float _volume = 0.5f;

    Vector3 _currentPosition;
    Camera _camera;
    Vector3 _targetPosition;
    Vector3 _myPosition = default;
    RectTransform _myUI;
    bool _start = false;

    private void Awake()
    {
        _camera = Camera.main;
        _myUI = GetComponentInChildren<Text>().GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (_start)
        {
            AudioSource.PlayClipAtPoint(_waveWipeSFX, _camera.transform.position, _volume);
        }
        _start = true;

    }

    private void Start()
    {
        RectTransform _target = (RectTransform) _Event_GetTarget.Return_Parameter();
        _targetPosition = _camera.ScreenToWorldPoint(_target.position);
    }

    private void Update()
    {
        Move();
        AtTargetPosiiton();
    }

    private void Move()
    {
        _myPosition = _camera.ScreenToWorldPoint(_myUI.position);
        _currentPosition = Vector3.MoveTowards(_myPosition, _targetPosition, _duration * Time.deltaTime);
        _myUI.position = _camera.WorldToScreenPoint(_currentPosition);
    }

    private void AtTargetPosiiton()
    {
        if (_currentPosition == _targetPosition)
        {
            gameObject.SetActive(false);
        }
    }

}
