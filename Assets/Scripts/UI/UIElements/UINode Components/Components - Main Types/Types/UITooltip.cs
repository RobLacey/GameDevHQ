using System;
using System.Collections;
using DG.Tweening;
using EZ.Inject;
using EZ.Service;
using UIElements;
using UnityEngine;
using UnityEngine.UI;


public interface IToolTipData: IParameters
{
    RectTransform FixedPosition { get; }
    Camera UiCamera { get; }
    LayoutGroup[] ListOfTooltips { get; }
    int CurrentToolTipIndex { get; }
    RectTransform[] ToolTipsRects { get; }
    Vector3[] MyCorners { get; }
    ToolTipScheme Scheme { get; }
    RectTransform ParentRectTransform { get; }
}


public class UITooltip : NodeFunctionBase, IToolTipData
{
    public UITooltip(ITooltipSettings settings) : base(settings.UiNodeEvents)
    {
        _settings = settings;
        FixedPosition = settings.FixedPosition;
        UiCamera = settings.UiCamera;
        CanActivate = true;
    }

    //Variables
    private Vector2 _tooltipPos;
    private Canvas[] _cachedToolTipCanvasList;
    private Coroutine _coroutineStart, _coroutineActivate, _coroutineBuild;
    private bool _allowKeys;
    private float _buildDelay;
    private IToolTipFade _toolTipFade;
    private IGetScreenPosition _getScreenPosition;
    private readonly ITooltipSettings _settings;
    private ICanvasOrderData _canvasOrderData;

    //Properties
    public ToolTipScheme Scheme => _settings.Scheme;
    public RectTransform FixedPosition { get; private set; }
    public Camera UiCamera { get; }
    public int CurrentToolTipIndex { get; private set; }
    public LayoutGroup[] ListOfTooltips => _settings.ToolTips;
    public RectTransform[] ToolTipsRects { get; private set; }
    public Vector3[] MyCorners { get; } = new Vector3[4];
    public RectTransform ParentRectTransform { get; private set; }


