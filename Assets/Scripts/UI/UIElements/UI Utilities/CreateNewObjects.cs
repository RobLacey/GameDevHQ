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
    private GameObject _newFixedPos;
    private GameObject _newToolTips;
    
    private const string FixedPositionsBin ="Fixed Positions Here";
    private const string ToolTipBin ="Tooltips Here";
    private const string MainBinName = "ToolTips & Fixed Positions Go Here";
    
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
        CreateTooltipSubFolders(_newFixedPos, FixedPositionsBin);
        CreateTooltipSubFolders(_newToolTips, ToolTipBin);
        return Create;
    }

    public Transform GetTooltipBin()
    {
        return _mainBinName.transform.Find(ToolTipBin);
    }
    
    private void CreateTooltipSubFolders(GameObject newFolder, string name)
    {
        newFolder = new GameObject();
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