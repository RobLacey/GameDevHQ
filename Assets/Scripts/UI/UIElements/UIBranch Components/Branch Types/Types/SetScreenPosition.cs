using System.Collections;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface ISetScreenPosition : IMonoEnable, IMonoDisable
{
    Transform InGameObjectPosition { get; set; }
    void StartSetting();
    void Stop();
}

public class SetScreenPosition : IServiceUser, ISetScreenPosition
{
    public SetScreenPosition(ISetPositionParms branch)
    {
        _mainCamera = Camera.main;
        _myBranch = branch.MyBranch;
        _myRectTransform = _myBranch.MyCanvas.GetComponent<RectTransform>();
    }

    private readonly IBranch _myBranch;
    private readonly Camera _mainCamera;
    private readonly RectTransform _myRectTransform;
    private Coroutine _coroutine;
    public Transform InGameObjectPosition { get; set; }
    private Vector3 _inGameObjectLastFrameScreenPosition;
    private IDataHub _myDataHub;

    public void OnEnable()
    {
        UseEZServiceLocator();
        GOUIEvents.Do.Subscribe<IOffscreen>(SetPositionWhenOffScreen);
    }
    
    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void OnDisable()
    {
        GOUIEvents.Do.Unsubscribe<IOffscreen>(SetPositionWhenOffScreen);
        Stop();
    }

    public void StartSetting()
    {
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(SetMyScreenPosition(InGameObjectPosition));
    }

    public void Stop() => StaticCoroutine.StopCoroutines(_coroutine);
    
    private void SetPositionWhenOffScreen(IOffscreen args)
    {
        if(args.TargetBranch.NotEqualTo(_myBranch)) return;
        
        if(args.IsOffscreen)
        {
            StaticCoroutine.StopCoroutines(_coroutine);
        }        
        else
        {
            StartSetting();
        }
    }

    private IEnumerator SetMyScreenPosition(Transform objTransform)
    {
        while (true)
        {
            SetPosition(objTransform);
            yield return null;
        }
    }

    private void SetPosition(Transform objTransform)
    {
        var position = objTransform.position;
        var currentScreenPos = _mainCamera.WorldToScreenPoint(position);
        var mainCanvasRectTransform = _myDataHub.MainCanvasRect;
        
        if(currentScreenPos == _inGameObjectLastFrameScreenPosition) return;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvasRectTransform, currentScreenPos, 
                                                                null, out var canvasPos);
        
        _myRectTransform.localPosition = canvasPos;
        _inGameObjectLastFrameScreenPosition = currentScreenPos;
    }

}