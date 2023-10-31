using System.Text.Json;

namespace Reveche.MazeRunner.Serializable;

public static class OptionsManager
{
    private const string OldOptionsJson = "MazeRunner.Options.json";
    private const string NewOptionsJson = "MazeRunner.Options.dat";

    private static readonly JsonSerializerOptions SourceGenOptions = new()
    {
        TypeInfoResolver = GameStateJsonContext.Default,
        WriteIndented = true
    };

    private static readonly GameStateJsonContext Context = new(SourceGenOptions);

    public static OptionsState LoadOptions()
    {
        var defaultOptions = new OptionsState
        {
            GameMode = GameMode.Classic,
            IsSoundOn = true,
            IsSoundFxOn = true,
            IsUtf8 = true,
            MazeDifficulty = MazeDifficulty.Normal
        };

        if (File.Exists(OldOptionsJson))
        {
            var oldJson = File.ReadAllText(OldOptionsJson);
            var options = JsonSerializer.Deserialize(
                oldJson, Context.OptionsState) ?? defaultOptions;
            SaveOptions(options);
            File.Delete(OldOptionsJson);
            return options;
        }

        if (!File.Exists(NewOptionsJson)) return defaultOptions;

        var json = File.ReadAllText(NewOptionsJson);
        return JsonSerializer.Deserialize(
            JsonScrambler.Decode(json), Context.OptionsState) ?? defaultOptions;
    }

    public static void SaveOptions(OptionsState optionsState)
    {
        var json = JsonSerializer.Serialize(optionsState, Context.OptionsState);
        File.WriteAllText(NewOptionsJson, JsonScrambler.Encode(json));
    }
}