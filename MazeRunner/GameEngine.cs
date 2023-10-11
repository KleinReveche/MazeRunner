using System.Text;

namespace Reveche.MazeRunner;

public partial class GameEngine
{
    private const int BlastRadius = 1;
    private readonly StringBuilder _buffer = new();
    private readonly GameState _gameState;
    private readonly MazeGen _mazeGen;
    private readonly MazeIcons _mazeIcons = new(GameMenu.GameState);
    private int _playerVisibilityRadius;
    private int _candleVisibilityRadius;
    private int _increasedVisibilityEffectRadius;
    private int PlayerX => _gameState.PlayerX;
    private int PlayerY => _gameState.PlayerY;
    private int LastPlayerX { get; set; }
    private int LastPlayerY { get; set; }
    private int BombX { get; set; }
    private int BombY { get; set; }
    private int ExitX => _gameState.ExitX;
    private int ExitY => _gameState.ExitY;
    private string[,] Maze => _gameState.Maze;
    
    public GameEngine(GameState gameState)
    {
        _gameState = gameState;
        _mazeGen = new MazeGen(_gameState);
    }
    
    public void Play()
    {
        Console.CursorVisible = false;
        var levelIsCompleted = true;
        var shouldRedraw = true;
        AdjustToDifficulty();
        
        while (true)
        {
            if (levelIsCompleted)
            {
                _gameState.IsCurrentlyPlaying = _gameState.CurrentLevel <= _gameState.MaxLevels;
                InitializeNewLevel();
                levelIsCompleted = false;
            }

            if (shouldRedraw)
            {
                BombSequence();
                DrawMaze();
                Console.Clear();
                Console.Write(_buffer);
                CheckPlayerEnemyCollision();

                if (_gameState.CurrentLevel != 1) MoveAllEnemies();
                shouldRedraw = false;
            }

            if (_gameState.PlayerLife == 0)
            {
                DisplayGameOver();
                break;
            }

            if (_gameState.PlayerX == _gameState.ExitX && _gameState.PlayerY == _gameState.ExitY)
            {
                Console.WriteLine($"Congratulations! You completed level {_gameState.CurrentLevel}.");
                _gameState.CurrentLevel++;
                if (!(_gameState.CurrentLevel <= _gameState.MaxLevels))
                {
                    Console.WriteLine("You have completed all levels. Press Any Key to exit.");
                    Console.ReadKey();
                    break;
                }

                shouldRedraw = true;
                levelIsCompleted = true;
            }
            
            var key = Console.ReadKey().Key;
            if (PlayerAction(key))
                shouldRedraw = true;
        }
        GameMenu.StartMenu(); // TODO: Create a proper game over screen
    }
    
    private void AdjustToDifficulty()
    {
        _gameState.MaxLevels = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 5,
            MazeDifficulty.Insanity => 6,
            _ => 6
        };
        _playerVisibilityRadius = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        _candleVisibilityRadius = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 2,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        _increasedVisibilityEffectRadius = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 3,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
    }

    private void CheckPlayerEnemyCollision()
    {
        var isInvincible = false;

        if (_gameState.IsPlayerInvulnerable)
        {
            _gameState.PlayerInvincibilityEffectDuration--;
            isInvincible = true;
        }

        if (!CheckEnemyCollision(PlayerX, PlayerY) || isInvincible) return;
        Console.WriteLine("You died!");
        _gameState.PlayerLife--;
    }

    private void DisplayGameOver()
    {
        DrawMaze();
        Console.Clear();
        Console.Write(_buffer);
        Console.SetCursorPosition(_gameState.MazeWidth / 2, _gameState.MazeHeight / 2);
        Console.WriteLine("Game Over!");
        Console.ReadKey();
    }

    private void InitializeNewLevel()
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

    private bool IsCellEmpty(int x, int y)
    {
        var maze = _gameState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0)) 
            return maze[y, x] == _mazeIcons.Empty;

        return false;
    }
}