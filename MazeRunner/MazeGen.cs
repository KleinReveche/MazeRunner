namespace Reveche.MazeRunner;

public class MazeGen
{
    private readonly GameState _gameState;
    private readonly MazeIcons _mazeIcons = new(GameMenu.GameState);

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
        _gameState.Maze = new string[mazeHeight, mazeWidth];

        for (var y = 0; y < mazeHeight; y++)
        {
            for (var x = 0; x < mazeWidth; x++)
            {
                var isBorder = x == 0 || x == mazeWidth - 1 || y == 0 || y == mazeHeight - 1;
                _gameState.Maze[y, x] = (isBorder) ? _mazeIcons.Border : _mazeIcons.Wall; 
            }
        }

        _gameState.Maze[_gameState.PlayerY, _gameState.PlayerX] = _gameState.Player;
    }

    public void GenerateMaze(int x, int y)
    {
        _gameState.Maze[y, x] = _mazeIcons.Empty;
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

            if (!IsInBounds(newX, newY) || _gameState.Maze[newY, newX] != _mazeIcons.Wall) continue;
            _gameState.Maze[newY, newX] = _mazeIcons.Empty;
            _gameState.Maze[y + (newY - y) / 2, x + (newX - x) / 2] = _mazeIcons.Empty;
            GenerateMaze(newX, newY);
        }
    }

    public void GenerateExitAndEnemy()
    {
        var random = new Random();
        var mazeHeight = _gameState.MazeHeight;
        var mazeWidth = _gameState.MazeWidth;

        var min = _gameState.CurrentLevel * 4 / 2;

        do
        {
            _gameState.ExitX = random.Next(min, mazeWidth - 1);
            _gameState.ExitY = random.Next(min, mazeHeight - 1);
        } while (
            (_gameState.ExitX == _gameState.PlayerX && _gameState.ExitY == _gameState.PlayerY)
            || _gameState.Maze[_gameState.ExitY, _gameState.ExitX] == _mazeIcons.Wall
        );

        _gameState.Maze[_gameState.ExitY, _gameState.ExitX] = _mazeIcons.Empty;

        if (_gameState.CurrentLevel == 1) return;
        do
        {
            _gameState.EnemyX = random.Next(min, mazeWidth - 1);
            _gameState.EnemyY = random.Next(min, mazeHeight - 1);
        } while (_gameState.EnemyX == _gameState.ExitX && _gameState.EnemyY == _gameState.ExitY);
    }

    public void GenerateTreasure()
    {
        var random = new Random();
        if (_gameState.CurrentLevel <= 2) return;
        var treasureCount = random.Next(1, _gameState.CurrentLevel - 1);

        for (var i = 0; i < treasureCount; i++)
        {
            int treasureX, treasureY, treasureTypeRandom, treasureCountRandom;
            var random2 = new Random();
            var random3 = new Random();
            var random4 = new Random();

            do
            {
                treasureX = random3.Next(2, _gameState.MazeWidth - 2);
                treasureY = random4.Next(2, _gameState.MazeHeight - 2);
                treasureTypeRandom = random3.Next(0, 100);
                treasureCountRandom = random2.Next(1, _gameState.CurrentLevel);
            } while (_gameState.Maze[treasureY, treasureX] != _mazeIcons.Empty);

            var treasureType = treasureTypeRandom switch
            {
                <= 35 => TreasureType.Bomb,
                <= 60 => TreasureType.Candle,
                <= 75 => TreasureType.IncreasedVisibilityEffect,
                <= 85 => TreasureType.TemporaryInvulnerabilityEffect,
                <= 96 => TreasureType.Life,
                <= 98 => TreasureType.AtAGlanceEffect,
                _ => TreasureType.None
            };

            if (treasureType == TreasureType.None) continue;
            _gameState.TreasureLocations.Add((treasureY, treasureX, treasureType,
                treasureType is TreasureType.Life
                    or TreasureType.IncreasedVisibilityEffect
                    or TreasureType.TemporaryInvulnerabilityEffect
                    ? 1 : treasureCountRandom));
            _gameState.Maze[treasureY, treasureX] = _mazeIcons.Empty;
        }
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

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < _gameState.MazeWidth && y >= 0 && y < _gameState.MazeHeight &&
               _gameState.Maze[y, x] != _mazeIcons.Border;
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