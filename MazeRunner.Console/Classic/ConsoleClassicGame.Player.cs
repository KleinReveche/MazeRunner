using Reveche.MazeRunner.Classic;
using Reveche.MazeRunner.Sound;

namespace Reveche.MazeRunner.Console.Classic;

public partial class ConsoleClassicGame
{
#if DEBUG
    private bool _debugIsFullyVisible;
#endif
    private int PlayerX => _classicState.PlayerX;
    private int PlayerY => _classicState.PlayerY;
    private int LastPlayerX { get; set; }
    private int LastPlayerY { get; set; }
    private char[,] Maze => _classicState.Maze;
    private List<ConsoleKey> CurrentKeys { get; } = [];

    private bool PlayerAction(ConsoleKey key, out bool isPlayerDead, out bool isGamePaused, out bool isItemPlaced)
    {
        isGamePaused = false;
        isPlayerDead = false;
        isItemPlaced = false;
        var placeBomb = false;
        var placeCandle = false;
        var newPlayerX = _classicState.PlayerX;
        var newPlayerY = _classicState.PlayerY;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (key)
        {
            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                newPlayerY--;
                break;
            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                newPlayerY++;
                break;
            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                newPlayerX--;
                break;
            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                newPlayerX++;
                break;
            case ConsoleKey.B:
                placeBomb = true;
                isItemPlaced = true;
                break;
            case ConsoleKey.C:
                placeCandle = true;
                isItemPlaced = true;
                break;
            case ConsoleKey.Escape:
                ClassicSaveManager.SaveCurrentSave(_classicState);
                isGamePaused = true;
                break;
            // ReSharper disable once RedundantEmptySwitchSection
            default:
                break;
        }

        CurrentKeys.Add(key);

        if (placeCandle)
        {
            if (_classicState.CandleCount == 0 ||
                _classicState.CandleLocations.Any(candleLocation =>
                    candleLocation.Item2 == LastPlayerX && candleLocation.Item1 == LastPlayerY) ||
                (LastPlayerX == 0 && LastPlayerY == 0)) return false;
            _gameSoundFx.PlayFx(SoundFx.PlaceItem);
            _classicState.CandleCount--;
            _classicState.CandleLocations.Add((LastPlayerY, LastPlayerX));
            return true;
        }

        var isBombAtNewPosition = _classicState.BombLocations.Any(bombLocation =>
            bombLocation.bombX == LastPlayerX && bombLocation.bombY == LastPlayerY);

        if (placeBomb && !isBombAtNewPosition && _classicState is { BombCount: > 0 })
        {
            _classicState.BombCount--;
            _classicState.BombLocations.Add((LastPlayerY, LastPlayerX, 2));
            _gameSoundFx.PlayFx(SoundFx.BombPlace);
            return true;
        }

        if (!_classicEngine.IsCellEmpty(newPlayerX, newPlayerY)) return false;
        LastPlayerX = _classicState.PlayerX;
        LastPlayerY = _classicState.PlayerY;
        // Clear previous player position
        Maze[PlayerY, PlayerX] = MazeIcons.Empty;
        // Set new player position
        _classicState.PlayerX = newPlayerX;
        _classicState.PlayerY = newPlayerY;

        if (_classicState is { IsPlayerInvulnerable: true, PlayerInvincibilityEffectDuration: > 0 })
            _classicState.PlayerInvincibilityEffectDuration--;
        else
            _classicState.IsPlayerInvulnerable = false;

        if (_classicState.AtAGlance)
            _classicState.AtAGlance = false;

#if DEBUG
        if (_debugIsFullyVisible)
            _classicState.AtAGlance = true;
#endif

        CheckSecretCombination();
        return true; // Player has moved, indicate that screen should be redrawn
    }

    private void CheckSecretCombination()
    {
        var fullyVisibleCombination = new[]
        {
            ConsoleKey.UpArrow,
            ConsoleKey.UpArrow,
            ConsoleKey.LeftArrow,
            ConsoleKey.LeftArrow,
            ConsoleKey.DownArrow,
            ConsoleKey.V
        };

        var partiallyVisibleCombination = new[]
        {
            ConsoleKey.DownArrow,
            ConsoleKey.DownArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.UpArrow
        };

        CurrentKeys.RemoveAll(key => key == ConsoleKey.None);

        // Check if the player has entered the exact secret combination
        if (CurrentKeys.SequenceEqual(fullyVisibleCombination) &&
            _classicState.MazeDifficulty is MazeDifficulty.Easy or MazeDifficulty.Normal)
        {
            _classicState.AtAGlance = true;
            CurrentKeys.Clear();
        }

        if (CurrentKeys.SequenceEqual(partiallyVisibleCombination) &&
            _classicState.MazeDifficulty is MazeDifficulty.Easy or MazeDifficulty.Normal)
        {
            _classicState.PlayerHasIncreasedVisibility = true;
            CurrentKeys.Clear();
        }

#if DEBUG
        var debugInvulnerableCombination = new[]
        {
            ConsoleKey.I,
            ConsoleKey.N,
            ConsoleKey.V
        };

        var debugSkipLevelCombination = new[]
        {
            ConsoleKey.S,
            ConsoleKey.K,
            ConsoleKey.I,
            ConsoleKey.P
        };

        var debugFullyVisibleCombination = new[]
        {
            ConsoleKey.V,
            ConsoleKey.I,
            ConsoleKey.S
        };

        var debugInfinityCombination = new[]
        {
            ConsoleKey.I,
            ConsoleKey.N,
            ConsoleKey.F
        };

        if (CurrentKeys.SequenceEqual(debugInfinityCombination))
        {
            _classicState.BombCount = 999;
            _classicState.CandleCount = 999;
            CurrentKeys.Clear();
        }

        if (CurrentKeys.SequenceEqual(debugFullyVisibleCombination))
        {
            _classicState.AtAGlance = true;
            _debugIsFullyVisible = !_debugIsFullyVisible;
            CurrentKeys.Clear();
        }

        if (CurrentKeys.SequenceEqual(debugSkipLevelCombination))
        {
            _classicState.CurrentLevel++;
            _levelIsCompleted = true;
            CurrentKeys.Clear();
        }

        if (CurrentKeys.SequenceEqual(debugInvulnerableCombination))
        {
            _classicState.IsPlayerInvulnerable = true;
            _classicState.PlayerInvincibilityEffectDuration = 999;
            CurrentKeys.Clear();
        }
#endif

        if (CurrentKeys.Count == 10 || CurrentKeys.Contains(ConsoleKey.Delete)) CurrentKeys.Clear();
    }
}