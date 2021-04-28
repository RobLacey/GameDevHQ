using UnityEngine;

namespace UIElements
{
    //TODO Improve and maybe add Functionality to make normal folders, preserve position etc etc. Use Builder Pattern if do that
    public class MakeFolderUtil
    {
        public static Transform MakeANewFolder(string newFolderName, RectTransform parent, Transform presetFolder)
        {
            if (UsePresetFolder(presetFolder)) return presetFolder;
            
            if (ReturnExistingFolder(newFolderName, out var folder)) return folder;

            return MakeNewFolder(newFolderName, parent);
        }

        private static bool UsePresetFolder(Transform presetFolder)
        {
            if (presetFolder == null) return false;
            
            presetFolder.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            return true;

        }

        private static bool ReturnExistingFolder(string newFolderName, out Transform transform)
        {
            var folderAlreadyExists = GameObject.Find(newFolderName);

            if (folderAlreadyExists)
            {
                transform = folderAlreadyExists.transform;
                return true;
            }

            transform = default;
            return false;
        }

        private static Transform MakeNewFolder(string newFolderName, RectTransform parent)
        {
            var newFolder = new GameObject();
            newFolder.transform.parent = parent.transform;
            newFolder.AddComponent<RectTransform>();
            newFolder.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            newFolder.name = newFolderName;
            return newFolder.transform;
        }
    }
}