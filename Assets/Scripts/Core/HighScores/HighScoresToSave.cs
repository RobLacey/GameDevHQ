using System;
using System.Collections.Generic;

[Serializable]
public class HighScoresToSave
{
    public List<HighScoreEntry> _highScoreEntries;
}

[Serializable]
public class HighScoreEntry
{
    public int _score;
    public string _name;
}
