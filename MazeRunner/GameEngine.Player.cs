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
        
        if (_gameState is { PlayerHasIncreasedVisibility: true, PlayerIncreasedVisibilityEffectDuration: > 0 })
            _gameState.PlayerIncreasedVisibilityEffectDuration--;
        else if (_gameState.PlayerHasIncreasedVisibility)
            _gameState.PlayerHasIncreasedVisibility = false;

        if (_gameState is { IsPlayerInvulnerable: true, PlayerInvincibilityEffectDuration: > 0 })
            _gameState.PlayerInvincibilityEffectDuration--;
        else if (_gameState.IsPlayerInvulnerable)
            _gameState.IsPlayerInvulnerable = false;
        
        if (_gameState.AtAGlance)
            _gameState.AtAGlance = false;
        
        var treasure = _gameState.TreasureLocations.FirstOrDefault(treasureLocation =>
            treasureLocation.treasureX == PlayerX && treasureLocation.treasureY == PlayerY);

        if (treasure != default)
        {
            AcquireTreasure(treasure);
            switch (treasure.treasureType)
            {
                case TreasureType.IncreasedVisibilityEffect:
                    Console.WriteLine($"You have increased visibility for {_gameState.PlayerIncreasedVisibilityEffectDuration} turns.");
                    break;
                case TreasureType.TemporaryInvulnerabilityEffect:
                    Console.WriteLine($"You have temporarily invulnerable for {_gameState.PlayerInvincibilityEffectDuration} turns.");
                    break;
                case TreasureType.AtAGlanceEffect:
                    Console.WriteLine("Glance at the maze. At your next turn, it'll be hidden again.");
                    break;
                case TreasureType.Bomb:
                case TreasureType.Candle:
                case TreasureType.Life:
                case TreasureType.None:
                default:
                    Console.WriteLine($"You found {treasure.count} {treasure.treasureType}!");
                    break;
            }
            Console.ReadKey();
        }
        
        // Set player in the maze
        Maze[PlayerY, PlayerX] = _gameState.Player;
        return true; // Player has moved, indicate that screen should be redrawn
    }

    private void AcquireTreasure((int treasureY, int treasureX, TreasureType treasureType, int count) treasure)
    {
        if (treasure.treasureType == TreasureType.None) return;
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
            case TreasureType.IncreasedVisibilityEffect:
                _gameState.PlayerHasIncreasedVisibility = true;
                _gameState.PlayerIncreasedVisibilityEffectDuration = 5;
                break;
            case TreasureType.TemporaryInvulnerabilityEffect:
                _gameState.IsPlayerInvulnerable = true;
                _gameState.PlayerInvincibilityEffectDuration = 5;
                break;
            case TreasureType.AtAGlanceEffect:
                _gameState.AtAGlance = true;
                break;
            case TreasureType.None:
            default:
                break;
        }
        _gameState.TreasureLocations.Remove(treasure);
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
                var playerIsInvulnerable = _gameState.IsPlayerInvulnerable;
                if (BombX + x == PlayerX && BombY + y == PlayerY && !playerIsInvulnerable)
                {
                    _gameState.PlayerLife--;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.WriteLine("You died!");
                    Console.ReadKey();
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