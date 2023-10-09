using System.Diagnostics.CodeAnalysis;
using System.Media;
using System.Reflection;

namespace Reveche.MazeRunner;

public class MusicPlayer
{
    private readonly GameState _gameState;

    public MusicPlayer(GameState gameState)
    {
        _gameState = gameState;
    }

    // This method is only available on Windows and is checked before calling it.
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public void PlayBackgroundMusic(CancellationToken cancellationToken)
    {
        const string resLoc = "Reveche.MazeRunner.Music.";
        var wavResources = new Dictionary<string, int>
        {
            { $"{resLoc}BitBeats3.wav", 82_000 },
            { $"{resLoc}KLPeachGameOverII.wav", 20_000 }
        };

        var random = new Random();

        SoundPlayer? player = null;

        while (!cancellationToken.IsCancellationRequested && _gameState.IsSoundOn)
        {
            var randomIndex = random.Next(wavResources.Count);
            var wavResourceKeys = wavResources.Keys.ToArray();
            var wavResourceLength = wavResources.Values.ToArray();
            var selectedResource = wavResourceKeys[randomIndex];

            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(selectedResource);
            player = new SoundPlayer(resourceStream);
            player.Play();
            Thread.Sleep(wavResourceLength[randomIndex]);
        }

        player?.Dispose();
    }
}