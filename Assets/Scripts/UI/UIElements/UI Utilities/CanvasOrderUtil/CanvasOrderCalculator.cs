using System.Collections.Generic;
using UnityEngine;

public class CanvasOrderCalculator: IEventUser
{
    private IBranch _activeBranch;
    private readonly Canvas _myCanvas;
    private int _startingOrder;
    private ISetStartingCanvasOrder _startingCanvasOrder;

    //Properties
    public BranchType GetBranchType { get; }

    public OrderInCanvas GetOrderInCanvas { get; }

    public int GetManualCanvasOrder { get; }
    

    public CanvasOrderCalculator(IBranch branch)
    {
        _myCanvas = branch.MyCanvas;
        GetOrderInCanvas = branch.CanvasOrder;
        GetBranchType = branch.ReturnBranchType;
        if (GetOrderInCanvas == OrderInCanvas.Manual)
        {
            GetManualCanvasOrder = branch.ReturnManualCanvasOrder;
        }
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetStartingSortingOrder);
    }

    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;

    private void SetStartingSortingOrder(ISetStartingCanvasOrder args)
    {
        _startingCanvasOrder = args.ReturnCanvasOrderData;
        SetUpCanvasOrderAtStart();
    }
    private void SetUpCanvasOrderAtStart()
    {
        var tempSavedCanvasStatus = _myCanvas.enabled;
        _myCanvas.enabled = true;
        
        if (CheckIfSetToDefaultOrder(tempSavedCanvasStatus)) return;
        
        SetStartingSortingOrder(tempSavedCanvasStatus);
    }

    private void SetStartingSortingOrder(bool tempSavedCanvasStatus)
    {
        _startingOrder = _startingCanvasOrder.ReturnPresetCanvasOrder(this);
        _myCanvas.overrideSorting = true;
        _myCanvas.sortingOrder = _startingOrder;
        _myCanvas.enabled = tempSavedCanvasStatus;
    }

    private bool CheckIfSetToDefaultOrder(bool storeCanvasSetting)
    {
        if (GetOrderInCanvas != OrderInCanvas.Default) return false;
        
        _startingOrder = _myCanvas.sortingOrder;
        _myCanvas.overrideSorting = false;
        _myCanvas.enabled = storeCanvasSetting;
        return true;
    }

    public void SetCanvasOrder()
    {
        if(_activeBranch.IsNull() || _myCanvas.sortingOrder > _startingOrder) return;

        switch (GetOrderInCanvas)
        {
            case OrderInCanvas.InFront:
                _myCanvas.sortingOrder++;
                break;
            case OrderInCanvas.Behind:
                _myCanvas.sortingOrder--;
                break;
            case OrderInCanvas.Manual:
                if (_activeBranch.CanvasOrder == GetOrderInCanvas)
                {
                    _myCanvas.sortingOrder++;
                }
                break;
        }
    }

    public void ResetCanvasOrder()
    {
        _myCanvas.sortingOrder = _startingOrder;
    }
    
    public void ProcessActiveCanvasses(List<Canvas> activeCanvasList)
    {
        for (var index = 0; index < activeCanvasList.Count; index++)
        {
            var canvasses = activeCanvasList[index];
            canvasses.sortingOrder = _startingOrder + index;
        }
    }
}
