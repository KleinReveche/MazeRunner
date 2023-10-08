namespace Reveche.MazeRunner;

public static class MazeRunner
{
    public static void Main(string[] args)
    {
        Console.CursorVisible = false;
        Console.Title = "Maze Runner";

        var cancellationTokenSource = new CancellationTokenSource();
        Thread? musicThread = null;

        // Start the background music thread
        if (OperatingSystem.IsWindows())
        {
            musicThread = new Thread(() =>
            {
                var musicPlayer = new MusicPlayer(GameMenu.GameState);
                musicPlayer.PlayBackgroundMusic(cancellationTokenSource.Token);
            });

            musicThread.Start();
        }

        GameMenu.StartMenu();
        Console.ReadKey();

        if (musicThread == null) return;
        cancellationTokenSource.Cancel();
        musicThread.Join();
    }
}