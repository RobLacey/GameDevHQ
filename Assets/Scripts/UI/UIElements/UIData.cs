using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

public interface IUIData
{
    UINode GetLastHighlighted { get; }
    GameObject GetLastHighlightedGO { get; }
    UINode GetLastSelected { get; }
    GameObject GetLastSelectedGO { get; }
    UIBranch GetActiveBranch { get; }
    bool GetOnHomeScreen { get; }
    bool GetControllingWithKeys { get; }
    bool GetInMenu { get; }
    List<UINode> GetSelectedNodes { get; }
    List<GameObject> GetSelectedGOs { get; }
}

[Serializable]
public class UIData : IMonoEnable, IEventUser, IUIData
{
    [SerializeField] private UINode _lastHighlighted = default;
    [SerializeField] private GameObject _lastHighlightedGO = default;
    [SerializeField] private UINode _lastSelected = default;
    [SerializeField] private GameObject _lastSelectedGO = default;
    [SerializeField] private UIBranch _activeBranch = default;
    [SerializeField] [ReadOnly] private bool _onHomeScreen = true;
    [SerializeField] [ReadOnly] private bool _controllingWithKeys = default;
    [SerializeField] [ReadOnly] private bool _inMenu;
    [SerializeField] private List<UINode> _selectedNodes = default;
    [SerializeField] private List<GameObject> _selectedGOs = default;

    //Properties
    public UINode GetLastHighlighted => _lastHighlighted;
    public GameObject GetLastHighlightedGO => _lastHighlightedGO;
    public UINode GetLastSelected => _lastSelected;
    public GameObject GetLastSelectedGO => _lastSelectedGO;
    public UIBranch GetActiveBranch => _activeBranch;
    public bool GetOnHomeScreen => _onHomeScreen;
    public bool GetControllingWithKeys => _controllingWithKeys;
    public bool GetInMenu => _inMenu;
    public List<UINode> GetSelectedNodes => _selectedNodes;
    public List<GameObject> GetSelectedGOs => _selectedGOs;

    //Main
    public void OnEnable() => ObserveEvents();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IHistoryData>(ManageHistory);
        EVent.Do.Subscribe<IHighlightedNode>(SaveLastHighlighted);
        EVent.Do.Subscribe<ISelectedNode>(SaveLastSelected);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IInMenu>(SaveInMenu);
        EVent.Do.Subscribe<ICloseAndResetBranch>(CloseAndReset);
    }

    private void CloseAndReset(ICloseAndResetBranch args)
    {
        if (_lastHighlightedGO.IsEqualTo(args.TargetBranch.LastHighlighted.InGameObject))
            _lastHighlightedGO = null;
        if (_lastSelectedGO.IsEqualTo(args.TargetBranch.LastSelected.InGameObject))
            _lastSelectedGO = null;
    }

    private void SaveLastHighlighted(IHighlightedNode args)
    {
        _lastHighlighted = (UINode) args.Highlighted;
        if (_lastHighlighted.InGameObject.IsNotNull())
            _lastHighlightedGO = _lastHighlighted.InGameObject;
    }
    private void SaveLastSelected(ISelectedNode args)  
    {
        _lastSelected = (UINode) args.UINode;
        if (_lastSelected.InGameObject.IsNotNull())
            _lastSelectedGO = _lastSelected.InGameObject;
    }
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = (UIBranch) args.ActiveBranch.ThisBranch;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveAllowKeys(IAllowKeys args) => _controllingWithKeys = args.CanAllowKeys;
    private void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;


    private void ManageHistory(IHistoryData args)
    {
        if (args.NodeToUpdate is null)
        {
            _selectedNodes.Clear();
            _selectedGOs.Clear();
            return;
        }
        if (_selectedNodes.Contains((UINode)args.NodeToUpdate))
        {
            _selectedNodes.Remove((UINode) args.NodeToUpdate);
            _selectedGOs.Remove(args.NodeToUpdate.InGameObject);
        }
        else
        {
            _selectedNodes.Add((UINode) args.NodeToUpdate);
            if(args.NodeToUpdate.InGameObject.IsNull())return;
            _selectedGOs.Add(args.NodeToUpdate.InGameObject);
        }
    }
}