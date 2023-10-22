namespace Reveche.MazeRunner;

public partial class GameEngine
{
    public void MoveAllEnemies()
    {
        var random = new Random();
        var enemyCount = gameState.EnemyLocations.Count;

        for (var i = 0; i < enemyCount; i++)
        {
            var enemyLocation = gameState.EnemyLocations[i];

            var enemyX = enemyLocation.enemyX;
            var enemyY = enemyLocation.enemyY;
            var exitX = gameState.ExitX;
            var exitY = gameState.ExitY;

            var tries = 5;

            while (tries-- > 0)
            {
                var direction = random.Next(4);
                var newEnemyX = enemyX;
                var newEnemyY = enemyY;

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
                }

                if (newEnemyX < 0 ||
                    newEnemyX >= gameState.MazeWidth ||
                    newEnemyY < 0 ||
                    newEnemyY >= gameState.MazeHeight ||
                    (newEnemyX == exitX && newEnemyY == exitY) ||
                    !IsCellEmpty(newEnemyX, newEnemyY) ||
                    gameState.EnemyLocations.Any(loc => loc.enemyX == newEnemyX && loc.enemyY == newEnemyY))
                    continue;

                enemyLocation.enemyX = newEnemyX;
                enemyLocation.enemyY = newEnemyY;
                gameState.EnemyLocations[i] = enemyLocation;
                break;
            }
        }
    }

    public bool CheckEnemyCollision(int x, int y)
    {
        return gameState.EnemyLocations.Any(enemyLocation => x == enemyLocation.enemyX && y == enemyLocation.enemyY);
    }

    private bool CheckEnemyCollision(int x, int y, out (int enemyX, int enemyY) enemy)
    {
        var enemyLocation = gameState.EnemyLocations.FirstOrDefault(enemyLocation =>
            x == enemyLocation.enemyX && y == enemyLocation.enemyY);

        enemy = enemyLocation != default ? (enemyLocation.enemyX, enemyLocation.enemyY) : (0, 0);
        return enemyLocation != default;
    }
}