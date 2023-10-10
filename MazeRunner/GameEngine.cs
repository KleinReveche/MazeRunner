using System.Text;

namespace Reveche.MazeRunner;

public partial class GameEngine
{
    private const int PlayerVisibilityRadius = 3;
    private const int CandleVisibilityRadius = 2;
    private const int IncreasedVisibilityEffectRadius = 2;
    private const int BlastRadius = 1;
    private readonly StringBuilder _buffer = new();
    private readonly GameState _gameState;
    private readonly MazeGen _mazeGen;
    private readonly MazeIcons _mazeIcons = new(GameMenu.GameState);
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
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        var levelIsCompleted = true;
        var shouldRedraw = true;

        while (_gameState.CurrentLevel <= _gameState.MaxLevels)
        {
            //For Testing Levels
            //var random = new Random();
            //_gameState.CurrentLevel = random.Next(1, _gameState.MaxLevels);
            //_gameState.AtAGlance = true;
            if (levelIsCompleted)
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
                levelIsCompleted = false;
            }

            if (shouldRedraw)
            {
                BombSequence();
                DrawMaze();
                Console.Clear();
                Console.Write(_buffer);

                var isInvincible = false;
                
                if (_gameState.IsPlayerInvulnerable)
                {
                    _gameState.PlayerInvincibilityEffectDuration--;
                    isInvincible = true;
                }

                if (CheckEnemyCollision(PlayerX, PlayerY) && !isInvincible)
                {
                    Console.WriteLine("You died!");
                    _gameState.PlayerLife--;
                }

                if (_gameState.CurrentLevel != 1) MoveAllEnemies();
                shouldRedraw = false;
            }

            if (_gameState.PlayerLife == 0)
            {
                DrawMaze();
                Console.Clear();
                Console.Write(_buffer);
                Console.SetCursorPosition(_gameState.MazeWidth / 2, _gameState.MazeHeight / 2);
                Console.WriteLine("Game Over!");
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
    }

    private bool IsCellEmpty(int x, int y)
    {
        var maze = _gameState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0)) 
            return maze[y, x] == _mazeIcons.Empty;

        return false;
    }
}