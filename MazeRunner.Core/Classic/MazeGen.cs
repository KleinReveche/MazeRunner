using DotNetXtensions.Cryptography;

namespace Reveche.MazeRunner.Classic;

public class MazeGen(ClassicState classicState)
{
    public void InitializeMaze()
    {
        var mazeHeight = classicState.MazeHeight;
        var mazeWidth = classicState.MazeWidth;

        classicState.PlayerX = 1;
        classicState.PlayerY = 1;
        classicState.Maze = new char[mazeHeight, mazeWidth];

        for (var y = 0; y < mazeHeight; y++)
        for (var x = 0; x < mazeWidth; x++)
        {
            var isBorder = x == 0 || x == mazeWidth - 1 || y == 0 || y == mazeHeight - 1;
            classicState.Maze[y, x] = isBorder ? MazeIcons.Border : MazeIcons.Wall;
        }

        classicState.TreasureLocations.Clear();
        classicState.EnemyLocations.Clear();

        GenerateMaze(1, 1); // Start generating maze from (1, 1)
        GenerateExit();
        GenerateEnemy();
        GenerateTreasure();
    }

    private void GenerateMaze(int x, int y)
    {
        classicState.Maze[y, x] = MazeIcons.Empty;
        int[] directions = { 0, 1, 2, 3 };
        Shuffle(directions);

        foreach (var dir in directions)
        {
            var (newX, newY) = GetNewPosition(x, y, dir);

            if (!IsInBounds(newX, newY) || classicState.Maze[newY, newX] != MazeIcons.Wall) continue;
            classicState.Maze[newY, newX] = MazeIcons.Empty;
            classicState.Maze[y + (newY - y) / 2, x + (newX - x) / 2] = MazeIcons.Empty;
            GenerateMaze(newX, newY);
        }
    }

    private void GenerateExit()
    {
        var random = new CryptoRandom();
        var min = classicState.CurrentLevel * 4 / 2;
        if (classicState.CurrentLevel > 15) min = 29;

        do
        {
            classicState.ExitX = random.Next(min, classicState.MazeWidth - 1);
            classicState.ExitY = random.Next(min, classicState.MazeHeight - 1);
        } while (
            (classicState.ExitX == classicState.PlayerX && classicState.ExitY == classicState.PlayerY)
            || classicState.Maze[classicState.ExitY, classicState.ExitX] == MazeIcons.Wall
        );

        classicState.Maze[classicState.ExitY, classicState.ExitX] = MazeIcons.Empty;
    }

    private void GenerateEnemy()
    {
        var random = new CryptoRandom();
        var difficultyMultiplier = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 1,
            MazeDifficulty.Normal => 1,
            MazeDifficulty.Hard => 2,
            _ => 3
        };
        var maxEnemies = random.Next(classicState.CurrentLevel + 1,
            classicState.CurrentLevel + 2 * difficultyMultiplier);
        var maxHighLevelEnemies = random.Next(0, 3 * difficultyMultiplier);

        if (classicState.CurrentLevel == 1) return;

        for (var i = 0; i < maxEnemies; i++)
        {
            CalculateEnemyPosition(out var enemyX, out var enemyY);
            classicState.EnemyLocations.Add((enemyY, enemyX));
        }

        if (classicState.CurrentLevel <= 6) return;

