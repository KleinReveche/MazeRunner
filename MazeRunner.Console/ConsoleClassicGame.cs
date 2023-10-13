using Reveche.MazeRunner.Console.Screens;
using static System.Console;

namespace Reveche.MazeRunner.Console;

public class ConsoleClassicGame 
{
    private readonly GameState _gameState;
    private readonly GameEngine _gameEngine;
    private readonly GameRenderer _gameRenderer;
    private readonly ScoreList _scoreList = ScoreManager.LoadScores();
    private readonly ScoreManager _scoreManager;
    private bool _levelIsCompleted = true;
    private bool _shouldRedraw = true;

    public ConsoleClassicGame(GameEngine gameEngine, GameState gameState)
    {
        _gameEngine = gameEngine;
        _gameState = gameState;
        _gameRenderer = new GameRenderer(gameEngine, gameState);
        _scoreManager = new ScoreManager(_scoreList);
    }
    
    public void Play()
    {
        var levelStartTime = new DateTime();
        
        _levelIsCompleted = true;
        _shouldRedraw = true;
        _gameEngine.AdjustToDifficulty();

        while (true)
        {
            if (_levelIsCompleted)
            {
                _gameEngine.CalculateLevelScore(levelStartTime);
                levelStartTime = DateTime.Now;
                _gameState.IsCurrentlyPlaying = _gameState.CurrentLevel <= _gameState.MaxLevels;
                _gameEngine.InitializeNewLevel();
                _levelIsCompleted = false;
            }
            
            if (_shouldRedraw) Draw();
            
            if (_gameState.PlayerLife == 0)
            {
                DisplayGameDone();
                break;
            }
            
            if (_gameState.PlayerX == _gameState.ExitX && _gameState.PlayerY == _gameState.ExitY)
                PlayerExit();
            
            var key = ReadKey().Key;

            if (!_gameEngine.PlayerAction(key, out var didPlayerDie)) continue;

            if (didPlayerDie)
            {
                SetCursorPosition(0, CursorTop);
                WriteLine("You died!");
                ReadKey();
            }
            
            if (_gameEngine.CheckForTreasure(out var treasure))
                PlayerAcquireTreasure(treasure);
            
            _shouldRedraw = true;
        }
        
        MainScreen.StartMenu();
    }

    private void Draw()
    {
        _gameEngine.BombSequence(out _);
        Clear();
        Write(_gameRenderer.DrawCombinedBuffer());

        _gameEngine.CheckPlayerEnemyCollision(out var isPlayerDead);

        if (isPlayerDead) WriteLine("You died!");
        if (_gameState.CurrentLevel != 1) _gameEngine.MoveAllEnemies();
        _shouldRedraw = false;
    }

    private void DisplayGameDone()
    {
        // TODO: Create a proper game over screen
        var combinedBuffer = _gameRenderer.DrawCombinedBuffer();
        Clear();
        Write(combinedBuffer);

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
        MainScreen.StartMenu();
    }

    private void PlayerExit()
    {
        WriteLine($"Congratulations! You completed level {_gameState.CurrentLevel}.");
        _gameState.CurrentLevel++;

        if (_gameState.CurrentLevel > _gameState.MaxLevels)
        {
            if (_gameState.GameMode == GameMode.Classic)
            {
                DisplayGameDone();
                return;
            }
            _gameState.Score += 15 * _gameState.CurrentLevel;
        }
                
        _shouldRedraw = true;
        _levelIsCompleted = true;
    }

    private void PlayerAcquireTreasure((int treasureY, int treasureX, TreasureType treasureType, int count) treasure)
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
}