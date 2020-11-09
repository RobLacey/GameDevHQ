using System;
using UnityEngine.EventSystems;

public interface IUiEvents
{
    int MasterNodeID { get; }
    UINode ReturnMasterNode { get; }
    event Action<bool> WhenPointerOver;
    event Action<bool> IsSelected;
    event Action IsPressed;
    event Action<bool> IsDisabled;
    event Action<MoveDirection> OnMove;
    event Action PlayCancelAudio;
    event Action PlayHighlightedAudio;
    event Action PlaySelectedAudio;
    void DoWhenPointerOver(bool pointer);
    void DoIsSelected(bool selected);
    void DoIsPressed();
    void DoIsDisabled(bool disabled);
    void DoOnMove(MoveDirection direction);
    void DoPlayCancelAudio();
    void DoPlayHighlightedAudio();
    void DoPlaySelectedAudio();
}

public class UiEvents : IUiEvents
{
    public UiEvents(int instanceId, UINode node)
    {
        MasterNodeID = instanceId;
        ReturnMasterNode = node;
    }

    public int MasterNodeID { get; }
    public UINode ReturnMasterNode { get; }
    
    //Events
    public event Action<bool> WhenPointerOver;
    public event Action<bool> IsSelected;
    public event Action IsPressed;
    public event Action<bool> IsDisabled;
    public event Action<MoveDirection> OnMove;
    
    public event Action PlayCancelAudio;
    public event Action PlayHighlightedAudio;
    public event Action PlaySelectedAudio;
    
    public void DoWhenPointerOver(bool pointer) 
    { WhenPointerOver?.Invoke(pointer);}
    public void DoIsSelected(bool selected)
    { IsSelected?.Invoke(selected);}
    public void DoIsPressed()
    { IsPressed?.Invoke();}
    public void DoIsDisabled(bool disabled)
    { IsDisabled?.Invoke(disabled);}
    public void DoOnMove(MoveDirection direction)
    { OnMove?.Invoke(direction);}
    public void DoPlayCancelAudio()
    {PlayCancelAudio?.Invoke();}
    public void DoPlayHighlightedAudio()
    { PlayHighlightedAudio?.Invoke();}
    public void DoPlaySelectedAudio()
    { PlaySelectedAudio?.Invoke();}
}
