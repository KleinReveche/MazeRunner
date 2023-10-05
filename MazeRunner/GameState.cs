namespace Reveche.MazeRunner;

public class GameState
{
    public string Player { get; set; } = "😀";
    public int PlayerX { get; set; } = 1;
    public int PlayerY { get; set; } = 1;
    public int PlayerLife { get; set; } = 3;
    public int CurrentLevel { get; set; } = 1; // Current level number
    public int MaxLevels { get; set; } = 3; // Set the maximum number of levels
    public int EnemyX { get; set; } = 1;
    public int EnemyY { get; set; } = 5;
    public int ExitX { get; set; } // Exit X-coordinate
    public int ExitY { get; set; } // Exit Y-coordinate
    public string[,] Maze { get; set; } = null!;
}