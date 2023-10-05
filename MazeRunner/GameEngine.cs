using System.Text;

namespace Reveche.MazeRunner;

public class Game
{
    private readonly GameState _gameState;
    private readonly MazeGen _mazeGen;
    private int PlayerX => _gameState.PlayerX;
    private int PlayerY => _gameState.PlayerY;
    private int ExitX => _gameState.ExitX;
    private int ExitY => _gameState.ExitY;
    private int EnemyX => _gameState.EnemyX;
    private int EnemyY => _gameState.EnemyY;
    private string[,] Maze => _gameState.Maze;
    
    public Game(GameState gameState)
    {
        _gameState = gameState;
        _mazeGen = new MazeGen(_gameState);
    }
    
    
    public void Play()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        var levelIsCompleted = true;
        
        while (_gameState.CurrentLevel <= _gameState.MaxLevels)
        {
            Console.Clear();

            if (levelIsCompleted)
            {
                _gameState.MazeHeight = GenerateRandomMazeSize(_gameState.CurrentLevel);
                _gameState.MazeWidth = GenerateRandomMazeSize(_gameState.CurrentLevel);
                _mazeGen.InitializeMaze();
                _mazeGen.GenerateMaze(_gameState.PlayerX, _gameState.PlayerY); // Start generating maze from (1, 1)
                _mazeGen.GenerateExitAndEnemy();
                levelIsCompleted = false;
            }
            
            if (_gameState.CurrentLevel != 1) MoveEnemy();
            
            PrintMaze();

            if (_gameState.PlayerLife == 0)
            {
                Console.SetCursorPosition(2,1);
                Console.WriteLine("🟥🟥🟥🟥🟥🟥🟥🟥");
                Console.SetCursorPosition(2,2);
                Console.WriteLine("🟥 Game Over! 🟥");
                Console.SetCursorPosition(2,3);
                Console.WriteLine("🟥🟥🟥🟥🟥🟥🟥🟥");
                break;
            }
            

            if (_gameState.PlayerX == _gameState.EnemyX && _gameState.PlayerY == _gameState.EnemyY)
            {
                Console.WriteLine("You died!");
                _gameState.PlayerLife--;
            }

            if (_gameState.PlayerX == _gameState.ExitX && _gameState.PlayerY == _gameState.ExitY)
            {
                Console.WriteLine($"Congratulations! You completed level {_gameState.CurrentLevel}.");
                switch (_gameState.CurrentLevel)
                {
                    case 2:
                        if (!WorcleQuest.Game.Start())
                        {
                            _gameState.PlayerLife--;
                            Console.WriteLine("You lost a life!");
                        }

                        break;
                }
                _gameState.CurrentLevel++;
                if (!(_gameState.CurrentLevel <= _gameState.MaxLevels))
                {
                    Console.WriteLine("You have completed all levels. Press Enter to exit.");
                    Console.ReadLine();
                    break;
                }

                levelIsCompleted = true;
            }

            var key = Console.ReadKey().Key;
            MovePlayer(key);
        }
    }

    private void PrintMaze()
    {
        
        _gameState.Player = _gameState.PlayerLife switch
        {
            2 => "😩",
            1 => "🤕",
            0 => "👻",
            _ => "😀"
        };
        
        for (var y = 0; y < _gameState.MazeHeight; y++)
        {
            for (var x = 0; x < _gameState.MazeWidth; x++)
            {
                if (x == PlayerX && y == PlayerY)
                {
                    Console.Write(_gameState.Player); // Player
                }
                else if (x == ExitX && y == ExitY)
                {
                    Console.Write(MazeIcons.Exit); // Exit
                }
                else if (x == EnemyX && y == EnemyY)
                {
                    Console.Write(MazeIcons.Enemy); // Enemy
                }
                else
                {
                    Console.Write(Maze[y, x]);
                }
            }
            Console.WriteLine();
        }
    }

    private void MovePlayer(ConsoleKey key)
    {
        var newPlayerX = _gameState.PlayerX;
        var newPlayerY = _gameState.PlayerY;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (key)
        {
            case ConsoleKey.UpArrow:
                newPlayerY--;
                break;
            case ConsoleKey.DownArrow:
                newPlayerY++;
                break;
            case ConsoleKey.LeftArrow:
                newPlayerX--;
                break;
            case ConsoleKey.RightArrow:
                newPlayerX++;
                break;
        }

        if (!IsCellEmpty(newPlayerX, newPlayerY)) return;
        // Clear previous player position
        _gameState.Maze[_gameState.PlayerY, _gameState.PlayerX] = MazeIcons.Empty;
        // Set new player position
        _gameState.PlayerX = newPlayerX;
        _gameState.PlayerY = newPlayerY;
        // Set player in the maze
        _gameState.Maze[_gameState.PlayerY, _gameState.PlayerX] = _gameState.Player;
    }

    private bool IsCellEmpty(int x, int y)
    {
        var maze = _gameState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0))
        {
            return maze[y, x] == MazeIcons.Empty;
        }
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

    private static int GenerateRandomMazeSize(int level)
    {
        var random = new Random();
        int randomNum;
        var min = 7 * level;
        var max = 10 * level;
        do
        {
            randomNum = random.Next(min, max + 1);
        } while (randomNum % 2 == 0);
        
        return randomNum;
    }
}