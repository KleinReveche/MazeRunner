namespace Reveche.MazeRunner;

public class MazeGen
{
    private readonly GameState _gameState;
    
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
                _gameState.Maze[y, x] = (isBorder) ? MazeIcons.Border : MazeIcons.Wall; 
            }
        }

        _gameState.Maze[_gameState.PlayerY, _gameState.PlayerX] = _gameState.Player;
    }

    public void GenerateMaze(int x, int y)
    {
        _gameState.Maze[y, x] = MazeIcons.Empty;
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

            if (!IsInBounds(newX, newY) || _gameState.Maze[newY, newX] != MazeIcons.Wall) continue;
            _gameState.Maze[newY, newX] = MazeIcons.Empty;
            _gameState.Maze[y + (newY - y) / 2, x + (newX - x) / 2] = MazeIcons.Empty;
            GenerateMaze(newX, newY);
        }
    }

    public void GenerateExitAndEnemy()
    {
        var random = new Random();
        var mazeHeight = _gameState.MazeHeight;
        var mazeWidth = _gameState.MazeWidth;

        var min = _gameState.CurrentLevel * 4 / 2;
        
        _gameState.ExitX = random.Next(min, mazeWidth - 1);
        _gameState.ExitY = random.Next(min, mazeHeight - 1);
        _gameState.Maze[_gameState.ExitY, _gameState.ExitX] = MazeIcons.Empty;

        if(_gameState.CurrentLevel == 1) return;
        do
        {
            _gameState.EnemyX = random.Next(min, mazeWidth - 1);
            _gameState.EnemyY = random.Next(min, mazeHeight - 1);
        } while (_gameState.EnemyX == _gameState.ExitX && _gameState.EnemyY == _gameState.ExitY);
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

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < _gameState.MazeWidth && y >= 0 && y < _gameState.MazeHeight && _gameState.Maze[y, x] != MazeIcons.Border;
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

public static class MazeIcons
{
    public static string Wall => "🟪";
    public static string Border => "🟦";
    public static string Exit => "🚪";
    public static string Enemy => "👾";
    public static string Empty => "  ";
}