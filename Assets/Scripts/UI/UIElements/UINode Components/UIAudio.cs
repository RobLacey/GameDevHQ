using UnityEngine;

public class UIAudio : NodeFunctionBase, IServiceUser
{
    public UIAudio(IAudioSettings settings, UiActions uiActions)
    {
        _uiActions = uiActions;
        _audioScheme = settings.AudioScheme;
        CanActivate = true;
        OnAwake(uiActions);
    }

    //Variables 
    private readonly AudioScheme _audioScheme;
    private IAudioService _audioService;
    private bool _canStart;
    private UiActions _uiActions;

    //Properties
    private bool UsingScheme() => _audioScheme;
    private void OnStart(IOnStart onStart) => _canStart = true;
    private void SetPlayCancelAudio() => PlayCancelAudio();

    //Main
    protected sealed override void OnAwake(UiActions uiActions)
    {
        base.OnAwake(uiActions);
        uiActions._canPlayCancelAudio += SetPlayCancelAudio;
        SubscribeToService();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.Subscribe<IOnStart>(OnStart, this);
    }

    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.Unsubscribe<IOnStart>(OnStart);
    }

    private void Play(AudioClip clip, float volume)
    {
        if(FunctionNotActive() || !_canStart) return;

        _audioService.Play(clip, volume);
    }

    protected override bool CanBeHighlighted() => !UsingScheme() ? false : _audioScheme.HighlightedClip;

    protected override bool CanBePressed() => !UsingScheme() ? false : _audioScheme.SelectedClip;

    protected override bool FunctionNotActive() => !CanActivate || !UsingScheme();

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(!CanBeHighlighted() || !pointerOver) return;
        Play(_audioScheme.HighlightedClip, _audioScheme.HighlighVolume);
    }

    protected override void SaveIsSelected(bool isSelected)
    {
        base.SaveIsSelected(isSelected);
        if(!isSelected) 
            ProcessPress();
    }

    private protected override void ProcessPress()
    {
        if(!CanBePressed()) return;
        
        if (_isSelected)
        {
            Play(_audioScheme.SelectedClip, _audioScheme.SelectedVolume);
        }
    }

    private void PlayCancelAudio()
    {
        if(FunctionNotActive()) return;
        Play(_audioScheme.CancelledClip, _audioScheme.CancelledVolume);
    }

    private protected override void ProcessDisabled()
    {
        //Do Nothing currently
    }

    public void SubscribeToService()
    {
        _audioService = ServiceLocator.GetNewService<IAudioService>(this);
        //return _audioService is null;
    }
}
