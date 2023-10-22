using Reveche.MazeRunner.Classic;
using Reveche.MazeRunner.Console.Screens;
using static System.Console;

namespace Reveche.MazeRunner.Console.Classic;

public class ConsoleClassicGame
{
    private readonly ClassicEngine _classicEngine;
    private readonly ClassicState _classicState;
    private readonly GameRenderer _gameRenderer;
    private readonly GameState _gameState;
    private readonly ScoreList _scoreList = ScoreManager.LoadScores();
    private readonly ScoreManager _scoreManager;
    private bool _levelIsCompleted = true;
    private bool _shouldRedraw = true;

    public ConsoleClassicGame(GameState gameState, ClassicEngine classicEngine, ClassicState classicState)
    {
        _gameState = gameState;
        _classicEngine = classicEngine;
        _classicState = classicState;
        _gameRenderer = new GameRenderer(gameState, classicEngine, classicState);
        _scoreManager = new ScoreManager(_scoreList);
    }

    public void Play()
    {
        var levelStartTime = new DateTime();

        _levelIsCompleted = true;
        _shouldRedraw = true;
        _classicEngine.AdjustToDifficulty();

        while (true)
        {
            if (_levelIsCompleted)
            {
                _classicEngine.CalculateLevelScore(levelStartTime);
                levelStartTime = DateTime.Now;
                _gameState.IsCurrentlyPlaying = _classicState.CurrentLevel <= _classicState.MaxLevels;
                _classicEngine.InitializeNewLevel();
                _levelIsCompleted = false;
            }

            if (_shouldRedraw) Draw();

            if (_classicState.PlayerLife == 0)
            {
                DisplayGameDone();
                break;
            }

            if (_classicState.PlayerX == _classicState.ExitX && _classicState.PlayerY == _classicState.ExitY)
                PlayerExit();

            var key = ReadKey().Key;

            if (!_classicEngine.PlayerAction(key, out var didPlayerDie)) continue;

            if (didPlayerDie)
            {
                SetCursorPosition(0, CursorTop);
                WriteLine("You died!");
                ReadKey();
            }

            if (_classicEngine.CheckForTreasure(out var treasure))
                PlayerAcquireTreasure(treasure);

            _shouldRedraw = true;
        }

        MainScreen.StartMenu();
    }

    private void Draw()
    {
        _classicEngine.BombSequence(out _);
        Clear();
        Write(_gameRenderer.DrawCombinedBuffer());

        _classicEngine.CheckPlayerEnemyCollision(out var isPlayerDead);

        if (isPlayerDead) WriteLine("You died!");
        if (_classicState.CurrentLevel != 1) _classicEngine.MoveAllEnemies();
        _shouldRedraw = false;
    }

    private void DisplayGameDone()
    {
        // TODO: Create a proper game over screen
        var combinedBuffer = _gameRenderer.DrawCombinedBuffer();
        Clear();
        Write(combinedBuffer);

        Write("Enter your name: ");
        _classicState.PlayerName = ReadLine() ?? "Anonymous";
        if (_classicState.CurrentLevel > _classicState.MaxLevels &&
            _classicState.PlayerLife > 0  && _gameState.GameMode == GameMode.Classic)
        {
            WriteLine("Congratulations! You have completed all levels. Press Any Key to exit.");
            _classicState.Score += 100;
            _scoreManager.AddScore(_classicState.PlayerName, _classicState.Score,
                _gameState.MazeDifficulty, _gameState.GameMode, _classicState.MaxLevels);
            ScoreManager.SaveScores(_scoreList);
        }
        else
        {
            _scoreManager.AddScore(_classicState.PlayerName, _classicState.Score,
                _gameState.MazeDifficulty, _gameState.GameMode, _classicState.CurrentLevel - 1);
            SetCursorPosition(_classicState.MazeWidth / 2, _classicState.MazeHeight / 2);
            WriteLine("Game Over!");
        }

        ReadKey();
        MainScreen.StartMenu();
    }

    private void PlayerExit()
    {
        WriteLine($"Congratulations! You completed level {_classicState.CurrentLevel}.");
        _classicState.CurrentLevel++;

        if (_classicState.CurrentLevel > _classicState.MaxLevels)
        {
            if (_gameState.GameMode == GameMode.Classic)
            {
                DisplayGameDone();
                return;
            }

            _classicState.Score += 15 * _classicState.CurrentLevel;
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
                    $"You have temporarily invulnerable for {_classicState.PlayerInvincibilityEffectDuration} turns.");
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