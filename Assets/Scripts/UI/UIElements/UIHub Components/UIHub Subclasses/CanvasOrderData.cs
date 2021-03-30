using System;
using UnityEngine;

namespace UIElements
{
    [Serializable]
    public class CanvasOrderData : IEventDispatcher, ISetStartingCanvasOrder
    {
        [SerializeField] private int _pauseMenuCanvasOrder = 10;
        [SerializeField] private int _toolTipCanvasOrder = 20;
        [SerializeField] private int _resolvePopUpCanvasOrder = 5;
        [SerializeField] private int _timedPopUpCanvasOrder = 5;
        [SerializeField] private int _optionalPopUpCanvasOrder = 5;
        [SerializeField] private int _virtualCursorCanvasOrder = 5;
        [SerializeField] private int _GOUICanvasOrder = -3;

        //Events
        private Action<ISetStartingCanvasOrder> SetStartingCanvasOrder { get; set; }

        //Properties & Getters / Setters
        public ISetStartingCanvasOrder ReturnCanvasOrderData => this;
        public void OnEnable() => FetchEvents();
        public void OnStart() => SetStartingCanvasOrder?.Invoke(this);
        public void FetchEvents() => SetStartingCanvasOrder = EVent.Do.Fetch<ISetStartingCanvasOrder>();
        public int ReturnToolTipCanvasOrder() => _toolTipCanvasOrder;
        public int ReturnVirtualCursorCanvasOrder() => _virtualCursorCanvasOrder;
        
        //Main
        public int ReturnPresetCanvasOrder(CanvasOrderCalculator calculator)
        {
            switch (calculator.GetBranchType)
            {
                case BranchType.ResolvePopUp:
                    return _resolvePopUpCanvasOrder;
                case BranchType.OptionalPopUp:
                    return _optionalPopUpCanvasOrder;
                case BranchType.TimedPopUp:
                    return _timedPopUpCanvasOrder;
                case BranchType.PauseMenu:
                    return _pauseMenuCanvasOrder;
                case BranchType.Standard:
                    return SetStandardCanvasOrder(calculator);
                case BranchType.Internal:
                    return SetStandardCanvasOrder(calculator);
                case BranchType.InGameUi:
                    return _GOUICanvasOrder;
                case BranchType.HomeScreen:
                    return SetStandardCanvasOrder(calculator);
                default:
                    return 0;
            }
        }
        
        public int SetStandardCanvasOrder(CanvasOrderCalculator calculator)
        {
            switch (calculator.GetOrderInCanvas)
            {
                case OrderInCanvas.InFront:
                    return 2;
                case OrderInCanvas.Behind:
                    return -1;
                case OrderInCanvas.Manual:
                    return calculator.GetManualCanvasOrder;
                case OrderInCanvas.Default:
                    break;
            }

            return 0;
        }

    }
}