using System.Text;
using Reveche.MazeRunner.Console.Screens;

namespace Reveche.MazeRunner.Console;

public class GameRenderer
{
    private readonly GameEngine _gameEngine;
    private readonly GameState _gameState;
    private readonly MazeIcons _mazeIcons = new(MainScreen.GameState);

    private int PlayerX => _gameState.PlayerX;
    private int PlayerY => _gameState.PlayerY;
    private int ExitX => _gameState.ExitX;
    private int ExitY => _gameState.ExitY;
    private string[,] Maze => _gameState.Maze;
    
    public GameRenderer(GameEngine gameEngine, GameState gameState)
    {
        _gameEngine = gameEngine;
        _gameState = gameState;
    }

    private StringBuilder DrawMaze()
    {
        var mazeBuffer = new StringBuilder();
        _gameState.Player = _gameState.PlayerLife switch
        {
            2 => _gameState.IsUtf8 ? "😐" : "P",
            1 => _gameState.IsUtf8 ? "🤕" : "P",
            0 => _gameState.IsUtf8 ? "👻" : "X",
            _ => _gameState.IsUtf8 ? "😀" : "P"
        };

        mazeBuffer.Clear();

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
                var isGameDone = _gameState.CurrentLevel > _gameState.MaxLevels && _gameState.GameMode == GameMode.Classic;

                if (
                    distanceToPlayer <= _gameState.PlayerVisibilityRadius +
                    (isTemporaryVisible ? _gameState.IncreasedVisibilityEffectRadius : 0)
                    || isWithinCandleRadius || _gameState.AtAGlance || isGameDone
                )
                {
                    if (x == PlayerX && y == PlayerY)
                        mazeBuffer.Append(_gameState.Player); // Player
                    else if (x == ExitX && y == ExitY)
                        mazeBuffer.Append(_mazeIcons.Exit); // Exit
                    else if (_gameEngine.CheckEnemyCollision(x, y) && _gameState.CurrentLevel != 1)
                        mazeBuffer.Append(_mazeIcons.Enemy); // Enemy
                    else if (isCandle)
                        mazeBuffer.Append(_mazeIcons.Candle); // Candle
                    else if (isTreasure)
                        mazeBuffer.Append(_mazeIcons.Treasure); // Treasure
                    else
                        mazeBuffer.Append(Maze[y, x]);
                }
                else
                {
                    mazeBuffer.Append(_gameState.PlayerLife == 0 ? _mazeIcons.RedSquare : _mazeIcons.Fog);
                }
            }

            mazeBuffer.AppendLine(); // Move to the next row in the buffer
        }

        return mazeBuffer;
    }
    
    private StringBuilder DrawInventory()
    {
        var inventoryBuffer = new StringBuilder();
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

        inventoryBuffer.Clear();
        var inventoryHeight = height - 2;

        const char verticalSide = '│';
        const char horizontalSide = '─';
        var middle = inventoryWidth / 2;
        var currentScore = $"Score: {_gameState.Score}";
        var playerLife = $"{_gameState.PlayerLife} {(_gameState.PlayerLife == 1 ? "Life" : "Lives")} Left";

        AppendCorner(true);
        inventoryBuffer.AppendLine("│".PadRight(middle - 6) +
                                    "Player Stats".PadRight(middle + 6) + "│");
        AppendHorizontalLine();
        AppendEmptyLine();
        
        switch (_gameState.GameMode)
        { 
            case GameMode.Classic:
                AppendLine($"Level {Math.Min(_gameState.CurrentLevel, _gameState.MaxLevels)} of {_gameState.MaxLevels}");
                break;
            case GameMode.Campaign:
                AppendLine("Campaign");
                break;
            case GameMode.Endless:
                AppendLine($"Level {_gameState.CurrentLevel} of ∞");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
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
        AppendCorner(false);

        return inventoryBuffer;

        void AppendEmptyLine() =>
            inventoryBuffer.AppendLine($"{verticalSide}".PadRight(inventoryWidth, ' ') + verticalSide);

        void AppendLine(string text) =>
            inventoryBuffer.AppendLine($"{verticalSide} {text}".PadRight(inventoryWidth) + verticalSide);

        void AppendCorner(bool isTop) =>
            inventoryBuffer.AppendLine((isTop ? "┌" : "└").PadRight(inventoryWidth, horizontalSide) + (isTop ? "┐" : "┘"));

        void AppendHorizontalLine() =>
            inventoryBuffer.AppendLine("├".PadRight(inventoryWidth, horizontalSide) + "┤");
    }

    public StringBuilder DrawCombinedBuffer()
    {
        var mazeBuffer = DrawMaze();
        var inventoryBuffer= DrawInventory();
        var combinedBuffer = new StringBuilder();
        
        combinedBuffer.Clear();
        var mazeBufferLines = mazeBuffer.ToString().Split("\r\n");
        var inventoryBufferLines = inventoryBuffer.ToString().Split("\r\n");
        var len = Math.Max(inventoryBufferLines.Length, mazeBufferLines.Length);

        for (var i = 0; i < len; i++)
        {
            if (i < mazeBufferLines.Length - 1)
                combinedBuffer.Append(mazeBufferLines[i]);
            else
                combinedBuffer.Append(' ', _gameState.MazeWidth * (_gameState.IsUtf8 ? 2 : 1));

            combinedBuffer.Append(' ', 2);

            if (i < inventoryBufferLines.Length)
                combinedBuffer.Append(inventoryBufferLines[i]);

            combinedBuffer.AppendLine();
        }

        return combinedBuffer;
    }
}