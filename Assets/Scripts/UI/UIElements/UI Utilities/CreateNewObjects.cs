﻿using System;
using UnityEngine;

public class CreateNewObjects
{
    public CreateNewObjects()
    {
        Create = this;
    }

    private CreateNewObjects Create { get; }
    private GameObject _newTree;
    private GameObject _newTooltipBin;
    private GameObject _newBranch;
    private GameObject _newNode;
    private GameObject _newFixedPos;
    private GameObject _newToolTips;

    public void CreateToolTipFolder(Transform hubTransform)
    {
        _newTooltipBin = new GameObject();
        _newTooltipBin.transform.parent = hubTransform;
        _newTooltipBin.name = $"ToolTips & Fixed Positions Go Here";
        _newTooltipBin.AddComponent<RectTransform>();
        _newTooltipBin.transform.position = hubTransform.position;
        CreateTooltipSubFolders(_newFixedPos, "Fixed Positions Here");
        CreateTooltipSubFolders(_newToolTips, "ToolTips Here");
    }

    private void CreateTooltipSubFolders(GameObject newFolder, string name)
    {
        newFolder = new GameObject();
        newFolder.transform.parent = _newTooltipBin.transform;
        newFolder.name = name;
        newFolder.AddComponent<RectTransform>();
        newFolder.transform.position = _newTooltipBin.transform.position;
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