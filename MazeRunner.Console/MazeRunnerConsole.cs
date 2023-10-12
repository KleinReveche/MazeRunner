﻿using System.Text;

namespace Reveche.MazeRunner.Console;

public static class MazeRunnerConsole
{
    private static Thread? _musicThread;
    private static CancellationTokenSource _cancellationTokenSource = new();

    public static void Main(string[] args)
    {
        System.Console.InputEncoding = Encoding.UTF8;
        System.Console.OutputEncoding = Encoding.UTF8;

        System.Console.CursorVisible = false;
        System.Console.Title = "Maze Runner";

        // Start the background music thread
        if (OperatingSystem.IsWindows())
        {
            _musicThread = new Thread(() =>
            {
                var musicPlayer = new MusicPlayer(GameMenu.GameState);
                musicPlayer.PlayBackgroundMusic(_cancellationTokenSource.Token);
            });

            _musicThread.Start();
        }

        GameMenu.StartMenu();
        System.Console.ReadKey();

        if (_musicThread == null) return;
        _cancellationTokenSource.Cancel();
        _musicThread.Join();
    }

    public static void StopMusic()
    {
        if (_musicThread == null) return;
        _cancellationTokenSource.Cancel();
        _musicThread.Join();
    }

    public static void RestartMusic()
    {
        if (_musicThread == null) return;
        _cancellationTokenSource.Cancel();
        _musicThread.Join();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        _musicThread = new Thread(() =>
        {
            var musicPlayer = new MusicPlayer(GameMenu.GameState);
            musicPlayer.PlayBackgroundMusic(_cancellationTokenSource.Token);
        });

        _musicThread.Start();
    }
}