using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[System.Serializable]
public class UITooltip
{
    [Header("General Settings")]
    [SerializeField] RectTransform _mainCanvas;
    [SerializeField] Camera _UICamera = null;
    [SerializeField] TooltipType _tooltipType = TooltipType.Follow;
    [Header("Mouse & Global Fixed Posiiton Settings", order = 2)]
    [SerializeField] [AllowNesting] [Label("Tooltip Position")] ToolTipAnchor _toolTipPosition;
    [SerializeField] [AllowNesting] [ShowIf("Fixed")] RectTransform _groupFixedPosition;
    [SerializeField] [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")] float _mousePaddingX;
    [SerializeField] [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")] float _mousePaddingY;
    [Header("Keyboard and Controller Settings")]
    [SerializeField] [AllowNesting] [Label("Tooltip Preset")] [HideIf("Fixed")] UseSide _positionToUse = UseSide.ToTheRightOf;
    [SerializeField] [AllowNesting] [Label("Offset Position")] [HideIf("Fixed")] ToolTipAnchor _keyboardPosition;
    [SerializeField] [AllowNesting] [Label("GameObject Marker")] [ShowIf("UseGameObject")] RectTransform _fixedPosition;
    [SerializeField] [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")] float _keyboardPaddingX;
    [SerializeField] [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")] float _keyboardPaddingY;
    [Header("Other Settings")]
    [SerializeField] [Range(0f, 50f)] float _screenSafeZone = 10;
    [SerializeField] [AllowNesting] [Label("Display Tooltip Delay")] public float _delay = 1f;
    [SerializeField] [AllowNesting] [ShowIf("BuildTips")] [Label("Delay Unitl Next..")] float _buildDelay = 1f;
    [SerializeField] LayoutGroup[] _listOfTooltips = new LayoutGroup[0];

    //Variables
    GameObject _tooltipsParent;
    string _toolTipBucketName = "ToolTipHolder";
    Vector2 tooltipPos;
    LayoutGroup _layout;
    RectTransform _rectTransform;
    RectTransform[] _tooltipsRects;
    Canvas _toolTipCanvas;
    Canvas[] _cachedCanvas;
    float _canvasWidth;
    float _canvasHeight;
    Setting _mySetting = Setting.TooplTip;
    Vector3[] _myCorners = new Vector3[4];

    //Enums & Properties
    enum UseSide { ToTheRightOf, ToTheLeftOf, GameObjectAsPosition  }
    public bool IsActive { get; set; }

    //Editor Scripts
    public bool Fixed() { return _tooltipType == TooltipType.Fixed; }
    public bool FollowMouse() { return _tooltipType == TooltipType.Follow; }
    public bool BuildTips()  { return _listOfTooltips.Length > 1; }
    public bool UseGameObject() { return _positionToUse == UseSide.GameObjectAsPosition && _tooltipType != TooltipType.Fixed; }

    //TODO Change size calcs to work from camera size rather than canvas so still works when aspect changes

    public void OnAwake(Setting setting, string parent) //Make OnAwake
    {
        if (!((setting & _mySetting) != 0)) return;

        if (_listOfTooltips.Length == 0)
        {
            Debug.Log("No tooltips set on " + parent);
            return;
        }

        CreateBucket();

        SetUpTooltips();

        //Debug.Log(Camera.main.pixelWidth + "Camera");
        //Debug.Log(_mainCanvas.rect.width + "Canvas");
        _canvasWidth = (_mainCanvas.rect.width / 2) - _screenSafeZone;
        _canvasHeight = (_mainCanvas.rect.height / 2) - _screenSafeZone;
    }

    private void SetUpTooltips()
    {
        IsActive = false;
        if (_listOfTooltips.Length <= 1) _buildDelay = 0;
        _tooltipsRects = new RectTransform[_listOfTooltips.Length];
        _cachedCanvas = new Canvas[_listOfTooltips.Length];

        for (int i = 0; i < _listOfTooltips.Length; i++)
        {
            _tooltipsRects[i] = _listOfTooltips[i].GetComponent<RectTransform>();
            _cachedCanvas[i] = _listOfTooltips[i].GetComponent<Canvas>();
            _cachedCanvas[i].enabled = false;
        }

        _toolTipCanvas = _cachedCanvas[0];
    }


    private void CreateBucket()
    {
        _tooltipsParent = GameObject.Find(_toolTipBucketName);
        if (!_tooltipsParent)
        {
            _tooltipsParent = new GameObject();
            _tooltipsParent.AddComponent<RectTransform>();
            _tooltipsParent.transform.parent = _mainCanvas.transform;
            _tooltipsParent.name = _toolTipBucketName;
            _tooltipsParent.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
    }

    public void HideToolTip(Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;
        
        _toolTipCanvas.enabled = false;
        IsActive = false;
    }

    public IEnumerator ToolTipBuild(RectTransform rect)
    {
        rect.GetWorldCorners(_myCorners);
        for (int i = 0; i < _listOfTooltips.Length; i++)
        {
            _layout = _listOfTooltips[i];
            if (i - 1 >= 0) _cachedCanvas[i - 1].enabled = false;
            _toolTipCanvas = _cachedCanvas[i];
            _rectTransform = _tooltipsRects[i];
            yield return new WaitForSeconds(_buildDelay);
        }
        yield return null;
    }


    public IEnumerator StartTooltip(bool isKeyboard)
    {
        while (IsActive)
        {
            if (_tooltipsParent != null) _layout.transform.parent = _tooltipsParent.transform;
            SetToolTipPosition(isKeyboard);

            if (!isKeyboard)
            {
                SetExactPosition(_toolTipPosition);
            }
            else
            {
                SetExactPosition(_keyboardPosition);
            }

            _rectTransform.anchoredPosition = new Vector2(tooltipPos.x, tooltipPos.y);
            _toolTipCanvas.enabled = true;
            if (_tooltipsParent != null) _layout.transform.parent = _tooltipsParent.transform;
            yield return null;
        }
        yield return null;
    }

    private void SetToolTipPosition(bool iskeyBoard)
    {
        Vector3 position = Vector3.zero;

        if (_tooltipType == TooltipType.Follow)
        {
            if (iskeyBoard)
            {
                if (_positionToUse == UseSide.ToTheRightOf)
                {
                    position = _myCorners[3] + ((_myCorners[2] - _myCorners[3]) / 2);
                }
                else if (_positionToUse == UseSide.ToTheLeftOf)
                {
                    position = _myCorners[1] + ((_myCorners[0] - _myCorners[1]) / 2);
                }
                else
                {
                    position = _fixedPosition.position;
                }
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvas, position, _UICamera, out tooltipPos);
                tooltipPos += new Vector2(_keyboardPaddingX, _keyboardPaddingY);
            }
            else
            {
                position = Input.mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvas, position, _UICamera, out tooltipPos);
                tooltipPos += new Vector2(_mousePaddingX, _mousePaddingY);
            }
        }
        else if (_tooltipType == TooltipType.Fixed)
        {
            position = _groupFixedPosition.position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvas, position, _UICamera, out tooltipPos);
        }
    }


    private void SetExactPosition(ToolTipAnchor toolTipAnchor)
    {
        switch (toolTipAnchor)
        {
            case ToolTipAnchor.Centre:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth) + (_layout.preferredWidth / 2),
                                                            (_canvasWidth) - (_layout.preferredWidth / 2));
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, (_layout.preferredHeight / 2) + (-_canvasHeight),
                                                            _canvasHeight - (_layout.preferredHeight / 2));
                _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            case ToolTipAnchor.middleLeft:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth) + _layout.preferredWidth, (_canvasWidth));
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, (_layout.preferredHeight / 2) + (-_canvasHeight),
                                                            _canvasHeight - (_layout.preferredHeight / 2));
                _rectTransform.pivot = new Vector2(1f, 0.5f);
                break;
            case ToolTipAnchor.middleRight:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth), (_canvasWidth) - _layout.preferredWidth);
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, (_layout.preferredHeight / 2) + (-_canvasHeight),
                                                            _canvasHeight - (_layout.preferredHeight / 2));
                _rectTransform.pivot = new Vector2(0f, 0.5f);
                break;
            case ToolTipAnchor.MiddleTop:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth) + (_layout.preferredWidth / 2),
                                                            (_canvasWidth) - (_layout.preferredWidth / 2));
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, (-_canvasHeight), (_canvasHeight) - _layout.preferredHeight);
                _rectTransform.pivot = new Vector2(0.5f, 0f);
                break;
            case ToolTipAnchor.MiddleBottom:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth) + (_layout.preferredWidth / 2),
                                                            (_canvasWidth) - (_layout.preferredWidth / 2));
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, _layout.preferredHeight + (-_canvasHeight), _canvasHeight);
                _rectTransform.pivot = new Vector2(0.5f, 1f);
                break;
            case ToolTipAnchor.TopLeft:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth) + _layout.preferredWidth, (_canvasWidth));
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, (-_canvasHeight), (_canvasHeight) - _layout.preferredHeight);
                _rectTransform.pivot = new Vector2(1f, 0f);
                break;
            case ToolTipAnchor.TopRight:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth), (_canvasWidth) - _layout.preferredWidth);
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, (-_canvasHeight), (_canvasHeight) - _layout.preferredHeight);
                _rectTransform.pivot = new Vector2(0f, 0f);
                break;
            case ToolTipAnchor.BottomLeft:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth) + _layout.preferredWidth, (_canvasWidth));
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, _layout.preferredHeight + (-_canvasHeight), _canvasHeight);
                _rectTransform.pivot = new Vector2(1f, 1f);
                break;
            case ToolTipAnchor.BottomRight:
                tooltipPos.x = Mathf.Clamp(tooltipPos.x, (-_canvasWidth), (_canvasWidth) - _layout.preferredWidth);
                tooltipPos.y = Mathf.Clamp(tooltipPos.y, _layout.preferredHeight + (-_canvasHeight), _canvasHeight);
                _rectTransform.pivot = new Vector2(0f, 1f);
                break;
            default:
                break;
        }
    }
}



