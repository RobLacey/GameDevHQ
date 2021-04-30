using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIElements.Input_System
{
    public class MultiSelectSystem
    {
        public MultiSelectSystem(HistoryTracker historyTrack)
        {
            _historyTracker = historyTrack;
        }

        private MultiSelectGroup? _currentGroup = null;

        public static event Action<INode> MultiSelectChange;

        private readonly HistoryTracker _historyTracker;

        private readonly List<INode> _multiSelected = new List<INode>();
        private bool Pressed => Input.GetKey(KeyCode.LeftControl);
        public bool MultiSelectActive => _multiSelected.Count > 0;

        public bool MultiSelectPressed(List<INode> history, INode newNode)
        {
            if (newNode.MultiSelectSettings.AllowMultiSelect == IsActive.No) return false;
            
            if (_currentGroup == null)
            {
                _currentGroup = newNode.MultiSelectSettings.MultiSelectGroup;
            }
            else
            {
                if (_currentGroup != newNode.MultiSelectSettings.MultiSelectGroup)
                {
                    CloseAllNodesInList(_multiSelected);
                    MultiSelectChange?.Invoke(null);
                    _multiSelected.Clear();
                    _currentGroup = newNode.MultiSelectSettings.MultiSelectGroup;
                }
            }
            
            if (Pressed && newNode.MultiSelectSettings.AllowMultiSelect == IsActive.Yes)
            {
                if (_multiSelected.Contains(newNode))
                {
                    RemoveFromMultiSelect(newNode);
                }
                else
                {
                    if(!MultiSelectActive)
                        CheckAndAddExistingMulti(history);
                    AddToMultiSelect(newNode);
                }

                return true;
            }
            
            ClearAllMultiSelect();
            return false;
        }

        private void CheckAndAddExistingMulti(List<INode> history)
        {
            if (history.Count <= 0) return;
            
            var firstNode = history[0];
                
            if (firstNode.MultiSelectSettings.AllowMultiSelect == IsActive.Yes 
                && firstNode.MultiSelectSettings.MultiSelectGroup == _currentGroup)
            {
                if (firstNode.MultiSelectSettings.OpenChildBranch == IsActive.No)
                {
                    CloseActiveBranch(firstNode);
                }
                AddToMultiSelect(firstNode);
                history.Remove(firstNode);
                _historyTracker.AddNodeToTestRunner(firstNode);
            }
                
            ClearUnusedHistory(history);
        }

        private void ClearUnusedHistory(List<INode> history)
        {
            if (history.Count == 0) return;

            CloseAllNodesInList(history, RemoveFromHistory);
            history.Clear();

            void RemoveFromHistory(INode node) => _historyTracker.AddNodeToTestRunner(node);
        }

        private void AddToMultiSelect(INode newNode)
        {
            _multiSelected.Add(newNode);
            MultiSelectChange?.Invoke(newNode);
        }

        public void NodeHasBeenDestroyed(INode oldNode)
        {
            if(!_multiSelected.Contains(oldNode)) return;
            oldNode.DeactivateNode();
            RemoveFromMultiSelect(oldNode);
        }

        private void RemoveFromMultiSelect(INode oldNode)
        {
            _multiSelected.Remove(oldNode);
            CloseActiveBranch(oldNode);
            MultiSelectChange?.Invoke(oldNode);
        }

        public void ClearAllMultiSelect()
        {
            if(_multiSelected.Count == 0) return;

            var firstNode = _multiSelected.First();
            
            CloseAllNodesInList(_multiSelected);
            
            firstNode.SetNodeAsActive();
            MultiSelectChange?.Invoke(null);
            _multiSelected.Clear();
        }

        public INode CloseAlMultiSelectONCancelPressed()
        {
            var firstNode = _multiSelected.First();
            CloseAllNodesInList(_multiSelected);
            _multiSelected.Clear();
            MultiSelectChange?.Invoke(null);
            return firstNode;
        }

        private static void CloseAllNodesInList(List<INode> activeNodes, Action<INode> extraAction = null)
        {
            foreach (var node in activeNodes)
            {
                node.DeactivateNode();
                CloseActiveBranch(node);
                extraAction?.Invoke(node);
            }
        }

        private static void CloseActiveBranch(INode node)
        {
            node.HasChildBranch.LastSelected.DeactivateNode();
            node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        }
    }
}