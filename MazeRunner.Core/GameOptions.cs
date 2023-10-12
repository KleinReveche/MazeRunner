namespace Reveche.MazeRunner;

[Serializable]
public class GameOptions
{
    public bool IsSoundOn { get; set; } = true;
    public bool IsUtf8 { get; set; } = true;
    public MazeDifficulty MazeDifficulty { get; set; } = MazeDifficulty.Normal;
}