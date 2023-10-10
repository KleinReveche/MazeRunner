namespace Reveche.MazeRunner;

public partial class GameEngine
{
    private void MoveAllEnemies()
    {
        var random = new Random();
        
        for (var i = 0; i < _gameState.EnemyLocations.Count; i++)
        {
            var enemyLocation = _gameState.EnemyLocations[i]; // Copy the struct

            var enemyX = enemyLocation.enemyX;
            var enemyY = enemyLocation.enemyY;
            var exitX = _gameState.ExitX;
            var exitY = _gameState.ExitY;

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
                    newEnemyX >= _gameState.MazeWidth || 
                    newEnemyY < 0 || 
                    newEnemyY >= _gameState.MazeHeight ||
                    newEnemyX == exitX && newEnemyY == exitY ||
                    !IsCellEmpty(newEnemyX, newEnemyY) ||
                    _gameState.EnemyLocations.Any(loc => loc.enemyX == newEnemyX && loc.enemyY == newEnemyY)) 
                    continue; 
                
                enemyLocation.enemyX = newEnemyX;
                enemyLocation.enemyY = newEnemyY;
                _gameState.EnemyLocations[i] = enemyLocation;
                break;
            }
        }
    }
    
    private bool CheckEnemyCollision(int x, int y)
    {
        return _gameState.EnemyLocations.Any(enemyLocation => x == enemyLocation.enemyX && y == enemyLocation.enemyY);
    }
    
    private bool CheckEnemyCollision(int x, int y, out (int enemyX, int enemyY) enemy)
    {
        var enemyLocation = _gameState.EnemyLocations.FirstOrDefault(enemyLocation =>
            x == enemyLocation.enemyX && y == enemyLocation.enemyY);
        
        enemy = enemyLocation != default ? (enemyLocation.enemyX, enemyLocation.enemyY) : (0, 0);
        return enemyLocation != default;
    }
}