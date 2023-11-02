using System.Text;
using Reveche.MazeRunner.Serializable;
using static System.Console;

namespace Reveche.MazeRunner.Console.Screens;

public class LeaderboardScreen
{
    private static readonly StringBuilder LeaderboardBuffer = new();
    private static int _selectedCategoryIndex;
    private static List<ScoreEntry> _leaderboard = ScoreManager.LoadScores().Scores;
    private int _completionPadding = 2;
    private int _difficultyPadding = 2;
    private int _levelPadding = 2;
    private int _namePadding = 2;
    private int _scorePadding = 2;

    public void ShowScreen()
    {
        _leaderboard = ScoreManager.LoadScores().Scores;
        while (true)
        {
            DisplayCategory();

            if (_leaderboard.Count == 0)
            {
                SetCursorPosition(MainScreen.CenterX, CursorTop);
                WriteLine("No scores yet!");
                ReadKey(true);
                break;
            }

            CalculateColumnWidths();
            SortLeaderboard();
            DrawLeaderboard();
            WriteLine(LeaderboardBuffer);

            var keyInfo = ReadKey(true);

            _selectedCategoryIndex = keyInfo.Key switch
            {
                ConsoleKey.LeftArrow => (_selectedCategoryIndex - 1 + 4) % 4,
                ConsoleKey.RightArrow => (_selectedCategoryIndex + 1) % 4,
                _ => _selectedCategoryIndex
            };

            if (keyInfo.Key is ConsoleKey.Escape or ConsoleKey.Spacebar or ConsoleKey.Enter) break;
        }

        MainScreen.StartMenu();
    }

    private static void DisplayCategory()
    {
        Clear();
        MainScreen.DisplayTitle();

        ForegroundColor = ConsoleColor.White;
        SetCursorPosition(MainScreen.CenterX - 1, CursorTop);

        Write("Leaderboard: ");
        Write(_selectedCategoryIndex == 0 ? "[Score]" : "Score");
        Write(" | ");
        Write(_selectedCategoryIndex == 1 ? "[Difficulty]" : "Difficulty");
        Write(" | ");
        Write(_selectedCategoryIndex == 2 ? "[Game Mode]" : "Game Mode");
        Write(" | ");
        Write(_selectedCategoryIndex == 3 ? "[Level]" : "Level");
        WriteLine("\n");
        ResetColor();
    }

    private void CalculateColumnWidths()
    {
        var maxNameLength = Math.Max(_leaderboard.Max(playerScore => playerScore.Name.Length), 6);
        var maxScoreLength = Math.Max(_leaderboard.Max(playerScore => playerScore.Score.ToString().Length), 5);
        var maxDifficultyLength = Math.Max(_leaderboard.Max(playerScore => playerScore.MazeDifficulty.ToString().Length), 10);
        var maxCompletionLength = Math.Max(_leaderboard.Max(playerScore => playerScore.GameMode.ToString().Length), 9);
        var maxLevelLength = Math.Max(_leaderboard.Max(playerScore => playerScore.CompletedLevels.ToString().Length), 5);

        _namePadding = maxNameLength + 2;
        _scorePadding = maxScoreLength + 2;
        _difficultyPadding = maxDifficultyLength + 2;
        _completionPadding = maxCompletionLength + 2;
        _levelPadding = maxLevelLength + 2;
    }

    private static void SortLeaderboard()
    {
        switch (_selectedCategoryIndex)
        {
            case 0: // Sort by Score
                _leaderboard.Sort((playerScoreA, playerScoreB) => playerScoreB.Score.CompareTo(playerScoreA.Score));
                break;
            case 1: // Sort by Difficulty
                _leaderboard.Sort((playerScoreA, playerScoreB) =>
                {
                    var difficulty = playerScoreB.MazeDifficulty.CompareTo(playerScoreA.MazeDifficulty);
                    return difficulty != 0 ? difficulty : playerScoreB.Score.CompareTo(playerScoreA.Score);
                });
                break;
            case 2: // Sort by GameMode
                _leaderboard.Sort((playerScoreA, playerScoreB) =>
                {
                    var gameMode = playerScoreB.GameMode.CompareTo(playerScoreA.GameMode);
                    return gameMode != 0 ? gameMode : playerScoreB.Score.CompareTo(playerScoreA.Score);
                });
                break;
            case 3: // Sort by Level
                _leaderboard.Sort((playerScoreA, playerScoreB) =>
                {
                    var level = playerScoreB.CompletedLevels.CompareTo(playerScoreA.CompletedLevels);
                    return level != 0 ? level : playerScoreB.Score.CompareTo(playerScoreA.Score);
                });
                break;
        }
    }

    private void DrawLeaderboard()
    {
        var padding = (int)(MainScreen.CenterX * 1.75) / 2;
        LeaderboardBuffer.Clear();

        var rowFormat = new string(' ', padding) + "│ {0,-" + _namePadding + "} │ {1,-"
                        + _scorePadding + "} │ {2,-"
                        + _difficultyPadding + "} │ {3,-"
                        + _completionPadding + "} │ {4,-"
                        + _levelPadding + "} │";

        var lineFormat = new string(' ', padding) + "{0}" + new string('─', _namePadding + 2) + "{1}"
                         + new string('─', _scorePadding + 2) + "{2}"
                         + new string('─', _difficultyPadding + 2) + "{3}"
                         + new string('─', _completionPadding + 2) + "{4}"
                         + new string('─', _levelPadding + 2) + "{5}";

        LeaderboardBuffer.AppendLine(string.Format(lineFormat, "┌", "┬", "┬", "┬", "┬", "┐"));
        LeaderboardBuffer.AppendLine(string.Format(rowFormat, "Player", "Score", "Difficulty", "Game Mode", "Level"));
        LeaderboardBuffer.AppendLine(string.Format(lineFormat, "├", "┼", "┼", "┼", "┼", "┤"));

        foreach (var score in _leaderboard)
            LeaderboardBuffer.AppendLine(string.Format(rowFormat, score.Name, score.Score, score.MazeDifficulty,
                score.GameMode, score.CompletedLevels));

        LeaderboardBuffer.AppendLine(string.Format(lineFormat, "└", "┴", "┴", "┴", "┴", "┘"));
    }
}