﻿using System.Text;
using Reveche.MazeRunner.Console.Screens;

namespace Reveche.MazeRunner.Console;

public static class MazeRunnerConsole
{
    public static BackgroundSoundManager BackgroundSoundManager { get; private set; } = null!;

    public static void Main()
    {
        System.Console.InputEncoding = Encoding.UTF8;
        System.Console.OutputEncoding = Encoding.UTF8;

        System.Console.CursorVisible = false;
        System.Console.Title = "Maze Runner";

        // Start the background music thread
        BackgroundSoundManager = new BackgroundSoundManager();
        BackgroundSoundManager.StartBackgroundMusic();

        MainScreen.StartMenu();

        BackgroundSoundManager.StopBackgroundMusic();
    }
}