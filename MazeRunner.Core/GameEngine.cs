namespace Reveche.MazeRunner;

public partial class GameEngine(GameState gameState)
{
    private const int BlastRadius = 1;
    private readonly MazeGen _mazeGen = new(gameState);

    private int PlayerX => gameState.PlayerX;
    private int PlayerY => gameState.PlayerY;
    private int LastPlayerX { get; set; }
    private int LastPlayerY { get; set; }
    private int BombX { get; set; }
    private int BombY { get; set; }
    private char[,] Maze => gameState.Maze;

    public void InitializeNewLevel()
    {
        if (gameState.PlayerHasIncreasedVisibility)
            gameState.PlayerHasIncreasedVisibility = false;
        gameState.CandleLocations.Clear();
        gameState.BombIsUsed = false;
        gameState.MazeHeight = _mazeGen.GenerateRandomMazeSize();
        gameState.MazeWidth = _mazeGen.GenerateRandomMazeSize();
        _mazeGen.InitializeMaze();
        _mazeGen.GenerateMaze(1, 1); // Start generating maze from (1, 1)
        _mazeGen.GenerateExit();
        _mazeGen.GenerateEnemy();
        _mazeGen.GenerateTreasure();
    }

    public void AdjustToDifficulty()
    {
        gameState.MaxLevels = gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 5,
            MazeDifficulty.Insanity => 6,
            _ => 6
        };
        gameState.PlayerVisibilityRadius = gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        gameState.CandleVisibilityRadius = gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 2,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        gameState.IncreasedVisibilityEffectRadius = gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 3,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        gameState.BombCount = gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        gameState.CandleCount = gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 6,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 4,
            _ => 3
        };
    }

    private bool IsCellEmpty(int x, int y)
    {
        var maze = gameState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0))
            return maze[y, x] == MazeIcons.Empty;

        return false;
    }

    public void CalculateLevelScore(DateTime levelStartTime)
    {
        var mazeArea = gameState.MazeWidth * gameState.MazeHeight;
        var maxTime = mazeArea switch
        {
            <= 50 => 45,
            <= 165 => 90,
            <= 400 => 150,
            _ => 210
        };

        var timeScore = maxTime - (int)(DateTime.Now - levelStartTime).TotalSeconds;

        gameState.Score += timeScore < 0 ? 0 : timeScore;
    }
}