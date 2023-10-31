using Reveche.MazeRunner.Sound;

namespace Reveche.MazeRunner.Classic;

public partial class ClassicEngine(OptionsState optionsState, ClassicState classicState)
{
    private const int BlastRadius = 1;
    private readonly MazeGen _mazeGen = new(classicState);
    private readonly GameSoundFx _gameSoundFx = new(optionsState);
    private int PlayerX => classicState.PlayerX;
    private int PlayerY => classicState.PlayerY;
    private int LastPlayerX { get; set; }
    private int LastPlayerY { get; set; }
    private int BombX { get; set; }
    private int BombY { get; set; }
    private char[,] Maze => classicState.Maze;

    public void InitializeNewLevel()
    {
        if (classicState.PlayerHasIncreasedVisibility)
            classicState.PlayerHasIncreasedVisibility = false;
        classicState.CandleLocations.Clear();
        classicState.BombIsUsed = false;
        classicState.MazeHeight = _mazeGen.GenerateRandomMazeSize();
        classicState.MazeWidth = _mazeGen.GenerateRandomMazeSize();
        _mazeGen.InitializeMaze();
        _mazeGen.GenerateMaze(1, 1); // Start generating maze from (1, 1)
        _mazeGen.GenerateExit();
        _mazeGen.GenerateEnemy();
        _mazeGen.GenerateTreasure();
    }

    public void AdjustToDifficulty()
    {
        classicState.MaxLevels = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 5,
            MazeDifficulty.Insanity => 6,
            _ => 6
        };
        classicState.PlayerVisibilityRadius = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        classicState.CandleVisibilityRadius = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 2,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        classicState.IncreasedVisibilityEffectRadius = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 3,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        
        if (optionsState.IsGameOngoing) return;
        
        classicState.BombCount = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        classicState.CandleCount = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 6,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 4,
            _ => 3
        };
    }

    private bool IsCellEmpty(int x, int y)
    {
        var maze = classicState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0))
            return maze[y, x] == MazeIcons.Empty;

        return false;
    }

    public void CalculateLevelScore(DateTime levelStartTime)
    {
        var mazeArea = classicState.MazeWidth * classicState.MazeHeight;
        var maxTime = mazeArea switch
        {
            <= 50 => 45,
            <= 165 => 90,
            <= 400 => 150,
            _ => 210
        };

        var timeScore = maxTime - (int)(DateTime.Now - levelStartTime).TotalSeconds;

        classicState.Score += timeScore < 0 ? 0 : timeScore;
    }
}