﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using NaughtyAttributes;

/// <summary>
/// UIHub is the core of the system and looks after starting the system Up and general state management 
/// </summary>

[RequireComponent(typeof(AudioSource))]

public partial class UIHub : MonoBehaviour
{
    [Header("Main Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Enable Controls After..")] float _atStartDelay;
    [SerializeField] 
    [ValidateInput("ProtectEscapeKeySetting", "Can't set Global Settings to Global Settings")]
    EscapeKey _globalCancelFunction = EscapeKey.BackOneLevel;
    [SerializeField] 
    [ReorderableList] [Label("Home Screen Branches (First Branch is Start Position)")] 
    List<UIBranch> _homeBranches;
    [SerializeField] 
    [ReorderableList] [Label("Hot Keys (CAN'T have PopUps as Hot keys)")] 
    List<HotKeys> _hotKeySettings;

    //Events
    public static event Action OnStart;

    //Variables
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private UINode _lastHomeScreenNode;
    private UINode _lastHighlighted;
    private bool _inMenu;
    private bool _startingInGame;

    //Properties

    private void SaveInMenu(bool isInMenu)
    {
        _inMenu = isInMenu;
        if(!_inMenu) SetEventSystem(null);
    }

    private void Awake()
    {
        CreateSubClasses();
        _startingInGame = GetComponent<UIInput>().StartInGame;
    }

    private void CreateSubClasses()
    {
        var unused = new PopUpController();
        var unused1 = new UIAudioManager(GetComponent<AudioSource>());
        var unused3 = new UIHomeGroup(_homeBranches.ToArray());
        var unused2 = new UICancel(_globalCancelFunction);
        SetUpHotKeys();
    }

    private void SetUpHotKeys()
    {
        if (_hotKeySettings.Count == 0) return;
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnAwake();
        }
    }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SetLastHighlighted);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
    }

    private void Start()
    {
        SetStartPositionsAndSettings();
        CheckIfStartingInGame();
        StartCoroutine(EnableStartControls());
    }

    private void SetStartPositionsAndSettings()
    {
        _lastHighlighted = _homeBranches[0].DefaultStartPosition;
        _homeBranches[0].DefaultStartPosition.ThisNodeIsHighLighted();
        _homeBranches[0].DefaultStartPosition.ThisNodeIsSelected();
    }

    private void CheckIfStartingInGame()
    {
        if (_startingInGame)
        {
            StartInGame();
            _inMenu = false;
        }
        else
        {
            StartInMenus();
            _inMenu = true;
        }
    }

    private void StartInGame()
    {
        ActivateAllHomeBranches(IsActive.No);
        OnStart?.Invoke();
    }

    private void StartInMenus()
    {
        EventSystem.current.SetSelectedGameObject(_homeBranches[0].DefaultStartPosition.gameObject);
        ActivateAllHomeBranches(IsActive.Yes);
    }

    private void ActivateAllHomeBranches(IsActive activateOnStart)
    {
        foreach (var branch in _homeBranches)
        {
            if (activateOnStart == IsActive.Yes && branch == _homeBranches[0])
            {
                branch.MoveToThisBranch();
            }
            else
            {
                branch.MoveToThisBranchDontSetAsActive();
            }
        }
    }

    private IEnumerator EnableStartControls()
    {
        yield return new WaitForSeconds(_atStartDelay);
        if(!_startingInGame)
            OnStart?.Invoke();

        foreach (var homeBranch in _homeBranches)
        {
            homeBranch.ActivateBranch();
        }
    }
    
    private void SetLastHighlighted(UINode newNode)
    {
        if (newNode != _lastHighlighted) _lastHighlighted.SetNotHighlighted();
        _lastHighlighted = newNode;
        if(_inMenu) SetEventSystem(_lastHighlighted.gameObject);
    }

    public static void SetEventSystem(GameObject newGameObject)
    {
        EventSystem.current.SetSelectedGameObject(newGameObject);
    }
}