    //Set / Getters
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => false;
    private protected override void ProcessPress() { }
    protected override bool FunctionNotActive() => !CanActivate || ListOfTooltips.Length == 0 || Scheme is null;
    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        HideToolTip();
    }
    
    private void SaveAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
    
    //TODO Change size calculations to work from camera size rather than canvas so still works when aspect changes

    public override void OnAwake() 
    {
        base.OnAwake();
        SetUp();
        SetTooltipsVariables();
        _toolTipFade = EZInject.Class.WithParams<IToolTipFade>(this);
        _getScreenPosition = EZInject.Class.WithParams<IGetScreenPosition>(this);
        _getScreenPosition.OnAwake();
    }

    private void SetUp()
    {
        if (FunctionNotActive()) return;
        CheckSetUpForError();
        SetUpTooltips();
    }

    public void SetFixedPositionAtRuntime(RectTransform fixPos) => FixedPosition = fixPos;

    private void SetTooltipsVariables()
    {
        ParentRectTransform = _uiEvents.ReturnMasterNode.GetComponent<RectTransform>();
        ParentRectTransform.GetLocalCorners(MyCorners);
        SetFixedPositionToDefault();
    }

    private void SetFixedPositionToDefault()
    {
        if (FixedPosition.Equals(null))
            FixedPosition = ParentRectTransform;
    }

    private void SetUpTooltips()
    {
        if (ListOfTooltips.Length > 1)
            _buildDelay = Scheme.BuildDelay;
        
        ToolTipsRects = new RectTransform[ListOfTooltips.Length];
        _cachedToolTipCanvasList = new Canvas[ListOfTooltips.Length];

        GetRectAndCanvasForEachToolTip();
    }

    private void GetRectAndCanvasForEachToolTip()
    {
        for (int index = 0; index < ListOfTooltips.Length; index++)
        {
            ToolTipsRects[index] = ListOfTooltips[index].GetComponent<RectTransform>();
            _cachedToolTipCanvasList[index] = ListOfTooltips[index].GetComponent<Canvas>();
            _cachedToolTipCanvasList[index].enabled = false;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _getScreenPosition.OnEnable();
    }

    protected override void LateStartSetUp()
    {
        base.LateStartSetUp();
        if (MyHubDataIsNull) return;
        _getScreenPosition.LateStartSetUp();
        _allowKeys = _myDataHub.AllowKeys;
    }

    public override void UseEZServiceLocator()
    {
        base.UseEZServiceLocator();
        _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        InputEvents.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        InputEvents.Do.Subscribe<ISwitchGroupPressed>(CloseTooltipImmediately);
        BranchEvent.Do.Subscribe<IClearScreen>(CloseTooltipImmediately);
        InputEvents.Do.Subscribe<IHotKeyPressed>(CloseTooltipImmediately);
    }

    protected override void UnObserveEvents()
    {
        base.UnObserveEvents();
        InputEvents.Do.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        InputEvents.Do.Unsubscribe<ISwitchGroupPressed>(CloseTooltipImmediately);
        BranchEvent.Do.Unsubscribe<IClearScreen>(CloseTooltipImmediately);
        InputEvents.Do.Unsubscribe<IHotKeyPressed>(CloseTooltipImmediately);
    }

    public override void OnDisable()
    {
        HideToolTip();
        base.OnDisable();
        UnObserveEvents();
        _getScreenPosition.OnDisable();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnObserveEvents();
        _getScreenPosition.OnDestroy();
        _toolTipFade = null;
        _getScreenPosition = null;
    }

    private void CheckSetUpForError()
    {
        if (ListOfTooltips.Length == 0)
            throw new Exception("No tooltips set");
    }

    public override void OnStart()
    {
        base.OnStart();
        _getScreenPosition.OnStart();
        SetToolTipCanvasOrder();
    }

    //Main
    private void SetToolTipCanvasOrder()
    {
        foreach (var canvas in _cachedToolTipCanvasList)
        {
            SetCanvasOrderUtil.Set(_canvasOrderData.ReturnToolTipCanvasOrder, canvas);
        }
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive()) return;
        
        if(pointerOver)
        {
            if(_pointerOver) return;
            _coroutineStart = StaticCoroutine.StartCoroutine(StartToolTip());
        }
        else 
        {
            if(!_pointerOver) return;
            HideToolTip();
        }
        _pointerOver = pointerOver;
    }

    private void CloseTooltipImmediately(ISwitchGroupPressed args) => ImmediateClose();
    private void CloseTooltipImmediately(IClearScreen args) => ImmediateClose();
    private void CloseTooltipImmediately(IHotKeyPressed args) => ImmediateClose();

    private void ImmediateClose()
    {
        if (_pointerOver)
            SavePointerStatus(false);
    }

    private void HideToolTip()
    {
        if (!CanActivate) return;
        StaticCoroutine.StopCoroutines(_coroutineStart);
        StaticCoroutine.StopCoroutines(_coroutineBuild);
        StaticCoroutine.StopCoroutines(_coroutineActivate);
        _cachedToolTipCanvasList[CurrentToolTipIndex].enabled = false;
        CurrentToolTipIndex = 0;
    }
    
    private IEnumerator StartToolTip()
    {
        yield return new WaitForSeconds(Scheme.StartDelay);
        _coroutineBuild = StaticCoroutine.StartCoroutine(ToolTipBuild());
        _coroutineActivate = StaticCoroutine.StartCoroutine(ActivateTooltip(_allowKeys));
    }

    private IEnumerator ToolTipBuild()
    {
        for (int toolTipIndex = 0; toolTipIndex < ListOfTooltips.Length; toolTipIndex++)
        {
            if(toolTipIndex > 0)
            {
                yield return FadeLastToolTipOut(toolTipIndex - 1).WaitForCompletion();
                _cachedToolTipCanvasList[toolTipIndex - 1].enabled = false;
            }
            CurrentToolTipIndex = toolTipIndex;
            _cachedToolTipCanvasList[toolTipIndex].enabled = true;
            yield return FadeNextToolTipIn(toolTipIndex).WaitForCompletion();
            yield return new WaitForSeconds(_buildDelay);
        }
        yield return null;
    }

    private Tween FadeLastToolTipOut(int toolTipIndex)
    {
        var iD = ToolTipsRects[toolTipIndex].GetInstanceID();
        return _toolTipFade.SetTweenTime(Scheme.FadeOutTime)
                           .StartFadeOut(iD);
    }
    
    private Tween FadeNextToolTipIn(int toolTipIndex)
    {
        var iD = ToolTipsRects[toolTipIndex].GetInstanceID();
        return _toolTipFade.SetTweenTime(Scheme.FadeInTime)
                           .StartFadeIn(iD);
    }

    private IEnumerator ActivateTooltip(bool isKeyboard)
    {
        while (_pointerOver)
        {
            _getScreenPosition.SetExactPosition(isKeyboard);
            yield return null;
        }
        yield return null;
    }
}

