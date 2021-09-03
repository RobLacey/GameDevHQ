using System;
using System.Collections;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Serializable]
public class CheckVisibility : IEZEventDispatcher, IMonoEnable, IMonoStart, IMonoDisable, IMonoOnDestroy, 
                               IOffscreen, IEZEventUser, IServiceUser
{
    [SerializeField] 
    private Renderer _myRenderer;
    [SerializeField] 
    [Range(0, 20)] [Label(FrequencyName)] [Tooltip(FrequencyTooltip)]
    private int _checkFrequency = 10;

    //Variables
    private GOUIModule _myGOUI;
    private readonly WaitFrameCustom _waitFrame = new WaitFrameCustom();
    private OffScreenMarker _offScreenMarker;
    private IDataHub _myDataHub;
    private bool _runVisibilityCheck;

    //Editor
    private const string FrequencyName = "Check Visible Frequency";
    private const string FrequencyTooltip = "How often the system checks and sets the position of the GOUI. " +
                                            "Increase to improve performance but decreases smoothness. " +
                                            "Effects both GOUI and Off Screen Marker";


    //Properties
    public IBranch TargetBranch { get; private set; }
    public bool IsOffscreen { get; private set; }
    public bool CanUseOffScreenMarker { get; set; }

    //Events
    public Action<IOffscreen> GOUIOffScreen { get; set; }
    
    
    //Main
    public void CanStart(IOnStart args) => StaticCoroutine.StartCoroutine(IsVisible(false));

    public void SetUpOnStart(GOUIModule goui)
    {
        _myGOUI = goui;
        TargetBranch = _myGOUI.TargetBranch;
        CanUseOffScreenMarker = _myGOUI.OffScreenMarkerData.CanUseOffScreenMarker;
        _offScreenMarker = new OffScreenMarker(_myGOUI.OffScreenMarkerData);
        _offScreenMarker.OnAwake(_myGOUI);
        OnStart();
    }

    public void OnEnable()
    {
        _runVisibilityCheck = true;
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
        RunTimeEnable();
    }

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void FetchEvents() => GOUIOffScreen = GOUIEvents.Do.Fetch<IOffscreen>();

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<IOnStart>(CanStart);
    
    public void UnObserveEvents() => HistoryEvents.Do.Unsubscribe<IOnStart>(CanStart);

    private void RunTimeEnable()
    {
        if(_myDataHub.IsNull()) return;
        
        if(_myDataHub.SceneStarted)
        {
            if (CanUseOffScreenMarker)
            {
                StaticCoroutine.StartCoroutine(IsVisible(true));
            }            
        }    
    }

    public void OnDisable()
    {
        if(CanUseOffScreenMarker)
            _offScreenMarker.OnDisable();
        UnObserveEvents();
        GOUIOffScreen = null;
        IsOffscreen = false;
        _runVisibilityCheck = false;
    }
    
    public void OnDestroy()
    {
        HistoryEvents.Do.Unsubscribe<IOnStart>(CanStart);
        _runVisibilityCheck = false;
        if(CanUseOffScreenMarker)
            _offScreenMarker.OnDisable();
    }


    public void OnStart()
    {
        if(CanUseOffScreenMarker)
            _offScreenMarker.OnStart();
        RunTimeEnable();
    }

    public void StopOffScreenMarker()
    {
        if(CanUseOffScreenMarker)
            _offScreenMarker.StopOffScreenMarker();
    }

    private IEnumerator IsVisible(bool isRestart)
    {
        if(isRestart)
            yield return _waitFrame.SetFrameTarget(_checkFrequency);
        
        while (_runVisibilityCheck)
        {
            if (_myRenderer.isVisible)
            {
                if(IsOffscreen)
                    DoTurnOn();
            }
            else
            {
                if(!IsOffscreen)
                    DoTurnOff();
            }

            yield return _waitFrame.SetFrameTarget(_checkFrequency);
        }
        yield return null;
    }

    private void DoTurnOff()
    {
        IsOffscreen = true;
        GOUIOffScreen?.Invoke(this);
        TargetBranch.MyCanvas.enabled = false;
        
        if(CanUseOffScreenMarker)
            _offScreenMarker.StartOffScreenMarker(_myGOUI);
    }

    private void DoTurnOn()
    {
        IsOffscreen = false;
        GOUIOffScreen?.Invoke(this);
        TargetBranch.MyCanvas.enabled = _myGOUI.GOUIIsActive || _myGOUI.AlwaysOnIsActive;
        
        if(CanUseOffScreenMarker)
            _offScreenMarker.StopOffScreenMarker();
    }
}