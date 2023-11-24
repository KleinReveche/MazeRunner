using DotNetXtensions.Cryptography;

namespace Reveche.MazeRunner.Classic;

public partial class ClassicEngine
{
    public void MoveAllEnemies(bool isHigherLevel = false)
    {
        var random = new CryptoRandom();
        var enemyLocations = classicState.EnemyLocations;
        var higherClassEnemyLocations = classicState.HigherClassEnemy;
        var enemyCount = isHigherLevel ? classicState.HigherClassEnemy.Count : classicState.EnemyLocations.Count;
        var exitX = classicState.ExitX;
        var exitY = classicState.ExitY;

        for (var i = 0; i < enemyCount; i++)
        {
            (int enemyY, int enemyX) enemyLocation = default;
            (int enemyY, int enemyX, Enemy enemy) higherClassEnemyLocation = default;

            if (isHigherLevel) higherClassEnemyLocation = higherClassEnemyLocations[i];
            else enemyLocation = enemyLocations[i];

            var playerX = classicState.PlayerX;
            var playerY = classicState.PlayerY;
            var enemyX = isHigherLevel ? higherClassEnemyLocation.enemyX : enemyLocation.enemyX;
            var enemyY = isHigherLevel ? higherClassEnemyLocation.enemyY : enemyLocation.enemyY;
            var radius = (int)(3 * (_difficultyModifier + _higherLevelModifier));

            // Derived from Euclidean distance formula
            var distance = Math.Sqrt(Math.Pow(playerX - enemyX, 2) + Math.Pow(playerY - enemyY, 2));
            var chanceToFollow = random.Next(1, 100);

            // Player is within the radius, move towards the player
            if (distance <= radius && chanceToFollow <= 60)
            {
                var newEnemyX = enemyX;
                var newEnemyY = enemyY;
                var deltaX = playerX - enemyX;
                var deltaY = playerY - enemyY;

                if (Math.Abs(deltaX) > Math.Abs(deltaY))
                {
                    newEnemyX += Math.Sign(deltaX); // Move in the X direction

                    if (IsCellValid(newEnemyX, enemyY))
                    {
                        if (isHigherLevel) higherClassEnemyLocation.enemyX = newEnemyX;
                        else enemyLocation.enemyX = newEnemyX;
                    }
                }
                else
                {
                    newEnemyY += Math.Sign(deltaY); // Move in the Y direction

                    if (IsCellValid(enemyX, newEnemyY))
                    {
                        if (isHigherLevel) higherClassEnemyLocation.enemyY = newEnemyY;
                        else enemyLocation.enemyY = newEnemyY;
                    }
                }
            }
            else
            {
                var tries = 10;

                while (tries-- > 0)
                {
                    var newEnemyX = enemyX;
                    var newEnemyY = enemyY;
                    var direction = random.Next(0, 3);

                    switch (direction)
                    {
                        case 0:
                            newEnemyY--;
                            break;
                        case 1:
                            newEnemyY++;
                            break;
                        case 2:
                            newEnemyX--;
                            break;
                        case 3:
                            newEnemyX++;
                            break;
                        default:
                            throw new Exception("Wrong direction");
                    }

                    if (!IsCellValid(newEnemyX, newEnemyY)) continue;

                    if (isHigherLevel)
                    {
                        higherClassEnemyLocation.enemyX = newEnemyX;
                        higherClassEnemyLocation.enemyY = newEnemyY;
                    }
                    else
                    {
                        enemyLocation.enemyX = newEnemyX;
                        enemyLocation.enemyY = newEnemyY;
                    }

                    break;
                }
            }

            if (isHigherLevel)
                classicState.HigherClassEnemy[i] = higherClassEnemyLocation;
            else
                classicState.EnemyLocations[i] = enemyLocation;
        }

        return;


        bool IsCellValid(int x, int y)
        {
            var isEnemyThere = isHigherLevel
                ? classicState.HigherClassEnemy.All(enemyLocation =>
                    enemyLocation.enemyX != x || enemyLocation.enemyY != y)
                : classicState.EnemyLocations.All(enemyLocation =>
                    enemyLocation.enemyX != x || enemyLocation.enemyY != y);

            return x >= 0 && x < classicState.MazeWidth &&
                   y >= 0 && y < classicState.MazeHeight &&
                   (x != exitX || y != exitY) &&
                   IsCellEmpty(x, y) && isEnemyThere;
        }
    }

    public bool CheckEnemyCollision(int x, int y)
    {
        return classicState.EnemyLocations.Any(enemyLocation => x == enemyLocation.enemyX && y == enemyLocation.enemyY);
    }

    private bool CheckEnemyCollision(int x, int y, out (int enemyX, int enemyY) enemy)
    {
        var enemyLocation = classicState.EnemyLocations.FirstOrDefault(enemyLocation =>
            x == enemyLocation.enemyX && y == enemyLocation.enemyY);

        enemy = enemyLocation != default ? (enemyLocation.enemyX, enemyLocation.enemyY) : (0, 0);
        return enemyLocation != default;
    }
    
    private bool CheckEnemyCollision(int x, int y, out (int enemyX, int enemyY, Enemy enemy) enemy)
    {
        var enemyLocation = classicState.HigherClassEnemy.FirstOrDefault(enemyLocation =>
            x == enemyLocation.enemyX && y == enemyLocation.enemyY);

        enemy = enemyLocation != default ? (enemyLocation.enemyX, enemyLocation.enemyY, enemyLocation.enemy) : (0, 0, Enemy.None);
        return enemyLocation != default;
    }
}