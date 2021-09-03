using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIElements
{

    public interface IRuntimeStart
    {
        void RuntimeStart(IBranch branch);
    }
    
    public class RuntimeSettingExample : MonoBehaviour, IRuntimeStart
    {
        [SerializeField] 
        [ReorderableList] [Foldout(ToolTipFoldOut)]
        private List<ToolTipRuntimeData> _tooltipText;
        
        [SerializeField]
        [Foldout(ToolTipFoldOut)]
        private RectTransform _worldFixedPos;

        [SerializeField] 
        [Foldout(NavigationFoldOut)]
        private UIBranch _childBranch;

        [SerializeField] 
        [Foldout(EventFoldOut)] [InfoBox(EventFoldOutInfo)]
        private UnityEvent _onEnterEvent;
        
        [SerializeField] 
        [Foldout(EventFoldOut)] 
        private UnityEvent _onExitEvent;
        
        [SerializeField] 
        [Foldout(EventFoldOut)] 
        private UnityEvent _onClickEvent;
        
        [SerializeField] 
        [Foldout(EventFoldOut)] 
        private OnDisabledEvent _onDisabledEvent;
        
        [SerializeField] 
        [Foldout(EventFoldOut)] 
        private OnToggleEvent _onToggleEvent;

        //Variables
        private IRunTimeSetter _mySetter;
        private IEventSettings _eventSettings;
        private const string ToolTipFoldOut = "Tool Tip Settings";
        private const string NavigationFoldOut = "Navigation Settings";
        private const string EventFoldOut = "Events Settings";
        private const string EventFoldOutInfo = "Use these events to Set up GOUI Events or Call 'GetAllEvents()'" +
                                                " to set them from another script";

        //Example of setting variables at runtime. Call this method from only at Start
        private void Start()
        {
            RuntimeStart(GetComponent<IRuntimeData>().TargetBranch);
        }

        public void RuntimeStart(IBranch branch)
        {
            SetRunTimeSetter(branch);
            SetTooltips(_tooltipText.ToArray());
            SetFixedPosition(_worldFixedPos);
            SetChildBranch(_childBranch);
            SetUpEvents();
        }

        private void SetRunTimeSetter(IBranch branch) 
            => _mySetter = branch.ThisBranchesGameObject.GetComponentInChildren<IRunTimeSetter>();

        private void SetUpEvents()
        {
            if (_mySetter.GetEvents().IsNull()) return;
            SetOnEnterEvents(TriggerOnEnter);
            SetOnExitEvents(TriggerOnExit);
            SetOnClickEvents(TriggerOnClick);
            SetOnDisableEvents(TriggerOnDisable);
            SetOnToggleEvents(TriggerOnToggle);
        }

        private void SetTooltips(ToolTipRuntimeData[] newTooltips)
        {
            if (newTooltips.Length == 0) return;
            var tips = _mySetter.ReturnToolTipObjects();
            
            for (var index = 0; index < _tooltipText.Count; index++)
            {
                tips[index].GetComponentInChildren<Text>().text = newTooltips[index].Text;
                tips[index].GetComponentInChildren<Text>().color = newTooltips[index].Color;

                var hasImage = tips[index].GetComponentsInChildren<Image>();
                if (hasImage.Length > 1 && newTooltips[index].Sprite.IsNotNull())
                {
                    hasImage[1].sprite = newTooltips[index].Sprite;
                }
            }
        }

        private void SetFixedPosition(RectTransform newPos = null)
        {
            if(newPos == null) return;
            _mySetter.SetWorldFixedPosition?.Invoke(newPos);
        }

        private void SetChildBranch(UIBranch childBranch)
        {
            if(childBranch.IsNull()) return;
            
            _mySetter.SetChildBranch?.Invoke(childBranch);
        }

        public void SetTooltipAtIndex(int indexOfToolTip, string newTooltips)
        {
            if (newTooltips.IsNull()) return;
            var tips = _mySetter.ReturnToolTipObjects();
            
            tips[indexOfToolTip].GetComponentInChildren<Text>().text = newTooltips;
        }

        public IEventSettings GetAllEvents() => _mySetter.GetEvents();

        private void SetOnEnterEvents(UnityAction newEvent) 
            => _mySetter.GetEvents().OnEnterEvent.AddListener(newEvent);
        
        public void AddEnterEvents(UnityAction newEvent) 
            => _onEnterEvent.AddListener(newEvent);

        private void SetOnExitEvents(UnityAction newEvent) 
            => _mySetter.GetEvents().OnExitEvent.AddListener(newEvent);
        public void AddOnExitEvents(UnityAction newEvent) 
            => _onExitEvent.AddListener(newEvent);

        private void SetOnClickEvents(UnityAction newEvent) 
            => _mySetter.GetEvents().OnButtonClickEvent.AddListener(newEvent);
        
        public void AddOnClickEvents(UnityAction newEvent) 
            => _onClickEvent.AddListener(newEvent);

        private void SetOnDisableEvents(UnityAction<bool> newEvent) 
            => _mySetter.GetEvents().DisableEvent.AddListener(newEvent);
        public void AddOnDisableEvents(UnityAction<bool> newEvent) 
            => _onDisabledEvent.AddListener(newEvent);

        private void SetOnToggleEvents(UnityAction<bool> newEvent) 
            => _mySetter.GetEvents().ToggleEvent.AddListener(newEvent);
        public void AddOnToggleEvents(UnityAction<bool> newEvent) 
            => _onToggleEvent.AddListener(newEvent);
        
        private void TriggerOnEnter() => _onEnterEvent?.Invoke();
        private void TriggerOnExit() => _onExitEvent?.Invoke();
        private void TriggerOnClick() => _onClickEvent?.Invoke();
        private void TriggerOnDisable(bool isDisabled) => _onDisabledEvent?.Invoke(isDisabled);
        private void TriggerOnToggle(bool isOn) => _onToggleEvent?.Invoke(isOn);
    }
}