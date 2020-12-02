
using System.Collections;

public class EVentBindings : BindBase
{
    private Hashtable _events = new Hashtable();

    public EVentBindings()
    {
        BindAllObjects();
        EVent.Do.EVentsList = _events;
    }

    private void CreateEvent<TType>() => _events.Add(typeof(TType), new CustomEVent<TType>());
    
    protected sealed override void BindAllObjects()
    {
        if (CheckIfAlreadyBound()) return;

        //PopUps
        CreateEvent<IRemoveOptionalPopUp>();//
        CreateEvent<IAddOptionalPopUp>();//
        CreateEvent<IAddResolvePopUp>();//
        CreateEvent<INoResolvePopUp>();//
        CreateEvent<INoPopUps>();//
        
        //History
        CreateEvent<IReturnToHome>();//
        CreateEvent<IOnHomeScreen>();//
        CreateEvent<IOnStart>();//
        CreateEvent<IGameIsPaused>();//
        CreateEvent<IInMenu>();//
        CreateEvent<ISceneChange>();
       // CreateEvent<ISceneStart>();
        
        //Input
        CreateEvent<IPausePressed>();
        CreateEvent<ISwitchGroupPressed>();//
        CreateEvent<IHotKeyPressed>();//
        CreateEvent<IMenuGameSwitchingPressed>();//
        
        //ChangeControl
        CreateEvent<IAllowKeys>();//
        CreateEvent<IChangeControlsPressed>();//
        
        //Cancel
        CreateEvent<ICancelHoverOverButton>();
        CreateEvent<ICancelPressed>();//
        CreateEvent<ICancelButtonActivated>();//
        CreateEvent<ICancelPopUp>();//
        CreateEvent<ICancelHoverOver>();
        
        // //Node
        CreateEvent<IHighlightedNode>();//
        CreateEvent<ISelectedNode>();//
        CreateEvent<IDisabledNode>();//
        
        //Branch
        CreateEvent<IActiveBranch>();//
        CreateEvent<IClearScreen>();//
        CreateEvent<ISetUpStartBranches>();//
        
        //Test
        CreateEvent<ITestList>();
    }

}