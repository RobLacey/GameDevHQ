using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[System.Serializable]
public class UITooltip : NodeFunctionBase
{
    [Header("General Settings")]
    [SerializeField] private RectTransform _mainCanvas;
    [SerializeField] private Camera _uiCamera;
    [SerializeField] private TooltipType _tooltipType = TooltipType.Follow;
    
    [Header("Mouse & Global Fixed Position Settings", order = 2)]
    [SerializeField] 
    [AllowNesting] [Label("Tooltip Position")] private ToolTipAnchor _toolTipPosition;
    [SerializeField] 
    [AllowNesting] [ShowIf("Fixed")] private RectTransform _groupFixedPosition;
    [SerializeField] 
    [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")] private float _mousePaddingX;
    [SerializeField] 
    [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")] private float _mousePaddingY;
    
    [Header("Keyboard and Controller Settings")]
    [SerializeField] 
    [AllowNesting] [Label("Tooltip Preset")] [HideIf("Fixed")] private UseSide _positionToUse = UseSide.ToTheRightOf;
    [SerializeField] 
    [AllowNesting] [Label("Offset Position")] [HideIf("Fixed")] private ToolTipAnchor _keyboardPosition;
    [SerializeField] 
    [AllowNesting] [Label("GameObject Marker")] [ShowIf("UseGameObject")] private RectTransform _fixedPosition;
    [SerializeField] 
    [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")] private float _keyboardPaddingX;
    [SerializeField] 
    [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")] private float _keyboardPaddingY;
    
    [Header("Other Settings")]
    [SerializeField] 
    [Range(0f, 50f)] private float _screenSafeZone = 10;
    [SerializeField] 
    [AllowNesting] [Label("Display Tooltip Delay")] private float _delay = 1f;
    [SerializeField] 
    [AllowNesting] [ShowIf("BuildTips")] [Label("Delay Until Next..")] private float _buildDelay = 1f;
    [SerializeField] 
    private LayoutGroup[] _listOfTooltips = new LayoutGroup[0];

    //Variables
    private Vector2 _tooltipPos;
    private Vector3 _anchoredPosition;
    private RectTransform[] _tooltipsRects;
    private RectTransform _parentRectTransform;
    private Canvas[] _cachedCanvas;
    private Vector3[] _myCorners = new Vector3[4];
    private Coroutine _coroutineStart, _coroutineActivate, _coroutineBuild;
    private bool _allowKeys, _setCorners;
    private ToolTipsCalcs _calculation;
    private int _index;
    private UIControlsEvents _uiControlsEvents = new UIControlsEvents();

    //Enums & Properties
    private enum UseSide { ToTheRightOf, ToTheLeftOf, GameObjectAsPosition  }
    private Vector2 KeyboardPadding => new Vector2(_keyboardPaddingX, _keyboardPaddingY);
    private Vector2 MousePadding => new Vector2(_mousePaddingX, _mousePaddingY);
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => false;
    private protected override void ProcessPress() { }
    protected override bool FunctionNotActive() => !CanActivate || _listOfTooltips.Length == 0;
    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        HideToolTip();
    }
    private void SaveAllowKeys(bool allow) => _allowKeys = allow;

    //Editor Scripts
    public bool Fixed() => _tooltipType == TooltipType.Fixed;
    public bool FollowMouse() => _tooltipType == TooltipType.Follow;
    public bool BuildTips() => _listOfTooltips.Length > 1;
    public bool UseGameObject() => _positionToUse == UseSide.GameObjectAsPosition && _tooltipType != TooltipType.Fixed;


    //TODO Change size calculations to work from camera size rather than canvas so still works when aspect changes

    public void OnAwake(UiActions uiActions, Setting activeFunctions, RectTransform rectTransform) 
    {
        base.OnAwake(uiActions, activeFunctions);
        CanActivate = (_enabledFunctions & Setting.TooplTip) != 0;
        SetUp(rectTransform);
    }

    private void SetUp(RectTransform rectTransform)
    {
        if (!CanActivate) return;
        _parentRectTransform = rectTransform;
        _anchoredPosition = _mainCanvas.anchoredPosition;
        _calculation = new ToolTipsCalcs(_mainCanvas, _screenSafeZone);
        _uiControlsEvents.SubscribeToAllowKeys(SaveAllowKeys);
        SetUpTooltips();
        CheckSetUpForError();
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive()) return;
        _pointerOver = pointerOver;
        
        if(pointerOver)
        {
            _coroutineStart = StaticCoroutine.StartCoroutine(StartToolTip());
        }
        else 
        {
            HideToolTip();
        }
    }

    private void CheckSetUpForError()
    {
        if (_listOfTooltips.Length == 0)
            throw new Exception("No tooltips set");
    }

    private void SetUpTooltips()
    {
        if (_listOfTooltips.Length <= 1) _buildDelay = 0;
        _tooltipsRects = new RectTransform[_listOfTooltips.Length];
        _cachedCanvas = new Canvas[_listOfTooltips.Length];

        for (int index = 0; index < _listOfTooltips.Length; index++)
        {
            _tooltipsRects[index] = _listOfTooltips[index].GetComponent<RectTransform>();
            _cachedCanvas[index] = _listOfTooltips[index].GetComponent<Canvas>();
            _cachedCanvas[index].enabled = false;
        }
    }

    public void HideToolTip()
    {
        if (!CanActivate) return;
        StaticCoroutine.StopCoroutines(_coroutineStart);
        StaticCoroutine.StopCoroutines(_coroutineBuild);
        StaticCoroutine.StopCoroutines(_coroutineActivate);
        _cachedCanvas[_index].enabled = false;
    }
    
    private IEnumerator StartToolTip()
    {
        SetCorners();
        yield return new WaitForSeconds(_delay);
        _coroutineBuild = StaticCoroutine.StartCoroutine(ToolTipBuild());
        _coroutineActivate = StaticCoroutine.StartCoroutine(ActivateTooltip(_allowKeys));
    }

    private void SetCorners()
    {
        if(_setCorners) return;
        _setCorners = true;
        _parentRectTransform.GetWorldCorners(_myCorners);
    }
    
    private IEnumerator ToolTipBuild()
    {
        for (int toolTipIndex = 0; toolTipIndex < _listOfTooltips.Length; toolTipIndex++)
        {
            _index = toolTipIndex;
            yield return new WaitForSeconds(_buildDelay);
            _cachedCanvas[_index].enabled = false;
        }
        yield return null;
    }


    private IEnumerator ActivateTooltip(bool isKeyboard)
    {
        while (_pointerOver)
        {
            GetToolTipsScreenPosition(isKeyboard, _tooltipType);
            SetExactPosition(!isKeyboard ? _toolTipPosition : _keyboardPosition);
            _cachedCanvas[_index].enabled = true;
            yield return null;
        }
        yield return null;
    }

    private void GetToolTipsScreenPosition(bool isKeyboard, TooltipType toolTipType)
    {
        switch (toolTipType)
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

    private void SetFixedToolTipPosition() => _tooltipPos = ReturnScreenPosition(_groupFixedPosition.position);

    private void SetMouseToolTipPosition() => _tooltipPos = ReturnScreenPosition(Input.mousePosition) + MousePadding;

    private void SetKeyboardTooltipPosition()
    {
        var position = Vector3.zero;
        switch (_positionToUse)
        {
            case UseSide.ToTheRightOf:
                position = _myCorners[3] + ((_myCorners[2] - _myCorners[3]) / 2);
                break;
            case UseSide.ToTheLeftOf:
                position = _myCorners[1] + ((_myCorners[0] - _myCorners[1]) / 2);
                break;
            case UseSide.GameObjectAsPosition:
                position = _fixedPosition.position;
                break;
        }
        _tooltipPos = ReturnScreenPosition(position) + KeyboardPadding;
    }
    
    
    private Vector2 ReturnScreenPosition(Vector3 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (_mainCanvas, screenPosition, _uiCamera, out var toolTipPos);
        return toolTipPos;
    }

    private void SetExactPosition(ToolTipAnchor toolTipAnchor)
    {
        var position = _parentRectTransform.transform.position;
        var offset = new Vector2(_anchoredPosition.x - position.x, 
                                 _anchoredPosition.y - position.y);
        var size = new Vector2( _listOfTooltips[_index].preferredWidth
                                        , _listOfTooltips[_index].preferredHeight);

        (_tooltipsRects[_index].anchoredPosition, _tooltipsRects[_index].pivot)
            = _calculation.CalculatePosition(_tooltipPos, offset, size, toolTipAnchor);
    }
}