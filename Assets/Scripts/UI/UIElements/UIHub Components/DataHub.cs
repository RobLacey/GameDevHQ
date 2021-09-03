

using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface IDataHub : IMonoAwake, IMonoEnable
{
    bool SceneStarted { get; }
    bool InMenu { get; }
    bool GamePaused { get; }
    bool OnHomeScreen { get; }
    void SetOnHomeScreen(bool IsOnHomeScreen);
    bool AllowKeys { get;}
    void SetAllowKeys(bool newAllowKeysSetting);
    bool NoResolvePopUp { get; }
    bool NoPopups { get; }
    IBranch ActiveBranch { get; }
    RectTransform MainCanvasRect { get; }
    IBranch[] AllBranches { get; }
    List<GameObject> SelectedGOs { get; }
    INode Selected { get; }
    INode Highlighted { get; }

}

public class DataHub: IEZEventUser, IIsAService, IDataHub
{
    public DataHub(RectTransform mainCanvasRect)
    {
        MainCanvasRect = mainCanvasRect;
    }

    public bool SceneStarted { get; private set; }
    public bool InMenu { get; private set; }
    public bool GamePaused { get; private set; }
    public bool OnHomeScreen { get; set; } = true;
    public bool AllowKeys { get; private set; }
    public bool NoResolvePopUp { get; private set; } = true;
    public bool NoPopups { get; private set; } = true;
    public IBranch ActiveBranch { get; private set; }
    public RectTransform MainCanvasRect { get; }
    public IBranch[] AllBranches => Object.FindObjectsOfType<UIBranch>().ToArray<IBranch>();
    public List<GameObject> SelectedGOs { get; private set; } = new List<GameObject>();
    public INode Selected { get; private set; }
    public INode Highlighted { get; private set; }

    public void OnAwake() => AddService();

    public void OnEnable() => ObserveEvents();

    public void AddService() => EZService.Locator.AddNew<IDataHub>(this);

    public void OnRemoveService() { }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IStoreNodeHistoryData>(ManageHistory);
        HistoryEvents.Do.Subscribe<IOnStart>(SetStarted);
        HistoryEvents.Do.Subscribe<IInMenu>(SetIfInMenu);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(SetIfGamePaused);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SetActiveBranch);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SetHighlighted);
        HistoryEvents.Do.Subscribe<ISelectedNode>(SetSelected);
        PopUpEvents.Do.Subscribe<INoResolvePopUp>(SetNoResolvePopUps);
        PopUpEvents.Do.Subscribe<INoPopUps>(SetNoPopUps);
    }

    public void UnObserveEvents() { }

    private void SetSelected(ISelectedNode args) => Selected = args.UINode;
    
    private void SetHighlighted(IHighlightedNode args) => Highlighted = args.Highlighted;

    private void SetActiveBranch(IActiveBranch args) => ActiveBranch = args.ActiveBranch;

    private void SetNoPopUps(INoPopUps args) => NoPopups = args.NoActivePopUps;

    private void SetNoResolvePopUps(INoResolvePopUp args) => NoResolvePopUp = args.NoActiveResolvePopUps;

    public void SetAllowKeys(bool newAllowKeysSetting) => AllowKeys = newAllowKeysSetting;
    public void SetOnHomeScreen(bool IsOnHomeScreen) => OnHomeScreen = IsOnHomeScreen;

    private void SetIfGamePaused(IGameIsPaused args) => GamePaused = args.IsPaused;

    private void SetIfInMenu(IInMenu args) => InMenu = args.InTheMenu;

    private void SetStarted(IOnStart args) => SceneStarted = true;
    
    private void ManageHistory(IStoreNodeHistoryData args)
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
