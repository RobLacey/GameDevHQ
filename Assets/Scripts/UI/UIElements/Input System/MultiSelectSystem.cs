using System.Collections.Generic;

namespace UIElements.Input_System
{
    public class MultiSelectSystem : IEServUser
    {
        public MultiSelectSystem(IHistoryTrack historyTrack)
        {
            _historyTracker = historyTrack;
            UseEServLocator();
        }
        
        public void UseEServLocator() => _inputScheme = EServ.Locator.Get<InputScheme>(this);

        //variables
        private InputScheme _inputScheme;
        private MultiSelectGroup? _currentGroup = null;
        private readonly IHistoryTrack _historyTracker;

        //Properties & Getters / Setters
        private bool Pressed => _inputScheme.MultiSelectPressed();
       public bool MultiSelectActive { get; set; }
        
        //Main
        public bool MultiSelectPressed(List<INode> history, INode newNode)
        {
            if (newNode.MultiSelectSettings.AllowMultiSelect == IsActive.No) return false;
            
            CheckTheCurrentGroupSelection(history, newNode);
            
            if (DoMultiSelectionProcess(history, newNode))
            {
                return true;
            }
            return false;
        }

        private void CheckTheCurrentGroupSelection(List<INode> history, INode newNode)
        {
            if (_currentGroup == null)
            {
                _currentGroup = newNode.MultiSelectSettings.MultiSelectGroup;
            }
            else
            {
                IsInAnotherGroup(history, newNode);
            }
        }

        private void IsInAnotherGroup(List<INode> history, INode newNode)
        {
            if (_currentGroup != newNode.MultiSelectSettings.MultiSelectGroup)
            {
                CloseAllNodesInList(history);
                _historyTracker.UpdateHistoryData(null);
                _currentGroup = newNode.MultiSelectSettings.MultiSelectGroup;
            }
        }

        private bool DoMultiSelectionProcess(List<INode> history, INode newNode)
        {
            if (Pressed)
            {
                if (history.Contains(newNode))
                {
                    RemoveFromMultiSelect(history, newNode);
                }
                else
                {
                    if (!MultiSelectActive)
                        CheckAndAddExistingMulti(history);
                    AddToMultiSelect(history, newNode);
                }
                return true;
            }
            return false;
        }

        private void CheckAndAddExistingMulti(List<INode> history)
        {
            if (history.Count <= 0) return;
            var currentHistory = history.ToArray();
            
            foreach (var node in currentHistory)
            {
                if (CanAddNode(node))
                {
                    AddNodeToMultiSelectList(history, node);
                }
                else
                {
                    node.DeactivateNode();
                    RemoveFromMultiSelect(history, node);
                }
            }
        }

        private bool CanAddNode(INode node)
        {
            return node.MultiSelectSettings.AllowMultiSelect == IsActive.Yes 
                   && node.MultiSelectSettings.MultiSelectGroup == _currentGroup;
        }

        private void AddNodeToMultiSelectList(List<INode> history, INode firstNode)
        {
            if (firstNode.MultiSelectSettings.OpenChildBranch == IsActive.No)
            {
                CloseActiveBranch(firstNode);
            }

            AddToMultiSelect(history, firstNode);
            history.Remove(firstNode);
            _historyTracker.UpdateHistoryData(firstNode);
        }

        private void AddToMultiSelect(List<INode> history, INode newNode)
        {
            history.Add(newNode);
            _historyTracker.UpdateHistoryData(newNode);
            MultiSelectActive = true;
        }

        private void RemoveFromMultiSelect(List<INode> history, INode oldNode)
        {
            history.Remove(oldNode);
            CloseActiveBranch(oldNode);
            _historyTracker.UpdateHistoryData(oldNode);
            if (history.Count == 0)
                MultiSelectActive = false;
        }

        public void ClearMultiSelect() => MultiSelectActive = false;

        private static void CloseAllNodesInList(List<INode> activeNodes)
        {
            foreach (var node in activeNodes)
            {
                node.DeactivateNode();
                CloseActiveBranch(node);
            }
        }

        private static void CloseActiveBranch(INode node)
        {
            if (node.HasChildBranch.IsNull()) return;
            
            node.HasChildBranch.LastSelected.DeactivateNode();
            node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        }
    }
}