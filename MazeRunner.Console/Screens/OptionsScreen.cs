using System.Text;
using Reveche.MazeRunner.Serializable;

namespace Reveche.MazeRunner.Console.Screens;

public class OptionsScreen
{
    private readonly List<string> _difficultyValues = new() { "Easy", "Normal", "Hard", "Insanity", "ASCII Insanity" };
    private readonly GameEngineConsole _gameEngineConsole;
    private readonly List<string> _gameModeValues = new() { "Classic", "Endless" };
    private readonly OptionsState _optionsState;
    private readonly Dictionary<string, string> _options;
    private readonly List<string> _soundValues = new() { "On", "Off" };
    private readonly List<string> _textStyleValues = new() { "Unicode", "ASCII" };

    public OptionsScreen(GameEngineConsole gameEngineConsole, OptionsState optionsState)
    {
        _optionsState = optionsState;
        _gameEngineConsole = gameEngineConsole;
        _options = new Dictionary<string, string>
        {
            { "Play", "" },
            { "Game Mode", _gameModeValues.ElementAt((int)_optionsState.GameMode) },
            { "Difficulty", _difficultyValues.ElementAt((int)_optionsState.MazeDifficulty) },
            { "Text Style", _optionsState.IsUtf8 ? _textStyleValues[0] : _textStyleValues[1] },
            { "Music", _optionsState.IsSoundOn ? _soundValues[0] : _soundValues[1] },
            { "Sound Effects", _optionsState.IsSoundFxOn ? _soundValues[0] : _soundValues[1] },
            { "Back", "" }
        };
    }

    public void DisplayOptions()
    {
        var buffer = new StringBuilder();
        var selectedIndex = 0;

        while (true)
        {
            buffer.Clear();
            System.Console.Clear();

            MainScreen.DisplayTitle();

            for (var i = 0; i < _options.Count; i++)
            {
                var option = _options.ElementAt(i);
                var colon = option.Key is "Back" or "Play" ? "" : ":";
                buffer.Append(' ', MainScreen.CenterX + 18);

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
                    OptionsManager.SaveOptions(_optionsState);
                    break;

                case ConsoleKey.RightArrow:
                    ChangeOptionValue(_options.ElementAt(selectedIndex).Key, _options, 1);
                    OptionsManager.SaveOptions(_optionsState);
                    break;

                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(_options.Count - 1, selectedIndex + 1);
                    break;

                case ConsoleKey.Enter:
                    OptionsManager.SaveOptions(_optionsState);

                    if (selectedIndex == 0)
                        _gameEngineConsole.Play();

                    if (selectedIndex == _options.Count - 1)
                        MainScreen.StartMenu();
                    break;
            }
        }
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void ChangeOptionValue(string optionKey, Dictionary<string, string> options, int change)
    {
        if (!options.TryGetValue(optionKey, out var value)) return;
        switch (optionKey)
        {
            case "Game Mode":
            {
                var currentIndex = _gameModeValues.IndexOf(value);
                var newIndex = (currentIndex + change + _gameModeValues.Count) % _gameModeValues.Count;
                options[optionKey] = _gameModeValues[newIndex];
                _optionsState.GameMode =
                    typeof(GameMode).GetEnumValuesAsUnderlyingType().Cast<GameMode>().ElementAt(newIndex);
                break;
            }
            case "Difficulty":
            {
                var currentIndex = _difficultyValues.IndexOf(value);
                var newIndex = (currentIndex + change + _difficultyValues.Count) % _difficultyValues.Count;
                options[optionKey] = _difficultyValues[newIndex];
                _optionsState.MazeDifficulty =
                    typeof(MazeDifficulty).GetEnumValuesAsUnderlyingType().Cast<MazeDifficulty>().ElementAt(newIndex);

                if (currentIndex == 4 && _optionsState.MazeDifficulty != MazeDifficulty.AsciiInsanity)
                    ChangeTextStyle(0);

                if (_optionsState.MazeDifficulty == MazeDifficulty.AsciiInsanity)
                    ChangeTextStyle(1);
                break;
            }
            case "Music":
            {
                var currentIndex = _soundValues.IndexOf(value);
                var newIndex = (currentIndex + change + _soundValues.Count) % _soundValues.Count;
                options[optionKey] = _soundValues[newIndex];
                _optionsState.IsSoundOn = !_optionsState.IsSoundOn;

                switch (_optionsState.IsSoundOn)
                {
                    case false:
                        MazeRunnerConsole.BackgroundSoundManager.StopBackgroundMusic();
                        break;
                    case true:
                        MazeRunnerConsole.BackgroundSoundManager.RestartBackgroundMusic();
                        break;
                }

                break;
            }
            case "Sound Effects":
            {
                var currentIndex = _soundValues.IndexOf(value);
                var newIndex = (currentIndex + change + _soundValues.Count) % _soundValues.Count;
                options[optionKey] = _soundValues[newIndex];
                _optionsState.IsSoundFxOn = !_optionsState.IsSoundFxOn;

                break;
            }
            case "Text Style":
            {
                var currentIndex = _textStyleValues.IndexOf(value);
                var newIndex = (currentIndex + change + _textStyleValues.Count) % _textStyleValues.Count;
                ChangeTextStyle(_optionsState.MazeDifficulty == MazeDifficulty.AsciiInsanity ? 1 : newIndex);
                break;
            }
        }
    }

    private void ChangeTextStyle(int style) // 0 for unicode, 1 for ascii
    {
        _optionsState.IsUtf8 = style == 0;
        _options["Text Style"] = _optionsState.IsUtf8 ? "Unicode" : "ASCII";
    }
}