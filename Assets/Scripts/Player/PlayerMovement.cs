using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Vector3 _startPosition = default;
    [SerializeField] float _speed = 1f;
    [SerializeField] string _horizontalAxis = null;
    [SerializeField] string _verticalAxis = null;
    [SerializeField] float _normalSpeed = 5f;
    [SerializeField] float _speedBoostMulitplier = 1.5f;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] GlobalVariables _myVars;

    //Variables
    bool _speedBoostActive = false;

    private void Awake()
    {
        _speed = _normalSpeed;
    }

    private void OnEnable()
    {
        _Event_ActivatePowerUp.AddListener(x => AdjustSpeed(x), this);
        _Event_DeactivatePowerUp.AddListener(x => AdjustSpeed(x), this);
    }

    private void Start()
    {
        transform.position = _startPosition;
    }

    private void Update()
    {
        CalculateMovement();
        CheckBounds();
    }

    /// <summary> Calculates where to move player by getting inputs </summary>
    private void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis(_horizontalAxis);
        float verticalInput = Input.GetAxis(_verticalAxis);
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

        transform.Translate(direction * _speed * Time.deltaTime);
    }

    /// <summary> Sets the vertical boundry of the player movement. Can be changed via the associated variables </summary>
    private void CheckBounds()
    {
        float yPosition = Mathf.Clamp(transform.position.y, _myVars.BottomBounds, _myVars.TopBounds);
        float xPosition = Mathf.Clamp(transform.position.x, _myVars.LeftBounds, _myVars.RightBounds);
        transform.position = new Vector3(xPosition, yPosition, 0);
    }

    private void AdjustSpeed(object newPowerUp)
    {
        if ((PowerUpTypes)newPowerUp == PowerUpTypes.SpeedBoost && !_speedBoostActive)
        {
            _speed = _normalSpeed * _speedBoostMulitplier;
            _speedBoostActive = true;
        }
        else
        {
            _speed = _normalSpeed;
            _speedBoostActive = false;
        }
    }
}
