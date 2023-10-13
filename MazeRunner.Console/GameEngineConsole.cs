using System.Text;
using static System.Console;

namespace Reveche.MazeRunner.Console;

public partial class GameEngineConsole
{
    private readonly StringBuilder _combinedBuffer = new();
    private readonly GameEngine _gameEngine;
    private readonly GameState _gameState;
    private readonly StringBuilder _inventoryBuffer = new();
    private readonly StringBuilder _mazeBuffer = new();
    private readonly MazeIcons _mazeIcons = new(GameMenu.GameState);
    private readonly ScoreList _scoreList = ScoreManager.LoadScores();
    private readonly ScoreManager _scoreManager;

    public GameEngineConsole(GameState gameState)
    {
        _gameState = gameState;
        _gameEngine = new GameEngine(_gameState);
        _scoreManager = new ScoreManager(_scoreList);
    }

    private int PlayerX => _gameState.PlayerX;
    private int PlayerY => _gameState.PlayerY;
    private int ExitX => _gameState.ExitX;
    private int ExitY => _gameState.ExitY;
    private string[,] Maze => _gameState.Maze;

    public void Play()
    {
        CursorVisible = false;
        var levelIsCompleted = true;
        var shouldRedraw = true;
        var levelStartTime = new DateTime();
        _gameEngine.AdjustToDifficulty();

        while (true)
        {
            if (levelIsCompleted)
            {
                _gameEngine.CalculateLevelScore(levelStartTime);
                levelStartTime = DateTime.Now;
                _gameState.IsCurrentlyPlaying = _gameState.CurrentLevel <= _gameState.MaxLevels;
                _gameEngine.InitializeNewLevel();
                levelIsCompleted = false;
            }

            if (shouldRedraw)
            {
                _gameEngine.BombSequence(out _);
                DrawMaze();
                DrawInventory();
                DrawCombinedBuffer();
                Clear();
                Write(_combinedBuffer);

                _gameEngine.CheckPlayerEnemyCollision(out var isPlayerDead);

                if (isPlayerDead) WriteLine("You died!");
                if (_gameState.CurrentLevel != 1) _gameEngine.MoveAllEnemies();
                shouldRedraw = false;
            }

            if (_gameState.PlayerLife == 0)
            {
                DisplayGameOver();
                break;
            }

            if (_gameState.PlayerX == _gameState.ExitX && _gameState.PlayerY == _gameState.ExitY)
            {
                WriteLine($"Congratulations! You completed level {_gameState.CurrentLevel}.");
                _gameState.CurrentLevel++;

                if (_gameState.CurrentLevel > _gameState.MaxLevels)
                {
                    if (_gameState.GameMode == GameMode.Classic)
                    {
                        DisplayGameOver();
                        break;
                    }
                    _gameState.Score += 15 * _gameState.CurrentLevel;
                }
                
                shouldRedraw = true;
                levelIsCompleted = true;
            }

            var key = ReadKey().Key;

            if (!_gameEngine.PlayerAction(key, out var didPlayerDie)) continue;

            if (didPlayerDie)
            {
                SetCursorPosition(0, CursorTop);
                WriteLine("You died!");
                ReadKey();
            }

            if (_gameEngine.CheckForTreasure(out var treasure))
            {
                switch (treasure.treasureType)
                {
                    case TreasureType.IncreasedVisibilityEffect:
                        WriteLine("You have increased visibility for the current level.");
                        break;
                    case TreasureType.TemporaryInvulnerabilityEffect:
                        WriteLine(
                            $"You have temporarily invulnerable for {_gameState.PlayerInvincibilityEffectDuration} turns.");
                        break;
                    case TreasureType.AtAGlanceEffect:
                        WriteLine(
                            "Glance at the maze after pressing a key. At your next turn, it'll be hidden again.");
                        break;
                    case TreasureType.Bomb:
                    case TreasureType.Candle:
                    case TreasureType.Life:
                    case TreasureType.None:
                    default:
                        WriteLine($"You found {treasure.count} {treasure.treasureType}!");
                        break;
                }

                ReadKey();
            }

            shouldRedraw = true;
        }
    }


    private void DisplayGameOver()
    {
        // TODO: Create a proper game over screen
        DrawMaze();
        DrawInventory();
        DrawCombinedBuffer();
        Clear();
        Write(_combinedBuffer);

        Write("Enter your name: ");
        _gameState.PlayerName = ReadLine() ?? "Anonymous";
        if (_gameState.CurrentLevel > _gameState.MaxLevels && _gameState is { PlayerLife: > 0, GameMode: GameMode.Classic })
        {
            WriteLine("Congratulations! You have completed all levels. Press Any Key to exit.");
            _gameState.Score += 100;
            _scoreManager.AddScore(_gameState.PlayerName, _gameState.Score, 
                _gameState.MazeDifficulty, _gameState.GameMode, _gameState.MaxLevels);
            ScoreManager.SaveScores(_scoreList);
        }
        else
        {
            _scoreManager.AddScore(_gameState.PlayerName, _gameState.Score, 
                _gameState.MazeDifficulty, _gameState.GameMode, _gameState.CurrentLevel - 1);
            SetCursorPosition(_gameState.MazeWidth / 2, _gameState.MazeHeight / 2);
            WriteLine("Game Over!");
        }

        ReadKey();
        GameMenu.StartMenu();
    }
}