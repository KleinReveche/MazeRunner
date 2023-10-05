namespace Reveche.MazeRunner;

public static class MazeGen
{
    public static void InitializeMaze(GameState gameState)
    {
        var mazeHeight = gameState.MazeHeight;
        var mazeWidth = gameState.MazeWidth;

        gameState.Maze = new string[mazeHeight, mazeWidth];

        for (var y = 0; y < mazeHeight; y++)
        {
            for (var x = 0; x < mazeWidth; x++)
            {
                var isBorder = x == 0 || x == mazeWidth - 1 || y == 0 || y == mazeHeight - 1;
                gameState.Maze[y, x] = (isBorder) ? MazeIcons.Border : MazeIcons.Wall; 
            }
        }

        // Set player, exit, and enemy in the maze
        gameState.Maze[gameState.PlayerY, gameState.PlayerX] = gameState.Player;
    }

    public static void GenerateMaze(GameState gameState, int x, int y)
    {
        gameState.Maze[y, x] = MazeIcons.Empty;
        int[] directions = { 0, 1, 2, 3 };
        Shuffle(directions);

        foreach (var dir in directions)
        {
            var newX = x;
            var newY = y;

            switch (dir)
            {
                case 0: // Up
                    newY -= 2;
                    break;
                case 1: // Right
                    newX += 2;
                    break;
                case 2: // Down
                    newY += 2;
                    break;
                case 3: // Left
                    newX -= 2;
                    break;
            }

            if (!IsInBounds(gameState, newX, newY) || gameState.Maze[newY, newX] != MazeIcons.Wall) continue;
            gameState.Maze[newY, newX] = MazeIcons.Empty;
            gameState.Maze[y + (newY - y) / 2, x + (newX - x) / 2] = MazeIcons.Empty;
            GenerateMaze(gameState, newX, newY);
        }
    }

    public static void GenerateExitAndEnemy(GameState gameState)
    {
        var random = new Random();
        var mazeHeight = gameState.MazeHeight;
        var mazeWidth = gameState.MazeWidth;

        gameState.ExitX = random.Next(1, mazeWidth - 1);
        gameState.ExitY = random.Next(1, mazeHeight - 1);

        do
        {
            gameState.EnemyX = random.Next(1, mazeWidth - 1);
            gameState.EnemyY = random.Next(1, mazeHeight - 1);
        } while (gameState.EnemyX == gameState.ExitX && gameState.EnemyY == gameState.ExitY);

        gameState.Maze[gameState.ExitY, gameState.ExitX] = MazeIcons.Empty;
    }

    private static void Shuffle(IList<int> array)
    {
        var rand = new Random();
        for (var i = array.Count - 1; i > 0; i--)
        {
            var j = rand.Next(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private static bool IsInBounds(GameState gameState, int x, int y)
    {
        return x >= 0 && x < gameState.MazeWidth && y >= 0 && y < gameState.MazeHeight && gameState.Maze[y, x] != MazeIcons.Border;
    }

}

public static class MazeIcons
{
    public static string Wall => "🟨";
    public static string Border => "🟩";
    public static string Exit => "🚪";
    public static string Enemy => "👾";
    public static string Empty => "  ";
}