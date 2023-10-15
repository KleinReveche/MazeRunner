using System.Text.Json;

namespace Reveche.MazeRunner;

public static class OptionsManager
{
    private const string OptionsFilePath = "MazeRunner.Options.json";

    public static GameOptions LoadOptions()
    {
        var defaultOptions = new GameOptions
        {
            GameMode = GameMode.Classic,
            IsSoundOn = true,
            IsUtf8 = true,
            MazeDifficulty = MazeDifficulty.Normal
        };

        if (File.Exists(OptionsFilePath))
        {
            var sourceGenOptions = new JsonSerializerOptions
            {
                TypeInfoResolver = GameOptionsJsonContext.Default
            };

            var json = File.ReadAllText(OptionsFilePath);
            return JsonSerializer.Deserialize(
                    json, typeof(GameOptions), sourceGenOptions)
                as GameOptions ?? defaultOptions;
        }

        SaveOptions(defaultOptions);
        return defaultOptions;
    }

    private static void SaveOptions(GameOptions options)
    {
        var sourceGenOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = GameOptionsJsonContext.Default,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(options, typeof(GameOptions), sourceGenOptions);
        File.WriteAllText(OptionsFilePath, json);
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