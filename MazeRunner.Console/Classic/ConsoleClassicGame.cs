﻿using Reveche.MazeRunner.Classic;
using Reveche.MazeRunner.Console.Screens;
using Reveche.MazeRunner.Serializable;
using Reveche.MazeRunner.Sound;
using static System.Console;

namespace Reveche.MazeRunner.Console.Classic;

public partial class ConsoleClassicGame
{
    private readonly ClassicEngine _classicEngine;
    private readonly ClassicState _classicState;
    private readonly GameRenderer _gameRenderer;
    private readonly GameSoundFx _gameSoundFx;
    private readonly OptionsState _optionsState;
    private readonly ScoreList _scoreList = ScoreManager.LoadScores();
    private readonly ScoreManager _scoreManager;
    private bool _levelIsCompleted = true;
    private bool _shouldRedraw = true;

    public ConsoleClassicGame(OptionsState optionsState, ClassicEngine classicEngine, ClassicState classicState)
    {
        _optionsState = optionsState;
        _classicEngine = classicEngine;
        _classicState = classicState;
        _gameSoundFx = new GameSoundFx(_optionsState);
        _gameRenderer = new GameRenderer(optionsState, classicEngine, classicState);
        _scoreManager = new ScoreManager(_scoreList);
    }

    public void Play()
    {
        var levelStartTime = new DateTime();
        var continueGame = _optionsState.IsGameOngoing;
        if (!continueGame) _classicState.MazeDifficulty = _optionsState.MazeDifficulty;

        _levelIsCompleted = true;
        _shouldRedraw = true;
        _classicEngine.AdjustToDifficulty();

        while (true)
        {
            if (_levelIsCompleted)
            {
                CurrentKeys.Clear();
                if (_classicState.CurrentLevel != 1) _classicEngine.CalculateLevelScore(levelStartTime);
                levelStartTime = DateTime.UtcNow;
                _optionsState.IsCurrentlyPlaying = _classicState.CurrentLevel <= _classicState.MaxLevels;
                if (!continueGame) _classicEngine.InitializeNewLevel();
                continueGame = false;
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

            if (!PlayerAction(key, out var didPlayerDie, out var isGamePaused, out var itemPlaced)
                && !isGamePaused) continue;

            if (isGamePaused) break;

            if (didPlayerDie)
            {
                SetCursorPosition(0, CursorTop);
                WriteLine("You died!");
                ReadKey();
            }

            if (_classicEngine.CheckForTreasure(out var treasure))
                PlayerAcquireTreasure(treasure);

            if (_classicState.CurrentLevel != 1 && !itemPlaced) _classicEngine.MoveAllEnemies();

            _shouldRedraw = true;
        }

        MainScreen.StartMenu();
    }

    private void Draw()
    {
        _classicEngine.BombSequence(out var isPlayerDeadByBomb);
        Clear();
        Write(_gameRenderer.DrawCombinedBuffer());

        _classicEngine.CheckPlayerEnemyCollision(out var isPlayerDead);

        if (isPlayerDead || isPlayerDeadByBomb) WriteLine("You died!");
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
        if (_classicState.PlayerName.Length == 0) _classicState.PlayerName = "Anonymous";
        if (_classicState.CurrentLevel > _classicState.MaxLevels &&
            _classicState.PlayerLife > 0 && _optionsState.GameMode == GameMode.Classic)
        {
            WriteLine("Congratulations! You have completed all levels. Press Any Key to exit.");
            _classicState.Score += 100;
            _scoreManager.AddScore(_classicState.PlayerName, _classicState.Score,
                _optionsState.MazeDifficulty, _optionsState.GameMode, _classicState.MaxLevels);
            ScoreManager.SaveScores(_scoreList);
        }
        else
        {
            _scoreManager.AddScore(_classicState.PlayerName, _classicState.Score,
                _optionsState.MazeDifficulty, _optionsState.GameMode, _classicState.CurrentLevel - 1);
            SetCursorPosition(_classicState.MazeWidth / 2, _classicState.MazeHeight / 2);
            WriteLine("Game Over!");
        }

        ReadKey();
        ClassicSaveManager.DeleteClassicSaveFile();
        MainScreen.StartMenu();
    }

    private void PlayerExit()
    {
        WriteLine($"Congratulations! You completed level {_classicState.CurrentLevel}.");
        _classicState.CurrentLevel++;

        if (_classicState.CurrentLevel > _classicState.MaxLevels)
        {
            if (_optionsState.GameMode == GameMode.Classic)
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
            default:
                WriteLine($"You found {treasure.count} {treasure.treasureType}!");
                break;
        }

        ReadKey();
    }
}