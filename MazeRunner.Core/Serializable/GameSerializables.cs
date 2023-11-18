using System.Text;
using System.Text.Json.Serialization;

namespace Reveche.MazeRunner.Serializable;

public class ScoreList
{
    public List<ScoreEntry> Scores { get; set; } = [];
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
[JsonSerializable(typeof(OptionsState))]
internal partial class GameStateJsonContext : JsonSerializerContext;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ScoreList))]
internal partial class ScoreListJsonContext : JsonSerializerContext;

/// <summary>
///     This is to just scramble the data to make it harder to read and edit.
///     This is not meant to be secure.
/// </summary>
public static class JsonScrambler
{
    private const string ScramblerVersion1 = "v1";

    public static string Encode(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(bytes);
        var base64EncodedBytes = Encoding.UTF8.GetBytes(base64);
        var hexString = Convert.ToHexString(base64EncodedBytes);

        // Add version number to the beginning of the string, to allow for changes later.
        return ScramblerVersion1 + hexString;
    }

    public static string Decode(string hexString)
    {
        var base64EncodedBytes = Convert.FromHexString(hexString[2..]);
        var base64 = Encoding.UTF8.GetString(base64EncodedBytes);
        var bytes = Convert.FromBase64String(base64);
        var json = Encoding.UTF8.GetString(bytes);
        return json;
    }
}