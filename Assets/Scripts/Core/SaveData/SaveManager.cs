using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] string _saveFileExtension;
    [SerializeField] string _newSavePath;
    [SerializeField] EventManager _Event_Save_File;
    [SerializeField] EventManager _Event_Load_File;
    [SerializeField] EventManager _Event_Check_For_File;

    private void Awake()
    {
        _newSavePath = Application.persistentDataPath + "/";
    }

    private void OnEnable()
    {
        _Event_Save_File.AddListener((x, y)=> SaveToDisk(x,y), this);
        _Event_Load_File.AddReturnParameter((x)=>LoadFromDisk(x), this);
        _Event_Check_For_File.AddReturnParameter((x) => CheckForSave(x), this);
    }

    private void SaveToDisk(object newSavedata, object saveFileName)
    {
        string newFile = _newSavePath + saveFileName.ToString() + _saveFileExtension;

        using (FileStream file = File.Create(newFile))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, newSavedata);
        }
    }

    private object LoadFromDisk(object fileName)
    {
        string newFile = _newSavePath + fileName.ToString() + _saveFileExtension;

        if (File.Exists(newFile))
        {
            using (FileStream file = File.Open(newFile, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                var temp = bf.Deserialize(file);
                return temp;
            }

        }            
        Debug.Log("File Error : Doesn't exist");
        return null;
    }

    private bool CheckForSave(object fileName)
    {
        if (File.Exists(_newSavePath + fileName.ToString() + _saveFileExtension))
        {
            return true;
        }
        return false;
    }


    private void JSONLoad() //In case i need it
    {
        //string jsonString = PlayerPrefs.GetString(_highScoreTableData);
        //_highScoresList = JsonUtility.FromJson<HighScoresToSave>(jsonString);

    }

    private void JSONSave()
    {
        //string json = JsonUtility.ToJson(_highScoresList);
        //PlayerPrefs.SetString(_highScoreTableData, json);

    }
}

public enum SaveFileNames
{
    HighScore, PlayerStats
}