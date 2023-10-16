﻿namespace Reveche.MazeRunner;

public class MazeGen
{
    private readonly GameState _gameState;
    private readonly Random _random = new();

    public MazeGen(GameState gameState)
    {
        _gameState = gameState;
    }

    public void InitializeMaze()
    {
        var mazeHeight = _gameState.MazeHeight;
        var mazeWidth = _gameState.MazeWidth;

        _gameState.PlayerX = 1;
        _gameState.PlayerY = 1;
        _gameState.Maze = new char[mazeHeight, mazeWidth];

        for (var y = 0; y < mazeHeight; y++)
        for (var x = 0; x < mazeWidth; x++)
        {
            var isBorder = x == 0 || x == mazeWidth - 1 || y == 0 || y == mazeHeight - 1;
            _gameState.Maze[y, x] = isBorder ? MazeIcons.Border : MazeIcons.Wall;
        }
    }

    public void GenerateMaze(int x, int y)
    {
        _gameState.Maze[y, x] = MazeIcons.Empty;
        int[] directions = { 0, 1, 2, 3 };
        Shuffle(directions);

        foreach (var dir in directions)
        {
            var (newX, newY) = GetNewPosition(x, y, dir);

            if (!IsInBounds(newX, newY) || _gameState.Maze[newY, newX] != MazeIcons.Wall) continue;
            _gameState.Maze[newY, newX] = MazeIcons.Empty;
            _gameState.Maze[y + (newY - y) / 2, x + (newX - x) / 2] = MazeIcons.Empty;
            GenerateMaze(newX, newY);
        }
    }

    public void GenerateExit()
    {
        var min = _gameState.CurrentLevel * 4 / 2;

        do
        {
            _gameState.ExitX = _random.Next(min, _gameState.MazeWidth - 1);
            _gameState.ExitY = _random.Next(min, _gameState.MazeHeight - 1);
        } while (
            (_gameState.ExitX == _gameState.PlayerX && _gameState.ExitY == _gameState.PlayerY)
            || _gameState.Maze[_gameState.ExitY, _gameState.ExitX] == MazeIcons.Wall
        );

        _gameState.Maze[_gameState.ExitY, _gameState.ExitX] = MazeIcons.Empty;
    }

    public void GenerateEnemy()
    {
        var min = _gameState.CurrentLevel * 4 / 2;
        var difficultyMultiplier = _gameState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 1,
            MazeDifficulty.Normal => 1,
            MazeDifficulty.Hard => 2,
            _ => 3
        };
        var maxEnemies = _random.Next(1, _gameState.CurrentLevel * difficultyMultiplier);

        if (_gameState.CurrentLevel == 1) return;

        for (var i = 0; i < maxEnemies; i++)
        {
            int enemyX, enemyY;

            do
            {
                enemyX = _random.Next(min, _gameState.MazeWidth - 1);
                enemyY = _random.Next(min, _gameState.MazeHeight - 1);
            } while (IsInvalidEnemyPosition(enemyX, enemyY));

            _gameState.EnemyLocations.Add((enemyY, enemyX));
        }
    }

    public void GenerateTreasure()
    {
        if (_gameState.CurrentLevel <= 2) return;
        var treasureCount = _random.Next(1, _gameState.CurrentLevel - 1);

        for (var i = 0; i < treasureCount; i++)
        {
            int treasureX, treasureY;
            var random2 = new Random();
            bool isTreasureAlreadyThere, isTreasureOnPlayer, isTreasureOnExit, isTreasureOnEnemy;

            do
            {
                treasureX = _random.Next(2, _gameState.MazeWidth - 2);
                treasureY = _random.Next(2, _gameState.MazeHeight - 2);

                isTreasureAlreadyThere = _gameState.TreasureLocations.Any(treasureLocation =>
                    treasureLocation.treasureX == treasureX && treasureLocation.treasureY == treasureY);
                isTreasureOnPlayer = treasureX == _gameState.PlayerX && treasureY == _gameState.PlayerY;
                isTreasureOnExit = treasureX == _gameState.ExitX && treasureY == _gameState.ExitY;
                isTreasureOnEnemy = _gameState.EnemyLocations.Any(enemyLocation =>
                    enemyLocation.enemyX == treasureX && enemyLocation.enemyY == treasureY);
            } while (!IsCellEmpty(treasureX, treasureY) || isTreasureAlreadyThere || isTreasureOnPlayer ||
                     isTreasureOnExit || isTreasureOnEnemy);

            var treasureType = GetRandomTreasureType();

            if (treasureType == TreasureType.None) continue;

            _gameState.TreasureLocations.Add((treasureY, treasureX, treasureType,
                treasureType is TreasureType.Life
                    or TreasureType.IncreasedVisibilityEffect
                    or TreasureType.TemporaryInvulnerabilityEffect
                    ? 1
                    : random2.Next(1, _gameState.CurrentLevel)));
        }

        foreach (var treasureLocation in _gameState.TreasureLocations)
            _gameState.Maze[treasureLocation.treasureY, treasureLocation.treasureX] = MazeIcons.Empty;
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
        var isEnemyAlreadyThere = _gameState.EnemyLocations.Any(enemyLocation =>
            enemyLocation.enemyX == x && enemyLocation.enemyY == y);

        return x == _gameState.ExitX || y == _gameState.ExitY || isEnemyAlreadyThere || !IsInBounds(x, y) ||
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
        return x >= 0 && x < _gameState.MazeWidth && y >= 0 && y < _gameState.MazeHeight &&
               _gameState.Maze[y, x] != MazeIcons.Border;
    }

    private bool IsInsideWalls(int x, int y)
    {
        return x >= 0 && x < _gameState.MazeWidth && y >= 0 && y < _gameState.MazeHeight &&
               _gameState.Maze[y, x] != MazeIcons.Wall;
    }

    private bool IsCellEmpty(int x, int y)
    {
        return x >= 0 && x < _gameState.MazeWidth && y >= 0 && y < _gameState.MazeHeight &&
               _gameState.Maze[y, x] == MazeIcons.Empty;
    }

    public int GenerateRandomMazeSize()
    {
        var random = new Random();
        int randomNum;
        var min = 5 * _gameState.CurrentLevel;
        var max = 7 * _gameState.CurrentLevel;

        do
        {
            randomNum = random.Next(min + 1, max + 1);
        } while (randomNum % 2 == 0);

        return randomNum;
    }
}