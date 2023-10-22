using System.Text;

namespace Reveche.MazeRunner.Console;

public partial class GameRenderer
{
    private char[,] Maze => gameState.Maze;
    private int PlayerX => gameState.PlayerX;
    private int PlayerY => gameState.PlayerY;
    private int ExitX => gameState.ExitX;
    private int ExitY => gameState.ExitY;

    public StringBuilder DrawMaze()
    {
        var mazeBuffer = new StringBuilder();
        var playerCharacter = GetPlayerCharacter();

        var mazeHeight = Maze.GetLength(0);
        var mazeWidth = Maze.GetLength(1);

        for (var y = 0; y < mazeHeight; y++)
        {
            for (var x = 0; x < mazeWidth; x++)
            {
                var isCellVisible = IsCellVisible(x, y);
                mazeBuffer.Append(GetCellContent(x, y, isCellVisible, playerCharacter));
            }

            mazeBuffer.AppendLine();
        }

        return mazeBuffer;
    }

    private string GetPlayerCharacter()
    {
        return gameState.PlayerLife switch
        {
            2 => gameState.IsUtf8 ? "😐" : "P",
            1 => gameState.IsUtf8 ? "🤕" : "P",
            0 => gameState.IsUtf8 ? "👻" : "X",
            _ => gameState.IsUtf8 ? "😀" : "P"
        };
    }

    private bool IsCellVisible(int x, int y)
    {
        var distanceToPlayer = Math.Abs(x - PlayerX) + Math.Abs(y - PlayerY);
        var isWithinCandleRadius = gameState.CandleLocations
            .Any(candleLocation => Math.Abs(x - candleLocation.Item2) <= gameState.CandleVisibilityRadius
                                   && Math.Abs(y - candleLocation.Item1) <= gameState.CandleVisibilityRadius);
        var isTemporaryVisible = gameState is { PlayerHasIncreasedVisibility: true };
        var isGameDone = gameState.CurrentLevel > gameState.MaxLevels && gameState.GameMode == GameMode.Classic;

        return distanceToPlayer <= gameState.PlayerVisibilityRadius +
               (isTemporaryVisible ? gameState.IncreasedVisibilityEffectRadius : 0)
               || isWithinCandleRadius || gameState.AtAGlance || isGameDone;
    }

    private string GetCellContent(int x, int y, bool isCellVisible, string playerCharacter)
    {
        var isCandle = gameState.CandleLocations
            .Any(candleLocation => x == candleLocation.CandleX && y == candleLocation.candleY);
        var isTreasure = gameState.TreasureLocations
            .Any(treasureLocation => x == treasureLocation.treasureX && y == treasureLocation.treasureY);

        if (!isCellVisible) return gameState.PlayerLife == 0 ? IsUtf8(MazeIcons.LostFog) : IsUtf8(MazeIcons.Fog);

        if (x == PlayerX && y == PlayerY) return playerCharacter;

        if (x == ExitX && y == ExitY) return IsUtf8(MazeIcons.Exit);

        if (gameEngine.CheckEnemyCollision(x, y) && gameState.CurrentLevel != 1) return IsUtf8(MazeIcons.Enemy);

        return isCandle ? IsUtf8(MazeIcons.Candle) : IsUtf8(isTreasure ? MazeIcons.Treasure : Maze[y, x]);

        string IsUtf8(char icon)
        {
            if (!gameState.IsUtf8) return icon.ToString();

            return icon switch
            {
                MazeIcons.Wall => "🟪",
                MazeIcons.Border => "🟦",
                MazeIcons.Empty => "  ",
                MazeIcons.Exit => "🚪",
                MazeIcons.Enemy => "👾",
                MazeIcons.Bomb => "💣",
                MazeIcons.Candle => "🕯️",
                MazeIcons.Treasure => "📦",
                MazeIcons.Fog => "🟫",
                MazeIcons.LostFog => "🟥",
                _ => icon.ToString()
            };
        }
    }
}