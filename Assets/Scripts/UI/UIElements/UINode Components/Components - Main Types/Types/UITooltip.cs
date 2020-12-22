using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public interface IToolTipData: IParameters
{
    RectTransform MainCanvas { get; }
    Camera UiCamera { get; }
    LayoutGroup[] ListOfTooltips { get; }
    int CurrentToolTipIndex { get; }
    RectTransform[] ToolTipsRects { get; }
    Vector3[] MyCorners { get; }
    ToolTipScheme Scheme { get; }
}

public interface IToolTipCanvasOrder
{
    int ToolTipCanvasOrder { set; }
}

public class UITooltip : NodeFunctionBase, IToolTipData, IToolTipCanvasOrder
{
    public UITooltip(ITooltipSettings settings)
    {
        MainCanvas = settings.UiNodeEvents.ReturnMasterNode.MainCanvas;
        UiCamera = settings.UiCamera;
        Scheme = settings.Scheme;
        ListOfTooltips = settings.ToolTips;
        CanActivate = true;
        OnAwake(settings.UiNodeEvents);
    }

    //Variables
    private Vector2 _tooltipPos;
    private Canvas[] _cachedToolTipCanvasList;
    private Coroutine _coroutineStart, _coroutineActivate, _coroutineBuild;
    private bool _allowKeys;
    private float _buildDelay;
    private IToolTipFade _toolTipFade;
    private IGetScreenPosition _getScreenPosition;

    //Properties
    public ToolTipScheme Scheme { get; }
    public RectTransform MainCanvas { get; }
    public Camera UiCamera { get; }
    public int CurrentToolTipIndex { get; private set; }
    public LayoutGroup[] ListOfTooltips { get; }
    public RectTransform[] ToolTipsRects { get; private set; }
    public Vector3[] MyCorners { get; } = new Vector3[4];
    public int ToolTipCanvasOrder { private get; set; }

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

    protected sealed override void OnAwake(IUiEvents uiEvents) 
    {
        base.OnAwake(uiEvents);
        SetUp(uiEvents.ReturnMasterNode.GetComponent<RectTransform>());
        SetUToolTipContainerObject();
        _toolTipFade = EJect.Class.WithParams<IToolTipFade>(this);
        _getScreenPosition = EJect.Class.WithParams<IGetScreenPosition>(this);
    }

    private void SetUp(RectTransform parentRectTransform)
    {
        if (FunctionNotActive()) return;
        SetUpTooltips();
        SetCorners(parentRectTransform);
        CheckSetUpForError();
    }

    private void SetCorners(RectTransform parentRectTransform) => parentRectTransform.GetWorldCorners(MyCorners);

    private void SetUToolTipContainerObject()
    {
        var bucketPosition = EJect.Class.NoParams<IBucketCreator>()
                                  .SetName("ToolTip Holder")
                                  .SetParent(MainCanvas.transform)
                                  .CreateBucket();
        
        foreach (var tooltip in ToolTipsRects)
        {
            tooltip.SetParent(bucketPosition);
        }
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
    }

    public override void Start()
    {
        base.Start();
        SetToolTipCanvasOrder();
    }

    private void SetToolTipCanvasOrder()
    {
        EVent.Do.Fetch<IToolTipCanvasOrder>().Invoke(this);
        foreach (var canvas in _cachedToolTipCanvasList)
        {
            canvas.enabled = true;
            canvas.overrideSorting = true;
            canvas.sortingOrder = ToolTipCanvasOrder;
            canvas.enabled = false;
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

    private void CheckSetUpForError()
    {
        if (ListOfTooltips.Length == 0)
            throw new Exception("No tooltips set");
    }

    private void SetUpTooltips()
    {
        if (ListOfTooltips.Length > 1)
            _buildDelay = Scheme.BuildDelay;
        
        ToolTipsRects = new RectTransform[ListOfTooltips.Length];
        _cachedToolTipCanvasList = new Canvas[ListOfTooltips.Length];

        for (int index = 0; index < ListOfTooltips.Length; index++)
        {
            ToolTipsRects[index] = ListOfTooltips[index].GetComponent<RectTransform>();
            _cachedToolTipCanvasList[index] = ListOfTooltips[index].GetComponent<Canvas>();
            _cachedToolTipCanvasList[index].enabled = false;
        }
    }
    
    private void HideToolTip()
    {
        if (!CanActivate) return;
        StaticCoroutine.StopCoroutines(_coroutineStart);
        StaticCoroutine.StopCoroutines(_coroutineBuild);
        StaticCoroutine.StopCoroutines(_coroutineActivate);
        _cachedToolTipCanvasList[CurrentToolTipIndex].enabled = false;
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

