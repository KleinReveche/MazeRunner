﻿using System.Text.Json;

namespace Reveche.MazeRunner;

public class ScoreManager
{
    private const string FilePath = "MazeRunner.Scores.json"; // Path to your JSON file
    private readonly ScoreList _scoreList;

    public ScoreManager(ScoreList scoreList)
    {
        _scoreList = scoreList;
    }

    public static ScoreList LoadScores()
    {
        var defaultScoreList = new ScoreList();
        if (!File.Exists(FilePath)) return defaultScoreList;
        var sourceGenOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = ScoreListJsonContext.Default
        };
        
        var json = File.ReadAllText(FilePath);
        return (JsonSerializer.Deserialize(
                json, typeof(ScoreList), sourceGenOptions)
            as ScoreList) ?? defaultScoreList;
    }

    public static void SaveScores(ScoreList scoreList)
    {
        var sourceGenOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = ScoreListJsonContext.Default
        };
        
        var json = JsonSerializer.Serialize(scoreList, typeof(ScoreList), sourceGenOptions);
        File.WriteAllText(FilePath, json);
    }

    public void AddScore(string name, int score, MazeDifficulty mazeDifficulty, bool completed)
    {
        _scoreList.Scores.Add(new ScoreEntry(name, score, mazeDifficulty, completed));
        SaveScores(_scoreList);
    }
}