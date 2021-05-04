using System;
using UnityEngine;

public class CreateNewObjects
{
    public CreateNewObjects()
    {
        Create = this;
    }

    private CreateNewObjects Create { get; }

    private GameObject _newTree;
    private GameObject _mainBinName;
    private GameObject _newBranch;
    private GameObject _newNode;
    
    private const string FixedPositionsBin ="Fixed Positions Here";
    private const string ToolTipBin ="Tooltips Here";
    private const string MainBinName = "ToolTips & Fixed Positions Go Here";
    private const string InGameObjectBinName = "In Game Objects UI Go Here";
    
    public CreateNewObjects CreateToolTipFolder(Transform hubTransform)
    {
        if (hubTransform.Find(MainBinName))
        {
            _mainBinName = hubTransform.Find(MainBinName).gameObject;
            return Create;
        }
        
        _mainBinName = new GameObject();
        _mainBinName.AddComponent<RectTransform>();
        _mainBinName.transform.parent = hubTransform;
        _mainBinName.transform.position = hubTransform.position;
        _mainBinName.name = MainBinName;
        CreateTooltipSubFolders(FixedPositionsBin);
        CreateTooltipSubFolders(ToolTipBin);
        return Create;
    }

    public Transform GetTooltipBin() => _mainBinName.transform.Find(ToolTipBin);

    public Transform CreateInGameUIBin(Transform hubTransform)
    {
        var existingBin = hubTransform.Find(InGameObjectBinName);
        
        if (existingBin)
            return existingBin;

        var newBin = new GameObject();
        newBin.AddComponent<RectTransform>();
        newBin.transform.parent = hubTransform;
        newBin.transform.position = hubTransform.position;
        newBin.name = InGameObjectBinName;
        return newBin.transform;
    }

    private void CreateTooltipSubFolders(string name)
    {
        var newFolder = new GameObject();
        newFolder.transform.parent = _mainBinName.transform;
        newFolder.name = name;
        newFolder.AddComponent<RectTransform>();
        newFolder.transform.position = _mainBinName.transform.position;
    }

    public CreateNewObjects CreateMainFolder(Transform hubTransform)
    {
        _newTree = new GameObject();
        _newTree.transform.parent = hubTransform;
        _newTree.name = $"New Tree Folder";
        _newTree.AddComponent<RectTransform>();
        return Create;
    }

    public CreateNewObjects CreateBranch(Transform parent = null)
    {
        if(_newTree is null && parent is null) 
            throw new Exception("Must Assign Parent or CreateMainFolder Method First");
        
        _newBranch = new GameObject();
        _newBranch.transform.parent = parent ? parent : _newTree.transform;
        _newBranch.name = $"New Branch";
        _newBranch.AddComponent<UIBranch>();
        return Create;
    }
    
    public void CreateNode(Transform parent = null)
    {
        if(_newBranch is null && parent is null) 
            throw new Exception("Must Assign Parent or CreateBranch Method First");

        _newNode = new GameObject();
        _newNode.transform.parent = parent ? parent : _newBranch.transform;
        _newNode.name = $"New Node ";
        _newNode.AddComponent<UINode>();
    }
}