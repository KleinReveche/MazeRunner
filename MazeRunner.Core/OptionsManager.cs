using System.Text.Json;

namespace Reveche.MazeRunner;

public static class OptionsManager
{
    private const string OldOptionsJson = "MazeRunner.Options.json";
    private const string NewOptionsJson = "MazeRunner.Options.dat";

    private static readonly JsonSerializerOptions SourceGenOptions = new()
    {
        TypeInfoResolver = GameOptionsJsonContext.Default,
        WriteIndented = true
    };

    private static readonly GameOptionsJsonContext Context = new(SourceGenOptions);

    public static GameOptions LoadOptions()
    {
        var defaultOptions = new GameOptions
        {
            GameMode = GameMode.Classic,
            IsSoundOn = true,
            IsUtf8 = true,
            MazeDifficulty = MazeDifficulty.Normal
        };

        if (File.Exists(OldOptionsJson))
        {
            var oldJson = File.ReadAllText(OldOptionsJson);
            var options = JsonSerializer.Deserialize(
                oldJson, Context.GameOptions) ?? defaultOptions;
            SaveOptions(options);
            File.Delete(OldOptionsJson);
            return options;
        }

        if (!File.Exists(NewOptionsJson)) return defaultOptions;

        var json = File.ReadAllText(NewOptionsJson);
        return JsonSerializer.Deserialize(
            JsonScrambler.Decode(json), Context.GameOptions) ?? defaultOptions;
    }

    private static void SaveOptions(GameOptions options)
    {
        var json = JsonSerializer.Serialize(options, Context.GameOptions);
        File.WriteAllText(NewOptionsJson, JsonScrambler.Encode(json));
    }

    public static void SaveCurrentOptions(GameState gameState, GameOptions gameOptions)
    {
        gameOptions.GameMode = gameState.GameMode;
        gameOptions.IsSoundOn = gameState.IsSoundOn;
        gameOptions.IsUtf8 = gameState.IsUtf8;
        gameOptions.MazeDifficulty = gameState.MazeDifficulty;
        SaveOptions(gameOptions);
    }
}