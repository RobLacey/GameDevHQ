using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class HighScoreContainer: MonoBehaviour
{
    [SerializeField] Text _rankText;
    [SerializeField] Text _scoreText;
    [SerializeField] Text _nameText;

    public void ChangeDisplay(int newrank, int newScore, string newName)
    {
        _rankText.text = newrank.ToString();
        _scoreText.text = newScore.ToString();
        _nameText.text = newName;
    }
}
