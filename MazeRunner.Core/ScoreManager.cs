using System.Text.Json;

namespace Reveche.MazeRunner;

public class ScoreManager(ScoreList scoreList)
{
    private const string FilePath = "MazeRunner.Scores.json";

    private static readonly JsonSerializerOptions SourceGenOptions = new()
    {
        TypeInfoResolver = ScoreListJsonContext.Default,
        WriteIndented = true
    };

    private static readonly ScoreListJsonContext Context = new(SourceGenOptions);

    public static ScoreList LoadScores()
    {
        var defaultScoreList = new ScoreList();
        if (!File.Exists(FilePath)) return defaultScoreList;

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize(
            json, Context.ScoreList) ?? defaultScoreList;
    }

    public static void SaveScores(ScoreList scoreList)
    {
        var json = JsonSerializer.Serialize(scoreList, Context.ScoreList);
        File.WriteAllText(FilePath, json);
    }

    public void AddScore(string name, int score, MazeDifficulty mazeDifficulty, GameMode gameMode, int completedLevels)
    {
        scoreList.Scores.Add(new ScoreEntry(name, score, mazeDifficulty, gameMode, completedLevels));
        SaveScores(scoreList);
    }
}