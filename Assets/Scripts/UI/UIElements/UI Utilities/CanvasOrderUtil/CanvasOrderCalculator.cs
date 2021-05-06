using System.Collections.Generic;
using UIElements;
using UnityEngine;


public interface ICanvasOrderCalculator : IMonoStart, IMonoEnable
{
    BranchType GetBranchType { get; }
    int GetManualCanvasOrder { get; }
    OrderInCanvas GetOrderInCanvas { get; }
    void SetCanvasOrder();
    void ResetCanvasOrder();
    void ProcessActiveCanvasses(List<Canvas> activeCanvasList);
}

public class CanvasOrderCalculator: IEventUser, IEServUser, ICanvasOrderCalculator
{
    public CanvasOrderCalculator(ICanvasCalcParms data)
    {
        _myCanvas = data.MyBranch.MyCanvas;
        GetOrderInCanvas = data.MyBranch.CanvasOrder;
        GetBranchType = data.MyBranch.ReturnBranchType;
        if (GetOrderInCanvas == OrderInCanvas.Manual)
        {
            GetManualCanvasOrder = data.MyBranch.ReturnManualCanvasOrder;
        }
    }
    
    //Variables
    private IBranch _activeBranch;
    private readonly Canvas _myCanvas;
    private int _startingOrder;
    private ISetCanvasOrder _canvasOrder;

    //Properties
    public BranchType GetBranchType { get; }
    public int GetManualCanvasOrder { get; }
    public OrderInCanvas GetOrderInCanvas { get; }

    //Main

    public void OnEnable()
    {
        UseEServLocator();
        ObserveEvents();
    }
    
    public void UseEServLocator() => _canvasOrder = EServ.Locator.Get<ISetCanvasOrder>(this);

    public void ObserveEvents() => EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);

    public void OnStart() => SetUpCanvasOrderAtStart();

    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;

    private void SetUpCanvasOrderAtStart()
    {
        var tempSavedCanvasStatus = _myCanvas.enabled;
        _myCanvas.enabled = true;
        
        if (CheckIfSetToDefaultOrder(tempSavedCanvasStatus)) return;
        
        SetStartingSortingOrder(tempSavedCanvasStatus);
    }

    private void SetStartingSortingOrder(bool tempSavedCanvasStatus)
    {
        _startingOrder = _canvasOrder.ReturnPresetCanvasOrder(this);
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
