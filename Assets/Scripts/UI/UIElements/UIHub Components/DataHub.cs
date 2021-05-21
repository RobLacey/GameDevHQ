

using System.Collections.Generic;
using System.Linq;
using EnhancedHierarchy;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface IDataHub : IMonoAwake, IMonoEnable
{
    bool SceneAlreadyStarted { get; }
    bool InMenu { get; }
    bool GamePaused { get; }
    bool OnHomeScreen { get; }
    bool AllowKeys { get; }
    bool NoResolvePopUp { get; }
    bool NoPopups { get; }
    IBranch ActiveBranch { get; }
    RectTransform MainCanvasRect { get; }
    IBranch[] AllBranches { get; }
    List<GameObject> SelectedGOs { get; }
}

public class DataHub: IEZEventUser, IIsAService, IDataHub
{
    public DataHub(RectTransform mainCanvasRect)
    {
        MainCanvasRect = mainCanvasRect;
    }

    public bool SceneAlreadyStarted { get; private set; }
    public bool InMenu { get; private set; }
    public bool GamePaused { get; private set; }
    public bool OnHomeScreen { get; private set; } = true;
    public bool AllowKeys { get; private set; }
    public bool NoResolvePopUp { get; private set; }
    public bool NoPopups { get; private set; }
    public IBranch ActiveBranch { get; private set; }
    public RectTransform MainCanvasRect { get; }
    public IBranch[] AllBranches => Object.FindObjectsOfType<UIBranch>().ToArray<IBranch>();
    public List<GameObject> SelectedGOs { get; private set; } = new List<GameObject>();

    
    public void OnAwake() => AddService();

    public void OnEnable() => ObserveEvents();

    public void AddService() => EZService.Locator.AddNew<IDataHub>(this);

    public void OnRemoveService() { }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IHistoryData>(ManageHistory);
        HistoryEvents.Do.Subscribe<IOnStart>(SetStarted);
        HistoryEvents.Do.Subscribe<IInMenu>(SetIfInMenu);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(SetIfGamePaused);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SetIfOnHomeScreen);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SetActiveBranch);
        InputEvents.Do.Subscribe<IAllowKeys>(SetAllowKeys);
        PopUpEvents.Do.Subscribe<INoResolvePopUp>(SetNoResolvePopUps);
        PopUpEvents.Do.Subscribe<INoPopUps>(SetNoPopUps);
    }

    private void SetActiveBranch(IActiveBranch args) => ActiveBranch = args.ActiveBranch;

    private void SetNoPopUps(INoPopUps args) => NoPopups = args.NoActivePopUps;

    private void SetNoResolvePopUps(INoResolvePopUp args) => NoResolvePopUp = args.ActiveResolvePopUps;

    private void SetAllowKeys(IAllowKeys args) => AllowKeys = args.CanAllowKeys;

    private void SetIfOnHomeScreen(IOnHomeScreen args) => OnHomeScreen = args.OnHomeScreen;

    private void SetIfGamePaused(IGameIsPaused args) => GamePaused = args.GameIsPaused;

    private void SetIfInMenu(IInMenu args) => InMenu = args.InTheMenu;

    private void SetStarted(IOnStart args) => SceneAlreadyStarted = true;
    
    private void ManageHistory(IHistoryData args)
    {
        if (args.NodeToUpdate is null)
        {
            SelectedGOs.Clear();
            return;
        }
        if (SelectedGOs.Contains(args.NodeToUpdate.InGameObject))
        {
            SelectedGOs.Remove(args.NodeToUpdate.InGameObject);
        }
        else
        {
            if(args.NodeToUpdate.InGameObject.IsNull())return;
            SelectedGOs.Add(args.NodeToUpdate.InGameObject);
        }
    }

}
