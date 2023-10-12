using System.Text.Json.Serialization;

namespace Reveche.MazeRunner;

[JsonSerializable(typeof(GameOptions))]
public class GameOptions
{
    public bool IsSoundOn { get; set; } = true;
    public bool IsUtf8 { get; set; } = true;
    public MazeDifficulty MazeDifficulty { get; set; } = MazeDifficulty.Normal;
}

[JsonSerializable(typeof(ScoreList))]
public class ScoreList
{
    public List<ScoreEntry> Scores { get; set; } = new();
    
}

[JsonSerializable(typeof(ScoreEntry))]
public class ScoreEntry
{
    public string Name { get; }
    public int Score { get; }
    public MazeDifficulty MazeDifficulty { get; }
    public bool Completed { get; }

    public ScoreEntry(string name, int score, MazeDifficulty mazeDifficulty, bool completed)
    {
        Name = name;
        Score = score;
        MazeDifficulty = mazeDifficulty;
        Completed = completed;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GameOptions))]
internal partial class GameOptionsJsonContext : JsonSerializerContext {}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ScoreList))]
internal partial class ScoreListJsonContext : JsonSerializerContext {}