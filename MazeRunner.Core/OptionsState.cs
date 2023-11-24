using System.Text.Json.Serialization;

namespace Reveche.MazeRunner;

public class OptionsState
{
    public bool IsSoundOn { get; set; }
    public bool IsSoundFxOn { get; set; }
    public bool IsUtf8 { get; set; }
    public MazeDifficulty MazeDifficulty { get; set; }
    public GameMode GameMode { get; set; }
    public bool IsCurrentlyPlaying { get; set; }
    [JsonIgnore] public bool IsGameOngoing { get; set; }
}