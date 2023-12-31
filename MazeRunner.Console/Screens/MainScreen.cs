﻿using System.Reflection;
using System.Text;
using Reveche.MazeRunner.Classic;
using Reveche.MazeRunner.Serializable;

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

    private const string HowToPlay = """
                                     How to Play
                                     -----------
                                     Use the arrow keys to move the runner.
                                     Go to the door to advance to the next level.
                                     Avoid the enemies.

                                     Controls:
                                     - Arrow Keys or WASD Keys to move player.
                                     - Place a bomb with the B Key. It will detonate after 2 turns.
                                     - Place a candle with the C Key. It has a 3x3 range and will last for the current level.
                                     - Note: The bomb and candle will be placed on the last position of the player.

                                     Game Modes:
                                     - Classic: Play through 6 levels of increasing difficulty.
                                     - Endless: Play through endless levels of increasing difficulty.

                                     Difficulty:
                                     - Easy: 4 levels on Classic, High visibility, more starting items.
                                     - Normal: 5 levels on Classic, Normal visibility, normal starting items.
                                     - Hard: 5 levels on Classic, Reduced visibility, less starting items.
                                     - Insanity: 6 levels on Classic, Almost no visibility, least starting items.
                                     - ASCII Insanity: Same with Insanity but a forced ASCII Look.
                                     """;

    private static readonly string About = $"""
                                            Credits
                                            --------
                                            Created by Klein Reveche.
                                            Version {GetVersion()}

                                            Music
                                            --------
                                            8-bit Air Fight by moodmode from Pixabay
                                            Bit Beats 3 by XtremeFreddy from Pixabay
                                            Other sound effects from Pixabay

                                            © 2023 Klein Reveche. All rights reserved.
                                            """;

    private static readonly string Separator = OperatingSystem.IsWindows() ? "\r\n" : "\n";
    public static readonly int CenterX = (System.Console.WindowWidth - Runner.Split(Separator)[0].Length) / 2;

    internal static OptionsState OptionsState = OptionsManager.LoadOptions();
    private static ClassicState _classicState = new();
    private static GameEngineConsole _gameEngineConsole = new(OptionsState, _classicState);
    private static OptionsScreen _optionsScreen = new(_gameEngineConsole, OptionsState);

    public static void DisplayTitle()
    {
        var buffer = new StringBuilder();
        System.Console.ForegroundColor = ConsoleColor.White;
        foreach (var line in Maze.Split(Separator))
        {
            buffer.Append(' ', CenterX + 8);
            buffer.AppendLine(line);
        }

        foreach (var line in Runner.Split(Separator))
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
            _optionsScreen = new OptionsScreen(new GameEngineConsole(OptionsState, _classicState), OptionsState);
        }

        if (ClassicSaveManager.ClassicSaveFileExists())
        {
            _classicState = ClassicSaveManager.LoadCurrentSave();
            _gameEngineConsole = new GameEngineConsole(OptionsState, _classicState);
            OptionsState.IsGameOngoing = true;
        }

        if (!ClassicSaveManager.ClassicSaveFileExists())
        {
            _optionsScreen = new OptionsScreen(new GameEngineConsole(OptionsState, _classicState), OptionsState);
            _gameEngineConsole = new GameEngineConsole(OptionsState, _classicState);
            OptionsState.IsGameOngoing = false;
        }

        var selectedIndex = 0;
        var buffer = new StringBuilder();

        while (true)
        {
            var menuOptions = new List<string>
            {
                "Continue",
                "Start",
                "Leaderboard",
                "How to Play",
                "Credits",
                "Quit"
            };

            if (!OptionsState.IsGameOngoing)
                menuOptions.Remove("Continue");
            
            buffer.Clear();
            System.Console.Clear();
            DisplayTitle();

            for (var i = 0; i < menuOptions.Count; i++)
            {
                var option = menuOptions[i];

                if (OptionsState.IsGameOngoing && option == "Start")
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

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(menuOptions.Count - 1, selectedIndex + 1);
                    break;

                case ConsoleKey.Enter:
                    var selectedOption = menuOptions[selectedIndex];
                    ShowScreen(selectedOption, out var quit);
                    if (quit) return;
                    continue;
                default:
                    continue;
            }
        }
    }

    private static void ShowScreen(string screenName, out bool quit)
    {
        quit = false;
        switch (screenName)
        {
            case "Continue":
                _gameEngineConsole.Play();
                break;
            case "Start":
                OptionsState = OptionsManager.LoadOptions();
                _classicState = new ClassicState();
                _optionsScreen =
                    new OptionsScreen(new GameEngineConsole(OptionsState, _classicState), OptionsState);
                _optionsScreen.DisplayOptions();
                break;
            case "Leaderboard":
                var leaderboardScreen = new LeaderboardScreen();
                leaderboardScreen.ShowScreen();
                break;
            case "How to Play":
                ShowTextScreen(HowToPlay, -12);
                break;
            case "Credits":
                ShowTextScreen(About, 8);
                break;
            case "Quit":
                quit = true;
                return;
            default:
                throw new ArgumentException("Invalid screen name");
        }
    }

    private static void ShowTextScreen(string text, int padding = 0)
    {
        System.Console.Clear();
        DisplayTitle();

        foreach (var line in text.Split(Separator))
        {
            System.Console.SetCursorPosition(CenterX + padding, System.Console.CursorTop);
            System.Console.WriteLine(line);
        }

        System.Console.ReadKey();
    }

    private static string GetVersion()
    {
        var version = "";

        try
        {
            var attributes = typeof(MainScreen).Assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);

            version = attributes.Length == 0
                ? "Unknown"
                : ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
            version = version[..(version.IndexOf('+') + 8)];

            if (!(version.Contains("rc") | version.Contains("alpha") | version.Contains("beta")))
                return version[..version.IndexOf('+')];
        }
        catch
        {
            version = "Unknown";
        }

        return version;
    }
}