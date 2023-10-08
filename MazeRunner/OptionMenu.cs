using System.Text;

namespace Reveche.MazeRunner;

public class OptionMenu
{
    private readonly GameState _gameState;

    public OptionMenu(GameState gameState)
    {
        _gameState = gameState;
    }

    public void DisplayOptions()
    {
        var buffer = new StringBuilder();
        var options = new Dictionary<string, string>
        {
            { "Difficulty", "Normal" },
            { "Sound", _gameState.IsSoundOn ? "On" : "Off" },
            { "Text Style", _gameState.IsUtf8 ? "UTF-8" : "ASCII" },
            { "Back", "" }
        };

        // This ensures that sound will not be an option on non-Windows platforms.
        // As the System.Media.SoundPlayer class is only available on Windows.
        if (!OperatingSystem.IsWindows())
            options.Remove("Sound");

        var selectedIndex = 0;

        while (true)
        {
            buffer.Clear();
            Console.Clear();

            GameMenu.DisplayTitle();

            for (var i = 0; i < options.Count; i++)
            {
                var option = options.ElementAt(i);
                var colon = option.Key == "Back" ? "" : ":";
                buffer.Append(' ', GameMenu.CenterX + 18);

                if (i == selectedIndex)
                    buffer.AppendLine("\u001b[93m>> " + option.Key + $"{colon} " + option.Value + " <<\u001b[0m");
                else
                    buffer.AppendLine("   " + option.Key + $"{colon} " + option.Value);
            }

            Console.WriteLine(buffer);
            var keyInfo = Console.ReadKey();

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    ChangeOptionValue(options.ElementAt(selectedIndex).Key, options, -1);
                    break;

                case ConsoleKey.RightArrow:
                    ChangeOptionValue(options.ElementAt(selectedIndex).Key, options, 1);
                    break;

                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(options.Count - 1, selectedIndex + 1);
                    break;

                case ConsoleKey.Enter:
                    if (selectedIndex == options.Count - 1)
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
                var difficultyValues = new List<string> { "Easy", "Normal", "Hard", "Insanity" };
                var currentIndex = difficultyValues.IndexOf(value);
                var newIndex = (currentIndex + change + difficultyValues.Count) % difficultyValues.Count;
                options[optionKey] = difficultyValues[newIndex];
                break;
            }
            case "Sound":
            {
                var soundValues = new List<string> { "On", "Off" };
                var currentIndex = soundValues.IndexOf(value);
                var newIndex = (currentIndex + change + soundValues.Count) % soundValues.Count;
                options[optionKey] = soundValues[newIndex];
                _gameState.IsSoundOn = !_gameState.IsSoundOn;
                break;
            }
            case "Text Style":
            {
                var textStyleValues = new List<string> { "UTF-8", "ASCII" };
                var currentIndex = textStyleValues.IndexOf(value);
                var newIndex = (currentIndex + change + textStyleValues.Count) % textStyleValues.Count;
                options[optionKey] = textStyleValues[newIndex];
                _gameState.IsUtf8 = !_gameState.IsUtf8;
                break;
            }
        }
    }
}