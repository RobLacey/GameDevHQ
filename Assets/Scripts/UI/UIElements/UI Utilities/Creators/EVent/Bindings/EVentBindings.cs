using System.Collections;
using UnityEngine;

public class EVentBindings : IEVentBindings
{
    private bool _autoRemoveEVent;
    
    public EVentBindings(ISetNewEVent setNewEVentHandler)
    {
        BindAllObjects();
        setNewEVentHandler.SetUpEVentHandler((IEVentBase) setNewEVentHandler, Events);
    }
    
    public Hashtable Events { get; } = new Hashtable();

    /// <summary>
    /// Means on scene change all EVents are automatically unsubscribed
    /// </summary>
    /// <returns></returns>
    private EVentBindings AutoRemove()
    {
        _autoRemoveEVent = true;
        return this;
    }
    
    public void CreateEvent<TType>()
    {
        Events.Add(typeof(TType), _autoRemoveEVent ? new CustomEVent<TType>().AutoRemove() 
                                                   : new CustomEVent<TType>());
        _autoRemoveEVent = false;
    }

    public void BindAllObjects()
    {
        //PopUps
        AutoRemove().CreateEvent<IRemoveOptionalPopUp>();
        AutoRemove().CreateEvent<IAddOptionalPopUp>();
        AutoRemove().CreateEvent<IAddResolvePopUp>();
        AutoRemove().CreateEvent<INoResolvePopUp>();
        AutoRemove().CreateEvent<INoPopUps>();
        
        //History
        AutoRemove().CreateEvent<IReturnToHome>();
        CreateEvent<IOnHomeScreen>();
        CreateEvent<IOnStart>();
        CreateEvent<IGameIsPaused>();
        CreateEvent<IInMenu>();
        CreateEvent<ISceneChange>();// Doesn't Clear until exit
        
        //Input
        AutoRemove().CreateEvent<IPausePressed>();
        AutoRemove().CreateEvent<ISwitchGroupPressed>();
        CreateEvent<IHotKeyPressed>();
        CreateEvent<IMenuGameSwitchingPressed>();
        
        //ChangeControl
        AutoRemove().CreateEvent<IAllowKeys>();
        AutoRemove().CreateEvent<IChangeControlsPressed>();
        
        //Cancel
        CreateEvent<ICancelHoverOverButton>();
        AutoRemove().CreateEvent<ICancelPressed>();
        CreateEvent<ICancelButtonActivated>();
        CreateEvent<ICancelPopUp>();
        CreateEvent<ICancelHoverOver>();
        
        // //Node
        CreateEvent<IHighlightedNode>();
        CreateEvent<ISelectedNode>();
        CreateEvent<IDisabledNode>();
        
        //Branch
        CreateEvent<IActiveBranch>();
        CreateEvent<IClearScreen>();
        CreateEvent<ISetUpStartBranches>();
        CreateEvent<IEndTween>();
        
        //Test
        CreateEvent<ITestList>();
    }

}