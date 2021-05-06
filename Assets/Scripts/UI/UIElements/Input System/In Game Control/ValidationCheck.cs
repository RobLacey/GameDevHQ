using UnityEngine;

public abstract class DialogueBox
{
    protected abstract void CancelAction();
    public abstract void ValidateDialogue();
}

public class ValidationCheck : DialogueBox
{
 //   private GOUIController _goController;
    private UIInput _uiInput;

    // public ValidationCheck(GOUIController parent)
    // {
    //     _goController = parent;
    // }
    
    public ValidationCheck(UIInput parent)
    {
        _uiInput = parent;
    }
    
    private bool _currentState;
    private ControlMethod _currentControlMethod;
    private InputScheme _scheme;
    private readonly string _title = $"Incompatible Input Types";
    private readonly string _ok = $"I'm Fine With This";
    private readonly string _cancel = $"cancel";

    protected override void CancelAction()
    {
        //_goController.ControlType = _currentState;
        _scheme.ControlType = _currentControlMethod;
    }

    private bool NonMouseOnlyControls => true;  // _goController.ControlType != VirtualControl.SwitcherMouseOnly;
    private bool MouseSwitchOnlyEditor => true; // _goController.ControlType == VirtualControl.SwitcherMouseOnly;

    public override void ValidateDialogue()
    {
        PopulateVariables();

      //  if(_goController.ControlType == VirtualControl.None || _scheme is null) return;
        
        var message =
            $"In game control set to '???' while 'Input Scheme' is set to '{_scheme.ControlType}'.";
        
        if(NonMouseOnlyControls && _scheme.ControlType == ControlMethod.MouseOnly )
        {
            UIEditorDialogue.DialogueWithCancelAction(_title, message, _ok, _cancel, CancelAction);
            return;
        }
        
        if(MouseSwitchOnlyEditor && _scheme.ControlType != ControlMethod.MouseOnly)
        {
            UIEditorDialogue.DialogueWithCancelAction(_title, message, _ok, _cancel, CancelAction);
        }

        //_currentState = _goController.ControlType;
        _currentControlMethod = _scheme.ControlType;
    }

    private void PopulateVariables()
    {
       // if (_uiInput is null)
           // _uiInput = _goController.GetComponent<UIInput>();

       // if (_goController is null)
      //  {
           // _goController = _uiInput.GetComponent<GOUIController>();
            //_currentState = _goController.ControlType;
      //  }

        // if (_scheme is null)
        //     _scheme = _uiInput.ReturnScheme;
    }
}