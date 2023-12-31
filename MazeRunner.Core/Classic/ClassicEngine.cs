﻿using DotNetXtensions.Cryptography;
using Reveche.MazeRunner.Sound;

namespace Reveche.MazeRunner.Classic;

public partial class ClassicEngine(OptionsState optionsState, ClassicState classicState)
{
    private const int BlastRadius = 1;

    private readonly double _difficultyModifier = classicState.MazeDifficulty switch
    {
        MazeDifficulty.Hard => 1.2,
        MazeDifficulty.Insanity => 1.3,
        MazeDifficulty.AsciiInsanity => 1.4,
        _ => 1
    };

    private readonly GameSoundFx _gameSoundFx = new(optionsState);

    private readonly double _higherLevelModifier = classicState.CurrentLevel switch
    {
        > 10 => 1.5,
        10 => 1.0,
        9 => 0.9,
        8 => 0.5,
        7 => 0.3,
        6 => 0.1,
        _ => 0
    };

    private readonly MazeGen _mazeGen = new(classicState);
    private int PlayerX => classicState.PlayerX;
    private int PlayerY => classicState.PlayerY;
    private char[,] Maze => classicState.Maze;

    public void InitializeNewLevel()
    {
        if (classicState.PlayerHasIncreasedVisibility)
            classicState.PlayerHasIncreasedVisibility = false;
        
        if (classicState.CurrentLevel > 4)
        {
            classicState.PlayerMaxHealth += 10 + classicState.CurrentLevel > 10 ? 20 : 10;
        }
        
        var random = new CryptoRandom();
        if (random.Next(0, 100) > 65)
        {
            classicState.PlayerHealth = classicState.PlayerMaxHealth;
            
            var itemIncrease = random.Next(0, 100) switch
            {
                <= 50 => 1,
                <= 80 => 2,
                <= 95 => 3,
                _ => 4
            };
            classicState.BombCount += itemIncrease;
            classicState.CandleCount += itemIncrease;
        }
        
        classicState.PlayerBurnDuration = 0;
        classicState.DecreasedVisibilityEffectDuration = 0;
        classicState.CandleLocations.Clear();
        classicState.BombLocations.Clear();
        classicState.MazeHeight = _mazeGen.GenerateRandomMazeSize();
        classicState.MazeWidth = _mazeGen.GenerateRandomMazeSize();
        _mazeGen.InitializeMaze();
    }

    public void AdjustToDifficulty()
    {
        classicState.MaxLevels = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 5,
            MazeDifficulty.Insanity => 6,
            _ => 6
        };
        classicState.PlayerVisibilityRadius = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        classicState.CandleVisibilityRadius = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 2,
            MazeDifficulty.Normal => 2,
            _ => 1
        };
        classicState.IncreasedVisibilityEffectRadius = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 3,
            MazeDifficulty.Normal => 2,
            _ => 1
        };

        if (optionsState.IsGameOngoing) return;

        classicState.BombCount = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 4,
            MazeDifficulty.Normal => 3,
            MazeDifficulty.Hard => 2,
            _ => 1
        };
        classicState.CandleCount = classicState.MazeDifficulty switch
        {
            MazeDifficulty.Easy => 6,
            MazeDifficulty.Normal => 5,
            MazeDifficulty.Hard => 4,
            _ => 3
        };
    }

    public bool IsCellEmpty(int x, int y)
    {
        var maze = classicState.Maze;
        if (x >= 0 && x < maze.GetLength(1) && y >= 0 && y < maze.GetLength(0))
            return maze[y, x] == MazeIcons.Empty;

        return false;
    }

    public void CalculateLevelScore(DateTime levelStartTime)
    {
        var mazeArea = classicState.MazeWidth * classicState.MazeHeight;
        var maxTime = mazeArea switch
        {
            <= 50 => 45,
            <= 165 => 90,
            <= 400 => 150,
            _ => 210
        };

        var timeScore = maxTime - (int)(DateTime.UtcNow - levelStartTime).TotalSeconds;

        classicState.Score += timeScore < 0 ? 0 : timeScore;
        classicState.Score += (int)(50 * (_difficultyModifier + _higherLevelModifier)); // For completing the level
    }

    public void AdjustEffects()
    {
        if (classicState.DecreasedVisibilityEffectDuration > 0)
            classicState.DecreasedVisibilityEffectDuration--;

        if (classicState.PlayerBurnDuration <= 0) return;
        var random = new CryptoRandom();
        classicState.PlayerHealth -= random.Next(3, 11);
        classicState.PlayerBurnDuration--;
    }
}