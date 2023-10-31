using Reveche.MazeRunner.Console.Screens;
using Reveche.MazeRunner.Sound;

namespace Reveche.MazeRunner.Console;

public class BackgroundSoundManager
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private Thread? _musicThread;

    public void StartBackgroundMusic()
    {
        _musicThread = new Thread(() =>
        {
            var musicPlayer = new MusicPlayer(MainScreen.OptionsState);
            musicPlayer.PlayBackgroundMusic(_cancellationTokenSource.Token);
        });

        _musicThread.Start();
    }

    public void StopBackgroundMusic()
    {
        if (_musicThread == null) return;
        _cancellationTokenSource.Cancel();
        _musicThread?.Join();
    }

    public void RestartBackgroundMusic()
    {
        if (_musicThread == null) return;
        _cancellationTokenSource.Cancel();
        _musicThread?.Join();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        StartBackgroundMusic();
    }
}