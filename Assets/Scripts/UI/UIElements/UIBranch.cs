using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public class UIBranch : MonoBehaviour
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] UILeaf _userDefinedStartPosition;
    [SerializeField] [Label("Always On Screen At Start")] bool _onScreenAtStart;
    [SerializeField] bool _turnOffOnMoveToChild;
    [SerializeField] [Label("Save Selection On Exit")] bool _saveExitSelection;
    [SerializeField] bool _isFullScreen;
    [Header("Tween Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [DisableIf("_running")] public PositionInTween _positionInTween = PositionInTween.NoTween;
    [SerializeField] [DisableIf("_running")] public PositionOutTween _positionOutTween = PositionOutTween.NoTween;
    [SerializeField] [DisableIf("_running")] public ScaleTween _scaleTransition = ScaleTween.NoTween;
    [SerializeField] [DisableIf("_running")] public FadeTween _canvasGroupFade = FadeTween.NoTween;

    //Variables
    UILeaf[] _selectables;
    UITrunk _UITrunk;
    UIBranch _currentChildrensParent;
    UITweener _UITweener;
    bool _running = false;
    int _counter = 0;
    int _endOfEffectCounter;
    int _startOfEffectCounter;
    CanvasGroup _myCanvasGroup;

    //Properties
    public UILeaf DefaultStartPosition { get { return _userDefinedStartPosition; } }
    public Canvas MyCanvas { get; set; }
    public UILeaf MouseOverLast { get; set; }
    public UILeaf LastSelected { get; set; }
    public UIGroupID MyUIGroup { get; set; }
    public bool KillAllOtherUI { get { return _isFullScreen; } }

    private void Awake()
    {
        _myCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        _UITrunk = FindObjectOfType<UITrunk>();
        MyCanvas = GetComponent<Canvas>();
        _selectables = GetComponentsInChildren<UILeaf>();
    }

    private void OnEnable() { _running = true; }

    private void OnDisable() { _running = false; }

    private void Start()
    {
        SetStartPositions();
        SetUpTweeners();

        if (!_onScreenAtStart)
        {
            MyCanvas.enabled = false;
        }
        else
        {
            MyCanvas.enabled = true;
        }
    }

    private void SetUpTweeners()
    {
        if (_UITweener)
        {
            if (_positionOutTween != PositionOutTween.NoTween || _positionInTween != PositionInTween.NoTween)
            {
                _counter++;
                _UITweener.SetUpPositionTweens(_positionOutTween, _positionInTween);
            }

            if (_scaleTransition != ScaleTween.NoTween)
            {
                _counter++;
                _UITweener.SetUpScaleTweens(_scaleTransition);
            }

            if (_canvasGroupFade != FadeTween.NoTween)
            {
                _counter++;
                _UITweener.SetUpFadeTweens(_canvasGroupFade);
            }
        }
    }

    private void SetStartPositions()
    {
        if (_userDefinedStartPosition == null)
        {
            foreach (Transform item in transform)
            {
                if (item.GetComponent<UILeaf>())
                {
                    _userDefinedStartPosition = item.GetComponent<UILeaf>();
                    break;
                }
            }
        }
        LastSelected = _userDefinedStartPosition;
        MouseOverLast = _userDefinedStartPosition;
    }

    public void MoveToNextLevel(UIBranch newParentController = null)
    {
        if (MyCanvas.enabled == true) //Stops Repeating start effect on Cancel
        {
            InitialiseFirstUIElement();
            _UITrunk.SetLastUIObject(LastSelected, MyUIGroup);
            SetCurrentBranchAsParent(newParentController);
            return;
        }
        _endOfEffectCounter = _counter;
        _startOfEffectCounter = _counter;

        if (_isFullScreen) { _UITrunk.ToFullScreen(this); }

        MyCanvas.enabled = true;
        SetCurrentBranchAsParent(newParentController); 
        _UITrunk.SetLastUIObject(LastSelected, MyUIGroup);

        if (_UITweener)
        {
            _myCanvasGroup.blocksRaycasts = false;
            ActivateEffects();
        }
    }

    private void SetCurrentBranchAsParent(UIBranch newParentController) //Needed in case menu is called from different places
    {
        if (newParentController != null && _currentChildrensParent != newParentController)
        {
            _currentChildrensParent = newParentController;
            MyUIGroup = newParentController.MyUIGroup;
            foreach (var item in _selectables)
            {
                item.MyParentController = newParentController;
            }
        }
    }

    private void InitialiseFirstUIElement()
    {
        if (_userDefinedStartPosition != null)
        {
            if (!_saveExitSelection)
            {
                LastSelected = DefaultStartPosition;
                LastSelected.SetNotHighlighted();
            }

            MouseOverLast.SetNotHighlighted();
            MouseOverLast = LastSelected;
            LastSelected.AllowKeys = false;
            EventSystem.current.SetSelectedGameObject(LastSelected.gameObject);
            LastSelected.InitialiseStartUp();
        }
    }

    public void TurnOffOnMoveToChild()
    {
        if (_turnOffOnMoveToChild){ MyCanvas.enabled = false; }
    }

    public void SaveLastSelected(UILeaf lastSelected)
    {
        _UITrunk.SetLastUIObject(lastSelected, MyUIGroup);
        LastSelected = lastSelected;
    }

    public void TurnOffBranch()
    {
        if (_endOfEffectCounter == 0)
        {
            MyCanvas.enabled = false;
            return;
        }

        _myCanvasGroup.blocksRaycasts = false;
        _UITweener.StopAllCoroutines();

        DeactivationEffect();
    }

    public void ActivateEffects()
    {
        _UITweener.StopAllCoroutines();

        if (_startOfEffectCounter == 0)
        {
            InitialiseFirstUIElement();
            _myCanvasGroup.blocksRaycasts = true;
        }

        if (_positionInTween != PositionInTween.NoTween)
        {
            _UITweener.DoInTween(true, () => StartEffectsCallback());
        }
        else if (_positionOutTween != PositionOutTween.NoTween)
        {
            _UITweener.DoOutTween(true, () => StartEffectsCallback());
        }

        if (_scaleTransition == ScaleTween.Scale_InOnly || _scaleTransition == ScaleTween.Scale_InAndOut)
        {
            _UITweener.ScaleInTween(true, () => StartEffectsCallback());
        }
        else if (_scaleTransition == ScaleTween.Scale_OutOnly)
        {
            _UITweener.ScaleOutTween(true, () => StartEffectsCallback());
        }
        else if (_scaleTransition == ScaleTween.Punch)
        {
            _UITweener.DoPunch(true, () => StartEffectsCallback());

        }
        else if (_scaleTransition == ScaleTween.Shake)
        {
            _UITweener.DoShake(true, () => StartEffectsCallback());
        }

        if (_canvasGroupFade == FadeTween.FadeIn || _canvasGroupFade == FadeTween.FadeInAndOut)
        {
            _UITweener.DoCanvasFadeIn(true, () => StartEffectsCallback());
        }
        else if (_canvasGroupFade == FadeTween.FadeOut)
        {
            _UITweener.DoCanvasFadeOut(true, () => StartEffectsCallback());
        }
    }

    private void DeactivationEffect()
    {
        if (_scaleTransition == ScaleTween.Scale_OutOnly || _scaleTransition == ScaleTween.Scale_InAndOut)
        {
            _UITweener.ScaleOutTween(false, () => EffectsEndCallback());
        }

        else if (_scaleTransition == ScaleTween.Scale_InOnly)
        {
            _UITweener.ScaleInTween(false, () => EffectsEndCallback());
        }
        else if (_scaleTransition == ScaleTween.Punch)
        {
            _UITweener.DoPunch(false, () => EffectsEndCallback());

        }
        else if (_scaleTransition == ScaleTween.Shake)
        {
            _UITweener.DoShake(false, () => EffectsEndCallback());
        }

        if (_positionOutTween != PositionOutTween.NoTween)
        {
            _UITweener.DoOutTween(false, () => EffectsEndCallback());
        }
        else if (_positionInTween != PositionInTween.NoTween)
        {
            _UITweener.DoInTween(false, () => EffectsEndCallback());
        }

        if (_canvasGroupFade == FadeTween.FadeOut || _canvasGroupFade == FadeTween.FadeInAndOut)
        {
            _UITweener.DoCanvasFadeOut(false, () => EffectsEndCallback());
        }
        else if (_canvasGroupFade == FadeTween.FadeIn)
        {
            _UITweener.DoCanvasFadeIn(false, () => EffectsEndCallback());
        }
    }

    private void EffectsEndCallback()
    {
        _endOfEffectCounter--;

        if (_endOfEffectCounter <= 0)
        {
            //Debug.Log("Effects Done");
            MyCanvas.enabled = false;
        }
    }

    private void StartEffectsCallback()
    {
        _startOfEffectCounter--;

        if (_startOfEffectCounter <= 0)
        {
            //Debug.Log("SetUp done");
            _myCanvasGroup.blocksRaycasts = true;
            InitialiseFirstUIElement();
        }
    }
}
