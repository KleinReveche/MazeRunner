namespace Reveche.MazeRunner;

public partial class GameEngine
{
    private void DrawMaze()
    {
        _gameState.Player = _gameState.PlayerLife switch
        {
            2 => _gameState.IsUtf8 ? "😐" : "P",
            1 => _gameState.IsUtf8 ? "🤕" : "P",
            0 => _gameState.IsUtf8 ? "👻" : "X",
            _ => _gameState.IsUtf8 ? "😀" : "P"
        };

        _buffer.Clear();

        for (var y = 0; y < Maze.GetLength(0); y++)
        {
            for (var x = 0; x < Maze.GetLength(1); x++)
            {
                var distanceToPlayer = Math.Abs(x - PlayerX) + Math.Abs(y - PlayerY);
                var isWithinCandleRadius = _gameState.CandleLocations
                    .Any(candleLocation => Math.Abs(x - candleLocation.Item2) <= CandleVisibilityRadius
                                           && Math.Abs(y - candleLocation.Item1) <= CandleVisibilityRadius);
                var isCandle = _gameState.CandleLocations
                    .Any(candleLocation => x == candleLocation.CandleX && y == candleLocation.candleY);
                var isTreasure = _gameState.TreasureLocations
                    .Any(treasureLocation => x == treasureLocation.treasureX && y == treasureLocation.treasureY);
                var isTemporaryVisible = _gameState is {PlayerHasIncreasedVisibility: true };

                if (
                    distanceToPlayer <= PlayerVisibilityRadius +
                    (isTemporaryVisible ? IncreasedVisibilityEffectRadius : 0)
                    || isWithinCandleRadius || _gameState.AtAGlance
                )
                {
                    if (x == PlayerX && y == PlayerY)
                        _buffer.Append(_gameState.Player); // Player
                    else if (x == ExitX && y == ExitY)
                        _buffer.Append(_mazeIcons.Exit); // Exit
                    else if (x == EnemyX && y == EnemyY && _gameState.CurrentLevel != 1)
                        _buffer.Append(_mazeIcons.Enemy); // Enemy
                    else if (isCandle)
                        _buffer.Append(_mazeIcons.Candle); // Candle
                    else if (isTreasure)
                        _buffer.Append(_mazeIcons.Treasure); // Treasure
                    else
                        _buffer.Append(Maze[y, x]);
                }
                else
                {
                    _buffer.Append(_mazeIcons.Darkness);
                }
            }

            _buffer.AppendLine(); // Move to the next row in the buffer
        }
    }
}