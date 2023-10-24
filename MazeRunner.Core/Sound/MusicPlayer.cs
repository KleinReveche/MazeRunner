using System.Reflection;

namespace Reveche.MazeRunner.Sound;

public class MusicPlayer(GameState gameState)
{
    public void PlayBackgroundMusic(CancellationToken cancellationToken)
    {
        var mp3Resources = new List<(CachedSound sound, int length)>
        {
            (new CachedSound(GetMusicStream("BitBeats3.mp3")), 82_155),
            (new CachedSound(GetMusicStream("KLPeachGameOverII.mp3")), 20_062)
        };

        var random = new Random();

        while (!cancellationToken.IsCancellationRequested && gameState.IsSoundOn)
        {
            var randomIndex = random.Next(mp3Resources.Count);
            var sound = mp3Resources[randomIndex].sound;
            var mp3ResourceLength = mp3Resources[randomIndex].length;

            AudioPlaybackEngine.Instance.RemoveLast();
            AudioPlaybackEngine.Instance.PlaySound(sound);

            if (gameState.IsCurrentlyPlaying) Thread.Sleep(mp3ResourceLength);

            // This ensures that unnecessary checks are not done when the game is not playing.
            var millisecondsPassed = 0;
            while (!cancellationToken.IsCancellationRequested
                   && gameState is { IsSoundOn: true, IsCurrentlyPlaying: false }
                   && millisecondsPassed < mp3ResourceLength)
            {
                millisecondsPassed += 500;
                Thread.Sleep(500);
            }
        }

        return;

        Stream GetMusicStream(string resourceName) 
            => Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Reveche.MazeRunner.Resources.Music.{resourceName}")!;
        
    }
}