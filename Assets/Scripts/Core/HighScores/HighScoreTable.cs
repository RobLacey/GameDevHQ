using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreTable : MonoBehaviour
{
    [SerializeField] HighScoreContainer _scorePrefab;
    [SerializeField] GameObject _highScoreParent;
    [SerializeField] Vector3 _startPosition;
    [SerializeField] Vector3 _gapBetweenEntries;
    [SerializeField] EventManager _Event_UpdateHighScoreDisplay;

    //Variables
    HighScoreContainer[] _highScoreDisplayEntries;
    HighScoresToSave _highScoresList;

    private void OnEnable()
    {
        _Event_UpdateHighScoreDisplay.AddListener((x) => UpdateHighScoreList(x));
    }

    private void Start()
    {
        LoadHighScoreData();
        PopulateHighScoreTable();
    }

    private void LoadHighScoreData()
    {
        string jsonString = PlayerPrefs.GetString(HighScoreController._highScoreTableData);
        _highScoresList = JsonUtility.FromJson<HighScoresToSave>(jsonString);
        _highScoreDisplayEntries = new HighScoreContainer[_highScoresList._highScoreEntries.Count];
    }

    private void PopulateHighScoreTable()
    {
        for (int index = 0; index < _highScoresList._highScoreEntries.Count; index++)
        {
            _highScoreDisplayEntries[index] = Instantiate(_scorePrefab, _highScoreParent.transform);
            _highScoreDisplayEntries[index].GetComponent<RectTransform>().anchoredPosition = _startPosition - (_gapBetweenEntries * index);
            _highScoreDisplayEntries[index].ChangeDisplay(index + 1,
                       _highScoresList._highScoreEntries[index]._score,
                       _highScoresList._highScoreEntries[index]._name);
        }
    }


    public void UpdateHighScoreList(object _newHighScores)
    {
        HighScoresToSave highscores = (HighScoresToSave) _newHighScores;
        int index = 0;
        foreach (var item in highscores._highScoreEntries)
        {
            _highScoreDisplayEntries[index].ChangeDisplay(index + 1, item._score, item._name);
            index++;
        }
    }
}
