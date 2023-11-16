using System.Text.Json;
using System.Text.Json.Serialization;
using Reveche.MazeRunner.Serializable;

namespace Reveche.MazeRunner.Classic;

public static class ClassicSaveManager
{
    private const string ClassicSaveJsonPath = "MazeRunner.ClassicCurrentSave.dat";

    private static readonly JsonSerializerOptions SourceGenOptions = new()
    {
        TypeInfoResolver = CurrentClassicSaveJsonContext.Default,
        IncludeFields = true,
        WriteIndented = true
    };

    private static readonly CurrentClassicSaveJsonContext Context = new(SourceGenOptions);

    public static ClassicState LoadCurrentSave()
    {
        if (!ClassicSaveFileExists()) return new ClassicState();

        var json = File.ReadAllText(ClassicSaveJsonPath);
        var gameState = JsonSerializer.Deserialize(
            JsonScrambler.Decode(json), Context.ClassicState) ?? new ClassicState();

        gameState.Maze = gameState.MazeList.To2DArray();

        return gameState;
    }

    public static void SaveCurrentSave(ClassicState classicState)
    {
        classicState.MazeList = classicState.Maze.ToListOfCharArray();

        var json = JsonSerializer.Serialize(classicState, Context.ClassicState);
        File.WriteAllText(ClassicSaveJsonPath, JsonScrambler.Encode(json));
    }

    private static T[,] To2DArray<T>(this IReadOnlyList<IReadOnlyList<T>> source)
    {
        var rows = source.Count;
        var cols = source.Max(row => row.Count);
        var result = new T[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            var row = source[i];
            for (var j = 0; j < row.Count; j++) result[i, j] = row[j];
        }

        return result;
    }

    private static List<char[]> ToListOfCharArray(this char[,] source)
    {
        var mazeList = new List<char[]>();

        for (var i = 0; i < source.GetLength(0); i++)
        {
            var line = new char[source.GetLength(1)];
            for (var j = 0; j < source.GetLength(1); j++) line[j] = source[i, j];
            mazeList.Add(line);
        }

        return mazeList;
    }
    
    public static bool ClassicSaveFileExists() => File.Exists(ClassicSaveJsonPath);
    
    public static void DeleteClassicSaveFile()
    {
        if (ClassicSaveFileExists())
            File.Delete(ClassicSaveJsonPath);
    }
}

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
[JsonSerializable(typeof(ClassicState))]
internal partial class CurrentClassicSaveJsonContext : JsonSerializerContext;