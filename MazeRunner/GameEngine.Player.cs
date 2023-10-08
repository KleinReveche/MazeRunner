namespace Reveche.MazeRunner;

public partial class GameEngine
{
    private bool PlayerAction(ConsoleKey key)
    {
        var placeBomb = false;
        var placeCandle = false;
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
            case ConsoleKey.B:
                placeBomb = true;
                break;
            case ConsoleKey.C:
                placeCandle = true;
                break;
        }

        if (placeCandle)
        {
            if (_gameState.CandleCount == 0) return false;
            _gameState.CandleCount--;
            _gameState.CandleLocations.Add((LastPlayerY, LastPlayerX));
            return true;
        }

        if (placeBomb && _gameState is { BombCount: > 0, BombIsUsed: false })
        {
            _gameState.BombCount--;
            _gameState.Maze[LastPlayerY, LastPlayerX] = _mazeIcons.Bomb;
            BombX = LastPlayerX;
            BombY = LastPlayerY;

            BombSequence(true);
            return true;
        }
        
        if (!IsCellEmpty(newPlayerX, newPlayerY)) return false;
        LastPlayerX = _gameState.PlayerX;
        LastPlayerY = _gameState.PlayerY;
        // Clear previous player position
        Maze[PlayerY, PlayerX] = _mazeIcons.Empty;
        // Set new player position
        _gameState.PlayerX = newPlayerX;
        _gameState.PlayerY = newPlayerY;
        // Set player in the maze
        Maze[PlayerY, PlayerX] = _gameState.Player;
        return true; // Player has moved, indicate that screen should be redrawn
    }

    private void BombSequence(bool startBomb = false)
    {
        if (startBomb)
        {
            _gameState.BombIsUsed = true;
            _gameState.BombTimer = 3;
            return;
        }

        if (_gameState.BombTimer != 0)
            _gameState.BombTimer--;

        if (_gameState is not { BombIsUsed: true, BombTimer: 0 }) return;
        for (var y = -BlastRadius; y <= BlastRadius; y++)
        {
            for (var x = -BlastRadius; x <= BlastRadius; x++)
            {
                if (BombX + x == PlayerX && BombY + y == PlayerY)
                {
                    _gameState.PlayerLife--;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.WriteLine("You died!");
                    Console.ReadKey();
                }
                //TODO: FIX THIS!!!
                var treasure = _gameState.TreasureLocations.FirstOrDefault(treasureLocation =>
                    treasureLocation.treasureX == PlayerX && treasureLocation.treasureY == PlayerY);

                if (treasure.treasureType != TreasureType.None)
                {
                    switch (treasure.treasureType)
                    {
                        case TreasureType.Bomb:
                            _gameState.BombCount += treasure.count;
                            break;
                        case TreasureType.Candle:
                            _gameState.CandleCount += treasure.count;
                            break;
                        case TreasureType.Life:
                            _gameState.PlayerLife += treasure.count;
                            break;
                        case TreasureType.None:
                        default:
                            break;
                    }
                    Console.WriteLine($"You found {treasure.count} {treasure.treasureType}!");
                    _gameState.TreasureLocations.Remove(treasure);
                }

                if (BombX + x == EnemyX && BombY + y == EnemyY)
                {
                    _gameState.EnemyX = -1;
                    _gameState.EnemyY = -1;
                }

                if (_mazeGen.IsInBounds(BombX + x, BombY + y))
                    Maze[BombY + y, BombX + x] = _mazeIcons.Empty;
            }
        }

        _gameState.BombIsUsed = false;
    }
}