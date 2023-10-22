namespace Reveche.MazeRunner;

public partial class GameEngine
{
    public bool PlayerAction(ConsoleKey key, out bool isPlayerDead)
    {
        isPlayerDead = false;
        var placeBomb = false;
        var placeCandle = false;
        var newPlayerX = gameState.PlayerX;
        var newPlayerY = gameState.PlayerY;

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
            if (gameState.CandleCount == 0) return false;
            gameState.CandleCount--;
            gameState.CandleLocations.Add((LastPlayerY, LastPlayerX));
            return true;
        }

        if (placeBomb && gameState is { BombCount: > 0, BombIsUsed: false })
        {
            gameState.BombCount--;
            gameState.Maze[LastPlayerY, LastPlayerX] = MazeIcons.Bomb;
            BombX = LastPlayerX;
            BombY = LastPlayerY;

            BombSequence(out isPlayerDead, true);
            return true;
        }

        if (!IsCellEmpty(newPlayerX, newPlayerY)) return false;
        LastPlayerX = gameState.PlayerX;
        LastPlayerY = gameState.PlayerY;
        // Clear previous player position
        Maze[PlayerY, PlayerX] = MazeIcons.Empty;
        // Set new player position
        gameState.PlayerX = newPlayerX;
        gameState.PlayerY = newPlayerY;

        if (gameState is { IsPlayerInvulnerable: true, PlayerInvincibilityEffectDuration: > 0 })
            gameState.PlayerInvincibilityEffectDuration--;
        else
            gameState.IsPlayerInvulnerable = false;

        if (gameState.AtAGlance)
            gameState.AtAGlance = false;

        return true; // Player has moved, indicate that screen should be redrawn
    }

    public void CheckPlayerEnemyCollision(out bool playerIsDead)
    {
        playerIsDead = false;

        if (!CheckEnemyCollision(PlayerX, PlayerY) || gameState.IsPlayerInvulnerable) return;
        gameState.PlayerLife--;
        playerIsDead = true;
    }

    public bool CheckForTreasure(out (int treasureY, int treasureX, TreasureType treasureType, int count) treasure)
    {
        treasure = gameState.TreasureLocations.FirstOrDefault(treasureLocation =>
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
                gameState.BombCount += treasure.count;
                break;
            case TreasureType.Candle:
                gameState.CandleCount += treasure.count;
                break;
            case TreasureType.Life:
                gameState.PlayerLife += treasure.count;
                break;
            case TreasureType.IncreasedVisibilityEffect:
                gameState.PlayerHasIncreasedVisibility = true;
                break;
            case TreasureType.TemporaryInvulnerabilityEffect:
                gameState.IsPlayerInvulnerable = true;
                gameState.PlayerInvincibilityEffectDuration = 10;
                break;
            case TreasureType.AtAGlanceEffect:
                gameState.AtAGlance = true;
                break;
            case TreasureType.None:
            default:
                break;
        }

        gameState.Score += treasure.count *
                            ((int)treasure.treasureType +
                             (treasure.treasureType == TreasureType.Bomb ? 1 : 0));
        gameState.TreasureLocations.Remove(treasure);
    }

    public void BombSequence(out bool isPlayerDead, bool startBomb = false)
    {
        isPlayerDead = false;
        if (startBomb)
        {
            gameState.BombIsUsed = true;
            gameState.BombTimer = 3;
            return;
        }

        if (gameState.BombTimer > 0)
            gameState.BombTimer--;

        if (gameState is not { BombIsUsed: true, BombTimer: 0 }) return;
        for (var y = -BlastRadius; y <= BlastRadius; y++)
        for (var x = -BlastRadius; x <= BlastRadius; x++)
        {
            var playerIsInvulnerable = gameState.IsPlayerInvulnerable;
            if (BombX + x == PlayerX && BombY + y == PlayerY && !playerIsInvulnerable)
            {
                gameState.PlayerLife--;
                isPlayerDead = true;
            }

            if (CheckEnemyCollision(BombX + x, BombY + y, out (int EnemyX, int EnemyY) enemy))
            {
                gameState.EnemyLocations.Remove((enemy.EnemyY, enemy.EnemyX));
                gameState.Score += 20;
            }

            if (_mazeGen.IsInBounds(BombX + x, BombY + y))
                Maze[BombY + y, BombX + x] = MazeIcons.Empty;
        }

        gameState.BombIsUsed = false;
    }
}