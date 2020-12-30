using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UIElements;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameObjectController : MonoBehaviour, IEventUser
{
    [SerializeField] private UIGameObject[] _playerObjects = default;
    [SerializeField] private IsActive _useCustomCursor = IsActive.No;
    [SerializeField] private Texture2D _cursor = default;
    [SerializeField] private Vector2 _hotSpot =default;
    [SerializeField] private RectTransform _virtualCursor = default;
    [SerializeField] private float _cursorSpeed = 1.5f;
    [SerializeField] private LayerMask _layerToHit;
    [InputAxis] public string _horizontal;
    [InputAxis] public string _vertical;
    
    /*[ShowNonSerializedField] */private bool _inGame = false;
    private Vector2 _newCursorPos = Vector2.zero;
    private int _index = 0;
    private readonly PointerEventData _pointerEventData = new PointerEventData(null);
    private GameObject _lastUI;
    private GameObject _lastGameObject;
    private Camera _mainCamera;
    private bool _noInput;
    private readonly Vector3 _direction2D = Vector3.forward;
    private readonly int _screenLeft = -Screen.width / 2;
    private readonly int _screenRight = Screen.width / 2;
    private readonly int _screenBottom = -Screen.height / 2;
    private readonly int _screenTop = Screen.height / 2;


    private void Awake()
    {
        _newCursorPos = Vector2.zero;
        _mainCamera = Camera.main;

        if (_useCustomCursor == IsActive.Yes)
        {
            Cursor.SetCursor(_cursor, _hotSpot, CursorMode.Auto);
        }
    }

    private void OnEnable()
    {
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IInMenu>(InGame);
   }

    private void InGame(IInMenu args)
    {
        _inGame = !args.InTheMenu;
        if (_inGame)
        {
            _playerObjects[_index].OverFocus();
        }
        else
        {
            _playerObjects[_index].UnFocus();
        }
    }

    private void Update()
    {
        SwapGameObjects();
        
        DoSelectedInGameObj();
        DoSelectedUI();
        
        _noInput = Input.GetAxis(_horizontal) == 0 && Input.GetAxis(_vertical) == 0;
        if(_noInput) return;
        
        MoveVirtualMouse();
        OverUi(DoUiRaycast());
    }

    private void SwapGameObjects()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _inGame)
        {
            Debug.Log("Space Pressed");
            _playerObjects[_index].UnFocus();
            _index = _index.PositiveIterate(_playerObjects.Length);
            _playerObjects[_index].OverFocus();
        }
    }

    private void FixedUpdate()
    {
        if(_noInput) return;
        OverGameObject(DoGameObjectRaycast());
    }

    private void DoSelectedInGameObj()
    {
        if (Input.GetKeyDown(KeyCode.Return) && _lastGameObject.IsNotNull())
        {
            _lastGameObject.GetComponent<ICursorHandler>().CursorDown();
        }
    }

    private void DoSelectedUI()
    {
        if (Input.GetKeyDown(KeyCode.Return) && _lastUI.IsNotNull())
        {
            _lastUI.gameObject.GetComponentInParent<UINode>().OnPointerDown(_pointerEventData);
        }
    }

    private void MoveVirtualMouse()
    {
        _newCursorPos.x = Input.GetAxis(_horizontal) * _cursorSpeed;
        _newCursorPos.y = Input.GetAxis(_vertical) * _cursorSpeed;
        CalculateNewPosition();
    }

    private void CalculateNewPosition()
    {
        var temp = _virtualCursor.anchoredPosition + _newCursorPos;
        temp.x = Mathf.Clamp(temp.x, _screenLeft, _screenRight);
        temp.y = Mathf.Clamp(temp.y, _screenBottom, _screenTop);
        _virtualCursor.anchoredPosition = temp;
    }

    private RaycastHit2D DoGameObjectRaycast()
    {
        var mousePos = _virtualCursor.position;
        mousePos.z = 10;
        var origin = _mainCamera.ScreenToWorldPoint(mousePos);
        return Physics2D.Raycast(origin, _direction2D, 0, _layerToHit);
    }
    
    private GameObject DoUiRaycast()
    {
        var results = new List<RaycastResult>();
        _pointerEventData.position = _virtualCursor.position;
        EventSystem.current.RaycastAll(_pointerEventData, results);
        return results.Count == 0 ? null : results[0].gameObject;
    }


    private void OverGameObject(RaycastHit2D hit)
    {
        if (hit.collider.IsNull())
        {
            if (_lastGameObject.IsNotNull())
            {
                _lastGameObject.GetComponent<ICursorHandler>().CursorExit();
                _lastGameObject = null;
            }
            return;
        }
        
        if (_lastGameObject.IsNotNull())
        {
            if (_lastGameObject == hit.collider.gameObject) return;
            _lastGameObject.GetComponent<ICursorHandler>().CursorExit();
        }

        _lastGameObject = hit.collider.gameObject;
        hit.collider.gameObject.GetComponent<ICursorHandler>().CursorEnter();
    }

    private void OverUi(GameObject hit)
    {
        if(hit.IsNull())
        {
            if(_lastUI.IsNotNull())
            {
                _lastUI.GetComponentInParent<IPointerExitHandler>().OnPointerExit(_pointerEventData);
                _lastUI = null;
            }            
            return;
        }
        
        if (_lastUI.IsNotNull())
        {
            if (_lastUI == hit) return;
            _lastUI.GetComponentInParent<IPointerExitHandler>().OnPointerExit(_pointerEventData);
        }

        _lastUI = hit;
        _lastUI.GetComponentInParent<IPointerEnterHandler>().OnPointerEnter(_pointerEventData);
    }
    
    private void Cast3D()
    {
        _virtualCursor.position = Input.mousePosition;
        var mousePos = _virtualCursor.position;
        mousePos.z = -10;
        var pos = Camera.main.ScreenToWorldPoint(mousePos);
        var direction = (Camera.main.transform.position - pos).normalized;
        var origin = Camera.main.transform.position;
        float laserLength = 1000f;

        RaycastHit[] hit3D; 
        hit3D = Physics.RaycastAll(origin ,-direction, laserLength);
        Debug.DrawRay(origin, direction * laserLength, Color.yellow);
        if (hit3D.Length > 0)
        {
            //Hit something, print the tag of the object
            Debug.Log("Hitting: " + hit3D[0]);
        }
    }
}
