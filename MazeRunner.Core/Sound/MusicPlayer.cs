namespace Reveche.MazeRunner.Sound;

public class MusicPlayer(OptionsState optionsState)
{
    private readonly List<(string soundName, int length)> _mp3Resources =
    [
        ("BitBeats3.mp3", 82_155),
        ("KLPeachGameOverII.mp3", 20_062)
    ];

    public void PlayBackgroundMusic(CancellationToken cancellationToken)
    {
        var player = new MusicPlayerCore(optionsState);
        var random = new Random();

        while (!cancellationToken.IsCancellationRequested && optionsState.IsSoundOn)
        {
            var randomIndex = random.Next(_mp3Resources.Count);
            using var sound = typeof(MusicPlayer).Assembly
                .GetManifestResourceStream(
                    $"Reveche.MazeRunner.Resources.Music.{_mp3Resources[randomIndex].soundName}")!;
            
            player.PlaySound(sound);
                
            var mp3ResourceLength = _mp3Resources[randomIndex].length;
            if (optionsState.IsCurrentlyPlaying) Thread.Sleep(mp3ResourceLength);

            // This ensures that unnecessary checks are not done when the game is not playing.
            var millisecondsPassed = 0;
            while (!cancellationToken.IsCancellationRequested
                   && optionsState is { IsSoundOn: true, IsCurrentlyPlaying: false }
                   && millisecondsPassed < mp3ResourceLength)
            {
                millisecondsPassed += 500;
                Thread.Sleep(500);
            }
        }
        player.Dispose();
    }
}