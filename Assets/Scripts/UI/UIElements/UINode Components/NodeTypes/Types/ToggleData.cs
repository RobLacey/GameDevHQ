using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ToggleData
{
    [SerializeField] 
    private ToggleGroup _toggleGroupId = ToggleGroup.TG1;
    [SerializeField] 
    private IsActive _startAsSelected = IsActive.No;
    [SerializeField] [AllowNesting] [ValidateInput("CheckTabBranch", "Must be a Standard Type branch")]
    private UIBranch _tabBranch;

    public ToggleGroup ReturnToggleId => _toggleGroupId;
    public UIBranch ReturnTabBranch => _tabBranch;

    public IsActive StartAsSelected => _startAsSelected;
    public void SetStartAsSelected() => _startAsSelected = IsActive.Yes;

    private bool CheckTabBranch()
    {
        if (_tabBranch is null) return true;
        return _tabBranch.ReturnBranchType == BranchType.Standard;
    }

}
