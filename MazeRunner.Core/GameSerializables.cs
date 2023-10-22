using System.Text.Json.Serialization;

namespace Reveche.MazeRunner;

public class GameOptions
{
    public GameMode GameMode { get; set; } = GameMode.Classic;
    public bool IsSoundOn { get; set; } = true;
    public bool IsUtf8 { get; set; } = true;
    public MazeDifficulty MazeDifficulty { get; set; } = MazeDifficulty.Normal;
}

public class ScoreList
{
    public List<ScoreEntry> Scores { get; set; } = new();
}

public class ScoreEntry(string name, int score, MazeDifficulty mazeDifficulty, GameMode gameMode, int completedLevels)
{
    public string Name { get; } = name;
    public int Score { get; } = score;
    public MazeDifficulty MazeDifficulty { get; } = mazeDifficulty;
    public GameMode GameMode { get; } = gameMode;
    public int CompletedLevels { get; } = completedLevels;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GameOptions))]
internal partial class GameOptionsJsonContext : JsonSerializerContext;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ScoreList))]
internal partial class ScoreListJsonContext : JsonSerializerContext;