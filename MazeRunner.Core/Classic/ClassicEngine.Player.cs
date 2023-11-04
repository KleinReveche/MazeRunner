using Reveche.MazeRunner.Sound;

namespace Reveche.MazeRunner.Classic;

public partial class ClassicEngine
{
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
                classicState.PlayerInvincibilityEffectDuration =
                    (int)(15 * (_difficultyModifier + _higherLevelModifier));
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