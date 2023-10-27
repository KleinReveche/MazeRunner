using System.Text;
using Reveche.MazeRunner.Classic;

namespace Reveche.MazeRunner.Console.Classic;

public partial class GameRenderer(GameState gameState, ClassicEngine classicEngine, ClassicState classicState)
{
    private StringBuilder DrawInventory()
    {
        var inventoryBuffer = new StringBuilder();
        var height = 9;
        var inventoryWidth = 16;

        var inventory = new List<(string, int)>
        {
            ("Bombs", classicState.BombCount),
            ("Candles", classicState.CandleCount)
        };

        if (classicState.IsPlayerInvulnerable || classicState.PlayerHasIncreasedVisibility)
        {
            if (classicState.IsPlayerInvulnerable)
                inventory.Add(("Invulnerability", classicState.PlayerInvincibilityEffectDuration));

            if (classicState.PlayerHasIncreasedVisibility)
                inventory.Add(("Increased Visibility", 1));

            inventoryWidth = 24;
            height++;
        }

        inventoryBuffer.Clear();
        var inventoryHeight = height - 2;

        const char verticalSide = '│';
        const char horizontalSide = '─';
        var middle = inventoryWidth / 2;
        var currentScore = $"Score: {classicState.Score}";
        var playerLife = $"{classicState.PlayerLife} {(classicState.PlayerLife == 1 ? "Life" : "Lives")} Left";

        AppendCorner(true);
        inventoryBuffer.AppendLine("│".PadRight(middle - 6) +
                                   "Player Stats".PadRight(middle + 6) + "│");
        AppendHorizontalLine();
        AppendEmptyLine();

        switch (gameState.GameMode)
        {
            case GameMode.Classic:
                AppendLine($"Level {Math.Min(classicState.CurrentLevel, classicState.MaxLevels)} of {classicState.MaxLevels}");
                break;
            case GameMode.Endless:
                AppendLine($"Level {classicState.CurrentLevel} of ∞");
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
        var inventoryBuffer = DrawInventory();
        var combinedBuffer = new StringBuilder();

        combinedBuffer.Clear();
        var separator = OperatingSystem.IsWindows() ? "\r\n" : "\n";
        var mazeBufferLines = mazeBuffer.ToString().Split(separator);
        var inventoryBufferLines = inventoryBuffer.ToString().Split(separator);
        var len = Math.Max(inventoryBufferLines.Length, mazeBufferLines.Length);

        for (var i = 0; i < len; i++)
        {
            if (i < mazeBufferLines.Length - 1)
                combinedBuffer.Append((string?)mazeBufferLines[i]);
            else
                combinedBuffer.Append(' ', classicState.MazeWidth * (gameState.IsUtf8 ? 2 : 1));

            combinedBuffer.Append(' ', 2);

            if (i < inventoryBufferLines.Length)
                combinedBuffer.Append(inventoryBufferLines[i]);

            combinedBuffer.AppendLine();
        }

        return combinedBuffer;
    }
}