        for (var i = 0; i < maxHighLevelEnemies; i++)
        {
            var higherClassEnemy = random.Next(0, 100) switch
            {
                <= 30 => HighClassEnemy.None,
                <= 80 => HighClassEnemy.Goblin,
                <= 97 => HighClassEnemy.Ogre,
                _ => HighClassEnemy.Dragon
            };

            if (higherClassEnemy == HighClassEnemy.None) continue;

            CalculateEnemyPosition(out var enemyX, out var enemyY);
            classicState.HigherClassEnemy.Add((enemyY, enemyX, higherClassEnemy));
        }
    }
    
    private void CalculateEnemyPosition(out int enemyX, out int enemyY)
    {
        var random = new CryptoRandom();
        var minCoords = classicState.CurrentLevel * 4 / 2;
        var minHigherLevelCoords = classicState.CurrentLevel / 2;
        if (classicState.CurrentLevel > 4) minCoords = minHigherLevelCoords;

        do
        {
            enemyX = random.Next(minCoords, classicState.MazeWidth - 1);
            enemyY = random.Next(minCoords, classicState.MazeHeight - 1);
        } while (IsInvalidEnemyPosition(enemyX, enemyY));
    }

    private void GenerateTreasure()
    {
        var random = new CryptoRandom();
        if (classicState.CurrentLevel <= 2) return;
        var treasureCount = random.Next(1, classicState.CurrentLevel - 1);

        for (var i = 0; i < treasureCount; i++)
        {
            int treasureX = 0, treasureY = 0;
            bool isTreasureAlreadyThere = false,
                isTreasureOnPlayer = false,
                isTreasureOnExit = false,
                isTreasureOnEnemy = false;

            do
            {
                treasureX = random.Next(2, classicState.MazeWidth - 2);
                treasureY = random.Next(2, classicState.MazeHeight - 2);

                isTreasureAlreadyThere = classicState.TreasureLocations.Any(treasureLocation =>
                    treasureLocation.treasureX == treasureX && treasureLocation.treasureY == treasureY);
                isTreasureOnPlayer = treasureX == classicState.PlayerX && treasureY == classicState.PlayerY;
                isTreasureOnExit = treasureX == classicState.ExitX && treasureY == classicState.ExitY;
                isTreasureOnEnemy = classicState.EnemyLocations.Any(enemyLocation =>
                    enemyLocation.enemyX == treasureX && enemyLocation.enemyY == treasureY);
            } while (!IsCellEmpty(treasureX, treasureY) || isTreasureAlreadyThere || isTreasureOnPlayer ||
                     isTreasureOnExit || isTreasureOnEnemy);

            var treasureType = GetRandomTreasureType();

            if (treasureType == TreasureType.None) continue;

            classicState.TreasureLocations.Add((treasureY, treasureX, treasureType,
                treasureType is TreasureType.Life
                    or TreasureType.IncreasedVisibilityEffect
                    or TreasureType.TemporaryInvulnerabilityEffect
                    ? 1
                    : random.Next(1, classicState.CurrentLevel)));
        }

        foreach (var treasureLocation in classicState.TreasureLocations)
            classicState.Maze[treasureLocation.treasureY, treasureLocation.treasureX] = MazeIcons.Empty;
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
        var isEnemyAlreadyThere = classicState.EnemyLocations.Any(enemyLocation =>
            enemyLocation.enemyX == x && enemyLocation.enemyY == y);
        var isHigherEnemyAlreadyThere = classicState.HigherClassEnemy.Any(enemyLocation =>
            enemyLocation.enemyX == x && enemyLocation.enemyY == y);

        return x == classicState.ExitX || y == classicState.ExitY || isEnemyAlreadyThere || isHigherEnemyAlreadyThere ||
               !IsInBounds(x, y) || IsInsideWalls(x, y);
    }

    private static TreasureType GetRandomTreasureType()
    {
        var random = new CryptoRandom();
        var treasureTypeRandom = random.Next(0, 100);

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

    private static void Shuffle(IList<int> array)
    {
        var random = new CryptoRandom();
        for (var i = array.Count - 1; i > 0; i--)
        {
            var j = random.Next(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < classicState.MazeWidth && y >= 0 && y < classicState.MazeHeight &&
               classicState.Maze[y, x] != MazeIcons.Border;
    }

    private bool IsInsideWalls(int x, int y)
    {
        return x >= 0 && x < classicState.MazeWidth && y >= 0 && y < classicState.MazeHeight &&
               classicState.Maze[y, x] != MazeIcons.Wall;
    }

    private bool IsCellEmpty(int x, int y)
    {
        return x >= 0 && x < classicState.MazeWidth && y >= 0 && y < classicState.MazeHeight &&
               classicState.Maze[y, x] == MazeIcons.Empty;
    }

    public int GenerateRandomMazeSize()
    {
        var random = new CryptoRandom();
        var randomNum = 0;
        var min = 5 * classicState.CurrentLevel;
        var max = 7 * classicState.CurrentLevel;
        if (min > 40) min = 40;
        if (max > 49) max = 49;

        do
        {
            randomNum = random.Next(min + 1, max + 1);
        } while (randomNum % 2 == 0);

        return randomNum;
    }
}