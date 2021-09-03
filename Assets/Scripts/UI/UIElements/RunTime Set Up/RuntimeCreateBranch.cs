using System;
using EnhancedHierarchy.Icons;
using UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

public class RuntimeCreateBranch
{
    private CreateNewObjects _binGetter = new CreateNewObjects();

    public IBranch CreateGOUI(UIBranch template)
    {
        var parent = _binGetter.MakeGOUIBin(HubTransform());
        return CreateBranch(template, parent);
    }
        
    public IBranch CreatePopUp(UIBranch template, Transform parent)
    {
        return CreateBranch(template, parent);
    }
    
    public IBranch CreatePopUp(UIBranch template)
    {
        var parent = _binGetter.MakePopUpBin(HubTransform());
        return CreateBranch(template, parent);
    }
    
    private Transform HubTransform() => Object.FindObjectOfType<UIHub>().transform;

    //Main
    private IBranch CreateBranch(UIBranch template, Transform createParent)
    {
        IBranch newBranch = template;
            
        if(template.ThisBranchesGameObject.GetIsAPrefab())
        {
            newBranch = Object.Instantiate(template, createParent);
        }
        else
        {
            newBranch.ThisBranchesGameObject.transform.parent = createParent;
        }
        
        return newBranch;
    }

}