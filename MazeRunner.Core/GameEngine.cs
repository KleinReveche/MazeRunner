namespace Reveche.MazeRunner;

public partial class GameEngine
{
    private const int BlastRadius = 1;
    private readonly GameState _gameState;
    private readonly MazeGen _mazeGen;

    public GameEngine(GameState gameState)
    {
        _gameState = gameState;
        _mazeGen = new MazeGen(_gameState);
    }

    private int PlayerX => _gameState.PlayerX;
    private int PlayerY => _gameState.PlayerY;
    private int LastPlayerX { get; set; }
    private int LastPlayerY { get; set; }
    private int BombX { get; set; }
    private int BombY { get; set; }
    private char[,] Maze => _gameState.Maze;

    public void InitializeNewLevel()
    {
        if (_gameState.PlayerHasIncreasedVisibility)
            _gameState.PlayerHasIncreasedVisibility = false;
        _gameState.CandleLocations.Clear();
        _gameState.BombIsUsed = false;
        _gameState.MazeHeight = _mazeGen.GenerateRandomMazeSize();
        _gameState.MazeWidth = _mazeGen.GenerateRandomMazeSize();
        _mazeGen.InitializeMaze();
        _mazeGen.GenerateMaze(1, 1); // Start generating maze from (1, 1)
        _mazeGen.GenerateExit();
        _mazeGen.GenerateEnemy();
        _mazeGen.GenerateTreasure();
    }

    public void AdjustToDifficulty()
    {
        _gameState.MaxLevels = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 5,
            MazeDifficulty.Insanity => 6,
            _ => 6
        };
        _gameState.PlayerVisibilityRadius = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        _gameState.CandleVisibilityRadius = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 2,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        _gameState.IncreasedVisibilityEffectRadius = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 3,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        _gameState.BombCount = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        _gameState.CandleCount = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 6,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 4,
            _ => 3
        };
    }

    private bool IsCellEmpty(int x, int y)
    {
        var maze = _gameState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0))
            return maze[y, x] == MazeIcons.Empty;

        return false;
    }

    public void CalculateLevelScore(DateTime levelStartTime)
    {
        var mazeArea = _gameState.MazeWidth * _gameState.MazeHeight;
        var maxTime = mazeArea switch
        {
            <= 50 => 45,
            <= 165 => 90,
            <= 400 => 150,
            _ => 210
        };

        var timeScore = maxTime - (int)(DateTime.Now - levelStartTime).TotalSeconds;

        _gameState.Score += timeScore < 0 ? 0 : timeScore;
    }
}