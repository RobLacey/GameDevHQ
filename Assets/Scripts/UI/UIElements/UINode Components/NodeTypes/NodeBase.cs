public interface INodeBase
{
    bool PointerOverNode { get; set; }
    void Start();
    void DeactivateNode();
    void OnDisable();
    void OnEnter(bool isDragEvent);
    void OnExit(bool isDragEvent);
    void OnSelected(bool isDragEvent);
}

public abstract class NodeBase : IServiceUser, IEventUser, INodeBase
{
    protected NodeBase(INode node)
    {
        _uiNode = node;
        MyBranch = _uiNode.MyBranch;
        SubscribeToService();
    }
    
    //Variables
    protected INode _uiNode;
    protected IHistoryTrack _uiHistoryTrack;

    //Properties
    public bool PointerOverNode { get; set; }
    public IBranch MyBranch { get; protected set; }
    
    public virtual void ObserveEvents() { }

    public virtual void RemoveFromEvents(){ }

    public void SubscribeToService() => _uiHistoryTrack = ServiceLocator.Get<IHistoryTrack>(this);

    public virtual void Start() => ObserveEvents();

    public abstract void DeactivateNode();

    public void OnDisable() => RemoveFromEvents();

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

    protected abstract void TurnNodeOnOff();

    public virtual void OnSelected(bool isDragEvent)
    {
        if(isDragEvent) return;
        TurnNodeOnOff();
    }
}
