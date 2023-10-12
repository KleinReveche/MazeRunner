namespace Reveche.MazeRunner.Console;

public partial class GameEngineConsole
{
    private void DrawMaze()
    {
        _gameState.Player = _gameState.PlayerLife switch
        {
            2 => _gameState.IsUtf8 ? "😐" : "P",
            1 => _gameState.IsUtf8 ? "🤕" : "P",
            0 => _gameState.IsUtf8 ? "👻" : "X",
            _ => _gameState.IsUtf8 ? "😀" : "P"
        };

        _mazeBuffer.Clear();

        for (var y = 0; y < Maze.GetLength(0); y++)
        {
            for (var x = 0; x < Maze.GetLength(1); x++)
            {
                var distanceToPlayer = Math.Abs(x - PlayerX) + Math.Abs(y - PlayerY);
                var isWithinCandleRadius = _gameState.CandleLocations
                    .Any(candleLocation => Math.Abs(x - candleLocation.Item2) <= _gameState.CandleVisibilityRadius
                                           && Math.Abs(y - candleLocation.Item1) <= _gameState.CandleVisibilityRadius);
                var isCandle = _gameState.CandleLocations
                    .Any(candleLocation => x == candleLocation.CandleX && y == candleLocation.candleY);
                var isTreasure = _gameState.TreasureLocations
                    .Any(treasureLocation => x == treasureLocation.treasureX && y == treasureLocation.treasureY);
                var isTemporaryVisible = _gameState is { PlayerHasIncreasedVisibility: true };

                if (
                    distanceToPlayer <= _gameState.PlayerVisibilityRadius +
                    (isTemporaryVisible ? _gameState.IncreasedVisibilityEffectRadius : 0)
                    || isWithinCandleRadius || _gameState.AtAGlance
                )
                {
                    if (x == PlayerX && y == PlayerY)
                        _mazeBuffer.Append(_gameState.Player); // Player
                    else if (x == ExitX && y == ExitY)
                        _mazeBuffer.Append(_mazeIcons.Exit); // Exit
                    else if (_gameEngine.CheckEnemyCollision(x, y) && _gameState.CurrentLevel != 1)
                        _mazeBuffer.Append(_mazeIcons.Enemy); // Enemy
                    else if (isCandle)
                        _mazeBuffer.Append(_mazeIcons.Candle); // Candle
                    else if (isTreasure)
                        _mazeBuffer.Append(_mazeIcons.Treasure); // Treasure
                    else
                        _mazeBuffer.Append(Maze[y, x]);
                }
                else
                {
                    _mazeBuffer.Append(_gameState.PlayerLife == 0 ? _mazeIcons.RedSquare : _mazeIcons.Fog);
                }
            }

            _mazeBuffer.AppendLine(); // Move to the next row in the buffer
        }
    }


    private void DrawInventory()
    {
        var height = 9;
        var inventoryWidth = 16;
        
        var inventory = new List<(string, int)>
        {
            ("Bombs", _gameState.BombCount),
            ("Candles", _gameState.CandleCount)
        };

        if (_gameState.IsPlayerInvulnerable || _gameState.PlayerHasIncreasedVisibility)
        {
            if (_gameState.IsPlayerInvulnerable)
                inventory.Add(("Invulnerability", _gameState.PlayerInvincibilityEffectDuration));

            if (_gameState.PlayerHasIncreasedVisibility)
                inventory.Add(("Increased Visibility", 1));

            inventoryWidth = 24;
            height++;
        }

        _inventoryBuffer.Clear();
        var inventoryHeight = height - 2;

        var corner =  _gameState.IsUtf8 ? '▩' : '+';
        var verticalSide = _gameState.IsUtf8 ? '▮' : '|';
        var horizontalSide = _gameState.IsUtf8 ? '▬' : '-';
        var middle = inventoryWidth / 2;
        var currentLevel = $"Level {_gameState.CurrentLevel} of {_gameState.MaxLevels}";
        var currentScore = $"Score: {_gameState.Score}";
        var playerLife = $"{_gameState.PlayerLife} {(_gameState.PlayerLife == 1 ? "Life" : "Lives")} Left";

        AppendCorner();
        _inventoryBuffer.AppendLine($"{verticalSide}".PadRight(middle - 6) +
                                    "Player Stats".PadRight(middle + 6) + verticalSide);
        AppendHorizontalLine();
        AppendEmptyLine();
        AppendLine(currentLevel);
        AppendLine(currentScore);
        AppendLine(playerLife);
        AppendEmptyLine();

        bool hasStatus = false, hasItem = false;

        foreach (var item in inventory)
        {
            if (item.Item1.Contains("ity") && !hasStatus)
            {
                AppendEmptyLine();
                AppendLine("Status Effects:");
                hasStatus = true;
            }

            if (!hasItem)
            {
                AppendLine("Items:");
                hasItem = true;
            }

            var item2 = item.Item1 == "Increased Visibility" ? "" : $"x{item.Item2}";
            AppendLine($"{item.Item1} {item2}");
        }

        for (var i = inventory.Count; i < inventoryHeight - 4; i++)
            AppendEmptyLine();
        AppendCorner();

        return;

        void AppendEmptyLine() =>
            _inventoryBuffer.AppendLine($"{verticalSide}".PadRight(inventoryWidth, ' ') + verticalSide);

        void AppendLine(string text) =>
            _inventoryBuffer.AppendLine($"{verticalSide} {text}".PadRight(inventoryWidth) + verticalSide);

        void AppendCorner() =>
            _inventoryBuffer.AppendLine($"{corner}".PadRight(inventoryWidth, horizontalSide) + corner);

        void AppendHorizontalLine() =>
            _inventoryBuffer.AppendLine($"{horizontalSide}".PadRight(inventoryWidth, horizontalSide) + horizontalSide);
    }

    private void DrawCombinedBuffer()
    {
        _combinedBuffer.Clear();
        var mazeBufferLines = _mazeBuffer.ToString().Split("\r\n");
        var inventoryBufferLines = _inventoryBuffer.ToString().Split("\r\n");
        var len = Math.Max(inventoryBufferLines.Length, mazeBufferLines.Length);
                
        for (var i = 0; i < len; i++)
        {
            if (i < mazeBufferLines.Length - 1)
                _combinedBuffer.Append(mazeBufferLines[i]);
            else
                _combinedBuffer.Append(' ', _gameState.MazeWidth * (_gameState.IsUtf8 ? 2 : 1));
                    
            _combinedBuffer.Append(' ', 2);
                    
            if (i < inventoryBufferLines.Length)
                _combinedBuffer.Append(inventoryBufferLines[i]);
                    
            _combinedBuffer.AppendLine();
        }
    }
}