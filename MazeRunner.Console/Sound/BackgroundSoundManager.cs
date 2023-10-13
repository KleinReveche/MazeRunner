using Reveche.MazeRunner.Console.Screens;

namespace Reveche.MazeRunner.Console.Sound;

public class BackgroundSoundManager
{
    private Thread? _musicThread;
    private CancellationTokenSource _cancellationTokenSource = new();

    public void StartBackgroundMusic()
    {
        _musicThread = new Thread(() =>
        {
            var musicPlayer = new MusicPlayer(MainScreen.GameState);
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
