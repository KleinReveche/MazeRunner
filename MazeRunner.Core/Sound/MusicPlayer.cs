using System.Reflection;
using NAudio.Wave;
using NLayer.NAudioSupport;

namespace Reveche.MazeRunner.Sound;

public class MusicPlayer(OptionsState optionsState)
{
    private readonly List<(string soundName, int length)> _mp3Resources = new()
    {
        ("BitBeats3.mp3", 82_155),
        ("KLPeachGameOverII.mp3", 20_062)
    };

    public void PlayBackgroundMusic(CancellationToken cancellationToken)
    {
        var random = new Random();

        while (!cancellationToken.IsCancellationRequested && optionsState.IsSoundOn)
        {
            var randomIndex = random.Next(_mp3Resources.Count);
            using var sound = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(
                        $"Reveche.MazeRunner.Resources.Music.{_mp3Resources[randomIndex].soundName}")!;

            if (OperatingSystem.IsWindows())
            {
                PlayInWindows(sound);
            }
            else if (OperatingSystem.IsLinux())
            {
                PlayInLinux(sound, optionsState);

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
            else
            {
                break;
            }
        }
    }

    public static void PlayInWindows(Stream sound)
    {
        using var audioFile = new Mp3FileReader(sound);
        using var outputDevice = new WaveOutEvent();
        outputDevice.Init(audioFile);
        outputDevice.Play();

        while (outputDevice.PlaybackState == PlaybackState.Playing) Thread.Sleep(1000);
    }

    public static void PlayInLinux(Stream sound, OptionsState optionsState)
    {
        var audioPlaybackEngineLinux = new MusicPlayerLinux(optionsState);
        audioPlaybackEngineLinux.PlaySound(sound);
    }

    private class Mp3FileReader(Stream inputStream) : Mp3FileReaderBase(inputStream, CreateAcmFrameDecompressor, false)
    {
        private static IMp3FrameDecompressor CreateAcmFrameDecompressor(WaveFormat mp3Format)
        {
            return new Mp3FrameDecompressor(mp3Format);
        }
    }
}