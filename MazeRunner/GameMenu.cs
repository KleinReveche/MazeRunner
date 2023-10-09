using System.Text;

namespace Reveche.MazeRunner;

public static class GameMenu
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

    public static readonly int CenterX = (Console.WindowWidth - Runner.Split('\n')[0].Length) / 2;

    public static readonly GameState GameState = new();
    private static readonly GameEngine GameEngine = new(GameState);
    private static readonly OptionMenu OptionMenu = new(GameState);

    public static void DisplayTitle()
    {
        var buffer = new StringBuilder();
        Console.ForegroundColor = ConsoleColor.White;
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
        Console.Clear();
        Console.Write(buffer.ToString());
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void StartMenu()
    {
        Dictionary<string, Action> menuOptions = new()
        {
            {
                "Start", () => { GameEngine.Play(); }
            },
            { "Options", OptionMenu.DisplayOptions },
            { "Credits", ShowCreditsScreen },
            { "Quit", () => Environment.Exit(0) }
        };

        var selectedIndex = 0;
        var buffer = new StringBuilder();

        while (true)
        {
            buffer.Clear();
            Console.Clear();
            DisplayTitle();

            var optionKeys = menuOptions.Keys.ToList();

            for (var i = 0; i < menuOptions.Count; i++)
            {
                var option = optionKeys[i];
                buffer.Append(' ', CenterX + 20);

                if (i == selectedIndex)
                    buffer.AppendLine("\u001b[93m>> " + option + " <<\u001b[0m");
                else
                    buffer.AppendLine("   " + option);
            }

            Console.WriteLine(buffer);
            var keyInfo = Console.ReadKey();

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
                             Bit Beats 3 by by XtremeFreddy from Pixabay

                             © 2023 Klein Reveche. All rights reserved
                             """;

        Console.Clear();
        DisplayTitle();

        foreach (var line in about.Split('\n'))
        {
            Console.SetCursorPosition(CenterX + 8, Console.CursorTop);
            Console.WriteLine(line);
        }

        Console.ReadKey();
        StartMenu();
    }
}