using System.Text;
using static System.Console;

namespace Reveche.MazeRunner.Console;

public class LeaderboardScreen
{
    private static readonly StringBuilder LeaderboardBuffer = new();
    private static int _selectedCategoryIndex;
    private static List<ScoreEntry> _leaderboard = ScoreManager.LoadScores().Scores;
    private int _completionPadding = 2;
    private int _difficultyPadding = 2;
    private int _namePadding = 2;
    private int _scorePadding = 2;
    private int _levelPadding = 2;

    public void ShowScreen()
    {
        _leaderboard = ScoreManager.LoadScores().Scores;
        while (true)
        {
            DisplayCategory();

            if (_leaderboard.Count == 0)
            {
                SetCursorPosition(GameMenu.CenterX, CursorTop);
                WriteLine("No scores yet!");
                ReadKey(true);
                break;
            }

            CalculateColumnWidths();
            SortLeaderboard();
            DrawLeaderboard();

            foreach (var line in LeaderboardBuffer.ToString().Split("\r\n"))
            {
                SetCursorPosition(GameMenu.CenterX - 5, CursorTop);
                WriteLine(line);
            }

            var keyInfo = ReadKey(true);

            _selectedCategoryIndex = keyInfo.Key switch
            {
                ConsoleKey.LeftArrow => (_selectedCategoryIndex - 1 + 4) % 4,
                ConsoleKey.RightArrow => (_selectedCategoryIndex + 1) % 4,
                _ => _selectedCategoryIndex
            };

            if (keyInfo.Key is ConsoleKey.Escape or ConsoleKey.Spacebar or ConsoleKey.Enter) break;
        }

        GameMenu.StartMenu();
    }

    private static void DisplayCategory()
    {
        Clear();
        GameMenu.DisplayTitle();

        ForegroundColor = ConsoleColor.White;
        SetCursorPosition(GameMenu.CenterX + 20, CursorTop);
        WriteLine("Leaderboard: \n");

        SetCursorPosition(GameMenu.CenterX + 10, CursorTop);
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
        var maxNameLength = Math.Max(_leaderboard.Max(p => p.Name.Length), 6);
        var maxScoreLength = Math.Max(_leaderboard.Max(p => p.Score.ToString().Length), 5);
        var maxDifficultyLength = Math.Max(_leaderboard.Max(p => p.MazeDifficulty.ToString().Length), 10);
        var maxCompletionLength = Math.Max(_leaderboard.Max(p => p.IsEndless.ToString().Length), 9);
        var maxLevelLength = Math.Max(_leaderboard.Max(p => p.CompletedLevels.ToString().Length), 5);

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
                _leaderboard.Sort((a, b) => b.Score.CompareTo(a.Score));
                break;
            case 1:
                _leaderboard.Sort((a, b) =>
                {
                    var difficulty = b.MazeDifficulty.CompareTo(a.MazeDifficulty);
                    return difficulty != 0 ? difficulty : b.Score.CompareTo(a.Score);
                });
                break;
            case 2: // Sort by Completion
                _leaderboard.Sort((a, b) =>
                {
                    var completed = b.IsEndless.CompareTo(a.IsEndless);
                    return completed != 0 ? completed : b.Score.CompareTo(a.Score);
                });
                break;
            case 3: // Sort by Level
                _leaderboard.Sort((a, b) =>
                {
                    var level = b.CompletedLevels.CompareTo(a.CompletedLevels);
                    return level != 0 ? level : b.Score.CompareTo(a.Score);
                });
                break;
        }
    }

    private void DrawLeaderboard()
    {
        LeaderboardBuffer.Clear();
        var rowFormat = "│ {0,-" + _namePadding + "} │ {1,-"
                        + _scorePadding + "} │ {2,-"
                        + _difficultyPadding + "} │ {3,-"
                        + _completionPadding + "} │ {4,-"
                        + _levelPadding + "} │";
        
        var lineFormat = "{0}" +  new string('─', _namePadding + 2) + "{1}"
                         + new string('─', _scorePadding + 2) + "{2}"
                         + new string('─', _difficultyPadding + 2) + "{3}"
                         + new string('─', _completionPadding + 2) + "{4}"
                         + new string('─', _levelPadding + 2) + "{5}";

        LeaderboardBuffer.AppendLine(string.Format(lineFormat, "┌", "┬", "┬", "┬", "┬", "┐"));
        LeaderboardBuffer.AppendLine(string.Format(rowFormat, "Player", "Score", "Difficulty", "Game Mode", "Level"));
        LeaderboardBuffer.AppendLine(string.Format(lineFormat, "├", "┼", "┼", "┼", "┼", "┤"));
        
        foreach (var score in _leaderboard)
        {
            var completed = score.IsEndless ? "Endless" : "Campaign";
            LeaderboardBuffer.AppendLine(string.Format(rowFormat, score.Name, score.Score, score.MazeDifficulty,
                completed, score.CompletedLevels));
        }
        
        LeaderboardBuffer.AppendLine(string.Format(lineFormat, "└", "┴", "┴", "┴", "┴", "┘"));
    }
}