using System;
using System.Collections.Generic;
using UnityEngine;

public class CanvasOrderCalculator
{
    public static void SetUpCanvasOrderAtStart(ICanvasOrder branch)
    {
        var myCanvas = branch.ThisBranchesGameObject.GetComponent<Canvas>();
        var storeCanvasSetting = myCanvas.enabled;
        myCanvas.enabled = true;
        
        if (CheckIfSetToDefault(branch, myCanvas, storeCanvasSetting)) return;
        
        myCanvas.overrideSorting = true;
        ResetCanvasOrder(branch, myCanvas);
        myCanvas.enabled = storeCanvasSetting;
    }

    private static bool CheckIfSetToDefault(ICanvasOrder branch, Canvas myCanvas, bool storeCanvasSetting)
    {
        if (branch.CanvasOrder != OrderInCanvas.Default) return false;
        
        myCanvas.overrideSorting = false;
        myCanvas.enabled = storeCanvasSetting;
        return true;
    }

    public static void SetCanvasOrder(ICanvasOrder oldBranch, ICanvasOrder newBranch)
    {
        int oldBranchSortingOrder = oldBranch.MyCanvas.sortingOrder;
        
        switch (newBranch.CanvasOrder)
        {
            case OrderInCanvas.InFront:
                oldBranch.MyCanvas.sortingOrder = oldBranchSortingOrder - 1;
                newBranch.MyCanvas.sortingOrder = oldBranchSortingOrder + 1;
                break;
            case OrderInCanvas.Behind:
                newBranch.MyCanvas.sortingOrder = oldBranchSortingOrder - 1;
                break;
        }
    }

    public static void ResetCanvasOrder(ICanvasOrder branch, Canvas canvas)
    {
        switch (branch.CanvasOrder)
        {
            case OrderInCanvas.InFront:
                canvas.sortingOrder = 2;
                break;
            case OrderInCanvas.Behind:
                canvas.sortingOrder = -1;
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
    
    public static void ProcessActiveCanvasses(List<Canvas> activeCanvasList, int typeOffset)
    {
        for (var index = 0; index < activeCanvasList.Count; index++)
        {
            var canvasses = activeCanvasList[index];
            canvasses.sortingOrder = SetSortingOrder(index, typeOffset);
        }
    }

    private static int SetSortingOrder(int index, int typeOffset) => typeOffset + index;
}
