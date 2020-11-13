using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ToggleData
{
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] 
    private bool _startAsSelected;
    [SerializeField]
    private UIBranch _tabBranch;

}
