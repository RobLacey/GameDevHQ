
public abstract class NodeBase : IServiceUser, IEventUser
{
    protected NodeBase(UINode node)
    {
        _uiNode = node;
        SubscribeToService();
    }
    
    //Variables
    protected UINode _uiNode;
    protected IHistoryTrack _uiHistoryTrack;
    
    //Properties
    public bool PointerOverNode { get; set; }
    
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
    }

    public virtual void OnExit(bool isDragEvent)
    {
        if(isDragEvent) return;
        PointerOverNode = false;
        _uiNode.SetNotHighlighted();
    }

    public abstract void TurnNodeOnOff();

    public virtual void OnSelected(bool isDragEvent)
    {
        if(isDragEvent) return;
        TurnNodeOnOff();
    }
}
