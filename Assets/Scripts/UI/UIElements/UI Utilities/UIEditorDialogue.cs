using System;
using UnityEditor;

public class UIEditorDialogue
{
    //Convert into Builder pattern if expanded in future

    public static void WarningDialogue(string title, string message, string okButton)
    {
        EditorUtility.DisplayDialog(title, message, okButton);
    }

    public static void DialogueWithCancelAction(string title, string message, string okButton, string cancel, Action cancelAction = null)
    {
        if (!EditorUtility.DisplayDialog(title, message, okButton, cancel))
        {
            cancelAction?.Invoke();
        }
    }
}