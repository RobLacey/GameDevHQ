using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[System.Serializable]
public class UITooltip
{
    [Header("General Settings")]
    [SerializeField]
    private RectTransform _mainCanvas;
    [SerializeField] private Camera _uiCamera = null;
    [SerializeField] private TooltipType _tooltipType = TooltipType.Follow;
    [Header("Mouse & Global Fixed Position Settings", order = 2)]
    [SerializeField] [AllowNesting] [Label("Tooltip Position")]
    private ToolTipAnchor _toolTipPosition;
    [SerializeField] [AllowNesting] [ShowIf("Fixed")]
    private RectTransform _groupFixedPosition;
    [SerializeField] [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")]
    private float _mousePaddingX;
    [SerializeField] [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")]
    private float _mousePaddingY;
    [Header("Keyboard and Controller Settings")]
    [SerializeField] [AllowNesting] [Label("Tooltip Preset")] [HideIf("Fixed")]
    private UseSide _positionToUse = UseSide.ToTheRightOf;
    [SerializeField] [AllowNesting] [Label("Offset Position")] [HideIf("Fixed")]
    private ToolTipAnchor _keyboardPosition;
    [SerializeField] [AllowNesting] [Label("GameObject Marker")] [ShowIf("UseGameObject")]
    private RectTransform _fixedPosition;
    [SerializeField] [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")]
    private float _keyboardPaddingX;
    [SerializeField] [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")]
    private float _keyboardPaddingY;
    [Header("Other Settings")]
    [SerializeField] [Range(0f, 50f)]
    private float _screenSafeZone = 10;
    [SerializeField] [AllowNesting] [Label("Display Tooltip Delay")] public float _delay = 1f;
    [SerializeField] [AllowNesting] [ShowIf("BuildTips")] [Label("Delay Until Next..")]
    private float _buildDelay = 1f;
    [SerializeField] private LayoutGroup[] _listOfTooltips = new LayoutGroup[0];

    //Variables
    private GameObject _tooltipsParent;
    private string _toolTipBucketName = "ToolTipHolder";
    private Vector2 _tooltipPos;
    private LayoutGroup _layout;
    private RectTransform _rectTransform;
    private RectTransform[] _tooltipsRects;
    private Canvas _toolTipCanvas;
    private Canvas[] _cachedCanvas;
    private Vector3[] _myCorners = new Vector3[4];
    private Coroutine _coroutineBuild;
    private Coroutine _coroutineStart;
    private bool _allowKeys;
    private ToolTipsCalcs _calcs;

    //Enums & Properties
    private enum UseSide { ToTheRightOf, ToTheLeftOf, GameObjectAsPosition  }
    public bool IsActive { get; set; }
    public bool CanActivate { get; private set; }
    private Vector2 KeyboardPadding => new Vector2(_keyboardPaddingX, _keyboardPaddingY);
    private Vector2 MousePadding => new Vector2(_mousePaddingX, _mousePaddingY);


    //Editor Scripts
    public bool Fixed() { return _tooltipType == TooltipType.Fixed; }
    public bool FollowMouse() { return _tooltipType == TooltipType.Follow; }
    public bool BuildTips()  { return _listOfTooltips.Length > 1; }
    public bool UseGameObject() { return _positionToUse == UseSide.GameObjectAsPosition && _tooltipType != TooltipType.Fixed; }

    //TODO Change size calcs to work from camera size rather than canvas so still works when aspect changes
    
    public void OnAwake(Setting setting, string parent) //Make OnAwake
    {
        CanActivate = (setting & Setting.TooplTip) != 0;
        SetUp(parent);
    }

    private void SetUp(string parent)
    {
        if (!CanActivate) return;
        if (_listOfTooltips.Length == 0)
        {
            Debug.Log("No tooltips set on " + parent);
            return;
        }

        CreateBucket();
        SetUpTooltips();

        _calcs = new ToolTipsCalcs(_mainCanvas, _screenSafeZone);
        ChangeControl.DoAllowKeys += SaveAllowKeys;
    }

    private void SaveAllowKeys(bool allow)
    {
        _allowKeys = allow;
    }

    private void CreateBucket()
    {
        _tooltipsParent = GameObject.Find(_toolTipBucketName);
        if (!_tooltipsParent)
        {
            _tooltipsParent = new GameObject();
            _tooltipsParent.AddComponent<RectTransform>();
            _tooltipsParent.transform.SetParent(_mainCanvas.transform);
            _tooltipsParent.name = _toolTipBucketName;
            _tooltipsParent.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        }
        foreach (var tooltip in _listOfTooltips)
        {
            tooltip.transform.SetParent(_tooltipsParent.transform);
        }
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


    public void HideToolTip()
    {
        if (!CanActivate) return;
        StaticCoroutine.StopCoroutines(_coroutineBuild);
        StaticCoroutine.StopCoroutines(_coroutineStart);

        _toolTipCanvas.enabled = false;
        IsActive = false;
    }

    public IEnumerator StartToolTip(RectTransform rectForTooltip)
    {
        if (CanActivate)
        {
            yield return new WaitForSeconds(_delay);
            IsActive = true;
            _coroutineBuild = StaticCoroutine.StartCoroutine(ToolTipBuild(rectForTooltip));
            _coroutineStart = StaticCoroutine.StartCoroutine(ActivateTooltip(_allowKeys));
        }

        yield return null;
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


    public IEnumerator ActivateTooltip(bool isKeyboard)
    {
        while (IsActive)
        {
            SetToolTipPosition(isKeyboard);
            SetExactPosition(!isKeyboard ? _toolTipPosition : _keyboardPosition);
            _rectTransform.anchoredPosition = new Vector2(_tooltipPos.x, _tooltipPos.y);
            _toolTipCanvas.enabled = true;
            yield return null;
        }
        yield return null;
    }

    private void SetToolTipPosition(bool isKeyboard)
    {
        switch (_tooltipType)
        {
            case TooltipType.Follow when isKeyboard:
                SetKeyboardTooltipPosition();
                break;
            case TooltipType.Follow:
                SetMouseToolTipPosition();
                break;
            case TooltipType.Fixed:
                SetFixedToolTipPosition();
                break;
        }
    }

    private void SetFixedToolTipPosition()
    {
        var position = _groupFixedPosition.position;
        _tooltipPos = ReturnScreenPosition(position);
    }

    private void SetMouseToolTipPosition()
    {
        var position = Input.mousePosition;
        _tooltipPos = ReturnScreenPosition(position) + MousePadding;
    }
    
    private void SetKeyboardTooltipPosition()
    {
        Vector3 position;
        switch (_positionToUse)
        {
            case UseSide.ToTheRightOf:
                position = _myCorners[3] + ((_myCorners[2] - _myCorners[3]) / 2);
                break;
            case UseSide.ToTheLeftOf:
                position = _myCorners[1] + ((_myCorners[0] - _myCorners[1]) / 2);
                break;
            default:
                position = _fixedPosition.position;
                break;
        }
        _tooltipPos = ReturnScreenPosition(position) + KeyboardPadding;
    }
    
    private Vector2 ReturnScreenPosition(Vector3 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (_mainCanvas, position, _uiCamera, out var toolTipPos);
        return toolTipPos;
    }


    private void SetExactPosition(ToolTipAnchor toolTipAnchor)
    {
        (_tooltipPos, _rectTransform.pivot) = _calcs.CalcCentreClamp(_tooltipPos, toolTipAnchor, _layout);
    }
}