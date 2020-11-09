public class UIAudio : NodeFunctionBase, IServiceUser
{
    public UIAudio(IAudioSettings settings, IUiEvents uiEvents)
    {
        _uiEvents = uiEvents;
        _audioScheme = settings.AudioScheme;
        CanActivate = true;
        OnAwake(uiEvents);
    }

    //Variables 
    private readonly AudioScheme _audioScheme;
    private IAudioService _audioService;
    private bool _canStart;
    private readonly IUiEvents _uiEvents;
    private INode _lastSelected;

    //Properties
    private bool UsingScheme() => _audioScheme;
    private void OnStart(IOnStart onStart) => _canStart = true;
    private void SaveLastSelected(ISelectedNode args) => _lastSelected = args.Selected;

    //Main
    protected sealed override void OnAwake(IUiEvents uiEvents)
    {
        base.OnAwake(uiEvents);
        uiEvents.PlayCancelAudio += PlayCancelAudio;
        uiEvents.PlaySelectedAudio += PlaySelectedAudio;
        uiEvents.PlayHighlightedAudio += PlayHighlightedAudio;
        SubscribeToService();
    }
    
    public void SubscribeToService() => _audioService = ServiceLocator.GetNewService<IAudioService>(this);

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.Subscribe<IOnStart>(OnStart, this);
        EventLocator.Subscribe<ISelectedNode>(SaveLastSelected, this);
        EventLocator.Subscribe<ICancelPressed>(CancelPressed, this);
        EventLocator.Subscribe<IHotKeyPressed>(HotKeyPressed, this);
    }

    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.Unsubscribe<IOnStart>(OnStart);
        EventLocator.Unsubscribe<ISelectedNode>(SaveLastSelected);
        EventLocator.Unsubscribe<ICancelPressed>(CancelPressed);
        EventLocator.Unsubscribe<IHotKeyPressed>(HotKeyPressed);
    }

    protected override bool CanBeHighlighted() => !UsingScheme() ? false : _audioScheme.HighlightedClip;

    protected override bool CanBePressed() => !UsingScheme() ? false : _audioScheme.SelectedClip;

    private bool HasDisabledSound() => !UsingScheme() ? false : _audioScheme.DisabledClip;

    private bool HasCancelSound() => !UsingScheme() ? false : _audioScheme.CancelledClip;

    protected override bool FunctionNotActive() => !CanActivate || !UsingScheme() || !_canStart;

    protected override void SavePointerStatus(bool pointerOver) { }

    protected override void SaveIsSelected(bool isSelected) { }

    private protected override void ProcessPress() { }

    private bool IsDisabled()
    {
        if (!_isDisabled || !HasDisabledSound()) return false;
        
        _audioService.PlayDisabled(_audioScheme.DisabledClip, _audioScheme.DisabledVolume);
        return true;
    }

    private void PlayCancelAudio()
    {
        if(FunctionNotActive() || !HasCancelSound()) return;
        _audioService.PlayCancel(_audioScheme.CancelledClip, _audioScheme.CancelledVolume);
    }
    
    private void PlaySelectedAudio()
    {
        if(FunctionNotActive() || !CanBePressed()) return;
        if(IsDisabled()) return;
        _audioService.PlaySelect(_audioScheme.SelectedClip, _audioScheme.SelectedVolume);
    }
    private void PlayHighlightedAudio()
    {
        if(FunctionNotActive() || !CanBeHighlighted()) return;
        if(IsDisabled()) return;
        _audioService.PlayHighlighted(_audioScheme.HighlightedClip, _audioScheme.HighlightedVolume);
    }

    private protected override void ProcessDisabled() { }

    private void CancelPressed(ICancelPressed args)
    {
        if (_lastSelected.ReturnNode == _uiEvents.ReturnMasterNode)
        {
            PlayCancelAudio();
        }
    }

    private void HotKeyPressed(IHotKeyPressed args)
    {
        if (args.ParentNode == _uiEvents.ReturnMasterNode.ReturnNode)
        {
            PlaySelectedAudio();
        }
    }
}
