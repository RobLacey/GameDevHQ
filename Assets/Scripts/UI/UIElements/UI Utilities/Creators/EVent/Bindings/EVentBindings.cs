using System.Collections;

public class EVentBindings : IEVentBindings
{
    
    public EVentBindings(ISetNewEVent setNewEVentHandler)
    {
        BindAllObjects();
        setNewEVentHandler.SetUpEVentHandler((IEVentBase) setNewEVentHandler, Events);
    }
    
    //Variables
    private bool _autoRemoveEVent;
    public Hashtable Events { get; } = new Hashtable();
    
    public void BindAllObjects()
    {
        //PopUps
        AutoRemove().CreateEvent<IRemoveOptionalPopUp>();
        AutoRemove().CreateEvent<IAddOptionalPopUp>();
        AutoRemove().CreateEvent<IAddResolvePopUp>();
        AutoRemove().CreateEvent<INoResolvePopUp>();
        AutoRemove().CreateEvent<INoPopUps>();
        AutoRemove().CreateEvent<ILastRemovedPopUp>();
        
        //History
        AutoRemove().CreateEvent<IReturnToHome>();
        AutoRemove().CreateEvent<IOnHomeScreen>();
        AutoRemove().CreateEvent<IOnStart>();
        AutoRemove().CreateEvent<IGameIsPaused>();
        AutoRemove().CreateEvent<IInMenu>();
        AutoRemove().CreateEvent<ISceneChange>();// Doesn't Clear until exit
        
        //Input
        AutoRemove().CreateEvent<IPausePressed>();
        AutoRemove().CreateEvent<ISwitchGroupPressed>();
        AutoRemove().CreateEvent<IHotKeyPressed>();
        AutoRemove().CreateEvent<IMenuGameSwitchingPressed>();
        
        //ChangeControl
        AutoRemove().CreateEvent<IAllowKeys>();
        AutoRemove().CreateEvent<IChangeControlsPressed>();
        
        //Cancel
        AutoRemove().CreateEvent<ICancelHoverOverButton>();
        AutoRemove().CreateEvent<ICancelPressed>();
        AutoRemove().CreateEvent<ICancelButtonActivated>();
        AutoRemove().CreateEvent<ICancelPopUp>();
        AutoRemove().CreateEvent<ICancelHoverOver>();
        
        // //Node
        AutoRemove().CreateEvent<IHighlightedNode>();
        AutoRemove().CreateEvent<ISelectedNode>();
        AutoRemove().CreateEvent<IDisabledNode>();
        
        //Branch
        AutoRemove().CreateEvent<IActiveBranch>();
        AutoRemove().CreateEvent<IClearScreen>();
        AutoRemove().CreateEvent<ISetUpStartBranches>();
        AutoRemove().CreateEvent<IEndTween>();
        AutoRemove().CreateEvent<IGetHomeBranches>();
        
        //Test
        AutoRemove().CreateEvent<ITestList>();
        
        //CanvasOrder
        AutoRemove().CreateEvent<IPauseCanvasOrder>();
        AutoRemove().CreateEvent<IToolTipCanvasOrder>();
        AutoRemove().CreateEvent<IAdjustCanvasOrder>();
    }
    
    public void CreateEvent<TType>()
    {
        Events.Add(typeof(TType), _autoRemoveEVent ? new CustomEVent<TType>().AutoRemove() 
                                                   : new CustomEVent<TType>());
        _autoRemoveEVent = false;
    }
    
    /// <summary>
    /// Means on scene change all EVents are automatically unsubscribed
    /// </summary>
    /// <returns></returns>

    private EVentBindings AutoRemove()
    {
        _autoRemoveEVent = true;
        return this;
    }
}
