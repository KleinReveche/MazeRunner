namespace Reveche.MazeRunner.Classic;

public partial class ClassicEngine
{
    public bool PlayerAction(ConsoleKey key, out bool isPlayerDead)
    {
        isPlayerDead = false;
        var placeBomb = false;
        var placeCandle = false;
        var newPlayerX = classicState.PlayerX;
        var newPlayerY = classicState.PlayerY;

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
            if (classicState.CandleCount == 0) return false;
            classicState.CandleCount--;
            classicState.CandleLocations.Add((LastPlayerY, LastPlayerX));
            return true;
        }

        if (placeBomb && classicState is { BombCount: > 0, BombIsUsed: false })
        {
            classicState.BombCount--;
            classicState.Maze[LastPlayerY, LastPlayerX] = MazeIcons.Bomb;
            BombX = LastPlayerX;
            BombY = LastPlayerY;

            BombSequence(out isPlayerDead, true);
            return true;
        }

        if (!IsCellEmpty(newPlayerX, newPlayerY)) return false;
        LastPlayerX = classicState.PlayerX;
        LastPlayerY = classicState.PlayerY;
        // Clear previous player position
        Maze[PlayerY, PlayerX] = MazeIcons.Empty;
        // Set new player position
        classicState.PlayerX = newPlayerX;
        classicState.PlayerY = newPlayerY;

        if (classicState is { IsPlayerInvulnerable: true, PlayerInvincibilityEffectDuration: > 0 })
            classicState.PlayerInvincibilityEffectDuration--;
        else
            classicState.IsPlayerInvulnerable = false;

        if (classicState.AtAGlance)
            classicState.AtAGlance = false;

        return true; // Player has moved, indicate that screen should be redrawn
    }

    public void CheckPlayerEnemyCollision(out bool playerIsDead)
    {
        playerIsDead = false;

        if (!CheckEnemyCollision(PlayerX, PlayerY) || classicState.IsPlayerInvulnerable) return;
        classicState.PlayerLife--;
        playerIsDead = true;
    }

    public bool CheckForTreasure(out (int treasureY, int treasureX, TreasureType treasureType, int count) treasure)
    {
        treasure = classicState.TreasureLocations.FirstOrDefault(treasureLocation =>
            treasureLocation.treasureX == PlayerX && treasureLocation.treasureY == PlayerY);

        if (treasure == default) return false;
        AcquireTreasure(treasure);
        return true;
    }

    private void AcquireTreasure((int treasureY, int treasureX, TreasureType treasureType, int count) treasure)
    {
        if (treasure.treasureType == TreasureType.None) return;
        switch (treasure.treasureType)
        {
            case TreasureType.Bomb:
                classicState.BombCount += treasure.count;
                break;
            case TreasureType.Candle:
                classicState.CandleCount += treasure.count;
                break;
            case TreasureType.Life:
                classicState.PlayerLife += treasure.count;
                break;
            case TreasureType.IncreasedVisibilityEffect:
                classicState.PlayerHasIncreasedVisibility = true;
                break;
            case TreasureType.TemporaryInvulnerabilityEffect:
                classicState.IsPlayerInvulnerable = true;
                classicState.PlayerInvincibilityEffectDuration = 10;
                break;
            case TreasureType.AtAGlanceEffect:
                classicState.AtAGlance = true;
                break;
            case TreasureType.None:
            default:
                break;
        }

        classicState.Score += treasure.count *
                              ((int)treasure.treasureType +
                               (treasure.treasureType == TreasureType.Bomb ? 1 : 0));
        classicState.TreasureLocations.Remove(treasure);
    }

    public void BombSequence(out bool isPlayerDead, bool startBomb = false)
    {
        isPlayerDead = false;
        if (startBomb)
        {
            classicState.BombIsUsed = true;
            classicState.BombTimer = 3;
            return;
        }

        if (classicState.BombTimer > 0)
            classicState.BombTimer--;

        if (classicState is not { BombIsUsed: true, BombTimer: 0 }) return;
        for (var y = -BlastRadius; y <= BlastRadius; y++)
        for (var x = -BlastRadius; x <= BlastRadius; x++)
        {
            var playerIsInvulnerable = classicState.IsPlayerInvulnerable;
            if (BombX + x == PlayerX && BombY + y == PlayerY && !playerIsInvulnerable)
            {
                classicState.PlayerLife--;
                isPlayerDead = true;
            }

            if (CheckEnemyCollision(BombX + x, BombY + y, out (int EnemyX, int EnemyY) enemy))
            {
                classicState.EnemyLocations.Remove((enemy.EnemyY, enemy.EnemyX));
                classicState.Score += 20;
            }

            if (_mazeGen.IsInBounds(BombX + x, BombY + y))
                Maze[BombY + y, BombX + x] = MazeIcons.Empty;
        }

        classicState.BombIsUsed = false;
    }
}