using System.Text;

namespace Reveche.MazeRunner.Console;

public class OptionMenu
{
    private readonly List<string> _difficultyValues = new() { "Easy", "Normal", "Hard", "Insanity", "ASCII Insanity" };
    private readonly GameEngineConsole _gameEngineConsole;
    private readonly GameState _gameState;
    private readonly Dictionary<string, string> _options;
    private readonly List<string> _soundValues = new() { "On", "Off" };
    private readonly List<string> _textStyleValues = new() { "Unicode", "ASCII" };

    public OptionMenu(GameState gameState)
    {
        _gameState = gameState;
        _gameEngineConsole = new GameEngineConsole(_gameState);
        _options = new Dictionary<string, string>
        {
            { "Play", "" },
            { "Difficulty", "Normal" },
            { "Text Style", _gameState.IsUtf8 ? "Unicode" : "ASCII" },
            { "Sound", _gameState.IsSoundOn ? "On" : "Off" },
            { "Back", "" }
        };
    }

    public void DisplayOptions()
    {
        var buffer = new StringBuilder();

        // This ensures that sound will not be an option on non-Windows platforms.
        // As the System.Media.SoundPlayer class is only available on Windows.
        if (!OperatingSystem.IsWindows())
            _options.Remove("Sound");

        var selectedIndex = 0;

        while (true)
        {
            buffer.Clear();
            System.Console.Clear();

            GameMenu.DisplayTitle();

            for (var i = 0; i < _options.Count; i++)
            {
                var option = _options.ElementAt(i);
                var colon = option.Key is "Back" or "Play" ? "" : ":";
                buffer.Append(' ', GameMenu.CenterX + 18);

                if (i == selectedIndex)
                    buffer.AppendLine("\u001b[93m>> " + option.Key + $"{colon} " + option.Value + " <<\u001b[0m");
                else
                    buffer.AppendLine("   " + option.Key + $"{colon} " + option.Value);

                if (i == 0)
                    buffer.AppendLine();
            }

            System.Console.WriteLine(buffer);
            var keyInfo = System.Console.ReadKey();

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    ChangeOptionValue(_options.ElementAt(selectedIndex).Key, _options, -1);
                    break;

                case ConsoleKey.RightArrow:
                    ChangeOptionValue(_options.ElementAt(selectedIndex).Key, _options, 1);
                    break;

                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(_options.Count - 1, selectedIndex + 1);
                    break;

                case ConsoleKey.Enter:
                    if (selectedIndex == 0)
                        _gameEngineConsole.Play();

                    if (selectedIndex == _options.Count - 1)
                        GameMenu.StartMenu();
                    return;
            }
        }
    }

    private void ChangeOptionValue(string optionKey, IDictionary<string, string> options, int change)
    {
        if (!options.TryGetValue(optionKey, out var value)) return;
        switch (optionKey)
        {
            case "Difficulty":
            {
                var currentIndex = _difficultyValues.IndexOf(value);
                var newIndex = (currentIndex + change + _difficultyValues.Count) % _difficultyValues.Count;
                options[optionKey] = _difficultyValues[newIndex];
                _gameState.MazeDifficulty =
                    typeof(MazeDifficulty).GetEnumValues().Cast<MazeDifficulty>().ElementAt(newIndex);

                if (currentIndex == 4 && _gameState.MazeDifficulty != MazeDifficulty.AsciiInsanity)
                    ChangeTextStyle(0);

                if (_gameState.MazeDifficulty == MazeDifficulty.AsciiInsanity)
                    ChangeTextStyle(1);
                break;
            }
            case "Sound":
            {
                var currentIndex = _soundValues.IndexOf(value);
                var newIndex = (currentIndex + change + _soundValues.Count) % _soundValues.Count;
                options[optionKey] = _soundValues[newIndex];
                _gameState.IsSoundOn = !_gameState.IsSoundOn;

                switch (_gameState.IsSoundOn)
                {
                    case false:
                        MazeRunnerConsole.StopMusic();
                        break;
                    case true:
                        MazeRunnerConsole.RestartMusic();
                        break;
                }

                break;
            }
            case "Text Style":
            {
                var currentIndex = _textStyleValues.IndexOf(value);
                var newIndex = (currentIndex + change + _textStyleValues.Count) % _textStyleValues.Count;
                ChangeTextStyle(_gameState.MazeDifficulty == MazeDifficulty.AsciiInsanity ? 1 : newIndex);
                break;
            }
        }
    }

    private void ChangeTextStyle(int style) // 0 for unicode, 1 for ascii
    {
        _gameState.IsUtf8 = style == 0;
        _options["Text Style"] = _gameState.IsUtf8 ? "Unicode" : "ASCII";
    }
}