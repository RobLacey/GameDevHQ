using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIElements
{
    public class RunTimeGetter : MonoBehaviour, IRunTimeGetter
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
        private string _branchTextOnCreation = String.Empty;
        private const string ToolTipFoldOut = "Tool Tip Settings";
        private const string NavigationFoldOut = "Navigation Settings";
        private const string EventFoldOut = "Events Settings";
        private const string EventFoldOutInfo = "Use these events to Set up GOUI Events or Call 'GetAllEvents()'" +
                                                " to set them from another script";

        //Main
        public IBranch CreateBranch(UIBranch template)
        {
            var hub = FindObjectOfType<UIHub>().transform;
            IBranch newBranch = template;
            
            if(template.ThisBranchesGameObject.GetIsAPrefab())
            {
                newBranch = Instantiate(template, new CreateNewObjects().CreateInGameUIBin(hub));
            }
            else
            {
                newBranch.ThisBranchesGameObject.transform.parent = new CreateNewObjects().CreateInGameUIBin(hub);
            }
            newBranch.ThisBranchesGameObject.name = $"{name} - In Game Ui";
            _mySetter = newBranch.ThisBranchesGameObject.GetComponentInChildren<IRunTimeSetter>();
            if(_branchTextOnCreation != String.Empty)
                _mySetter.ChangeMainText(_branchTextOnCreation);
            return newBranch;
        }

        public void BufferMainTextFroCreation(string newText) => _branchTextOnCreation = newText;

        public void SetToolTipDataOnCreation(ToolTipRuntimeData[] tooltipData)
        {
            foreach (var toolTipRuntimeData in tooltipData)
            {
                _tooltipText.Add(toolTipRuntimeData);
            }
        }
        

        private void Start()
        {
            SetTooltips(_tooltipText.ToArray());
            SetFixedPosition(_worldFixedPos);
            SetChildBranch(_childBranch);
            SetUpEvents();
        }

        private void SetUpEvents()
        {
            if (_mySetter.GetEvents().IsNull()) return;
            SetOnEnterEvents(_onEnterEvent);
            SetOnExitEvents(_onExitEvent);
            SetOnClickEvents(_onClickEvent);
            SetOnDisableEvents(_onDisabledEvent);
            SetOnToggleEvents(_onToggleEvent);
        }

        public void SetTooltips(ToolTipRuntimeData[] newTooltips)
        {
            if (newTooltips.IsNull()) return;
            var tips = _mySetter.ReturnToolTipObjects();
            
            for (var index = 0; index < _tooltipText.Count; index++)
            {
                tips[index].GetComponentInChildren<Text>().text = newTooltips[index].Text;
            }
        }
        
        public void SetTooltipAtIndex(int indexOfToolTip, string newTooltips)
        {
            if (newTooltips.IsNull()) return;
            var tips = _mySetter.ReturnToolTipObjects();
            
            tips[indexOfToolTip].GetComponentInChildren<Text>().text = newTooltips;
        }

        public void SetFixedPosition(RectTransform newPos = null)
        {
            if(newPos == null) return;
            _mySetter.SetWorldFixedPosition?.Invoke(newPos);
        }

        public void SetChildBranch(UIBranch childBranch)
        {
            if(childBranch.IsNull()) return;
            
            _mySetter.SetChildBranch?.Invoke(childBranch);
        }

        public IEventSettings GetAllEvents() => _mySetter.GetEvents();

        public void SetOnEnterEvents(UnityEvent newEvent) => _mySetter.GetEvents().OnEnterEvent = newEvent;

        public void SetOnExitEvents(UnityEvent newEvent) => _mySetter.GetEvents().OnExitEvent = newEvent;

        public void SetOnClickEvents(UnityEvent newEvent) => _mySetter.GetEvents().OnButtonClickEvent = newEvent;

        public void SetOnDisableEvents(OnDisabledEvent newEvent) => _mySetter.GetEvents().DisableEvent = newEvent;

        public void SetOnToggleEvents(OnToggleEvent newEvent) => _mySetter.GetEvents().ToggleEvent = newEvent;
    }
}

