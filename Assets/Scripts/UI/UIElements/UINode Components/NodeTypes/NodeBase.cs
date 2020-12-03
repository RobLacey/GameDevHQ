public abstract class NodeBase : IEServUser, IEventUser, INodeBase
{
    protected NodeBase(INode node)
    {
        _uiNode = node;
        MyBranch = _uiNode.MyBranch;
        UseEServLocator();
    }
    
    //Variables
    protected INode _uiNode;
    protected IHistoryTrack _uiHistoryTrack;

    //Properties
    public bool PointerOverNode { get; set; }
    public IBranch MyBranch { get; protected set; }

    public virtual void Start() { }

    public void OnEnable() => ObserveEvents();
    public void OnDisable() => RemoveEvents();

    public virtual void ObserveEvents() { }

    public virtual void RemoveEvents() { }

    public void UseEServLocator() => _uiHistoryTrack = EServ.Locator.Get<IHistoryTrack>(this);

    public abstract void DeactivateNode();

    public virtual void OnEnter(bool isDragEvent)
    {
        if(isDragEvent) return;
        PointerOverNode = true;
        _uiNode.UINodeEvents.DoWhenPointerOver(PointerOverNode);
        //_uiNode.SetAsHighlighted();
    }

    public virtual void OnExit(bool isDragEvent)
    {
        if(isDragEvent) return;
        PointerOverNode = false;
        _uiNode.UINodeEvents.DoWhenPointerOver(PointerOverNode);
        //_uiNode.SetNotHighlighted();
    }

    protected abstract void TurnNodeOnOff();

    public virtual void OnSelected(bool isDragEvent)
    {
        if(isDragEvent) return;
        TurnNodeOnOff();
    }
}
