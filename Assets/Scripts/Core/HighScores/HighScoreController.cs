using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreController : MonoBehaviour
{
    [SerializeField] Canvas _highScoreTab;
    [SerializeField] InputField _inputField;
    [SerializeField] Text _scoreText;
    [SerializeField] bool _resetHighScoreTable;
    [SerializeField] EventManager _Event_UpdateHighScoreDisplay;
    [SerializeField] EventManager _Event_AddHighScore;
    [SerializeField] EventManager _Event_NoHighScore;

    //Variables
    public const string _highScoreTableData = "HIGH SCORE TABLE DATA";
    HighScoresToSave _highScoresList;
    int _newScore = default;

    private void Awake()
    {
        ResetHighScore();
        FirstTimeSetUp();
        Load();
    }

    private void OnEnable()
    {
        _Event_AddHighScore.AddListener((x) => CheckForHighScore(x));
    }

    private void ResetHighScore()
    {
        if (_resetHighScoreTable)
        {
            PlayerPrefs.DeleteKey(_highScoreTableData);
        }
    }

    private void FirstTimeSetUp()
    {
        if (!PlayerPrefs.HasKey(_highScoreTableData))
        {
            _highScoresList = new HighScoresToSave();
            _highScoresList._highScoreEntries = new List<HighScoreEntry> 
            {
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
                new HighScoreEntry { _name = "TBC", _score = 10000 },
            };

            Save();
            Debug.Log("New High Score Data Table");
        }
    }

    private void Load()
    {
        string jsonString = PlayerPrefs.GetString(_highScoreTableData);
        _highScoresList = JsonUtility.FromJson<HighScoresToSave>(jsonString);
    }

    public void CheckForHighScore(object newscore)
    {
        int score = (int)newscore;
        if (_highScoresList._highScoreEntries.Where(n => n._score < score).Count() > 0)
        {
            _newScore = score;
            _highScoreTab.enabled = true;
            _scoreText.text = _newScore.ToString();
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }
        else
        {
            _Event_NoHighScore.Invoke();
        }
    }


    public void AddToHighScoreList()
    {
        if (_inputField.text == string.Empty) return;

        var higherThanNewScore = _highScoresList._highScoreEntries.TakeWhile(n => n._score > _newScore).ToList();
        var lowerThanNewScore = _highScoresList._highScoreEntries.Where(n => n._score <= _newScore).ToList();

        higherThanNewScore.Add(new HighScoreEntry { _name = _inputField.text, _score = _newScore }); // Adds new score

        var _newTable = higherThanNewScore.Concat(lowerThanNewScore).ToList(); // Combines lists

        _newTable.RemoveAt(_newTable.Count - 1); //Removes 11th entry

        _highScoresList._highScoreEntries = new List<HighScoreEntry>(_newTable);
        _Event_UpdateHighScoreDisplay.Invoke(_highScoresList);
        Save();

    }   

    private void Save()
    {
        string json = JsonUtility.ToJson(_highScoresList);
        PlayerPrefs.SetString(_highScoreTableData, json);
        PlayerPrefs.Save();
    }
}
