using System;
using System.Collections;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Serializable]
public class CheckVisibility : IEventDispatcher, IMono, IOffscreen, IEventUser
{
    [SerializeField] 
    private Renderer _myRenderer;
    [SerializeField] 
    [Range(0, 20)] [Label(FrequencyName)] [Tooltip(FrequencyTooltip)]
    private int _checkFrequency = 10;

    //Variables
    private GOUIModule _myGOUI;
    private Coroutine _coroutine = null;
    private readonly WaitFrameCustom _waitFrame = new WaitFrameCustom();
    private OffScreenMarker _offScreenMarker;
    private OffscreenMarkerData _data;
    private bool _canStart;

    //Editor
    private const string FrequencyName = "Check Visible Frequency";
    private const string FrequencyTooltip = "How often the system checks and sets the position of the GOUI. " +
                                            "Increase to improve performance but decreases smoothness. " +
                                            "Effects both GOUI and Off Screen Marker";


    public IBranch TargetBranch { get; private set; }

    public bool IsOffscreen { get; private set; }
    public bool CanUseOffScreenMarker { get; set; }

    public Action<IOffscreen> GOUIOffScreen { get; set; }
    
    public void CanStart(IOnStart args = null)
    {
        _canStart = true;
        _coroutine = StaticCoroutine.StartCoroutine(IsVisible(false));
    }

    //Check if Close GOUIModule can be moved to GOUIBranch

    public void SetUp(GOUIModule goui)
    {
        _myGOUI = goui;
        TargetBranch = _myGOUI.TargetBranch;
        _data = _myGOUI.OffScreenMarkerData;
        CanUseOffScreenMarker = _myGOUI.OffScreenMarkerData.CanUseOffScreenMarker;
        _offScreenMarker = new OffScreenMarker(_data);
    }

    public void OnAwake()
    {
        if (!CanUseOffScreenMarker) return;
        
        _offScreenMarker = new OffScreenMarker(_myGOUI.OffScreenMarkerData);
        _offScreenMarker.SetParent(_myGOUI);
        _offScreenMarker.OnAwake();
        FetchEvents();
    }

    public void OnEnable()
    {
        ObserveEvents();
        
        if(CanUseOffScreenMarker && !_canStart)
            _offScreenMarker.OnEnable();
    }

    public void ObserveEvents()
    {
        if(_canStart) return;
        EVent.Do.Subscribe<IOnStart>(CanStart);
    }

    public void OnDisable()
    {
        if(CanUseOffScreenMarker)
            _offScreenMarker.OnDisable();
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = null;
    }
    
    public void OnStart()
    {
        if(CanUseOffScreenMarker)
            _offScreenMarker.OnStart();
    }

    public void FetchEvents() => GOUIOffScreen = EVent.Do.Fetch<IOffscreen>();
    
    public void StopOffScreenMarker()
    {
        if(CanUseOffScreenMarker)
            _offScreenMarker.StopOffScreenMarker();
    }

    private IEnumerator IsVisible(bool isRestart)
    {
        if(isRestart)
            yield return _waitFrame.SetFrameTarget(_checkFrequency);
        
        while (true)
        {
            if (_myRenderer.isVisible)
            {
                if(IsOffscreen)
                {
                    IsOffscreen = false;
                    DoTurnOn();
                }                
            }
            else
            {
                if(!IsOffscreen)
                {
                    IsOffscreen = true;
                    DoTurnOff();
                }                
            }

            yield return _waitFrame.SetFrameTarget(_checkFrequency);
        }
    }

    private void DoTurnOff()
    {
        GOUIOffScreen?.Invoke(this);

        TargetBranch.MyCanvas.enabled = false;
        if(CanUseOffScreenMarker)
            _offScreenMarker.StartOffScreenMarker(_myGOUI);
    }

    private void DoTurnOn()
    {
        GOUIOffScreen?.Invoke(this);

        TargetBranch.MyCanvas.enabled = _myGOUI.GOUIIsActive || _myGOUI.AlwaysOnIsActive;
        if(CanUseOffScreenMarker)
            _offScreenMarker.StopOffScreenMarker();
    }
}