using System.Collections;
using EZ.Service;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UIElements
{
    public class OffScreenMarker : IServiceUser, IMonoAwake, IMonoStart,  IMonoDisable
    {
        public OffScreenMarker(OffscreenMarkerData data) => _offscreenMarkerData = data;

        //Variables
        private int _screenLeft, _screenRight, _screenTop, _screenBottom;
        private IDataHub _myDataHub;
        private Canvas _offScreenMarkerCanvas;
        private Vector3 _lastPosition = Vector3.zero;
        private Camera _camera;
        private Coroutine _offScreenMarkerCoroutine;
        private RectTransform _offScreenMarkerRect;
        private readonly WaitFrameCustom _waitFrameCustom = new WaitFrameCustom();
        private Transform _parentTransform;
        private ICanvasOrderData _canvasOrderData;
        private readonly OffscreenMarkerData _offscreenMarkerData;
        private string _targetBranchesName;
        private bool _canRun = true;

        //Editor
        private const string OffScreenMarkerFolderName = "Off Screen Marker";
        
        //TODO Add Max Distance from Camera cull process and setting

        //Properties & Getters / Setters
        private GameObject ScreenMarker => _offscreenMarkerData.ScreenMarker;
        private StartOffscreen WhenToStartOffScreenMarker => _offscreenMarkerData.WhenToStartOffScreenMarker;
        private Vector2 ScreenSafeMargin { get; set; }
        private int FrameFrequency => _offscreenMarkerData.FrameFrequency;
        private Transform MarkerFolder => _offscreenMarkerData.MarkerFolder;
        

        //Main
        public void OnAwake(GOUIModule parentGOUI)
        {
            _parentTransform = parentGOUI.transform;
            _targetBranchesName = parentGOUI.TargetBranch.ThisBranchesGameObject.name;
            OnAwake();
        }

        public void OnAwake()
        {
            _camera = Camera.main;
            UseEZServiceLocator();
        }

        public void UseEZServiceLocator()
        {
            _myDataHub = EZService.Locator.Get<IDataHub>(this);
            _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
        }

        public void OnDisable() => StopOffScreenMarker();

        public void OnStart()
        {
            SetUpOffScreenMarker();
            SetUpCursorCanvas();
            SetScreenSize(_myDataHub.MainCanvasRect.sizeDelta);
            SetStartingCanvasOrder();
        }

        private void SetUpOffScreenMarker()
        {
            var newOffScreenMarker = Object.Instantiate(ScreenMarker, 
                                                        _myDataHub.MainCanvasRect.transform, 
                                                        true);
            _offScreenMarkerRect = newOffScreenMarker.GetComponent<RectTransform>();
            _offScreenMarkerRect.anchoredPosition3D = Vector3.zero;
            _offScreenMarkerRect.name = $"OffScreen - {_targetBranchesName}";
            
            _offScreenMarkerRect.transform.parent = MakeFolderUtil.MakeANewFolder(OffScreenMarkerFolderName, 
                                                                                        _myDataHub.MainCanvasRect, 
                                                                                        MarkerFolder);
        }

        private void SetUpCursorCanvas()
        {
            _offScreenMarkerCanvas = _offScreenMarkerRect.GetComponent<Canvas>();
            _offScreenMarkerCanvas.enabled = false;
            
            if(_canvasOrderData.IsNotNull())
                SetCanvasOrderUtil.Set(_canvasOrderData.ReturnOffScreenMarkerCanvasOrder,
                                       _offScreenMarkerCanvas);
        }

        private void SetScreenSize(Vector2 mainCanvasSize)
        {
            SetScreenSafeMargin();
            _screenLeft = Mathf.RoundToInt((mainCanvasSize.x - ScreenSafeMargin.x) * -0.5f);
            _screenRight = Mathf.RoundToInt((mainCanvasSize.x - ScreenSafeMargin.x)  * 0.5f);
            _screenBottom = Mathf.RoundToInt((mainCanvasSize.y - ScreenSafeMargin.y) * -0.5f);
            _screenTop = Mathf.RoundToInt((mainCanvasSize.y - ScreenSafeMargin.y)  * 0.5f);
        }

        private void SetScreenSafeMargin()
        {
            ScreenSafeMargin = _offscreenMarkerData.ScreenSafeMargin;
            var sizeDelta = _offScreenMarkerRect.rect.size;
            var xSize = sizeDelta.x;
            var ySize = sizeDelta.y;
            ScreenSafeMargin = new Vector2(Mathf.RoundToInt(xSize * 0.5f),
                                            Mathf.RoundToInt(ySize * 0.5f)) + _offscreenMarkerData.ScreenSafeMargin;
        }

        private void SetStartingCanvasOrder()
        {
            if(_offScreenMarkerCanvas)
                SetCanvasOrderUtil.Set(_canvasOrderData.ReturnOffScreenMarkerCanvasOrder, _offScreenMarkerCanvas);
        }

        private IEnumerator SetOffScreenMarkerPosition(Transform moduleTransform)
        {
            while (_canRun)
            {
                SetPosition(moduleTransform);
                yield return _waitFrameCustom.SetFrameTarget(FrameFrequency);
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
                .ScreenPointToLocalPointInRectangle(_myDataHub.MainCanvasRect,
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
            StaticCoroutine.StopCoroutines(_offScreenMarkerCoroutine);
            if(_offScreenMarkerCanvas == null) return;
            _canRun = false;
            _offScreenMarkerCanvas.enabled = false;
        }

        public void StartOffScreenMarker(IGOUIModule myGoui)
        {
            if(WhenToStartOffScreenMarker == StartOffscreen.OnlyWhenSelected && !myGoui.NodeIsSelected) return;
            _canRun = true;
            _offScreenMarkerCanvas.enabled = true;
            StaticCoroutine.StopCoroutines(_offScreenMarkerCoroutine);
            _offScreenMarkerCoroutine = StaticCoroutine.StartCoroutine(SetOffScreenMarkerPosition(_parentTransform));
        }
    }
}