using System.Text;

namespace Reveche.MazeRunner.Console.Classic;

public partial class GameRenderer
{
    private char[,] Maze => classicState.Maze;
    private int PlayerX => classicState.PlayerX;
    private int PlayerY => classicState.PlayerY;
    private int ExitX => classicState.ExitX;
    private int ExitY => classicState.ExitY;

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
        return classicState.PlayerLife switch
        {
            2 => optionsState.IsUtf8 ? "😐" : "P",
            1 => optionsState.IsUtf8 ? "🤕" : "P",
            0 => optionsState.IsUtf8 ? "👻" : "X",
            _ => optionsState.IsUtf8 ? "😀" : "P"
        };
    }

    private bool IsCellVisible(int x, int y)
    {
        var distanceToPlayer = Math.Abs(x - PlayerX) + Math.Abs(y - PlayerY);
        var isWithinCandleRadius = classicState.CandleLocations
            .Any(candleLocation => Math.Abs(x - candleLocation.Item2) <= classicState.CandleVisibilityRadius
                                   && Math.Abs(y - candleLocation.Item1) <= classicState.CandleVisibilityRadius);
        var isTemporaryVisible = classicState is { PlayerHasIncreasedVisibility: true };
        var isGameDone = classicState.CurrentLevel > classicState.MaxLevels && optionsState.GameMode == GameMode.Classic;

        return distanceToPlayer <= classicState.PlayerVisibilityRadius +
               (isTemporaryVisible ? classicState.IncreasedVisibilityEffectRadius : 0)
               || isWithinCandleRadius || classicState.AtAGlance || isGameDone;
    }

    private string GetCellContent(int x, int y, bool isCellVisible, string playerCharacter)
    {
        var isCandle = classicState.CandleLocations
            .Any(candleLocation => x == candleLocation.CandleX && y == candleLocation.candleY);
        var isTreasure = classicState.TreasureLocations
            .Any(treasureLocation => x == treasureLocation.treasureX && y == treasureLocation.treasureY);

        if (!isCellVisible) return classicState.PlayerLife == 0 ? IsUtf8(MazeIcons.LostFog) : IsUtf8(MazeIcons.Fog);

        if (x == PlayerX && y == PlayerY) return playerCharacter;

        if (x == ExitX && y == ExitY) return IsUtf8(MazeIcons.Exit);

        if (classicEngine.CheckEnemyCollision(x, y) && classicState.CurrentLevel != 1) return IsUtf8(MazeIcons.Enemy);

        return isCandle ? IsUtf8(MazeIcons.Candle) : IsUtf8(isTreasure ? MazeIcons.Treasure : Maze[y, x]);

        string IsUtf8(char icon)
        {
            if (!optionsState.IsUtf8) return icon.ToString();

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