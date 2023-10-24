using Reveche.MazeRunner.Serializable;

namespace Reveche.MazeRunner;

public class GameState
{
    public bool IsSoundOn { get; set; }
    public bool IsSoundFxOn { get; set; }
    public bool IsUtf8 { get; set; }
    public MazeDifficulty MazeDifficulty { get; set; }
    public GameMode GameMode { get; set; }
    public bool IsCurrentlyPlaying { get; set; }
    public bool IsGameOngoing { get; set; }
}