using EZ.Events;
using UnityEngine;
using NaughtyAttributes;

public class MovementTest : MonoBehaviour, IEZEventUser
{
    Transform _myTransform;
    [SerializeField] [InputAxis] string _horizontal;
    [SerializeField] [InputAxis] string _vertical;
    [SerializeField] float _speed = 1f;

    float _horizontalSpeed;
    float _verticalSpeed;
    private Transform _child;

    bool _active = false;

    private void Awake()
    {
        //SetUp _inMenu
        _myTransform = GetComponent<Transform>();
        _child = GetComponentInChildren<SpriteRenderer>().transform;
    }
    public void ObserveEvents()
    {
      //  EVent.Do.Subscribe<Ista>();
    }


    // Update is called once per frame
    void Update()
    {
        if (_active)
        {
            Vector3 rotationSpeed = new Vector3(0, 0, 30);
            _child.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
            // _horizontalSpeed = Input.GetAxis(_horizontal) * _speed;
            // _verticalSpeed = Input.GetAxis(_vertical) * _speed;

            //_myTransform.position += new Vector3(_horizontalSpeed, _verticalSpeed, 0);
        }    
    }

    public void ActivateObject(bool active) => _active = active;

    public void SwitchToInGame(InMenuOrGame inGame) => Debug.Log(inGame);
}
