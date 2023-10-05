namespace Reveche.MazeRunner;

public class GameState
{
    public string Player { get; set; } = "😀";
    public int PlayerX { get; set; } = 1;
    public int PlayerY { get; set; } = 1;
    public int PlayerLife { get; set; } = 3;
    public int CurrentLevel { get; set; } = 1; // Current level number
    public int MaxLevels { get; set; } = 5; // Set the maximum number of levels
    public int EnemyX { get; set; }
    public int EnemyY { get; set; }
    public int ExitX { get; set; } // Exit X-coordinate
    public int ExitY { get; set; } // Exit Y-coordinate
    public int MazeWidth { get; set; } = 7;
    public int MazeHeight { get; set; } = 9;
    public string[,] Maze { get; set; } = null!;
}