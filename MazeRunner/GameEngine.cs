using System.Text;

namespace Reveche.MazeRunner;

public static class Game
{
    public static void Play(GameState gameState)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        while (gameState.CurrentLevel <= gameState.MaxLevels)
        {
            Console.Clear();
            InitializeLevel(gameState, gameState.CurrentLevel);
            
            if (gameState.CurrentLevel != 1) MoveEnemy(gameState);
            
            PrintMaze(gameState);

            if (gameState.PlayerLife == 0)
            {
                Console.SetCursorPosition(2,1);
                Console.WriteLine("🟥🟥🟥🟥🟥🟥🟥🟥");
                Console.SetCursorPosition(2,2);
                Console.WriteLine("🟥 Game Over! 🟥");
                Console.SetCursorPosition(2,3);
                Console.WriteLine("🟥🟥🟥🟥🟥🟥🟥🟥");
                break;
            }
            

            if (gameState.PlayerX == gameState.EnemyX && gameState.PlayerY == gameState.EnemyY)
            {
                //TODO: Add Game-over message. Use Console.SetCursorPosition
                Console.WriteLine("You died!");
                gameState.PlayerLife--;
            }

            if (gameState.PlayerX == gameState.ExitX && gameState.PlayerY == gameState.ExitY)
            {
                Console.WriteLine($"Congratulations! You completed level {gameState.CurrentLevel}.");
                switch (gameState.CurrentLevel)
                {
                    case 2:
                        if (!WorcleQuest.Game.Start())
                        {
                            gameState.PlayerLife--;
                            Console.WriteLine("You lost a life!");
                        }

                        break;
                }
                gameState.CurrentLevel++;
                if (!(gameState.CurrentLevel <= gameState.MaxLevels))
                {
                    Console.WriteLine("You have completed all levels. Press Enter to exit.");
                    Console.ReadLine();
                    break;
                }
            }

            var key = Console.ReadKey().Key;
            MovePlayer(gameState, key);
        }
    }

    private static void InitializeLevel(GameState gameState, int level)
    {
        switch (level)
        {
            case 1:
                gameState.ExitX = 8;
                gameState.ExitY = 1;
                gameState.Maze = new[,]
                {
                    {"🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩"},
                    {"🟩", "  ", "🟩", "  ", "  ", "  ", "  ", "🟩", "  ", "🟩"},
                    {"🟩", "  ", "🟩", "🟩", "  ", "🟩", "  ", "🟩", "  ", "🟩"},
                    {"🟩", "  ", "  ", "  ", "  ", "🟩", "  ", "  ", "  ", "🟩"},
                    {"🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩"},
                };
                break;
            case 2:
                gameState.ExitX = 8;
                gameState.ExitY = 8;
                gameState.EnemyX = 1;
                gameState.ExitY = 7;
                gameState.Maze = new[,]
                {
                    {"🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩"},
                    {"🟩", "  ", "🟩", "  ", "  ", "  ", "  ", "  ", "  ", "🟩"},
                    {"🟩", "  ", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "  ", "🟩"},
                    {"🟩", "  ", "  ", "  ", "  ", "  ", "  ", "🟩", "  ", "🟩"},
                    {"🟩", "🟩", "  ", "🟩", "🟩", "  ", "🟩", "🟩", "  ", "🟩"},
                    {"🟩", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "🟩"},
                    {"🟩", "🟩", "🟩", "🟩", "  ", "🟩", "🟩", "🟩", "🟩", "🟩"},
                    {"🟩", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "🟩"},
                    {"🟩", "  ", "🟩", "🟩", "🟩", "🟩", "🟩", "  ", "  ", "🟩"},
                    {"🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩"},
                };
                break;
            case 3:
                gameState.ExitX = 1;
                gameState.ExitY = 2;
                gameState.Maze = new[,]
                {
                    {"🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩"},
                    {"🟩", "  ", "🟩", "  ", "  ", "  ", "  ", "  ", "  ", "🟩"},
                    {"🟩", "  ", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "  ", "🟩"},
                    {"🟩", "  ", "  ", "  ", "  ", "  ", "  ", "🟩", "  ", "🟩"},
                    {"🟩", "🟩", "  ", "🟩", "🟩", "  ", "🟩", "🟩", "  ", "🟩"},
                    {"🟩", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "🟩"},
                    {"🟩", "🟩", "🟩", "🟩", "  ", "🟩", "🟩", "🟩", "🟩", "🟩"},
                    {"🟩", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "🟩"},
                    {"🟩", "  ", "🟩", "🟩", "🟩", "🟩", "🟩", "  ", "  ", "🟩"},
                    {"🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩", "🟩"},
                };
                break;
        }
    }

    private static void PrintMaze(GameState gameState)
    {
        var maze = gameState.Maze;
        var playerX = gameState.PlayerX;
        var playerY = gameState.PlayerY;
        var enemyX = gameState.EnemyX;
        var enemyY = gameState.EnemyY;
        var exitX = gameState.ExitX;
        var exitY = gameState.ExitY;
        
        for (var y = 0; y < maze.GetLength(0); y++)
        {
            for (var x = 0; x < maze.GetLength(1); x++)
            {
                if (x == playerX && y == playerY)
                {
                    Console.Write("😀"); // Player
                }
                else if (x == exitX && y == exitY)
                {
                    Console.Write("🚪"); // Exit
                }
                else if (x == enemyX && y == enemyY)
                {
                    Console.Write("👾"); // Enemy
                }
                else
                {
                    Console.Write(maze[y, x]);
                }
            }
            Console.WriteLine();
        }
    }

    private static void MovePlayer(GameState gameState, ConsoleKey key)
    {
        var newPlayerX = gameState.PlayerX;
        var newPlayerY = gameState.PlayerY;

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
            default:
                return;
        }

        if (!IsCellEmpty(gameState, newPlayerX, newPlayerY)) return;
        gameState.PlayerX = newPlayerX;
        gameState.PlayerY = newPlayerY;
    }

    private static bool IsCellEmpty(GameState gameState, int x, int y)
    {
        var maze = gameState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0))
        {
            return maze[y, x] == "  ";
        }
        return false;
    }


    private static void MoveEnemy(GameState gameState)
    {
        var enemyX = gameState.EnemyX;
        var enemyY = gameState.EnemyY;
        var exitX = gameState.ExitX;
        var exitY = gameState.ExitY;
        
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
        if (!IsCellEmpty(gameState, newEnemyX, newEnemyY)) return;
        gameState.EnemyX = newEnemyX;
        gameState.EnemyY = newEnemyY;
    }
}

