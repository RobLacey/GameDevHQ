using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public class UIBranch : MonoBehaviour
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] UILeaf _userDefinedStartPosition;
    [SerializeField] [Label("Home Screen Object")] bool _onScreenAtStart;
    [SerializeField] [Label("Full Screen From Home")] [DisableIf("_turnOffOnMoveToChild")] bool _isFullScreen;
    [SerializeField] [DisableIf("_isFullScreen")] bool _turnOffOnMoveToChild;
    [SerializeField] bool _alwaysTweenOnReturn;
    [SerializeField] [Label("Save Selection On Exit")] bool _saveExitSelection;
    [SerializeField] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;
    [Header("Tween Settings")]
    [SerializeField] [DisableIf("_running")] public PositionTweenType _positionTween = PositionTweenType.NoTween;
    [SerializeField] [DisableIf("_running")] public RotationTweenType _rotationTween = RotationTweenType.NoTween;
    [SerializeField] [DisableIf("_running")] public ScaleTween _scaleTransition = ScaleTween.NoTween;
    [SerializeField] [DisableIf("_running")] public FadeTween _canvasGroupFade = FadeTween.NoTween;
    [Header("Tween Trigger Settings")]
    [SerializeField] [Label("Event At End/Mid-Point of Tween")] TweenTrigger _endOfTweenAction;

    //Classes
    [Serializable]
    public class TweenTrigger : UnityEvent<bool> { }

    //Variables
    UILeaf[] _childUILeafs;
    UITrunk _UITrunk;
    UITweener _UITweener;
    int _counter = 0;
    int _endOfEffectCounter;
    int _startOfEffectCounter;
    bool _running = false;               //To disable Tween settings as they break when changed during runtime

    //Properties
    public UILeaf DefaultStartPosition { get { return _userDefinedStartPosition; } 
                                         set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; set; }
    public UILeaf LastSelected { get; set; }
    public UIGroupID MyUIGroup { get; set; }
    public bool KillAllOtherUI { get { return _isFullScreen; } }
    public bool IsCancelling { get; set; }
    public bool DontTweenNow { get; set; }   //Set as True to stop a tween on certain transitions
    public UILeaf[] ThisGroupsUILeafs { get { return _childUILeafs; } }
    public bool AllowKeys { get; set; }
    public CanvasGroup _myCanvasGroup { get; set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }

    private void Awake()
    {
        GetChildUILeafs();
        IsCancelling = false;
        _myCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        _UITweener.OnAwake(_myCanvasGroup);
        _UITrunk = FindObjectOfType<UITrunk>();
        MyCanvas = GetComponent<Canvas>();
        SetCurrentBranchAsParent(this);
        SetStartPositions();
        if (!_onScreenAtStart)
        {
            MyCanvas.enabled = false;
        }
        else
        {
            MyCanvas.enabled = true;
        }
        SetUpTweeners();
        _myCanvasGroup.blocksRaycasts = false;
    }

    private void OnEnable() { _running = true; }
    private void OnDisable() { _running = false; }

    private void SetUpTweeners()
    {
        if (_UITweener)
        {
            if (_positionTween != PositionTweenType.NoTween)
            {
                _counter++;
                _UITweener.SetUpPositionTweens(_positionTween);
            }

            if (_rotationTween != RotationTweenType.NoTween)
            {
                _counter++;
                _UITweener.SetUpRotateTweens(_rotationTween);
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

    private void GetChildUILeafs() //Only gets Childrenn directly below. Ingnore ones inside other game objects
    {
        List<UILeaf> temp = new List<UILeaf>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<UILeaf>())
            {
                temp.Add(transform.GetChild(i).GetComponent<UILeaf>());
            }
        }
        _childUILeafs = temp.ToArray();
    }

    private void SetStartPositions()
    {
        if (DefaultStartPosition == null)
        {
            foreach (Transform item in transform)
            {
                if (item.GetComponent<UILeaf>())
                {
                    DefaultStartPosition = item.GetComponent<UILeaf>();
                    break;
                }
            }
        }
        LastSelected = DefaultStartPosition;
    }

    public void MoveBackALevel()
    {
        MyCanvas.enabled = true;
        _UITrunk.SetLastUIObject(LastSelected, MyUIGroup);
        SetCurrentBranchAsParent();
        InitialiseFirstUIElement();

        if (_alwaysTweenOnReturn && !DontTweenNow)
        {
            ActivateEffects();
        }

        DontTweenNow = false;
    }

    public void MoveToNextLevel(UIBranch newParentController = null)
    {
        MyCanvas.enabled = true;

        if (_isFullScreen) { _UITrunk.ToFullScreen(this); }

        SetCurrentBranchAsParent(newParentController);
        _UITrunk.SetLastUIObject(LastSelected, MyUIGroup);

        if (!DontTweenNow)
        {
            ActivateEffects();
        }
        else
        {
            InitialiseFirstUIElement();
        }
        DontTweenNow = false;
    }

    private void SetCurrentBranchAsParent(UIBranch newParentController = null) //Needed in case menu is called from different places
    {
        if (newParentController != null)
        {
            MyUIGroup = newParentController.MyUIGroup;
            foreach (var item in _childUILeafs)
            {
                item.MyParentController = newParentController;
            }
        }
    }

    private void InitialiseFirstUIElement()
    {
        if (DefaultStartPosition != null)
        {
            LastSelected.SetNotHighlighted();
            if (!_saveExitSelection)
            {
                LastSelected = DefaultStartPosition;
            }
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
        _myCanvasGroup.blocksRaycasts = false;
        _UITweener.StopAllCoroutines();
        DeactivationEffect();
    }

    public void ActivateEffects()
    {
        _UITweener.StopAllCoroutines();
        _endOfEffectCounter = _counter;
        _startOfEffectCounter = _counter;
        _myCanvasGroup.blocksRaycasts = false;

        if (_startOfEffectCounter == 0)
        {
            InTweenCallback();
        }

        if (_positionTween == PositionTweenType.In || _positionTween == PositionTweenType.InAndOut)
        {
            _UITweener.DoInTween(true, () => InTweenCallback());
        }
        else if (_positionTween == PositionTweenType.Out)
        {
            _UITweener.DoOutTween(true, () => InTweenCallback());
        }

        if (_rotationTween == RotationTweenType.In || _rotationTween == RotationTweenType.InAndOut)
        {
            _UITweener.DoRotateInTween(true, () => InTweenCallback());
        }
        else if (_rotationTween == RotationTweenType.Out)
        {
            _UITweener.DoRotateOutTween(true, () => InTweenCallback());
        }

        if (_scaleTransition == ScaleTween.Scale_InOnly || _scaleTransition == ScaleTween.Scale_InAndOut)
        {
            _UITweener.ScaleInTween(true, () => InTweenCallback());
        }
        else if (_scaleTransition == ScaleTween.Scale_OutOnly)
        {
            _UITweener.ScaleOutTween(true, () => InTweenCallback());
        }
        else if (_scaleTransition == ScaleTween.Punch)
        {
            _UITweener.DoPunch(true, () => InTweenCallback());
        }
        else if (_scaleTransition == ScaleTween.Shake)
        {
            _UITweener.DoShake(true, () => InTweenCallback());
        }

        if (_canvasGroupFade == FadeTween.FadeIn || _canvasGroupFade == FadeTween.FadeInAndOut)
        {
            _UITweener.DoCanvasFadeIn(true, () => InTweenCallback());
        }
        else if (_canvasGroupFade == FadeTween.FadeOut)
        {
            _UITweener.DoCanvasFadeOut(true, () => InTweenCallback());
        }
    }

    private void DeactivationEffect()
    {
        if (_endOfEffectCounter == 0)
        {
            OutTweenCallback();
            return;
        }

        if (_scaleTransition == ScaleTween.Scale_OutOnly || _scaleTransition == ScaleTween.Scale_InAndOut)
        {
            _UITweener.ScaleOutTween(false, () => OutTweenCallback());
        }

        else if (_scaleTransition == ScaleTween.Scale_InOnly)
        {
            _UITweener.ScaleInTween(false, () => OutTweenCallback());
        }
        else if (_scaleTransition == ScaleTween.Punch)
        {
            _UITweener.DoPunch(false, () => OutTweenCallback());
        }
        else if (_scaleTransition == ScaleTween.Shake)
        {
            _UITweener.DoShake(false, () => OutTweenCallback());
        }

        if (_positionTween == PositionTweenType.Out || _positionTween == PositionTweenType.InAndOut)
        {
            _UITweener.DoOutTween(false, () => OutTweenCallback());
        }
        else if (_positionTween == PositionTweenType.In)
        {
            _UITweener.DoInTween(false, () => OutTweenCallback());
        }

        if (_rotationTween == RotationTweenType.Out || _rotationTween == RotationTweenType.InAndOut)
        {
            _UITweener.DoRotateOutTween(false, () => OutTweenCallback());
        }
        else if (_rotationTween == RotationTweenType.In)
        {
            _UITweener.DoRotateInTween(false, () => OutTweenCallback());
        }

        if (_canvasGroupFade == FadeTween.FadeOut || _canvasGroupFade == FadeTween.FadeInAndOut)
        {
            _UITweener.DoCanvasFadeOut(false, () => OutTweenCallback());
        }
        else if (_canvasGroupFade == FadeTween.FadeIn)
        {
            _UITweener.DoCanvasFadeIn(false, () => OutTweenCallback());
        }

        _endOfTweenAction.Invoke(false);
    }

    private void OutTweenCallback()
    {
        _endOfEffectCounter--;
        IsCancelling = false;

        if (_endOfEffectCounter <= 0)
        {
            MyCanvas.enabled = false;
        }
    }

    private void InTweenCallback()
    {
        _startOfEffectCounter--;

        if (_startOfEffectCounter <= 0)
        {
            _endOfTweenAction.Invoke(true);
            _myCanvasGroup.blocksRaycasts = true;

            if (!IsCancelling) { InitialiseFirstUIElement(); }

            IsCancelling = false;
        }
    }
}
