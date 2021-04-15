using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UIElements
{
    [Serializable]
    public class OffScreenMarker : IEServUser, IMono
    {
        [SerializeField] 
        private GameObject _offScreenMarker;
        [SerializeField] 
        private Vector2 _screenSafeMargin = Vector2.zero;
        [SerializeField] 
        [Range(0, 10)] private int _frameFrequency = 5;
        [SerializeField]
        [Label(MarkerFolderName)]
        private Transform _markerFolder;

        //Variables
        private int _screenLeft, _screenRight, _screenTop, _screenBottom;
        private IHub _hub;
        private Canvas _offScreenMarkerCanvas;
        private Vector3 _lastPosition = Vector3.zero;
        private Camera _camera;
        private Coroutine _offScreenMarkerCoroutine;
        private RectTransform _offScreenMarkerRect;
        private readonly WaitFrameCustom _waitFrameCustom = new WaitFrameCustom();
        private Transform _parentTransform;
        private GOUIModule _gouiModule;

        //Editor
        private const string OffScreenMarkerFolderName = "Off Screen Marker";
        private const string MarkerFolderName = "Marker Folder(Optional)";
        
        //TODO Add Max Distance from Camera cull process and setting

        //Properties & Getters / Setters
        private bool UseOffScreen => _gouiModule.CanUseOffScreen;

        public void SetParent(GOUIModule parent)
        {
            _gouiModule = parent;
            _parentTransform = parent.transform;
        }

        public void OnAwake()
        {
            if(!UseOffScreen) return;
            _camera = Camera.main;
            UseEServLocator();
        }

        public void UseEServLocator() => _hub = EServ.Locator.Get<IHub>(this);

        public void OnEnable() => ObserveEvents();

        public void ObserveEvents() => EVent.Do.Subscribe<ISetStartingCanvasOrder>(SetStartingCanvasOrder);
        
        public void OnDisable() => StaticCoroutine.StopCoroutines(_offScreenMarkerCoroutine);

        public void OnStart()
        {
            if(!UseOffScreen) return;
            SetUpOffScreenMarker();
            SetScreenSize(_hub.MainCanvasRect.sizeDelta);
        }

        private void SetUpOffScreenMarker()
        {
            var newOffScreenMarker = Object.Instantiate(_offScreenMarker, 
                                                        _hub.MainCanvasRect.transform, 
                                                        true);
            _offScreenMarkerRect = newOffScreenMarker.GetComponent<RectTransform>();
            _offScreenMarkerRect.anchoredPosition3D = Vector3.zero;
            newOffScreenMarker.transform.parent 
                = MakeFolderUtil.MakeANewFolder(OffScreenMarkerFolderName, _hub.MainCanvasRect, _markerFolder);
            SetUpCursorCanvas();
        }

        private void SetUpCursorCanvas()
        {
            _offScreenMarkerCanvas = _offScreenMarkerRect.GetComponent<Canvas>();
            _offScreenMarkerCanvas.enabled = false;
        }

        private void SetScreenSize(Vector2 mainCanvasSize)
        {
            SetScreenSafeMargin();
            _screenLeft = Mathf.RoundToInt((mainCanvasSize.x - _screenSafeMargin.x) * -0.5f);
            _screenRight = Mathf.RoundToInt((mainCanvasSize.x - _screenSafeMargin.x)  * 0.5f);
            _screenBottom = Mathf.RoundToInt((mainCanvasSize.y - _screenSafeMargin.y) * -0.5f);
            _screenTop = Mathf.RoundToInt((mainCanvasSize.y - _screenSafeMargin.y)  * 0.5f);
        }

        private void SetScreenSafeMargin()
        {
            var sizeDelta = _offScreenMarkerRect.sizeDelta;
            var xSize = sizeDelta.x;
            var ySize = sizeDelta.y;
            _screenSafeMargin = new Vector2(Mathf.RoundToInt(xSize * 0.75f),
                                            Mathf.RoundToInt(ySize * 0.75f)) + _screenSafeMargin;
        }

        private void SetStartingCanvasOrder(ISetStartingCanvasOrder args)
        {
            if(!UseOffScreen) return;

            SetCanvasOrderUtil.Set(args.ReturnOffScreenMarkerCanvasOrder, _offScreenMarkerCanvas);
        }

        private IEnumerator SetOffScreenMarkerPosition(Transform moduleTransform)
        {
            while (true)
            {
                SetPosition(moduleTransform);
                yield return _waitFrameCustom.SetFrameTarget(_frameFrequency);
            }
        }

        private void SetPosition(Transform moduleTransform)
        {
            var modulePosition = _camera.WorldToScreenPoint(moduleTransform.position);
            
            if (ModuleHasNotMovedSinceLastCheck(modulePosition)) return;
            
            var modulePositionOnScreen = FindModulePositionOnScreen(modulePosition);
            CalculateNewMarkerPosition(_offScreenMarkerRect, modulePositionOnScreen);
            LookAtGOUIModule(modulePositionOnScreen);
        }

        private bool ModuleHasNotMovedSinceLastCheck(Vector3 modulePosition)
        {
            if (_lastPosition == modulePosition) return true;
            _lastPosition = modulePosition;
            return false;
        }

        private void LookAtGOUIModule(Vector2 modulePositionOnScreen) 
            => _offScreenMarkerRect.right = _offScreenMarkerRect.anchoredPosition.Direction(modulePositionOnScreen);

        private Vector2 FindModulePositionOnScreen(Vector3 modulePosition)
        {
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(_hub.MainCanvasRect,
                                                    screenPoint: modulePosition,
                                                    cam: null,
                                                    localPoint: out var canvasPos);
            return canvasPos;
        }

        private void CalculateNewMarkerPosition(RectTransform markerRect, Vector3 modulePositionOnScreen)
        {
            var clampedScreenPosition = modulePositionOnScreen;

            clampedScreenPosition.x = Mathf.Clamp(clampedScreenPosition.x, _screenLeft, _screenRight);
            clampedScreenPosition.y = Mathf.Clamp(clampedScreenPosition.y, _screenBottom, _screenTop);
            
            markerRect.anchoredPosition = clampedScreenPosition;
        }


        public void StopOffScreenMarker()
        {
            if(!UseOffScreen) return;
            StaticCoroutine.StopCoroutines(_offScreenMarkerCoroutine);
            _offScreenMarkerCanvas.enabled = false;
        }

        public void StartOffScreenMarker()
        {
            if(!UseOffScreen) return;
            _offScreenMarkerCanvas.enabled = true;
            StaticCoroutine.StopCoroutines(_offScreenMarkerCoroutine);
            _offScreenMarkerCoroutine = StaticCoroutine.StartCoroutine(SetOffScreenMarkerPosition(_parentTransform));
        }
    }
}