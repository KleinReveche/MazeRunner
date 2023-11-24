using DotNetXtensions.Cryptography;
using Reveche.MazeRunner.Sound;

namespace Reveche.MazeRunner.Classic;

public partial class ClassicEngine
{
    public void CheckPlayerEnemyCollision(out bool playerIsDead)
    {
        var random = new CryptoRandom();
        var playerDamage = 0;
        playerIsDead = false;

        if (CheckEnemyCollision(PlayerX, PlayerY) && !classicState.IsPlayerInvulnerable)
            playerDamage = random.Next(20, 33);

        if (CheckEnemyCollision(PlayerX, PlayerY, out (int EnemyX, int EnemyY, Enemy Enemy) higherClassEnemy) &&
            classicState is { CurrentLevel: > 6, IsPlayerInvulnerable: false })
            switch (higherClassEnemy.Enemy)
            {
                case Enemy.Goblin:
                    if (random.Next(1, 100) <= 50)
                    {
                        if (classicState.BombCount > 0) classicState.BombCount--;
                    }
                    else
                    {
                        if (classicState.CandleCount > 0) classicState.CandleCount--;
                    }

                    playerDamage += random.Next(35, 47);
                    break;
                case Enemy.Ogre:
                    playerDamage += random.Next(51, 69);
                    classicState.DecreasedVisibilityEffectDuration =
                        random.Next(1, (int)(3 * (_difficultyModifier + _higherLevelModifier)));
                    break;
                case Enemy.Dragon:
                    playerDamage += random.Next(70, 93);
                    classicState.PlayerBurnDuration =
                        random.Next(1, (int)(3 * (_difficultyModifier + _higherLevelModifier)));
                    break;
                case Enemy.None:
                    break;
                default:
                    throw new Exception("Wrong enemy type");
            }

        classicState.PlayerHealth -= playerDamage;

        if (classicState.PlayerHealth > 0) return;
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
                classicState.PlayerMaxHealth += (int)(5 * (_difficultyModifier + _higherLevelModifier));
                classicState.PlayerHealth = classicState.PlayerMaxHealth;
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
                break;
            default:
                throw new Exception("Wrong treasure type");
        }

        _gameSoundFx.PlayFx(SoundFx.ItemPickup);
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

            _gameSoundFx.PlayFx(SoundFx.BombExplode);
            for (var y = -BlastRadius; y <= BlastRadius; y++)
            for (var x = -BlastRadius; x <= BlastRadius; x++)
            {
                var playerIsInvulnerable = classicState.IsPlayerInvulnerable;
                if (bomb.bombX + x == PlayerX && bomb.bombY + y == PlayerY && !playerIsInvulnerable)
                {
                    classicState.PlayerHealth -= 75;
                    if (classicState.PlayerHealth <= 0)
                    {
                        classicState.PlayerLife--;
                        isPlayerDead = true;
                    }
                }

                if (CheckEnemyCollision(bomb.bombX + x, bomb.bombY + y, out (int EnemyX, int EnemyY) enemy))
                {
                    classicState.EnemyLocations.Remove((enemy.EnemyY, enemy.EnemyX));
                    classicState.Score += (int)(20 * (_difficultyModifier + _higherLevelModifier));
                }

                if (CheckEnemyCollision(bomb.bombX + x, bomb.bombY + y,
                        out (int EnemyX, int EnemyY, Enemy Enemy) higherClassEnemy))
                {
                    classicState.HigherClassEnemy.Remove((higherClassEnemy.EnemyY, higherClassEnemy.EnemyX,
                        higherClassEnemy.Enemy));
                    classicState.Score += (int)(50 * (_difficultyModifier + _higherLevelModifier));
                }

                if (_mazeGen.IsInBounds(bomb.bombX + x, bomb.bombY + y))
                    Maze[bomb.bombY + y, bomb.bombX + x] = MazeIcons.Empty;
            }

            classicState.BombLocations.Remove(bomb);
        }
    }
}