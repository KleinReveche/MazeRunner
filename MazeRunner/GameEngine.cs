using System.Text;

namespace Reveche.MazeRunner;

public class Game
{
    public static void Play(GameState gameState)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        var levelIsCompleted = true;
        
        while (gameState.CurrentLevel <= gameState.MaxLevels)
        {
            Console.Clear();

            if (levelIsCompleted)
            {
                gameState.MazeHeight = GenerateRandomMazeSize(gameState.CurrentLevel);
                gameState.MazeWidth = GenerateRandomMazeSize(gameState.CurrentLevel);
                MazeGen.InitializeMaze(gameState);
                MazeGen.GenerateMaze(gameState, gameState.PlayerX, gameState.PlayerY); // Start generating maze from (1, 1)
                MazeGen.GenerateExitAndEnemy(gameState);
                levelIsCompleted = false;
            }
            
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

                levelIsCompleted = true;
            }

            var key = Console.ReadKey().Key;
            MovePlayer(gameState, key);
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
        
        gameState.Player = gameState.PlayerLife switch
        {
            2 => "😩",
            1 => "🤕",
            0 => "👻",
            _ => "😀"
        };
        
        for (var y = 0; y < gameState.MazeHeight; y++)
        {
            for (var x = 0; x < gameState.MazeWidth; x++)
            {
                if (x == playerX && y == playerY)
                {
                    Console.Write(gameState.Player); // Player
                }
                else if (x == exitX && y == exitY)
                {
                    Console.Write(MazeIcons.Exit); // Exit
                }
                else if (x == enemyX && y == enemyY)
                {
                    Console.Write(MazeIcons.Enemy); // Enemy
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

        if (!Game.IsCellEmpty(gameState, newPlayerX, newPlayerY)) return;
        // Clear previous player position
        gameState.Maze[gameState.PlayerY, gameState.PlayerX] = MazeIcons.Empty;
        // Set new player position
        gameState.PlayerX = newPlayerX;
        gameState.PlayerY = newPlayerY;
        // Set player in the maze
        gameState.Maze[gameState.PlayerY, gameState.PlayerX] = gameState.Player;
    }

    private static bool IsCellEmpty(GameState gameState, int x, int y)
    {
        var maze = gameState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0))
        {
            return maze[y, x] == MazeIcons.Empty;
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

    private static int GenerateRandomMazeSize(int level)
    {
        var random = new Random();
        int randomNum;
        var min = 7 * level;
        var max = 10 * level;
        do
        {
            randomNum = random.Next(min, max + 1);
        } while (randomNum % 2 == 0); // Ensure it's odd
        
        return randomNum;
    }
}