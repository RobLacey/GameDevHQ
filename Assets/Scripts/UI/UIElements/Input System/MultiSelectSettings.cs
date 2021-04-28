using System;
using UnityEngine;

namespace UIElements.Input_System
{
    [Serializable]
    public class MultiSelectSettings
    {
        [SerializeField] 
        private IsActive _allowMultiSelect = IsActive.No;
        
        [SerializeField] 
        private IsActive _openChildBranch = IsActive.No;

        [SerializeField] 
        private MultiSelectGroup _multiSelectGroup;
        
        public IsActive AllowMultiSelect => _allowMultiSelect;
        public IsActive OpenChildBranch => _openChildBranch;
        public MultiSelectGroup MultiSelectGroup => _multiSelectGroup;
    }
}