
using System;
using UnityEngine;

[Serializable]
public class CanvasOrderCalculator
{

    public static void SetUpCanvasOrderAtStart(ICanvasOrder branch)
    {
        var myCanvas = branch.ThisBranchesGameObject.GetComponent<Canvas>();
        var isEnabled = myCanvas.enabled;
        myCanvas.enabled = true;
        
        if(branch.CanvasOrder == OrderInCanvas.Default)
        {
            myCanvas.overrideSorting = false;
            myCanvas.enabled = isEnabled;
            return;
        }
        myCanvas.overrideSorting = true;
        ResetCanvasOrder(branch, myCanvas);
        myCanvas.enabled = isEnabled;
    }
    
    public static void SetCanvasOrder(ICanvasOrder oldBranch, ICanvasOrder newBranch)
    {
        int oldBranchSortingOrder = oldBranch.MyCanvas.sortingOrder;
        
        // if (oldBranch.CanvasOrder == default)
        // {
        //     oldBranchSortingOrder = 0;
        // }
        // else
        // {
        //     oldBranchSortingOrder = oldBranch.MyCanvas.sortingOrder;
        // }
        
        switch (newBranch.CanvasOrder)
        {
            case OrderInCanvas.InFront:
                newBranch.MyCanvas.sortingOrder = oldBranchSortingOrder + 1;
                oldBranch.MyCanvas.sortingOrder = oldBranchSortingOrder -1;
                break;
            case OrderInCanvas.Behind:
                newBranch.MyCanvas.sortingOrder = oldBranchSortingOrder - 1;
                Debug.Log($"{newBranch} : {oldBranch} : {newBranch.MyCanvas.sortingOrder}");
                break;
        }
    }

    public static void ResetCanvasOrder(ICanvasOrder branch, Canvas canvas)
    {
        switch (branch.CanvasOrder)
        {
            case OrderInCanvas.InFront:
                canvas.sortingOrder = 1;
                break;
            case OrderInCanvas.Behind:
                canvas.sortingOrder = - 1;
                break;
            case OrderInCanvas.Manual:
                canvas.sortingOrder = branch.ManualCanvasOrder;
                break;
            case OrderInCanvas.Default:
                canvas.sortingOrder = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
