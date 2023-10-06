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
    
    #pragma warning disable CA1416 //Before called, the OS is checked to ensure that the SoundPlayer class is available.
    public void PlayBackgroundMusic(CancellationToken cancellationToken)
    {
        const string resLoc = "Reveche.MazeRunner.Music.";
        var wavResources = new Dictionary<string, int>
        {
            { $"{resLoc}8BitAirFight.wav", 115_000},
            { $"{resLoc}BitBeats3.wav", 82_000 },
            { $"{resLoc}KLPeachGameOverII.wav", 20_000}
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