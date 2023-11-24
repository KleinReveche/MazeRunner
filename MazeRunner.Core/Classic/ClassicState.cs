using System.Text.Json.Serialization;
using Reveche.MazeRunner.Serializable;

namespace Reveche.MazeRunner.Classic;

public class ClassicState
{
    public List<(int bombY, int bombX, int timer)> BombLocations = [];
    public List<(int candleY, int CandleX)> CandleLocations = [];
    public List<(int enemyY, int enemyX)> EnemyLocations = [];
    public List<(int enemyY, int enemyX, HighClassEnemy enemy)> HigherClassEnemy = [];

    [JsonIgnore] public char[,] Maze = null!;

    public List<(int treasureY, int treasureX, TreasureType treasureType, int count)>
        TreasureLocations = [];

    public ClassicState()
    {
        var options = OptionsManager.LoadOptions();

        MazeDifficulty = options.MazeDifficulty;
    }

    public int PlayerX { get; set; } = 1;
    public int PlayerY { get; set; } = 1;
    public int PlayerLife { get; set; } = 3;
    public int PlayerHealth { get; set; } = 100;
    public int PlayerMaxHealth { get; set; } = 100;
    public int BombCount { get; set; } = 2;
    public int CandleCount { get; set; } = 4;
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
    public List<char[]> MazeList { get; set; } = null!; // This is for serialization
    public int PlayerVisibilityRadius { get; set; }
    public int CandleVisibilityRadius { get; set; }
    public int IncreasedVisibilityEffectRadius { get; set; }
    public int Score { get; set; }
    public string PlayerName { get; set; } = "Anonymous";
    public MazeDifficulty MazeDifficulty { get; set; }
    public int PlayerBurnDuration { get; set; }
    public int DecreasedVisibilityEffectDuration { get; set; }
}