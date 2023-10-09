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
    private int EnemyX => _gameState.EnemyX;
    private int EnemyY => _gameState.EnemyY;
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
                _gameState.CandleLocations.Clear();
                _gameState.BombIsUsed = false;
                _gameState.MazeHeight = _mazeGen.GenerateRandomMazeSize();
                _gameState.MazeWidth = _mazeGen.GenerateRandomMazeSize();
                _mazeGen.InitializeMaze();
                _mazeGen.GenerateMaze(1, 1); // Start generating maze from (1, 1)
                _mazeGen.GenerateExitAndEnemy();
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

                if (
                    _gameState.PlayerX == _gameState.EnemyX 
                    && _gameState.PlayerY == _gameState.EnemyY 
                    && !isInvincible
                )
                {
                    Console.WriteLine("You died!");
                    _gameState.PlayerLife--;
                }

                if (_gameState.CurrentLevel != 1) MoveEnemy();
                shouldRedraw = false;
            }

            if (_gameState.PlayerLife == 0)
            {
                Console.SetCursorPosition(2, 1);
                Console.WriteLine("🟥🟥🟥🟥🟥🟥🟥🟥");
                Console.SetCursorPosition(2, 2);
                Console.WriteLine("🟥 Game Over! 🟥");
                Console.SetCursorPosition(2, 3);
                Console.WriteLine("🟥🟥🟥🟥🟥🟥🟥🟥");
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

    private void MoveEnemy()
    {
        var enemyX = _gameState.EnemyX;
        var enemyY = _gameState.EnemyY;
        var exitX = _gameState.ExitX;
        var exitY = _gameState.ExitY;

        var random = new Random();
        var direction = random.Next(4); // 0: up, 1: down, 2: left, 3: right

        var newEnemyX = enemyX;
        var newEnemyY = enemyY;

        switch (direction)
        {
            case 0:
                newEnemyY--;
                break;
            case 1:
                newEnemyY++;
                break;
            case 2:
                newEnemyX--;
                break;
            case 3:
                newEnemyX++;
                break;
        }

        if (newEnemyY == exitX && newEnemyY == exitY) return;
        if (!IsCellEmpty(newEnemyX, newEnemyY)) return;
        _gameState.EnemyX = newEnemyX;
        _gameState.EnemyY = newEnemyY;
    }
}