using System;
using UnityEngine;

namespace UIElements
{
    [Serializable]
    public class ToolTipRuntimeData
    {
        [TextArea(3,3)]
        [SerializeField] private string _text = "Place Holder Text";
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _colour = Color.white;

        public string Text => _text;
        public Sprite Sprite => _sprite;
        public Color Color => _colour;
    }
}