using System.Text.Json;

namespace Reveche.MazeRunner;

public class OptionsManager
{
    private const string OptionsFilePath = "MazeRunner.Options.json";

    public static GameOptions LoadOptions()
    {
        if (File.Exists(OptionsFilePath))
        {
            var json = File.ReadAllText(OptionsFilePath);
            return JsonSerializer.Deserialize<GameOptions>(json)!;
        }

        // Create a default instance of MyOptions
        var defaultOptions = new GameOptions
        {
            IsSoundOn = true,
            IsUtf8 = true,
            MazeDifficulty = MazeDifficulty.Normal
        };
        SaveOptions(defaultOptions); // Save default options to a file
        return defaultOptions;
    }

    public static void SaveOptions(GameOptions options)
    {
        var json = JsonSerializer.Serialize(options);
        File.WriteAllText(OptionsFilePath, json);
    }
}