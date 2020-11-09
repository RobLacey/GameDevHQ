
public abstract class NodeBase : IServiceUser, IEventUser, IChildIsActive
{
    protected NodeBase(UINode node)
    {
        _uiNode = node;
        _uiNodeEvents = _uiNode.ReturnUINodeEvents;
        MyBranch = _uiNode.MyBranch;
        SubscribeToService();
    }
    
    //Variables
    protected UINode _uiNode;
    protected readonly IUiEvents _uiNodeEvents;
    protected IHistoryTrack _uiHistoryTrack;
    public UIBranch MyBranch { get; protected set; }

    public bool NodeActivated { get; protected set; }

    //Properties
    public bool PointerOverNode { get; set; }

    protected static CustomEvent<IChildIsActive> ChildIsActive { get; } = new CustomEvent<IChildIsActive>();
    
    public virtual void ObserveEvents() => EventLocator.Subscribe<ISwitchGroupPressed>(HomeGroupChanged, this);

    public virtual void RemoveFromEvents() => EventLocator.Unsubscribe<ISwitchGroupPressed>(HomeGroupChanged);

    public void SubscribeToService() => _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);

    public virtual void Start() => ObserveEvents();

    public abstract void DeactivateNode();

    public void OnDisable() => RemoveFromEvents();

    private void HomeGroupChanged(ISwitchGroupPressed args) => PointerOverNode = false;

    public virtual void OnEnter(bool isDragEvent)
    {
        if(isDragEvent) return;
        PointerOverNode = true;
        _uiNode.SetAsHighlighted();
        _uiNodeEvents.DoPlayHighlightedAudio();
    }

    public virtual void OnExit(bool isDragEvent)
    {
        if(isDragEvent) return;
        PointerOverNode = false;
        _uiNode.SetNotHighlighted();
    }

    protected abstract void TurnNodeOnOff();

    public virtual void OnSelected(bool isDragEvent)
    {
        if(isDragEvent) return;
        TurnNodeOnOff();
    }
}
