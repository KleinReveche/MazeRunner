using System.Text;
using Reveche.MazeRunner.Classic;

namespace Reveche.MazeRunner.Console.Screens;

public static class MainScreen
{
    private const string Maze = """
                                ███╗░░░███╗░█████╗░███████╗███████╗
                                ████╗░████║██╔══██╗╚════██║██╔════╝
                                ██╔████╔██║███████║░░███╔═╝█████╗░░
                                ██║╚██╔╝██║██╔══██║██╔══╝░░██╔══╝░░
                                ██║░╚═╝░██║██║░░██║███████╗███████╗
                                ╚═╝░░░░░╚═╝╚═╝░░╚═╝╚══════╝╚══════╝
                                """;

    private const string Runner = """
                                  ██████╗░██╗░░░██╗███╗░░██╗███╗░░██╗███████╗██████╗░
                                  ██╔══██╗██║░░░██║████╗░██║████╗░██║██╔════╝██╔══██╗
                                  ██████╔╝██║░░░██║██╔██╗██║██╔██╗██║█████╗░░██████╔╝
                                  ██╔══██╗██║░░░██║██║╚████║██║╚████║██╔══╝░░██╔══██╗
                                  ██║░░██║╚██████╔╝██║░╚███║██║░╚███║███████╗██║░░██║
                                  ╚═╝░░╚═╝░╚═════╝░╚═╝░░╚══╝╚═╝░░╚══╝╚══════╝╚═╝░░╚═╝
                                  """;

    public static readonly int CenterX = (System.Console.WindowWidth - Runner.Split('\n')[0].Length) / 2;

    internal static readonly GameState GameState = new();
    private static ClassicState _classicState = new();
    private static OptionsScreen _optionsScreen = new(GameState, _classicState);

    public static void DisplayTitle()
    {
        var buffer = new StringBuilder();
        System.Console.ForegroundColor = ConsoleColor.White;
        foreach (var line in Maze.Split('\n'))
        {
            buffer.Append(' ', CenterX + 8);
            buffer.AppendLine(line);
        }

        foreach (var line in Runner.Split('\n'))
        {
            buffer.Append(' ', CenterX);
            buffer.AppendLine(line);
        }

        // Clear the console and render the entire frame
        System.Console.Clear();
        System.Console.Write(buffer.ToString());
        System.Console.WriteLine();
        System.Console.ResetColor();
    }

    public static void StartMenu()
    {
        if (_classicState.CurrentLevel > _classicState.MaxLevels || _classicState.PlayerLife <= 0)
        {
            _classicState = new ClassicState();
            _optionsScreen = new OptionsScreen(GameState, _classicState);
        }

        Dictionary<string, Action> menuOptions = new()
        {
            { "Continue", () => { } },
            { "Start", () => _optionsScreen.DisplayOptions() },
            {
                "Leaderboard", () =>
                {
                    var leaderboardScreen = new LeaderboardScreen();
                    leaderboardScreen.ShowScreen();
                }
            },
            { "Credits", ShowCreditsScreen },
            { "Quit", () => Environment.Exit(0) }
        };

        if (!GameState.IsCampaignOngoing)
            menuOptions.Remove("Continue");
        
        var selectedIndex = 0;
        var buffer = new StringBuilder();

        while (true)
        {
            buffer.Clear();
            System.Console.Clear();
            DisplayTitle();

            var optionKeys = menuOptions.Keys.ToList();

            for (var i = 0; i < menuOptions.Count; i++)
            {
                var option = optionKeys[i];

                if (GameState.IsCampaignOngoing && option == "Start")
                    option = "New Game";
                
                buffer.Append(' ', CenterX + 20);

                if (i == selectedIndex)
                    buffer.AppendLine("\u001b[93m>> " + option + " <<\u001b[0m");
                else
                    buffer.AppendLine("   " + option);

                if (i == 0)
                    buffer.AppendLine();
            }

            System.Console.WriteLine(buffer);
            var keyInfo = System.Console.ReadKey();

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(menuOptions.Count - 1, selectedIndex + 1);
                    break;

                case ConsoleKey.Enter:
                    var selectedOption = optionKeys[selectedIndex];
                    if (menuOptions.TryGetValue(selectedOption, out var value)) value();
                    return;
            }
        }
    }

    private static void ShowCreditsScreen()
    {
        const string about = """
                             Credits
                             --------
                             Created by Klein Reveche.
                             Version 0.5.0

                             Music
                             --------
                             8-bit Air Fight by moodmode from Pixabay
                             Bit Beats 3 by XtremeFreddy from Pixabay

                             © 2023 Klein Reveche. All rights reserved.
                             """;

        System.Console.Clear();
        DisplayTitle();

        foreach (var line in about.Split('\n'))
        {
            System.Console.SetCursorPosition(CenterX + 8, System.Console.CursorTop);
            System.Console.WriteLine(line);
        }

        System.Console.ReadKey();
        StartMenu();
    }
}