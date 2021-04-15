using System;
using UnityEngine;

namespace UIElements
{
    [Serializable]
    public class CanvasOrderData : IEventDispatcher, ISetStartingCanvasOrder
    {
        [SerializeField] private int _pauseMenu = 20;
        [SerializeField] private int _toolTip = 25;
        [SerializeField] private int _resolvePopUp = 20;
        [SerializeField] private int _timedPopUp = 15;
        [SerializeField] private int _optionalPopUp = 15;
        [SerializeField] private int _virtualCursor = 30;
        [SerializeField] private int _inGameObject = -3;
        [SerializeField] private int _offScreenMarker = 10;
        [SerializeField] private int _controlBar = 12;

        //Events
        private Action<ISetStartingCanvasOrder> SetStartingCanvasOrder { get; set; }

        //Properties & Getters / Setters
        public ISetStartingCanvasOrder ReturnCanvasOrderData => this;
        public void OnEnable() => FetchEvents();
        public void OnStart() => SetStartingCanvasOrder?.Invoke(this);
        public void FetchEvents() => SetStartingCanvasOrder = EVent.Do.Fetch<ISetStartingCanvasOrder>();
        public int ReturnToolTipCanvasOrder() => _toolTip;
        public int ReturnVirtualCursorCanvasOrder() => _virtualCursor;
        public int ReturnOffScreenMarkerCanvasOrder() => _offScreenMarker;
        public int ReturnControlBarCanvasOrder() => _controlBar;
        
        //Main
        public int ReturnPresetCanvasOrder(CanvasOrderCalculator calculator)
        {
            switch (calculator.GetBranchType)
            {
                case BranchType.ResolvePopUp:
                    return _resolvePopUp;
                case BranchType.OptionalPopUp:
                    return _optionalPopUp;
                case BranchType.TimedPopUp:
                    return _timedPopUp;
                case BranchType.PauseMenu:
                    return _pauseMenu;
                case BranchType.Standard:
                    return SetStandardCanvasOrder(calculator);
                case BranchType.Internal:
                    return SetStandardCanvasOrder(calculator);
                case BranchType.InGameObject:
                    return _inGameObject;
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