namespace Reveche.MazeRunner;

public class GameState
{
    public readonly List<(int candleY, int CandleX)> CandleLocations = new();

    public readonly List<(int treasureY, int treasureX, TreasureType treasureType, int count)>
        TreasureLocations = new();
    
    public readonly List<(int enemyY, int enemyX)> EnemyLocations = new();

    public string Player { get; set; } = "😀";
    public int PlayerX { get; set; } = 1;
    public int PlayerY { get; set; } = 1;
    public int PlayerLife { get; set; } = 3;
    public int BombCount { get; set; } = 2;
    public int CandleCount { get; set; } = 4;
    public bool BombIsUsed { get; set; }
    public int BombTimer { get; set; } = 2;
    public int PlayerInvincibilityEffectDuration { get; set; }
    public bool IsPlayerInvulnerable { get; set; }
    public bool PlayerHasIncreasedVisibility { get; set; }
    public bool AtAGlance { get; set; }
    public int CurrentLevel { get; set; } = 1; // Current level number
    public int MaxLevels { get; set; } = 5; // Set the maximum number of levels
    public int ExitX { get; set; } // Exit X-coordinate
    public int ExitY { get; set; } // Exit Y-coordinate
    public int MazeWidth { get; set; } = 7;
    public int MazeHeight { get; set; } = 9;
    public string[,] Maze { get; set; } = null!;
    public bool IsSoundOn { get; set; } = true;
    public bool IsUtf8 { get; set; } = true;
    public MazeDifficulty MazeDifficulty { get; set; } = MazeDifficulty.Normal;
    public bool IsCurrentlyPlaying { get; set; }
}