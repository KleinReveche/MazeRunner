using System.Text.Json;

namespace Reveche.MazeRunner;

public class ScoreManager(ScoreList scoreList)
{
    private const string OldScoreJsonPath = "MazeRunner.Scores.json";
    private const string NewScoreJsonPath = "MazeRunner.Scores.dat";

    private static readonly JsonSerializerOptions SourceGenOptions = new()
    {
        TypeInfoResolver = ScoreListJsonContext.Default,
        WriteIndented = true
    };

    private static readonly ScoreListJsonContext Context = new(SourceGenOptions);

    public static ScoreList LoadScores()
    {
        var defaultScoreList = new ScoreList();
        
        // This ensures backwards compatibility with the old JSON format.
        if (File.Exists(OldScoreJsonPath))
        {
            var oldJson = File.ReadAllText(OldScoreJsonPath);
            var scoreList = JsonSerializer.Deserialize(
                oldJson, Context.ScoreList) ?? defaultScoreList;
            SaveScores(scoreList);
            File.Delete(OldScoreJsonPath);
            return scoreList;
        }
        
        if (!File.Exists(NewScoreJsonPath)) return defaultScoreList;

        var json = File.ReadAllText(NewScoreJsonPath);
        return JsonSerializer.Deserialize(
            JsonScrambler.Decode(json), Context.ScoreList) ?? defaultScoreList;
    }

    public static void SaveScores(ScoreList scoreList)
    {
        var json = JsonSerializer.Serialize(scoreList, Context.ScoreList);
        File.WriteAllText(NewScoreJsonPath, JsonScrambler.Encode(json));
    }

    public void AddScore(string name, int score, MazeDifficulty mazeDifficulty, GameMode gameMode, int completedLevels)
    {
        scoreList.Scores.Add(new ScoreEntry(name, score, mazeDifficulty, gameMode, completedLevels));
        SaveScores(scoreList);
    }
}