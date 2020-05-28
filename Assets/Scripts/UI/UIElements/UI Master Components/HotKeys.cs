using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class HotKeys 
{
    [InputAxis] [AllowNesting] public string _hotkeyAxis;
    public UIBranch _UIBranch;


    public void CheckHotkeys()
    {
        if (Input.GetButtonDown(_hotkeyAxis))
        {
            _UIBranch.HotKeyTrigger();
        }
    }
}
