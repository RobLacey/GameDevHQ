using UnityEngine;

namespace UIElements
{
    //TODO Improve and maybe add Functionality to make normal folders, preserve position etc etc. Use Builder Pattern if do that
    public class MakeFolderUtil
    {
        public static Transform MakeANewFolder(string newFolderName, Transform parent, Transform existingFolder)
        {
            if(existingFolder != null)
            {
                existingFolder.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                return existingFolder;
            }
            
            var newFolder = Object.Instantiate(new GameObject(), parent);
            newFolder.AddComponent<RectTransform>();
            newFolder.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            newFolder.name = newFolderName;
            return newFolder.transform;
        }
    }
}