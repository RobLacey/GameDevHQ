using System;
using UnityEditor;

public class UIEditorDialogue
{
    //Convert into Builder pattern if expanded in future
    
    public static void NewDialogue(string title, string message, string ok, string cancel, Action cancelAction = null)
    {
        if (!EditorUtility.DisplayDialog
        (title: $"{title}",
         message:$"{message}",
         ok: $"{ok}",
         cancel: $"{cancel}"))
        {
            cancelAction?.Invoke();
        }
    }
}