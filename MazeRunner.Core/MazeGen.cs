namespace Reveche.MazeRunner;

public class MazeGen(GameState gameState)
{
    private readonly Random _random = new();

    public void InitializeMaze()
    {
        var mazeHeight = gameState.MazeHeight;
        var mazeWidth = gameState.MazeWidth;

        gameState.PlayerX = 1;
        gameState.PlayerY = 1;
        gameState.Maze = new char[mazeHeight, mazeWidth];

        for (var y = 0; y < mazeHeight; y++)
        for (var x = 0; x < mazeWidth; x++)
        {
            var isBorder = x == 0 || x == mazeWidth - 1 || y == 0 || y == mazeHeight - 1;
            gameState.Maze[y, x] = isBorder ? MazeIcons.Border : MazeIcons.Wall;
        }
    }

    public void GenerateMaze(int x, int y)
    {
        gameState.Maze[y, x] = MazeIcons.Empty;
        int[] directions = { 0, 1, 2, 3 };
        Shuffle(directions);

        foreach (var dir in directions)
        {
            var (newX, newY) = GetNewPosition(x, y, dir);

            if (!IsInBounds(newX, newY) || gameState.Maze[newY, newX] != MazeIcons.Wall) continue;
            gameState.Maze[newY, newX] = MazeIcons.Empty;
            gameState.Maze[y + (newY - y) / 2, x + (newX - x) / 2] = MazeIcons.Empty;
            GenerateMaze(newX, newY);
        }
    }

    public void GenerateExit()
    {
        var min = gameState.CurrentLevel * 4 / 2;

        do
        {
            gameState.ExitX = _random.Next(min, gameState.MazeWidth - 1);
            gameState.ExitY = _random.Next(min, gameState.MazeHeight - 1);
        } while (
            (gameState.ExitX == gameState.PlayerX && gameState.ExitY == gameState.PlayerY)
            || gameState.Maze[gameState.ExitY, gameState.ExitX] == MazeIcons.Wall
        );

        gameState.Maze[gameState.ExitY, gameState.ExitX] = MazeIcons.Empty;
    }

    public void GenerateEnemy()
    {
        var min = gameState.CurrentLevel * 4 / 2;
        var difficultyMultiplier = gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 1,
            MazeDifficulty.Normal => 1,
            MazeDifficulty.Hard => 2,
            _ => 3
        };
        var maxEnemies = _random.Next(1, gameState.CurrentLevel * difficultyMultiplier);

        if (gameState.CurrentLevel == 1) return;

        for (var i = 0; i < maxEnemies; i++)
        {
            int enemyX, enemyY;

            do
            {
                enemyX = _random.Next(min, gameState.MazeWidth - 1);
                enemyY = _random.Next(min, gameState.MazeHeight - 1);
            } while (IsInvalidEnemyPosition(enemyX, enemyY));

            gameState.EnemyLocations.Add((enemyY, enemyX));
        }
    }

    public void GenerateTreasure()
    {
        if (gameState.CurrentLevel <= 2) return;
        var treasureCount = _random.Next(1, gameState.CurrentLevel - 1);

        for (var i = 0; i < treasureCount; i++)
        {
            int treasureX, treasureY;
            var random2 = new Random();
            bool isTreasureAlreadyThere, isTreasureOnPlayer, isTreasureOnExit, isTreasureOnEnemy;

            do
            {
                treasureX = _random.Next(2, gameState.MazeWidth - 2);
                treasureY = _random.Next(2, gameState.MazeHeight - 2);

                isTreasureAlreadyThere = gameState.TreasureLocations.Any(treasureLocation =>
                    treasureLocation.treasureX == treasureX && treasureLocation.treasureY == treasureY);
                isTreasureOnPlayer = treasureX == gameState.PlayerX && treasureY == gameState.PlayerY;
                isTreasureOnExit = treasureX == gameState.ExitX && treasureY == gameState.ExitY;
                isTreasureOnEnemy = gameState.EnemyLocations.Any(enemyLocation =>
                    enemyLocation.enemyX == treasureX && enemyLocation.enemyY == treasureY);
            } while (!IsCellEmpty(treasureX, treasureY) || isTreasureAlreadyThere || isTreasureOnPlayer ||
                     isTreasureOnExit || isTreasureOnEnemy);

            var treasureType = GetRandomTreasureType();

            if (treasureType == TreasureType.None) continue;

            gameState.TreasureLocations.Add((treasureY, treasureX, treasureType,
                treasureType is TreasureType.Life
                    or TreasureType.IncreasedVisibilityEffect
                    or TreasureType.TemporaryInvulnerabilityEffect
                    ? 1
                    : random2.Next(1, gameState.CurrentLevel)));
        }

        foreach (var treasureLocation in gameState.TreasureLocations)
            gameState.Maze[treasureLocation.treasureY, treasureLocation.treasureX] = MazeIcons.Empty;
    }

    private static (int newX, int newY) GetNewPosition(int x, int y, int dir)
    {
        return dir switch
        {
            0 => (x, y - 2), // Up
            1 => (x + 2, y), // Right
            2 => (x, y + 2), // Down
            3 => (x - 2, y), // Left
            _ => (x, y)
        };
    }

    private bool IsInvalidEnemyPosition(int x, int y)
    {
        var isEnemyAlreadyThere = gameState.EnemyLocations.Any(enemyLocation =>
            enemyLocation.enemyX == x && enemyLocation.enemyY == y);

        return x == gameState.ExitX || y == gameState.ExitY || isEnemyAlreadyThere || !IsInBounds(x, y) ||
               IsInsideWalls(x, y);
    }

    private TreasureType GetRandomTreasureType()
    {
        var treasureTypeRandom = _random.Next(0, 100);

        return treasureTypeRandom switch
        {
            <= 35 => TreasureType.Bomb,
            <= 60 => TreasureType.Candle,
            <= 75 => TreasureType.IncreasedVisibilityEffect,
            <= 85 => TreasureType.TemporaryInvulnerabilityEffect,
            <= 96 => TreasureType.Life,
            <= 98 => TreasureType.AtAGlanceEffect,
            _ => TreasureType.None
        };
    }

    private void Shuffle(IList<int> array)
    {
        for (var i = array.Count - 1; i > 0; i--)
        {
            var j = _random.Next(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < gameState.MazeWidth && y >= 0 && y < gameState.MazeHeight &&
               gameState.Maze[y, x] != MazeIcons.Border;
    }

    private bool IsInsideWalls(int x, int y)
    {
        return x >= 0 && x < gameState.MazeWidth && y >= 0 && y < gameState.MazeHeight &&
               gameState.Maze[y, x] != MazeIcons.Wall;
    }

    private bool IsCellEmpty(int x, int y)
    {
        return x >= 0 && x < gameState.MazeWidth && y >= 0 && y < gameState.MazeHeight &&
               gameState.Maze[y, x] == MazeIcons.Empty;
    }

    public int GenerateRandomMazeSize()
    {
        var random = new Random();
        int randomNum;
        var min = 5 * gameState.CurrentLevel;
        var max = 7 * gameState.CurrentLevel;

        do
        {
            randomNum = random.Next(min + 1, max + 1);
        } while (randomNum % 2 == 0);

        return randomNum;
    }
}