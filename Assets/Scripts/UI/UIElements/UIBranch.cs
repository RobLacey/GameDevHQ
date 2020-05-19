using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(GraphicRaycaster))]
[RequireComponent(typeof(UITweener))]

public class UIBranch : MonoBehaviour
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] UINode _userDefinedStartPosition;
    [SerializeField] [Label("Home Screen Object")] bool _onScreenAtStart;
    [SerializeField] [Label("Full Screen From Home")] [DisableIf("_turnOffOnMoveToChild")] bool _isFullScreen;
    [SerializeField] [DisableIf("_isFullScreen")] bool _turnOffOnMoveToChild;
    [SerializeField] bool _alwaysTweenOnReturn;
    [SerializeField] [Label("Save Selection On Exit")] bool _saveExitSelection;
    [SerializeField] EscapeKey _escapeKeyFunction = EscapeKey.GlobalSetting;

    //Variables
    UINode[] _childUILeafs;
    UIMasterController _UITrunk;
    UITweener _UITweener;

    //Properties
    public UINode DefaultStartPosition { get { return _userDefinedStartPosition; } 
                                         set { _userDefinedStartPosition = value; } }
    public Canvas MyCanvas { get; set; }
    public UINode LastSelected { get; set; }
    public UIGroupID MyUIGroup { get; set; }
    public bool KillAllOtherUI { get { return _isFullScreen; } }
    public bool IsCancelling { get; set; }
    public bool DontTweenNow { get; set; }   //Set as True to stop a tween on certain transitions
    public UINode[] ThisGroupsUILeafs { get { return _childUILeafs; } }
    public bool AllowKeys { get; set; }
    public CanvasGroup _myCanvasGroup { get; set; }
    public EscapeKey EscapeKeySetting { get { return _escapeKeyFunction; } }

    private void Awake()
    {
        GetChildUILeafs();
        IsCancelling = false;
        _myCanvasGroup = GetComponent<CanvasGroup>();
        _UITweener = GetComponent<UITweener>();
        _UITrunk = FindObjectOfType<UIMasterController>();
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
        _UITweener.OnAwake(_myCanvasGroup);
        _myCanvasGroup.blocksRaycasts = false;
        _UITweener.IsRunning = true;
    }

    private void OnEnable() { _UITweener.IsRunning = true; }
    private void OnDisable() { _UITweener.IsRunning = false; }

    private void GetChildUILeafs() //Only gets Childrenn directly below. Ingnore ones inside other game objects
    {
        List<UINode> temp = new List<UINode>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<UINode>())
            {
                temp.Add(transform.GetChild(i).GetComponent<UINode>());
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
                if (item.GetComponent<UINode>())
                {
                    DefaultStartPosition = item.GetComponent<UINode>();
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

    public void SaveLastSelected(UINode lastSelected)
    {
        _UITrunk.SetLastUIObject(lastSelected, MyUIGroup);
        LastSelected = lastSelected;
    }

    public void TurnOffBranch()
    {
        _myCanvasGroup.blocksRaycasts = false;
        _UITweener.StopAllCoroutines();
        _UITweener.StartOutTweens(() => MyCanvas.enabled = false);
    }

    public void ActivateEffects()
    {
        _myCanvasGroup.blocksRaycasts = false;
        _UITweener.ActivateTweens(()=> InTweenCallback());
    }

    private void InTweenCallback()
    {
        _myCanvasGroup.blocksRaycasts = true;

        if (!IsCancelling) { InitialiseFirstUIElement(); }

        IsCancelling = false;
    }
}
