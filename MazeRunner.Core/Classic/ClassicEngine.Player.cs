using Reveche.MazeRunner.Sound;

namespace Reveche.MazeRunner.Classic;

public partial class ClassicEngine
{
    public bool ConsolePlayerAction(ConsoleKey key, out bool isPlayerDead, out bool isGamePaused)
    {
        Console.Write("\b \b"); // To remove input key from showing on screen
        isGamePaused = false;
        isPlayerDead = false;
        var placeBomb = false;
        var placeCandle = false;
        var newPlayerX = classicState.PlayerX;
        var newPlayerY = classicState.PlayerY;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (key)
        {
            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                newPlayerY--;
                break;
            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                newPlayerY++;
                break;
            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                newPlayerX--;
                break;
            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                newPlayerX++;
                break;
            case ConsoleKey.B:
                placeBomb = true;
                break;
            case ConsoleKey.C:
                placeCandle = true;
                break;
            case ConsoleKey.Escape:

                ClassicSaveManager.SaveCurrentSave(classicState);
                isGamePaused = true;
                break;
        }

        if (placeCandle)
        {
            if (classicState.CandleCount == 0 ||
                classicState.CandleLocations.Any(candleLocation =>
                    candleLocation.Item2 == LastPlayerX && candleLocation.Item1 == LastPlayerY) ||
                (LastPlayerX == 0 && LastPlayerY == 0)) return false;
            _gameSoundFx.PlayFx(ConsoleGameSoundFx.PlaceItem);
            classicState.CandleCount--;
            classicState.CandleLocations.Add((LastPlayerY, LastPlayerX));
            return true;
        }

        var isBombAtNewPosition = classicState.BombLocations.Any(bombLocation =>
            bombLocation.bombX == LastPlayerX && bombLocation.bombY == LastPlayerY);

        if (placeBomb && !isBombAtNewPosition && classicState is { BombCount: > 0 })
        {
            classicState.BombCount--;
            classicState.BombLocations.Add((LastPlayerY, LastPlayerX, 2));
            _gameSoundFx.PlayFx(ConsoleGameSoundFx.BombPlace);
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

        _gameSoundFx.PlayFx(ConsoleGameSoundFx.ItemPickup);
        classicState.Score += (int)(treasure.count *
                                    ((int)treasure.treasureType +
                                     (treasure.treasureType == TreasureType.Bomb ? 1 : 0) *
                                     (_difficultyModifier + _higherLevelModifier)));
        classicState.TreasureLocations.Remove(treasure);
    }

    public void BombSequence(out bool isPlayerDead)
    {
        isPlayerDead = false;

        for (var bombIndex = 0; bombIndex < classicState.BombLocations.Count; bombIndex++)
        {
            var bomb = classicState.BombLocations[bombIndex];

            if (bomb.timer > 0)
            {
                var newBombTimer = bomb.timer - 1;
                classicState.BombLocations[bombIndex] = (bomb.bombY, bomb.bombX, newBombTimer);
                continue;
            }

            _gameSoundFx.PlayFx(ConsoleGameSoundFx.BombExplode);
            for (var y = -BlastRadius; y <= BlastRadius; y++)
            for (var x = -BlastRadius; x <= BlastRadius; x++)
            {
                var playerIsInvulnerable = classicState.IsPlayerInvulnerable;
                if (bomb.bombX + x == PlayerX && bomb.bombY + y == PlayerY && !playerIsInvulnerable)
                {
                    classicState.PlayerLife--;
                    isPlayerDead = true;
                }

                if (CheckEnemyCollision(bomb.bombX + x, bomb.bombY + y, out (int EnemyX, int EnemyY) enemy))
                {
                    classicState.EnemyLocations.Remove((enemy.EnemyY, enemy.EnemyX));
                    classicState.Score += (int)(20 * (_difficultyModifier + _higherLevelModifier));
                }

                if (_mazeGen.IsInBounds(bomb.bombX + x, bomb.bombY + y))
                    Maze[bomb.bombY + y, bomb.bombX + x] = MazeIcons.Empty;
            }

            classicState.BombLocations.Remove(bomb);
        }
    }
}