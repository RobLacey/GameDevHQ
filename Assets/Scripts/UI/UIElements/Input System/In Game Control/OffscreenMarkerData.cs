using System;
using NaughtyAttributes;
using UnityEngine;

namespace UIElements
{
    [Serializable]
    public class OffscreenMarkerData
    {
        [Header("Settings")]
        [SerializeField] 
        private GameObject _offScreenMarker;
        [SerializeField]
        private StartOffscreen _whenToStartOffScreenMarker = StartOffscreen.OnlyWhenSelected;
        [SerializeField]
        [Range(0, 10)] private int _frameFrequency = 5;
        [SerializeField]
        private Vector2 _screenSafeMargin = Vector2.zero;
        [SerializeField]
        [Label(MarkerFolderName)]
        private Transform _markerFolder;

        //Editor
        private const string MarkerFolderName = "Marker Folder(Optional)";

        //Getters / Setters
        public GameObject ScreenMarker => _offScreenMarker;

        public StartOffscreen WhenToStartOffScreenMarker => _whenToStartOffScreenMarker;

        public Vector2 ScreenSafeMargin => _screenSafeMargin;

        public int FrameFrequency => _frameFrequency;

        public Transform MarkerFolder => _markerFolder;
        public bool CanUseOffScreenMarker => _offScreenMarker;
    }
}