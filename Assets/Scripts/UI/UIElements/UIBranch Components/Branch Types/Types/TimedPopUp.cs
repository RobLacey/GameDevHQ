using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimedPopUpBranch : IBranchBase { }

public class TimedPopUp : BranchBase, IStartPopUp, ITimedPopUpBranch, IPopUpCanvasOrder
{
    public TimedPopUp(IBranch branch) : base(branch)
    {
        _myBranch.OnStartPopUp += StartPopUp;

        EVent.Do.Fetch<IPopUpCanvasOrder>()?.Invoke(this);
        branch.ManualCanvasOrder = PopUpCanvasOrder;
        branch.CanvasOrder = OrderInCanvas.Manual;
        timedPopUpCount = new List<Canvas>();
    }

    //Variables
    private bool _running;
    private Coroutine _coroutine;
    private int _optionalPopUpCount;
    private static List<Canvas> timedPopUpCount;

    //Set / Getters
    private void SavePopUpNumbers(INoPopUps args) 
        => _optionalPopUpCount = args.ActiveOptionalPopUpCount;

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<INoPopUps>(SavePopUpNumbers);
    }

    public int PopUpCanvasOrder { protected get; set; }

    public void StartPopUp()
    {
        if (_gameIsPaused || !_canStart || _resolvePopUps) return;

        SetIfRunningOrNot();
        _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
    }

    private void SetIfRunningOrNot()
    {
        if (!_running)
        {
            SetCanvas(ActiveCanvas.Yes);
            _running = true;
            SetCanvasOrder();
        }
        else
        {
            _myBranch.DoNotTween();
        }
    }

    protected override void SetCanvasOrder()
    {
        timedPopUpCount.Add(_myBranch.MyCanvas);
        
        for (var index = 0; index < timedPopUpCount.Count; index++)
        {
            var canvase = timedPopUpCount[index];
            canvase.sortingOrder = SetSortingOrder(canvase, index);
        }
    }

    protected override int SetSortingOrder(Canvas currentCanvas, int index)
    {
        currentCanvas.sortingOrder = PopUpCanvasOrder;
        return PopUpCanvasOrder + _optionalPopUpCount + index;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
    }

    private IEnumerator TimedPopUpProcess()
    {
        yield return new WaitForSeconds(_myBranch.Timer);
        ExitTimedPopUp();
    }
    
    private void ExitTimedPopUp()
    {
        timedPopUpCount.Remove(_myBranch.MyCanvas);
        SetCanvasOrder();
        _running = false;
        _myBranch.StartBranchExitProcess(OutTweenType.Cancel);
    }
}
