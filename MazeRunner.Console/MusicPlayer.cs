using System.Diagnostics.CodeAnalysis;
using System.Media;
using System.Reflection;

namespace Reveche.MazeRunner.Console;

public class MusicPlayer
{
    private readonly GameState _gameState;
    private SoundPlayer? _player;

    public MusicPlayer(GameState gameState)
    {
        _gameState = gameState;
        _player = null;
    }

    // This method is only available on Windows and is checked before calling it.
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public void PlayBackgroundMusic(CancellationToken cancellationToken)
    {
        const string resLoc = "Reveche.MazeRunner.Console.Resources.Music.";
        var wavResources = new Dictionary<string, int>
        {
            { $"{resLoc}BitBeats3.wav", 82_155 },
            { $"{resLoc}KLPeachGameOverII.wav", 20_062 }
        };

        var random = new Random();

        while (!cancellationToken.IsCancellationRequested && _gameState.IsSoundOn)
        {
            var randomIndex = random.Next(wavResources.Count);
            var wavResourceKeys = wavResources.Keys.ToArray();
            var wavResourceLength = wavResources.Values.ToArray();
            var selectedResource = wavResourceKeys[randomIndex];

            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(selectedResource);
            _player = new SoundPlayer(resourceStream);
            _player.Play();

            if (_gameState.IsCurrentlyPlaying) Thread.Sleep(wavResourceLength[randomIndex]);

            // This ensures that unnecessary checks are not done when the game is not playing.
            var millisecondsPassed = 0;
            while (!cancellationToken.IsCancellationRequested
                   && _gameState is { IsSoundOn: true, IsCurrentlyPlaying: false }
                   && millisecondsPassed < wavResourceLength[randomIndex])
            {
                millisecondsPassed += 500;
                Thread.Sleep(500);
            }
        }

        _player?.Stop();
        _player?.Dispose();
    }
}