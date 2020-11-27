using UnityEngine;

public class EVentBindings : BindBase
{
    protected override void BindAllObjects()
    {
        if (CheckIfAlreadyBound()) return;

        //PopUps
        EVentMaster.CreateEvent<IRemoveOptionalPopUp>();
        EVentMaster.CreateEvent<IAddOptionalPopUp>();
        EVentMaster.CreateEvent<IAddResolvePopUp>();
        EVentMaster.CreateEvent<INoResolvePopUp>();
        EVentMaster.CreateEvent<INoPopUps>();
        
        //History
        EVentMaster.CreateEvent<IReturnToHome>();
        EVentMaster.CreateEvent<IOnHomeScreen>();
        EVentMaster.CreateEvent<IOnStart>();
        EVentMaster.CreateEvent<IGameIsPaused>();
        EVentMaster.CreateEvent<IInMenu>();
        
        //Input
        EVentMaster.CreateEvent<IPausePressed>();
        EVentMaster.CreateEvent<ISwitchGroupPressed>();
        EVentMaster.CreateEvent<IHotKeyPressed>();
        EVentMaster.CreateEvent<IMenuGameSwitchingPressed>();
        
        //ChangeControl
        EVentMaster.CreateEvent<IAllowKeys>();
        EVentMaster.CreateEvent<IChangeControlsPressed>();
        
        //Cancel
        EVentMaster.CreateEvent<ICancelHoverOverButton>();
        EVentMaster.CreateEvent<ICancelPressed>();
        EVentMaster.CreateEvent<ICancelButtonActivated>();
        EVentMaster.CreateEvent<ICancelPopUp>();
        EVentMaster.CreateEvent<ICancelHoverOver>();
        
        //Node
        EVentMaster.CreateEvent<IHighlightedNode>();
        EVentMaster.CreateEvent<ISelectedNode>();
        EVentMaster.CreateEvent<IDisabledNode>();
        
        //Branch
        EVentMaster.CreateEvent<IActiveBranch>();
        EVentMaster.CreateEvent<IClearScreen>();
        EVentMaster.CreateEvent<ISetUpStartBranches>();
        
        //Test
        EVentMaster.CreateEvent<ITestList>();


        
        
        
    }

}