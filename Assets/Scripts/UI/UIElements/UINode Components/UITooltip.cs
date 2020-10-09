using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UITooltip : NodeFunctionBase,  IServiceUser
{
    [Header("General Settings")]
    [SerializeField] 
    private RectTransform _mainCanvas;
    [SerializeField] 
    private Camera _uiCamera;
    [SerializeField] 
    [AllowNesting] [ValidateInput("IsNull", "Add a Tooltip Scheme")] private ToolTipScheme _scheme;
    [SerializeField] 
    private LayoutGroup[] _listOfTooltips = new LayoutGroup[0];

    //Variables
    private Vector2 _tooltipPos;
    private RectTransform[] _tooltipsRects;
    private RectTransform _parentRectTransform;
    private Transform _bucketPosition;
    private Canvas[] _cachedCanvas;
    private Vector3[] _myCorners = new Vector3[4];
    private Coroutine _coroutineStart, _coroutineActivate, _coroutineBuild;
    private bool _allowKeys, _setCorners;
    private ToolTipsCalcs _calculation;
    private int _index;
    private float _buildDelay;
    private UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private IBucketCreator _bucketCreator;

    //Enums & Properties
    private Vector2 KeyboardPadding => new Vector2(_scheme.KeyboardXPadding, _scheme.KeyboardYPadding);
    private Vector2 MousePadding => new Vector2(_scheme.MouseXPadding, _scheme.MouseYPadding);
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => false;
    private protected override void ProcessPress() { }
    protected override bool FunctionNotActive() => !CanActivate || _listOfTooltips.Length == 0 || _scheme is null;
    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        HideToolTip();
    }
    private void SaveAllowKeys(bool allow) => _allowKeys = allow;

    //Editor Scripts
    private bool IsNull(ToolTipScheme scheme) => scheme;
    
    //TODO Change size calculations to work from camera size rather than canvas so still works when aspect changes

    public void OnAwake(UiActions uiActions, Setting activeFunctions, RectTransform rectTransform) 
    {
        base.OnAwake(uiActions, activeFunctions);
        CanActivate = (_enabledFunctions & Setting.TooplTip) != 0;
        SetUp(rectTransform);
        SubscribeToService();
    }

    private void SetUp(RectTransform rectTransform)
    {
        if (FunctionNotActive()) return;
        _parentRectTransform = rectTransform;
        _calculation = new ToolTipsCalcs(_mainCanvas, _scheme.ScreenSafeZone);
        _uiControlsEvents.SubscribeToAllowKeys(SaveAllowKeys);
        SetUpTooltips();
        CheckSetUpForError();
    }
    
    public void SubscribeToService()
    {
        _bucketCreator = ServiceLocator.GetNewService<IBucketCreator>(this);
        //return _bucketCreator is null;
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
        if (_listOfTooltips.Length > 1)
            _buildDelay = _scheme.BuildDelay;
        
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
        SetBucket();
        yield return new WaitForSeconds(_scheme.StartDelay);
        _coroutineBuild = StaticCoroutine.StartCoroutine(ToolTipBuild());
        _coroutineActivate = StaticCoroutine.StartCoroutine(ActivateTooltip(_allowKeys));
    }

    private void SetCorners()
    {
        if(_setCorners) return;
        _setCorners = true;
        _parentRectTransform.GetWorldCorners(_myCorners);
    }

    private void SetBucket()
    {
        if(!(_bucketPosition is null)) return;
        _bucketPosition = _bucketCreator.CreateBucket();
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
            GetToolTipsScreenPosition(isKeyboard, _scheme.ToolTipType);
            SetExactPosition(!isKeyboard ? _scheme.ToolTipPosition : _scheme.KeyboardPosition);
            _tooltipsRects[_index].SetParent(_bucketPosition);
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

    private void SetFixedToolTipPosition() => _tooltipPos = ReturnScreenPosition(_scheme.GroupFixedPosition.position);

    private void SetMouseToolTipPosition() => _tooltipPos = ReturnScreenPosition(Input.mousePosition) + MousePadding;

    private void SetKeyboardTooltipPosition()
    {
        var position = Vector3.zero;
        switch (_scheme.PositionToUse)
        {
            case UseSide.ToTheRightOf:
                position = _myCorners[3] + ((_myCorners[2] - _myCorners[3]) / 2);
                break;
            case UseSide.ToTheLeftOf:
                position = _myCorners[1] + ((_myCorners[0] - _myCorners[1]) / 2);
                break;
            case UseSide.GameObjectAsPosition:
                position = _scheme.FixedPosition.position;
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
        var size = new Vector2( _listOfTooltips[_index].preferredWidth
                                        , _listOfTooltips[_index].preferredHeight);

        (_tooltipsRects[_index].anchoredPosition, _tooltipsRects[_index].pivot)
            = _calculation.CalculatePosition(_tooltipPos, size, toolTipAnchor);
    }

}